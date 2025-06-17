
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities

{
    public class Product
    {
        public string? ProductId { get; set; }
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Category? Category { get; set; }

    }
}