using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OracleDemo.Controllers;
using OracleDemo.Models;
using OracleDemo.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OracleDemo.Tests;

public class ProductsControllerTests
{
  [Fact]
  public async Task GetProducts_ReturnsOkResult_WithListOfProducts()
  {
    // Arrange
    var mockRepo = new Mock<IProductRepository>();
    var expectedProducts = new List<Product>
        {
            new Product { ProductId = 1, ProductName = "Test1", Price = 10 },
            new Product { ProductId = 2, ProductName = "Test2", Price = 20 }
        };
    mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedProducts);

    var controller = new ProductsController(mockRepo.Object);

    // Act
    var result = await controller.GetProducts();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedProducts = Assert.IsType<List<Product>>(okResult.Value);
    Assert.Equal(2, returnedProducts.Count);
  }

  [Fact]
  public async Task GetProduct_WithInvalidId_ReturnsNotFound()
  {
    // Arrange
    var mockRepo = new Mock<IProductRepository>();
    mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Product?)null);

    var controller = new ProductsController(mockRepo.Object);

    // Act
    var result = await controller.GetProduct(999);

    // Assert
    Assert.IsType<NotFoundResult>(result.Result);
  }
}