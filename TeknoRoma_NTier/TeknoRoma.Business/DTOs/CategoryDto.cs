namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Category entity için Data Transfer Object
/// API response'larında ve request'lerinde kullanılır
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Yeni kategori oluşturma için DTO
/// Id alanı yok çünkü yeni kayıt oluşturuluyor
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Kategori güncelleme için DTO
/// </summary>
public class UpdateCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
