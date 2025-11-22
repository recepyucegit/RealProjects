namespace TeknoRoma.Entities;

/// <summary>
/// Müşteri siparişlerini temsil eden entity sınıfı
/// Sipariş başlık bilgilerini içerir (header table)
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// Sipariş numarası - Benzersiz sipariş takip numarası
    /// Örnek: ORD-2024-00001
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Sipariş tarihi
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Sipariş durumu
    /// Pending: Beklemede, Processing: İşleniyor, Shipped: Kargoya verildi, Delivered: Teslim edildi, Cancelled: İptal edildi
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Ödeme yöntemi
    /// CreditCard: Kredi Kartı, BankTransfer: Havale/EFT, CashOnDelivery: Kapıda Ödeme
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Ödeme durumu
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Ödeme tarihi
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Teslimat adresi
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Teslimat şehri
    /// </summary>
    public string ShippingCity { get; set; } = string.Empty;

    /// <summary>
    /// Teslimat ilçesi
    /// </summary>
    public string? ShippingDistrict { get; set; }

    /// <summary>
    /// Teslimat posta kodu
    /// </summary>
    public string? ShippingPostalCode { get; set; }

    /// <summary>
    /// Kargo ücreti
    /// </summary>
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>
    /// Sipariş toplam tutarı (kargo dahil)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Sipariş notu
    /// </summary>
    public string? Notes { get; set; }

    // Foreign Key
    /// <summary>
    /// Bu siparişi veren müşteri ID'si
    /// </summary>
    public int CustomerId { get; set; }

    // Navigation Properties
    /// <summary>
    /// Bu siparişi veren müşteri
    /// Many-to-One ilişki: Birden fazla sipariş aynı müşteriye ait olabilir
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Bu siparişe ait ürün detayları
    /// One-to-Many ilişki: Bir siparişte birden fazla ürün olabilir
    /// </summary>
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

/// <summary>
/// Sipariş durumlarını belirten enum
/// </summary>
public enum OrderStatus
{
    Pending = 1,      // Beklemede
    Processing = 2,   // İşleniyor
    Shipped = 3,      // Kargoya verildi
    Delivered = 4,    // Teslim edildi
    Cancelled = 5     // İptal edildi
}

/// <summary>
/// Ödeme yöntemlerini belirten enum
/// </summary>
public enum PaymentMethod
{
    CreditCard = 1,      // Kredi Kartı
    BankTransfer = 2,    // Havale/EFT
    CashOnDelivery = 3   // Kapıda Ödeme
}
