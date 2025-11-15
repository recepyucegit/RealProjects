using Domain.Enums;
using static System.Formats.Asn1.AsnWriter;

namespace Domain.Entities
{
    /// <summary>
    /// Satış Entity (Satış Başlığı)
    /// Gül Satar ve Fahri Cepçi'nin yaptığı satışları tutar
    /// 
    /// İŞ AKIŞI:
    /// 1. Kasa/Mobil satış yapılır → Sale kaydı oluşturulur (Beklemede)
    /// 2. Ödeme yapılır → Status = Hazirlaniyor
    /// 3. Depo ürünleri kasaya getirir → Status = Tamamlandi
    /// </summary>
    public class Sale : BaseEntity
    {
        /// <summary>
        /// Satış Numarası (Benzersiz)
        /// Fahri Cepçi: "Müşteriye bir satış numarası verse, ben o numarayı girip görebilsem"
        /// Format: "S-2024-00001", "S-2024-00002"
        /// </summary>
        public string SaleNumber { get; set; }

        /// <summary>
        /// Satış Tarihi
        /// </summary>
        public DateTime SaleDate { get; set; }

        /// <summary>
        /// Hangi müşteriye yapıldı? (Foreign Key)
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Hangi çalışan yaptı? (Foreign Key)
        /// Gül Satar veya Fahri Cepçi
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        /// Hangi mağazada yapıldı? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Satış Durumu
        /// Beklemede → Hazirlaniyor → Tamamlandi/Iptal
        /// </summary>
        public SaleStatus Status { get; set; } = SaleStatus.Beklemede;

        /// <summary>
        /// Ödeme Türü
        /// Nakit, Kredi Kartı, Havale, Çek
        /// </summary>
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Ara Toplam (Ürün fiyatları toplamı)
        /// Hesaplama: SaleDetails.Sum(sd => sd.Subtotal)
        /// NEDEN ayrı sütun? Performance - her seferinde hesaplama yapmamak için
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// KDV Toplamı
        /// Hesaplama: Subtotal * 0.20 (KDV %20)
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// İndirim Tutarı
        /// Kampanya veya özel indirim
        /// </summary>
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Genel Toplam (Müşterinin ödeyeceği tutar)
        /// Hesaplama: Subtotal + TaxAmount - DiscountAmount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Hangi kasadan yapıldı?
        /// Kerim Zulacı: "Talebin hangi kasadan geldiğini bilmeliyim"
        /// Örn: "Kasa 1", "Kasa 2", "Mobil"
        /// Null olabilir (Mobil satışta kasa yok)
        /// </summary>
        public string CashRegisterNumber { get; set; }

        /// <summary>
        /// Satış açıklaması/notu
        /// Opsiyonel notlar
        /// </summary>
        public string Notes { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Satışı yapan müşteri
        /// Many-to-One ilişki
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Satışı yapan çalışan
        /// Many-to-One ilişki
        /// </summary>
        public virtual Employee Employee { get; set; }

        /// <summary>
        /// Satışın yapıldığı mağaza
        /// Many-to-One ilişki
        /// </summary>
        public virtual Store Store { get; set; }

        /// <summary>
        /// Satışın detayları (Satılan ürünler)
        /// One-to-Many ilişki
        /// Bir satışta birden fazla ürün olabilir
        /// </summary>
        public virtual ICollection<SaleDetail> SaleDetails { get; set; }
    }
}