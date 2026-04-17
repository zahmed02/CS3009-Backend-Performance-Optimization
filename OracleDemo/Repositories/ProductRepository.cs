// -----------------------------------------------------------------------------
// ProductRepository – the real data access code
// -----------------------------------------------------------------------------
// This class implements IProductRepository using EF Core's DbContext.
// It contains all the actual database queries.
// The controller never sees this code – it only knows the interface.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Models;

namespace OracleDemo.Repositories;

public class ProductRepository : IProductRepository
{
  private readonly AppDbContext _context;

  // Constructor receives DbContext via Dependency Injection
  public ProductRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<Product>> GetAllAsync()
  {
    return await _context.Products.ToListAsync();
  }

  public async Task<Product?> GetByIdAsync(int id)
  {
    return await _context.Products.FindAsync(id);
  }

  public async Task<Product> AddAsync(Product product)
  {
    _context.Products.Add(product);
    await _context.SaveChangesAsync();  // save to database
    return product;
  }

  public async Task UpdateAsync(Product product)
  {
    _context.Entry(product).State = EntityState.Modified;
    await _context.SaveChangesAsync();  // save changes
  }

  public async Task DeleteAsync(int id)
  {
    var product = await _context.Products.FindAsync(id);
    if (product != null)
    {
      _context.Products.Remove(product);
      await _context.SaveChangesAsync();
    }
  }

  public async Task<int> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync();
  }
}