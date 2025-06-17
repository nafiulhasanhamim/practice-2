using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AddCategoryDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be 2 to 50 characters long")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Description must be 2 to 100 characters long")]
        public string? Description { get; set; }
    }

    public class GetCategoryDto
    {
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}