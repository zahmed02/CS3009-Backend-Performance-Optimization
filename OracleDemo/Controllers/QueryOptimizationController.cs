// -----------------------------------------------------------------------------
// STEP 2 – QUERY OPTIMIZATION BENCHMARKING
// -----------------------------------------------------------------------------
// This controller demonstrates various query optimization techniques:
// 1. Projection (select only needed columns)
// 2. Tracking vs AsNoTracking (disabling change tracking for read‑only queries)
// 3. LINQ vs Raw SQL (hand‑written SQL can sometimes be faster)
// 4. Contains vs Like (leading wildcard prevents index usage)
// 5. A combined benchmark that runs all tests multiple times.
// Each endpoint returns execution time and record count for comparison.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Models;
using System.Diagnostics;

namespace OracleDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QueryOptimizationController : ControllerBase
{
  private readonly AppDbContext _context;

  public QueryOptimizationController(AppDbContext context)
  {
    _context = context;
  }

  // -------------------------------------------------------------------------
  // 2.1 – Projection Comparison (SELECT * vs SELECT only needed columns)
  // -------------------------------------------------------------------------
  // Loading full entities (all columns) transfers more data and uses more memory.
  // Projection selects only the columns we actually need, reducing network and memory overhead.
  // This endpoint compares both and shows the performance improvement.
  // -------------------------------------------------------------------------
  [HttpGet("projection")]
  public async Task<IActionResult> CompareProjection()
  {
    var sw = Stopwatch.StartNew();

    // Full entity load (SELECT *)
    var fullEntities = await _context.Products.ToListAsync();
    sw.Stop();
    var fullTime = sw.ElapsedMilliseconds;

    sw.Restart();
    // Projection: only select needed columns (ProductId, ProductName, Price)
    var projected = await _context.Products
        .Select(p => new { p.ProductId, p.ProductName, p.Price })
        .ToListAsync();
    sw.Stop();
    var projectionTime = sw.ElapsedMilliseconds;

    return Ok(new
    {
      fullEntityLoad = new { timeMs = fullTime, count = fullEntities.Count },
      projection = new { timeMs = projectionTime, count = projected.Count },
      improvement = $"{(fullTime - projectionTime) / (double)fullTime * 100:F2}%"
    });
  }

  // -------------------------------------------------------------------------
  // 2.2 – Tracking vs AsNoTracking
  // -------------------------------------------------------------------------
  // By default, EF Core tracks changes to entities (for later updates).
  // For read‑only queries, we don't need tracking. AsNoTracking() disables it,
  // which reduces memory usage and speeds up execution.
  // -------------------------------------------------------------------------
  [HttpGet("tracking")]
  public async Task<IActionResult> CompareTracking()
  {
    var sw = Stopwatch.StartNew();

    // With tracking (default)
    var withTracking = await _context.Products.ToListAsync();
    sw.Stop();
    var trackingTime = sw.ElapsedMilliseconds;

    sw.Restart();
    // Without tracking (AsNoTracking)
    var withoutTracking = await _context.Products.AsNoTracking().ToListAsync();
    sw.Stop();
    var noTrackingTime = sw.ElapsedMilliseconds;

    return Ok(new
    {
      withTracking = new { timeMs = trackingTime, count = withTracking.Count },
      withoutTracking = new { timeMs = noTrackingTime, count = withoutTracking.Count },
      improvement = $"{(trackingTime - noTrackingTime) / (double)trackingTime * 100:F2}%"
    });
  }

  // -------------------------------------------------------------------------
  // 2.3 – LINQ vs Raw SQL
  // -------------------------------------------------------------------------
  // EF Core translates LINQ queries to SQL, but sometimes the generated SQL is not optimal.
  // Hand‑written raw SQL can be faster for complex queries.
  // This endpoint compares a simple LINQ query with its raw SQL equivalent.
  // -------------------------------------------------------------------------
  [HttpGet("linq-vs-sql")]
  public async Task<IActionResult> CompareLinqVsSql()
  {
    var searchTerm = "A%";
    var sw = Stopwatch.StartNew();

    // LINQ query (EF Core translates to SQL)
    var linqResult = await _context.Products
        .Where(p => EF.Functions.Like(p.ProductName, searchTerm))
        .ToListAsync();
    sw.Stop();
    var linqTime = sw.ElapsedMilliseconds;

    sw.Restart();
    // Raw SQL – parameterized to avoid injection
    var sqlResult = await _context.Products
        .FromSqlRaw("SELECT * FROM PRODUCTS WHERE PRODUCT_NAME LIKE {0}", searchTerm)
        .ToListAsync();
    sw.Stop();
    var sqlTime = sw.ElapsedMilliseconds;

    return Ok(new
    {
      linq = new { timeMs = linqTime, count = linqResult.Count },
      rawSql = new { timeMs = sqlTime, count = sqlResult.Count },
      improvement = $"{(linqTime - sqlTime) / (double)linqTime * 100:F2}%"
    });
  }

  // -------------------------------------------------------------------------
  // 2.4 – Contains vs Like (Index Usage)
  // -------------------------------------------------------------------------
  // Using Contains (or LIKE with a leading wildcard) prevents index usage.
  // A prefix search (LIKE 'A%') can use the index, making it much faster.
  // This endpoint demonstrates the dramatic difference.
  // -------------------------------------------------------------------------
  [HttpGet("contains-vs-like")]
  public async Task<IActionResult> CompareContainsVsLike()
  {
    var sw = Stopwatch.StartNew();

    // Contains (cannot use index because of leading wildcard)
    var containsResult = await _context.Products
        .Where(p => p.ProductName != null && p.ProductName.Contains("a"))
        .ToListAsync();
    sw.Stop();
    var containsTime = sw.ElapsedMilliseconds;

    sw.Restart();
    // Like with prefix (can use index)
    var likeResult = await _context.Products
        .Where(p => p.ProductName != null && EF.Functions.Like(p.ProductName, "A%"))
        .ToListAsync();
    sw.Stop();
    var likeTime = sw.ElapsedMilliseconds;

    return Ok(new
    {
      contains = new { timeMs = containsTime, count = containsResult.Count },
      like = new { timeMs = likeTime, count = likeResult.Count },
      improvement = $"{(containsTime - likeTime) / (double)containsTime * 100:F2}%"
    });
  }

  // -------------------------------------------------------------------------
  // 2.5 – Full Benchmark (runs all tests multiple times)
  // -------------------------------------------------------------------------
  // This endpoint runs each comparison (projection, tracking, LINQ vs SQL) for a given number of iterations.
  // It returns average, min, max times and improvement percentages.
  // Note: Caching may affect later runs, so the first run is most representative.
  // -------------------------------------------------------------------------
  [HttpGet("benchmark-all")]
  public async Task<IActionResult> BenchmarkAll([FromQuery] int iterations = 5)
  {
    var results = new Dictionary<string, object>();

    // Projection benchmark
    var projTimes = await RunComparison(iterations,
        async () => await _context.Products.ToListAsync(),
        async () => await _context.Products.Select(p => new { p.ProductId, p.ProductName }).ToListAsync());
    results["Projection"] = projTimes;

    // Tracking benchmark
    var trackingTimes = await RunComparison(iterations,
        async () => await _context.Products.ToListAsync(),
        async () => await _context.Products.AsNoTracking().ToListAsync());
    results["Tracking"] = trackingTimes;

    // LINQ vs SQL benchmark
    var sqlTimes = await RunComparison(iterations,
        async () => await _context.Products.Where(p => EF.Functions.Like(p.ProductName, "A%")).ToListAsync(),
        async () => await _context.Products.FromSqlRaw("SELECT * FROM PRODUCTS WHERE PRODUCT_NAME LIKE 'A%'").ToListAsync());
    results["LinqVsSql"] = sqlTimes;

    return Ok(results);
  }

  // Helper method to run a comparison multiple times and collect statistics
  private async Task<object> RunComparison(int iterations,
      Func<Task> slowFunc, Func<Task> fastFunc)
  {
    var slowTimes = new List<long>();
    var fastTimes = new List<long>();

    for (int i = 0; i < iterations; i++)
    {
      var sw = Stopwatch.StartNew();
      await slowFunc();
      sw.Stop();
      slowTimes.Add(sw.ElapsedMilliseconds);

      sw.Restart();
      await fastFunc();
      sw.Stop();
      fastTimes.Add(sw.ElapsedMilliseconds);
    }

    return new
    {
      slow = new { avg = slowTimes.Average(), min = slowTimes.Min(), max = slowTimes.Max() },
      fast = new { avg = fastTimes.Average(), min = fastTimes.Min(), max = fastTimes.Max() },
      improvement = $"{(slowTimes.Average() - fastTimes.Average()) / slowTimes.Average() * 100:F2}%"
    };
  }
}