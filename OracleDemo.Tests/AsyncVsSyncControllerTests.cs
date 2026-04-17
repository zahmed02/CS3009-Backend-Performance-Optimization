using Xunit;
using Microsoft.AspNetCore.Mvc;
using OracleDemo.Controllers;
using System.Threading.Tasks;

namespace OracleDemo.Tests;

public class AsyncVsSyncControllerTests
{
  private readonly AsyncVsSyncController _controller;

  public AsyncVsSyncControllerTests()
  {
    _controller = new AsyncVsSyncController();
  }

  [Fact]
  public void SyncIO_ReturnsOkResult()
  {
    var result = _controller.SyncIO();
    Assert.IsType<OkObjectResult>(result);
  }

  [Fact]
  public async Task AsyncIO_ReturnsOkResult()
  {
    var result = await _controller.AsyncIO();
    Assert.IsType<OkObjectResult>(result);
  }

  [Fact]
  public async Task ParallelAsync_ReturnsOkResult()
  {
    var result = await _controller.ParallelAsync(10);
    Assert.IsType<OkObjectResult>(result);
  }
}