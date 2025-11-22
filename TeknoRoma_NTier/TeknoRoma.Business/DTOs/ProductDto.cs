namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Product entity için Data Transfer Object
/// İlişkili entity'lerin adlarını da içerir (Category ve Supplier)
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }

    // İlişkili veriler
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }

    /// <summary>
    /// İndirimlı fiyat varsa indirimli, yoksa normal fiyat döner
    /// </summary>
    public decimal EffectivePrice => DiscountPrice ?? Price;

    /// <summary>
    /// İndirim yüzdesi hesaplama
    /// </summary>
    public decimal? DiscountPercentage
    {
        get
        {
            if (DiscountPrice.HasValue && Price > 0)
            {
                return Math.Round(((Price - DiscountPrice.Value) / Price) * 100, 2);
            }
            return null;
        }
    }
}

/// <summary>
/// Yeni ürün oluşturma için DTO
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }
}

/// <summary>
/// Ürün güncelleme için DTO
/// </summary>
public class UpdateProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }
}
