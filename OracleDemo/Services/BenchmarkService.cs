// -----------------------------------------------------------------------------
// BenchmarkService – Singleton Pattern
// -----------------------------------------------------------------------------
// This service collects performance measurements (like query times) from anywhere in the app.
// Singleton means there is only ONE instance of this class in the whole application.
// Every controller uses the same instance, so measurements are shared.
// -----------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace OracleDemo.Services;

public class BenchmarkService
{
  // Lazy<T> creates the instance only when first needed (thread‑safe)
  private static readonly Lazy<BenchmarkService> _instance =
      new(() => new BenchmarkService());

  // Public property to access the single instance
  public static BenchmarkService Instance => _instance.Value;

  // Thread‑safe dictionary to store measurements (key = test name, value = list of times)
  private readonly ConcurrentDictionary<string, List<long>> _measurements;

  // Private constructor – nobody else can create an instance
  private BenchmarkService()
  {
    _measurements = new ConcurrentDictionary<string, List<long>>();
  }

  // Record a measurement (e.g., "slow-query", 150 milliseconds)
  public void RecordMeasurement(string testName, long milliseconds)
  {
    _measurements.AddOrUpdate(testName,
        new List<long> { milliseconds },
        (key, list) => { list.Add(milliseconds); return list; });
  }

  // Get a summary of all measurements (average, min, max, count)
  public Dictionary<string, object> GetSummary()
  {
    return _measurements.ToDictionary(
        kvp => kvp.Key,
        kvp => (object)new
        {
          Average = kvp.Value.Average(),
          Min = kvp.Value.Min(),
          Max = kvp.Value.Max(),
          Count = kvp.Value.Count
        });
  }

  // Clear all measurements (useful for starting a fresh test)
  public void Clear() => _measurements.Clear();
}