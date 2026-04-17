// -----------------------------------------------------------------------------
// IProductRepository – the contract for data access
// -----------------------------------------------------------------------------
// This interface lists all the database operations we need for Product.
// Any class that implements it must provide these methods.
// The controller uses this interface, not the real database class.
// This is the "abstraction" in the Repository pattern.
// -----------------------------------------------------------------------------

using OracleDemo.Models;

namespace OracleDemo.Repositories;

public interface IProductRepository
{
  Task<IEnumerable<Product>> GetAllAsync();       // get all products
  Task<Product?> GetByIdAsync(int id);            // get one product by ID
  Task<Product> AddAsync(Product product);        // add a new product
  Task UpdateAsync(Product product);              // update an existing product
  Task DeleteAsync(int id);                       // delete a product by ID
  Task<int> SaveChangesAsync();                   // save all changes (useful for unit of work)
}