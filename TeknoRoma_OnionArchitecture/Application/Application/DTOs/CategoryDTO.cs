using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CategoryDTO
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateCategoryDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
    }

    public class UpdateCategoryDTO : CreateCategoryDTO
    {
        [Required]
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}