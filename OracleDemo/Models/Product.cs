using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleDemo.Models;

[Table("PRODUCTS")]
public class Product
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [Column("PRODUCT_ID")]
  public int ProductId { get; set; }

  [Column("PRODUCT_NAME")]
  public string? ProductName { get; set; }

  [Column("PRICE")]
  public decimal Price { get; set; }

  [Column("CREATED_DATE")]
  public DateTime CreatedDate { get; set; }

  // -------------------------------------------------------------------------
  // Concurrency token: used for optimistic concurrency.
  // The [ConcurrencyCheck] attribute tells EF Core to include this column
  // in the WHERE clause of UPDATE and DELETE statements.
  // Every time a record is updated, we increment this number.
  // If the value in the database differs from what the user read,
  // EF Core throws a DbUpdateConcurrencyException.
  // -------------------------------------------------------------------------
  [ConcurrencyCheck]
  public long RowVersion { get; set; }
}