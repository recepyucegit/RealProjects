namespace TeknoRoma.Entities;

/// <summary>
/// Satış Detayı Entity (Sipariş Satırları)
/// Her bir satır, satıştaki bir ürünü temsil eder
///
/// NEDEN Ayrı Tablo?
/// - Bir satışta birden fazla ürün olabilir
/// - Her ürünün ayrı miktarı, fiyatı, indirimi olabilir
/// - Raporlarda "en çok satan ürün" analizleri için
/// - Normalize database design (3NF)
/// </summary>
public class SaleDetail : BaseEntity
{
    /// <summary>
    /// Satış edilen miktar
    /// Örn: 2 adet iPhone, 1 adet laptop
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Birim fiyatı - Satış anındaki fiyat
    /// NEDEN? Ürün fiyatı değişse bile satıştaki fiyat sabit kalır
    /// "O tarihte bu fiyattan satmıştık" bilgisi korunur
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// İndirim oranı (%)
    /// Örn: %10 indirim
    /// 0 ise indirim yok
    /// </summary>
    public decimal DiscountRate { get; set; } = 0;

    /// <summary>
    /// Satır toplamı
    /// Hesaplama: (Quantity * UnitPrice) - (Quantity * UnitPrice * DiscountRate / 100)
    /// NEDEN? Hızlı raporlama için hesaplanmış alan
    /// Her seferinde hesaplamamak için database'de tutuyoruz
    /// </summary>
    public decimal LineTotal { get; set; }

    // ====== FOREIGN KEYS ======

    /// <summary>
    /// Hangi satışa ait? (Foreign Key)
    /// </summary>
    public int SaleId { get; set; }

    /// <summary>
    /// Hangi ürün satıldı? (Foreign Key)
    /// </summary>
    public int ProductId { get; set; }


    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Bu detayın ait olduğu satış
    /// Many-to-One ilişki
    /// </summary>
    public virtual Sale Sale { get; set; } = null!;

    /// <summary>
    /// Satılan ürün
    /// Many-to-One ilişki
    /// NEDEN? Ürün bilgilerine (ad, kategori vb.) erişmek için
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
