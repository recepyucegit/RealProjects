namespace TeknoRoma.Entities;

/// <summary>
/// Ürün kategorilerini temsil eden entity sınıfı
/// Örnek: Telefon, Bilgisayar, Tablet, Aksesuar vb.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Kategori adı (örn: "Akıllı Telefonlar", "Dizüstü Bilgisayarlar")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kategori açıklaması
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Kategori resmi URL'si
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Kategorinin aktif olup olmadığını belirtir
    /// Pasif kategoriler müşterilere gösterilmez
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Property - İlişkili veriler için
    /// <summary>
    /// Bu kategoriye ait ürünler listesi
    /// One-to-Many ilişki: Bir kategorinin birden fazla ürünü olabilir
    /// Virtual keyword'ü Lazy Loading için kullanılır
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
