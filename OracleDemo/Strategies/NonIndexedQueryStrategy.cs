// -----------------------------------------------------------------------------
// NonIndexedQueryStrategy – forces a full table scan (slow)
// -----------------------------------------------------------------------------
// This strategy searches for product names containing the letter 'a' anywhere.
// The leading wildcard ('%a%') prevents the database from using the index.
// As a result, the database must scan every row – this is slow.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Models;

namespace OracleDemo.Strategies;

public class NonIndexedQueryStrategy : IQueryStrategy
{
  public async Task<List<Product>> ExecuteAsync(AppDbContext context)
  {
    // Leading wildcard – cannot use the index
    return await context.Products
        .Where(p => p.ProductName != null && EF.Functions.Like(p.ProductName, "%a%"))
        .ToListAsync();
  }
}