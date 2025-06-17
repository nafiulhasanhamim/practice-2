using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    [Key]
    public string CategoryId { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }
}