// -----------------------------------------------------------------------------
// 1.4 – Database Indexing Impact Analysis – Demonstrate the effect of the index
// -----------------------------------------------------------------------------
// This controller provides endpoints to compare query performance with and without using the index. 
// The "slow" endpoint uses a pattern that cannot benefit from the index (leading wildcard). 
// The "fast" endpoint uses a prefix search that can use the index. 
// The "benchmark" endpoint runs both multiple times and returns average times.
// NEW: Strategy pattern endpoint allows runtime selection of query algorithms.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Models;
using OracleDemo.Strategies;      // Strategy pattern
using OracleDemo.Services;        // Singleton pattern
using System.Diagnostics;

namespace OracleDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IndexingDemoController : ControllerBase
{
  private readonly AppDbContext _context;
  private readonly IServiceProvider _serviceProvider;  // For strategy resolution
  private readonly BenchmarkService _benchmarkService;  // Singleton instance

  // Dependency Injection: both DbContext and IServiceProvider are injected.
  // BenchmarkService is a singleton obtained via DI (already registered).
  public IndexingDemoController(AppDbContext context, IServiceProvider serviceProvider, BenchmarkService benchmarkService)
  {
    _context = context;
    _serviceProvider = serviceProvider;
    _benchmarkService = benchmarkService;
  }

  // 1.4.1 - Slow endpoint: no index usage (leading wildcard)
  // This query uses %a% which has a leading wildcard.
  // Oracle cannot use the index here, so it performs a full table scan (slower).
  [HttpGet("slow")]
  public async Task<IActionResult> GetProductsSlow()
  {
    var sw = Stopwatch.StartNew();
    var products = await _context.Products
        .Where(p => p.ProductName != null && EF.Functions.Like(p.ProductName, "%a%"))
        .ToListAsync();
    sw.Stop();

    // Record measurement in singleton service
    _benchmarkService.RecordMeasurement("slow-query", sw.ElapsedMilliseconds);

    return Ok(new
    {
      timeMs = sw.ElapsedMilliseconds,
      count = products.Count,
      method = "SLOW - table scan (no index usage)",
      data = products.Take(10)
    });
  }

  // 1.4.2 - Fast endpoint: uses the index (prefix search)
  [HttpGet("fast")]
  public async Task<IActionResult> GetProductsFast()
  {
    var sw = Stopwatch.StartNew();
    var products = await _context.Products
        .Where(p => p.ProductName != null && EF.Functions.Like(p.ProductName, "A%"))
        .ToListAsync();
    sw.Stop();

    _benchmarkService.RecordMeasurement("fast-query", sw.ElapsedMilliseconds);

    return Ok(new
    {
      timeMs = sw.ElapsedMilliseconds,
      count = products.Count,
      method = "FAST - index scan (index on ProductName)",
      data = products.Take(10)
    });
  }

  // 1.4.3 - Benchmark endpoint: runs both versions multiple times
  [HttpGet("benchmark")]
  public async Task<IActionResult> Benchmark([FromQuery] int iterations = 10)
  {
    var slowTimes = new List<long>();
    var fastTimes = new List<long>();

    for (int i = 0; i < iterations; i++)
    {
      var sw = Stopwatch.StartNew();
      await _context.Products
          .Where(p => p.ProductName != null && EF.Functions.Like(p.ProductName, "%a%"))
          .ToListAsync();
      sw.Stop();
      slowTimes.Add(sw.ElapsedMilliseconds);

      sw.Restart();
      await _context.Products
          .Where(p => p.ProductName != null && EF.Functions.Like(p.ProductName, "A%"))
          .ToListAsync();
      sw.Stop();
      fastTimes.Add(sw.ElapsedMilliseconds);
    }

    return Ok(new
    {
      iterations,
      slow = new { avg = slowTimes.Average(), min = slowTimes.Min(), max = slowTimes.Max() },
      fast = new { avg = fastTimes.Average(), min = fastTimes.Min(), max = fastTimes.Max() },
      improvement = $"{(slowTimes.Average() - fastTimes.Average()) / slowTimes.Average() * 100:F2}%"
    });
  }

  // 1.4.4 - Strategy Pattern Endpoint: execute any query algorithm at runtime.
  // Strategy pattern: defines a family of algorithms, encapsulates each, and makes them interchangeable.
  // This endpoint accepts a strategy name and executes the corresponding query implementation.
  [HttpGet("strategy/{strategyName}")]
  public async Task<IActionResult> ExecuteStrategy(string strategyName)
  {
    // Resolve the appropriate strategy from DI container.
    IQueryStrategy strategy = strategyName.ToLower() switch
    {
      "indexed" => _serviceProvider.GetRequiredService<IndexedQueryStrategy>(),
      "nonindexed" => _serviceProvider.GetRequiredService<NonIndexedQueryStrategy>(),
      "rawsql" => _serviceProvider.GetRequiredService<RawSqlQueryStrategy>(),
      _ => throw new ArgumentException("Unknown strategy. Use 'indexed', 'nonindexed', or 'rawsql'.")
    };

    var sw = Stopwatch.StartNew();
    var products = await strategy.ExecuteAsync(_context);
    sw.Stop();

    // Record result in singleton benchmark service
    _benchmarkService.RecordMeasurement($"strategy-{strategyName}", sw.ElapsedMilliseconds);

    return Ok(new
    {
      strategy = strategyName,
      timeMs = sw.ElapsedMilliseconds,
      count = products.Count,
      description = strategy.GetType().Name
    });
  }

  // 1.4.5 - Helper: Generate test data
  [HttpPost("generate-test-data")]
  public async Task<IActionResult> GenerateTestData([FromQuery] int count = 10000)
  {
    var random = new Random();
    var products = new List<Product>();
    for (int i = 0; i < count; i++)
    {
      string namePrefix = (i % 26).ToString()[0].ToString();
      products.Add(new Product
      {
        ProductName = $"{namePrefix} Product {i}",
        Price = random.Next(10, 1000),
        CreatedDate = DateTime.Now
      });
    }

    await _context.Products.AddRangeAsync(products);
    await _context.SaveChangesAsync();

    return Ok($"Added {count} test products");
  }

  // NEW: Get all benchmark results aggregated by the singleton service.
  [HttpGet("benchmark-summary")]
  public IActionResult GetBenchmarkSummary()
  {
    // Singleton pattern ensures only one instance holds all measurements.
    return Ok(_benchmarkService.GetSummary());
  }
}