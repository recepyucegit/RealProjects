namespace TeknoRoma.Entities;

/// <summary>
/// Ürünleri tedarik eden firmaları temsil eden entity sınıfı
/// </summary>
public class Supplier : BaseEntity
{
    /// <summary>
    /// Tedarikçi firma adı
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Yetkili kişi adı soyadı
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Yetkili kişi unvanı
    /// </summary>
    public string? ContactTitle { get; set; }

    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Adres bilgisi
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Şehir
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Ülke
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Posta kodu
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Tedarikçinin aktif olup olmadığını belirtir
    /// Pasif tedarikçilerden yeni sipariş alınmaz
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Property
    /// <summary>
    /// Bu tedarikçiye ait ürünler
    /// One-to-Many ilişki: Bir tedarikçinin birden fazla ürünü olabilir
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
