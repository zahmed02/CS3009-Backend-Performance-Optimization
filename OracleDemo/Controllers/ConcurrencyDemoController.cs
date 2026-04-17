// -----------------------------------------------------------------------------
// STEP 3 – CONCURRENCY HANDLING (Optimistic Concurrency)
// -----------------------------------------------------------------------------
// This controller demonstrates how to prevent lost updates when multiple users
// try to modify the same record at the same time.
// We use a "RowVersion" column (long integer) that increments on every update.
// When updating, we check if the version still matches the one the user read.
// If not, a conflict occurs and we return a 409 error.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Models;

namespace OracleDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConcurrencyDemoController : ControllerBase
{
  private readonly AppDbContext _context;
  private readonly IServiceScopeFactory _scopeFactory;

  // Dependency Injection: we receive DbContext and a scope factory
  // to create fresh DbContext instances for background tasks.
  public ConcurrencyDemoController(AppDbContext context, IServiceScopeFactory scopeFactory)
  {
    _context = context;
    _scopeFactory = scopeFactory;
  }

  // 3.1 – Get a product by ID (includes the current RowVersion)
  // Clients must read the product first to get the latest version.
  [HttpGet("{id}")]
  public async Task<ActionResult<Product>> GetProduct(int id)
  {
    var product = await _context.Products.FindAsync(id);
    if (product == null) return NotFound();
    return product;
  }

  // 3.2 – Update a product with optimistic concurrency check
  // The client must send the product with the RowVersion they originally read.
  // If the version in the database has changed, we reject the update.
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
  {
    if (id != updatedProduct.ProductId) return BadRequest();

    // Fetch the current product from the database to get its RowVersion
    var existing = await _context.Products.FindAsync(id);
    if (existing == null) return NotFound();

    // Ensure the incoming RowVersion matches the current one (optimistic check)
    if (existing.RowVersion != updatedProduct.RowVersion)
    {
      return Conflict(new
      {
        message = "Concurrency conflict: The product was modified by another user.",
        currentVersion = existing.RowVersion,
        sentVersion = updatedProduct.RowVersion
      });
    }

    // Update fields
    existing.ProductName = updatedProduct.ProductName;
    existing.Price = updatedProduct.Price;
    // Increment the version (this is the key step for optimistic concurrency)
    existing.RowVersion++;

    _context.Entry(existing).State = EntityState.Modified;

    try
    {
      await _context.SaveChangesAsync();
      return NoContent(); // Success
    }
    catch (DbUpdateConcurrencyException)
    {
      // Another change happened between our check and save
      return Conflict(new { message = "Concurrency conflict. Please refresh and try again." });
    }
  }

  // 3.3 – Simulate a race condition: two users try to update the same product at the same time.
  // We launch two background tasks that each try to update the same product.
  // Only one will succeed; the other will get a concurrency conflict.
  [HttpPost("simulate-race")]
  public async Task<IActionResult> SimulateRace()
  {
    var product = await _context.Products.FirstOrDefaultAsync();
    if (product == null) return NotFound("No product to test.");

    // Use a scope factory to create a new DbContext in each task
    var task1 = UpdateInBackground(product.ProductId, "User A", "Updated by A");
    var task2 = UpdateInBackground(product.ProductId, "User B", "Updated by B");

    await Task.WhenAll(task1, task2);

    return Ok(new { userA = task1.Result, userB = task2.Result });
  }

  // Helper method that runs in a separate background task (simulates a different user session)
  private async Task<string> UpdateInBackground(int id, string userName, string newName)
  {
    try
    {
      using var scope = _scopeFactory.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

      var product = await db.Products.FindAsync(id);
      if (product == null) return $"{userName}: Product not found";

      // Simulate change: if the product's RowVersion hasn't changed since we read it, update
      // For simplicity, we'll just increment the version and save.
      product.ProductName = newName;
      product.RowVersion++;   // increment version

      await db.SaveChangesAsync();
      return $"{userName}: Update succeeded";
    }
    catch (DbUpdateConcurrencyException)
    {
      return $"{userName}: Update failed – concurrency conflict";
    }
  }
}