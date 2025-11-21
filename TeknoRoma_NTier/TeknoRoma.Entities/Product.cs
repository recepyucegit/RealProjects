namespace TeknoRoma.Entities;

/// <summary>
/// Satışa sunulan ürünleri temsil eden entity sınıfı
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Ürün adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ürün açıklaması - Detaylı bilgi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Ürün fiyatı (TL cinsinden)
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// İndirimli fiyat - Kampanyalı ürünler için
    /// Null ise indirim yok demektir
    /// </summary>
    public decimal? DiscountPrice { get; set; }

    /// <summary>
    /// Stok miktarı
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Ürün resmi URL'si
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Ürünün aktif olup olmadığını belirtir
    /// Pasif ürünler satışa sunulmaz
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Ürünün öne çıkan ürün olup olmadığını belirtir
    /// Ana sayfada öne çıkan ürünler gösterilir
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    // Foreign Keys
    /// <summary>
    /// Bu ürünün ait olduğu kategori ID'si
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Bu ürünü tedarik eden firma ID'si
    /// </summary>
    public int SupplierId { get; set; }

    // Navigation Properties
    /// <summary>
    /// Bu ürünün ait olduğu kategori
    /// Many-to-One ilişki: Birden fazla ürün aynı kategoriye ait olabilir
    /// </summary>
    public virtual Category Category { get; set; } = null!;

    /// <summary>
    /// Bu ürünü tedarik eden firma
    /// Many-to-One ilişki: Birden fazla ürün aynı tedarikçiden gelebilir
    /// </summary>
    public virtual Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Bu ürüne ait sipariş detayları
    /// One-to-Many ilişki: Bir ürün birden fazla siparişte yer alabilir
    /// </summary>
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
