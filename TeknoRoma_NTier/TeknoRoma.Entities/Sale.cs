using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Entities;

/// <summary>
/// Satış Entity (Sipariş Başlık Bilgileri)
/// Gül Satar'ın (Kasa Satış Temsilcisi) kaydettiği satışlar
///
/// İş Akışı:
/// 1. Gül Satar müşteri bilgilerini girer, ürünleri seçer → Beklemede
/// 2. Müşteri ödeme yapar → Hazirlaniyor
/// 3. Durna Sabit (Depo) ürünleri kasaya getirir
/// 4. Gül Satar müşteriye teslim eder → Tamamlandi
///
/// NEDEN Order Değil Sale?
/// - TeknoRoma terminolojisinde "satış" terimi kullanılıyor
/// - Haluk Bey: "Satış raporları"ndan bahsediyor
/// - OnionArchitecture'deki gibi tutarlılık için
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>
    /// Satış Numarası (Benzersiz)
    /// Format: "S-2024-00001"
    /// Fatura ve takip için kullanılır
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Satış Tarihi
    /// </summary>
    public DateTime SaleDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Satış Durumu
    /// Beklemede → Hazirlaniyor → Tamamlandi/Iptal
    /// NEDEN? Her departman kendi işini görebilmeli
    /// </summary>
    public SaleStatus Status { get; set; } = SaleStatus.Beklemede;

    /// <summary>
    /// Ödeme Türü
    /// Nakit, KrediKarti, Havale, Cek
    /// Feyza Paragöz'ün (Muhasebe) kasa sayımı için önemli
    /// </summary>
    public PaymentType PaymentMethod { get; set; }

    /// <summary>
    /// Ödeme yapıldı mı?
    /// false: Beklemede (henüz ödeme alınmadı)
    /// true: Ödeme alındı
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Ödeme tarihi
    /// Null olabilir (ödeme alınmadıysa)
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Teslimat adresi
    /// Müşteri mağazadan almıyorsa teslimat adresi
    /// Boş olabilir (müşteri mağazadan aldıysa)
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Kargo ücreti
    /// Teslimat varsa kargo ücreti eklenir
    /// 0 ise müşteri mağazadan aldı
    /// </summary>
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>
    /// Toplam Tutar (Kargo dahil)
    /// Ürünlerin toplamı + Kargo
    /// NEDEN? Hızlı raporlama için hesaplanmış alan
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// İndirim tutarı
    /// Kampanya veya manuel indirim
    /// 0 ise indirim yok
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Satış notu
    /// Özel talepler, notlar
    /// </summary>
    public string? Notes { get; set; }

    // ====== FOREIGN KEYS ======

    /// <summary>
    /// Hangi müşteri? (Foreign Key)
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Hangi mağaza? (Foreign Key)
    /// Haluk Bey: "Hangi mağazam daha çok satış yapıyor"
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Hangi çalışan sattı? (Foreign Key)
    /// Gül Satar gibi satış temsilcisi
    /// NEDEN? Prim hesabı ve performans takibi için
    /// </summary>
    public int EmployeeId { get; set; }


    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Satışı yapan müşteri
    /// Many-to-One ilişki
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Satışın yapıldığı mağaza
    /// Many-to-One ilişki
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// Satışı yapan çalışan
    /// Many-to-One ilişki
    /// </summary>
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// Satış detayları (satılan ürünler)
    /// One-to-Many ilişki
    /// Bir satışta birden fazla ürün olabilir
    /// </summary>
    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
}
