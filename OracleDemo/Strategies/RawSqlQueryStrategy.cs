// -----------------------------------------------------------------------------
// RawSqlQueryStrategy – uses hand‑written SQL instead of LINQ
// -----------------------------------------------------------------------------
// Sometimes EF Core generates inefficient SQL. This strategy lets us write
// our own SQL to get better performance. The query is still parameterized
// to prevent SQL injection attacks.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Models;

namespace OracleDemo.Strategies;

public class RawSqlQueryStrategy : IQueryStrategy
{
  public async Task<List<Product>> ExecuteAsync(AppDbContext context)
  {
    // Hand‑written SQL – can be faster than LINQ in some cases
    return await context.Products
        .FromSqlRaw("SELECT * FROM PRODUCTS WHERE PRODUCT_NAME LIKE 'A%'")
        .ToListAsync();
  }
}