namespace TeknoRoma.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateCategoryDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
}
