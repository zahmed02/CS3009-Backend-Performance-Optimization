// -----------------------------------------------------------------------------
// IndexedQueryStrategy – uses the database index for fast searching
// -----------------------------------------------------------------------------
// This strategy searches for product names that start with 'A' (LIKE 'A%').
// Because there is no leading wildcard, the database can use the index
// on ProductName. This makes the query very fast.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Models;

namespace OracleDemo.Strategies;

public class IndexedQueryStrategy : IQueryStrategy
{
  public async Task<List<Product>> ExecuteAsync(AppDbContext context)
  {
    // Prefix search – can use the index
    return await context.Products
        .Where(p => p.ProductName != null && EF.Functions.Like(p.ProductName, "A%"))
        .ToListAsync();
  }
}