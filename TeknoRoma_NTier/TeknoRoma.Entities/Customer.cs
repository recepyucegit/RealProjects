namespace TeknoRoma.Entities;

/// <summary>
/// Sistem kullanıcılarını/müşterilerini temsil eden entity sınıfı
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Müşteri adı
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Müşteri soyadı
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email adresi - Giriş için kullanılır
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Şifre - Hash'lenmiş olarak saklanmalıdır
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Adres bilgisi
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Şehir
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// İlçe
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// Posta kodu
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Müşterinin aktif olup olmadığını belirtir
    /// Pasif müşteriler sisteme giriş yapamaz
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Email doğrulandı mı?
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>
    /// Son giriş tarihi
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    // Navigation Property
    /// <summary>
    /// Bu müşteriye ait siparişler
    /// One-to-Many ilişki: Bir müşterinin birden fazla siparişi olabilir
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
