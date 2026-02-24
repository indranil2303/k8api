using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Products")]
public class Product
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();
    [Required, StringLength(80)]
    public string? Name { get; init; }
    public decimal Price { get; init; }
}