using Microsoft.EntityFrameworkCore;
using OracleDemo.Models;

namespace OracleDemo.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  public DbSet<Product> Products { get; set; }

  // 1.1 Database Indexing Impact Analysis 
  // Step 1 First, we added the index configuration in AppDbContext.cs
  // This tells EF Core to create an index on the ProductName column.
  // Indexes make search queries faster by organizing data for quick lookup.
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    // 1.1 This will be applied via migration.
    // Create an index on ProductName to optimize searches
    // After this, we create a migration and apply it to the database.

    // 1.2 - Generate a migration that adds the index 
    // (The first command creates a migration file in the Migrations folder.)
    // D:\DevTools\dotnet-projects\OracleDemo> dotnet ef migrations add AddIndexOnProductName

    // 1.3 - Apply the migration to create the index in the Oracle database 
    // (The second command executes the SQL to add the index to the PRODUCTS table.)
    // D:\DevTools\dotnet-projects\OracleDemo> dotnet ef database update
    modelBuilder.Entity<Product>()
        .HasIndex(p => p.ProductName)
        .HasDatabaseName("IX_PRODUCTS_NAME");
  }
}