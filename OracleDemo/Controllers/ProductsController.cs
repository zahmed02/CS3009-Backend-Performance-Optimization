// -----------------------------------------------------------------------------
// ProductsController – uses Repository Pattern
// -----------------------------------------------------------------------------
// The controller does NOT talk directly to the database (DbContext).
// Instead, it uses IProductRepository, which hides all data access details.
// This makes the controller easy to test and easy to change the database later.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using OracleDemo.Models;
using OracleDemo.Repositories;   // Repository pattern interface

namespace OracleDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
  private readonly IProductRepository _productRepository;

  // Dependency Injection: the repository is given to us automatically.
  // We don't create it ourselves (no "new" keyword).
  public ProductsController(IProductRepository productRepository)
  {
    _productRepository = productRepository;
  }

  // GET: api/products – get all products
  [HttpGet]
  public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
  {
    var products = await _productRepository.GetAllAsync();
    return Ok(products);
  }

  // GET: api/products/{id} – get a single product by ID
  [HttpGet("{id}")]
  public async Task<ActionResult<Product>> GetProduct(int id)
  {
    var product = await _productRepository.GetByIdAsync(id);
    if (product == null) return NotFound();
    return product;
  }

  // POST: api/products – create a new product
  [HttpPost]
  public async Task<ActionResult<Product>> PostProduct(Product product)
  {
    product.CreatedDate = DateTime.Now;
    var created = await _productRepository.AddAsync(product);
    // Return a 201 Created response with the location of the new product
    return CreatedAtAction(nameof(GetProduct), new { id = created.ProductId }, created);
  }

  // PUT: api/products/{id} – update an existing product
  [HttpPut("{id}")]
  public async Task<IActionResult> PutProduct(int id, Product product)
  {
    if (id != product.ProductId) return BadRequest();
    await _productRepository.UpdateAsync(product);
    return NoContent(); // 204 No Content means success
  }

  // DELETE: api/products/{id} – delete a product
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteProduct(int id)
  {
    await _productRepository.DeleteAsync(id);
    return NoContent();
  }
}