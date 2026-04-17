// -----------------------------------------------------------------------------
// STEP 4 – MULTITHREADING vs ASYNCHRONOUS PROCESSING
// -----------------------------------------------------------------------------
// This controller demonstrates the difference between:
// - Synchronous (blocking) vs Asynchronous (non‑blocking) I/O
// - Sequential vs Parallel execution
// - CPU‑bound work on the request thread vs offloaded to thread pool
// - Load testing to compare async vs sync under concurrent requests
//
// Key takeaways:
// - Async I/O (Task.Delay) frees the thread while waiting, improving scalability.
// - Parallel async tasks (Task.WhenAll) run concurrently, reducing total time.
// - For CPU‑heavy work, offloading to thread pool keeps the request thread free.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading;

namespace OracleDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AsyncVsSyncController : ControllerBase
{
  // -------------------------------------------------------------------------
  // 4.1 – Synchronous I/O (blocking)
  // -------------------------------------------------------------------------
  // Thread.Sleep blocks the current thread for 2 seconds.
  // While sleeping, the thread cannot serve any other requests.
  // This is bad for scalability because threads are a limited resource.
  // -------------------------------------------------------------------------
  [HttpGet("sync")]
  public IActionResult SyncIO()
  {
    var sw = Stopwatch.StartNew();
    var threadId = Thread.CurrentThread.ManagedThreadId;

    // Simulate a slow I/O operation (e.g., network call, disk read)
    Thread.Sleep(2000); // blocks the current thread for 2 seconds

    sw.Stop();
    return Ok(new
    {
      type = "Synchronous I/O (blocking)",
      threadId = threadId,
      timeMs = sw.ElapsedMilliseconds
    });
  }

  // -------------------------------------------------------------------------
  // 4.2 – Asynchronous I/O (non‑blocking)
  // -------------------------------------------------------------------------
  // Task.Delay does NOT block the thread. It returns a task that completes after 2 seconds.
  // While waiting, the thread is freed to handle other requests.
  // This allows the server to handle many concurrent I/O operations with few threads.
  // -------------------------------------------------------------------------
  [HttpGet("async")]
  public async Task<IActionResult> AsyncIO()
  {
    var sw = Stopwatch.StartNew();
    var threadId = Thread.CurrentThread.ManagedThreadId;

    // Simulate the same I/O operation without blocking a thread
    await Task.Delay(2000);

    sw.Stop();
    return Ok(new
    {
      type = "Asynchronous I/O (non-blocking)",
      threadId = threadId,
      timeMs = sw.ElapsedMilliseconds
    });
  }

  // -------------------------------------------------------------------------
  // 4.3 – Parallel asynchronous tasks (concurrent)
  // -------------------------------------------------------------------------
  // We start 10 tasks that each wait 1 second asynchronously.
  // Task.WhenAll waits for all tasks to complete.
  // Because they run concurrently, total time is ~1 second, not 10 seconds.
  // This demonstrates how async enables concurrency without extra threads.
  // -------------------------------------------------------------------------
  [HttpGet("parallel-async")]
  public async Task<IActionResult> ParallelAsync([FromQuery] int count = 10)
  {
    var sw = Stopwatch.StartNew();
    var tasks = new List<Task>();

    for (int i = 0; i < count; i++)
    {
      tasks.Add(Task.Delay(1000)); // each waits 1 second asynchronously
    }

    await Task.WhenAll(tasks);
    sw.Stop();

    return Ok(new
    {
      type = $"Parallel async ({count} concurrent tasks)",
      timeMs = sw.ElapsedMilliseconds,
      expectedIfSequential = count * 1000
    });
  }

  // -------------------------------------------------------------------------
  // 4.4 – Sequential synchronous tasks (for contrast)
  // -------------------------------------------------------------------------
  // We run 10 synchronous delays one after another.
  // Each Thread.Sleep blocks for 1 second, so total time is ~10 seconds.
  // This is the opposite of parallel async – it's slow and uses threads inefficiently.
  // -------------------------------------------------------------------------
  [HttpGet("parallel-sync")]
  public IActionResult ParallelSync([FromQuery] int count = 10)
  {
    var sw = Stopwatch.StartNew();
    var threadId = Thread.CurrentThread.ManagedThreadId;

    for (int i = 0; i < count; i++)
    {
      Thread.Sleep(1000); // each blocks for 1 second
    }

    sw.Stop();
    return Ok(new
    {
      type = $"Sequential sync ({count} operations)",
      threadId = threadId,
      timeMs = sw.ElapsedMilliseconds,
      expectedIfAsync = 1000  // async would take ~1 sec total
    });
  }

  // -------------------------------------------------------------------------
  // 4.5 – CPU-bound synchronous (heavy calculation on request thread)
  // -------------------------------------------------------------------------
  // A long loop runs on the request thread. While it runs, the thread is busy.
  // If many such requests arrive, threads get blocked and the server slows down.
  // -------------------------------------------------------------------------
  [HttpGet("cpu-bound-sync")]
  public IActionResult CpuBoundSync()
  {
    var sw = Stopwatch.StartNew();
    var threadId = Thread.CurrentThread.ManagedThreadId;

    long sum = 0;
    for (int i = 0; i < 100_000_000; i++)
    {
      sum += i;
    }

    sw.Stop();
    return Ok(new
    {
      type = "CPU-bound synchronous (main thread)",
      threadId = threadId,
      timeMs = sw.ElapsedMilliseconds,
      result = sum
    });
  }

  // -------------------------------------------------------------------------
  // 4.6 – CPU-bound offloaded to thread pool (request thread freed)
  // -------------------------------------------------------------------------
  // Task.Run moves the heavy calculation to a thread pool thread.
  // The request thread is free to handle other requests while the calculation runs.
  // This improves responsiveness even though the total work time is similar.
  // -------------------------------------------------------------------------
  [HttpGet("cpu-bound-async")]
  public async Task<IActionResult> CpuBoundAsync()
  {
    var sw = Stopwatch.StartNew();
    var startThreadId = Thread.CurrentThread.ManagedThreadId;

    // Offload heavy work to a thread pool thread
    long sum = await Task.Run(() =>
    {
      var workerThreadId = Thread.CurrentThread.ManagedThreadId;
      long s = 0;
      for (int i = 0; i < 100_000_000; i++)
      {
        s += i;
      }
      return s;
    });

    sw.Stop();
    return Ok(new
    {
      type = "CPU-bound offloaded (request thread freed)",
      requestThreadId = startThreadId,
      timeMs = sw.ElapsedMilliseconds,
      result = sum
    });
  }

  // -------------------------------------------------------------------------
  // 4.7 – Load test helper: compare async vs sync for many concurrent requests
  // -------------------------------------------------------------------------
  // This endpoint simulates a number of concurrent "requests" (tasks).
  // With useAsync=true, we use Task.Delay (non‑blocking) → total time ~500ms.
  // With useAsync=false, we use Thread.Sleep (blocking) → total time ~requests*500ms.
  // This clearly shows the scalability advantage of asynchronous programming.
  // -------------------------------------------------------------------------
  [HttpGet("load-test")]
  public async Task<IActionResult> LoadTest([FromQuery] int requests = 10, [FromQuery] bool useAsync = true)
  {
    var sw = Stopwatch.StartNew();
    var tasks = new List<Task>();

    for (int i = 0; i < requests; i++)
    {
      if (useAsync)
        tasks.Add(Task.Delay(500));  // async I/O
      else
        tasks.Add(Task.Run(() => Thread.Sleep(500))); // sync blocking
    }

    await Task.WhenAll(tasks);
    sw.Stop();

    return Ok(new
    {
      mode = useAsync ? "Asynchronous" : "Synchronous (blocking)",
      concurrentRequests = requests,
      totalTimeMs = sw.ElapsedMilliseconds,
      expectedAsyncTime = 500,
      expectedSyncTime = requests * 500
    });
  }
}