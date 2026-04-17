// -----------------------------------------------------------------------------
// IQueryStrategy – the blueprint for all query strategies
// -----------------------------------------------------------------------------
// This is the "Strategy" interface. It defines a common method "ExecuteAsync"
// that all query strategies must implement.
// The controller can use any strategy without knowing the details inside.
// -----------------------------------------------------------------------------

using OracleDemo.Data;
using OracleDemo.Models;

namespace OracleDemo.Strategies;

public interface IQueryStrategy
{
  // Every strategy must have a method that takes the database context
  // and returns a list of products.
  Task<List<Product>> ExecuteAsync(AppDbContext context);
}