
using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.DTOs
{
    public class AddProductDto
    {
        [Required(ErrorMessage = "CategoryId is required")]
        public string? CategoryId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Length must be 2 to 50 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Length must be 2 to 50 characters")]
        public string? Description { get; set; }
    }

    public class GetProductDto
    {
        public string? ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public GetCategoryDto? Category { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

}