namespace TeknoRoma.Entities;

/// <summary>
/// Sipariş detaylarını temsil eden entity sınıfı (detail/line items table)
/// Her bir satır, siparişteki bir ürünü temsil eder
/// </summary>
public class OrderDetail : BaseEntity
{
    /// <summary>
    /// Sipariş edilen miktar
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Birim fiyatı - Sipariş anındaki fiyat
    /// Ürün fiyatı değişse bile siparişteki fiyat sabit kalır
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// İndirim oranı (%)
    /// </summary>
    public decimal DiscountRate { get; set; } = 0;

    /// <summary>
    /// Satır toplamı - (Quantity * UnitPrice) - (Discount)
    /// Hesaplanmış alan olarak da tutulabilir
    /// </summary>
    public decimal LineTotal { get; set; }

    // Foreign Keys
    /// <summary>
    /// Bu detayın ait olduğu sipariş ID'si
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Sipariş edilen ürün ID'si
    /// </summary>
    public int ProductId { get; set; }

    // Navigation Properties
    /// <summary>
    /// Bu detayın ait olduğu sipariş
    /// Many-to-One ilişki: Birden fazla detay aynı siparişe ait olabilir
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// Sipariş edilen ürün
    /// Many-to-One ilişki: Aynı ürün birden fazla sipariş detayında yer alabilir
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
