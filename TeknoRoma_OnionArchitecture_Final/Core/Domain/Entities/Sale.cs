// ============================================================================
// Sale.cs - Satış Entity (Satış Başlığı / Fatura Başlığı)
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA mağazalarında yapılan satış işlemlerinin başlık bilgilerini tutar.
// Her Sale kaydı bir fatura/fiş'e karşılık gelir.
// Satış detayları (ürünler) SaleDetail tablosunda tutulur.
//
// VERİTABANI İLİŞKİSİ:
// - Sale (1) → SaleDetail (N): Master-Detail ilişki
// - Bir satışta birden fazla ürün olabilir
//
// İŞ KURALLARI:
// - Her satışa benzersiz numara verilir (S-2024-00001)
// - Satış tutarları TL cinsinden
// - KDV oranı %20 (2024 Türkiye)
// - İptal edilen satışlar için iade işlemi yapılır
// ============================================================================

using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Satış Entity Sınıfı (Satış Başlığı)
    ///
    /// MASTER-DETAIL PATTERN:
    /// Sale (Master/Başlık) → SaleDetail (Detail/Kalem)
    ///
    /// Örnek:
    /// Sale: { Id: 1, SaleNumber: "S-2024-00001", TotalAmount: 15000 }
    ///   └── SaleDetail: { ProductId: 10, Quantity: 1, UnitPrice: 12000 }  // iPhone
    ///   └── SaleDetail: { ProductId: 25, Quantity: 2, UnitPrice: 1500 }   // Kılıf x2
    /// </summary>
    public class Sale : BaseEntity
    {
        // ====================================================================
        // SATIŞ TANIMLAYICI
        // ====================================================================

        /// <summary>
        /// Satış Numarası (Benzersiz)
        ///
        /// AÇIKLAMA:
        /// - Her satışa verilen benzersiz referans numarası
        /// - Fatura/fiş numarası olarak kullanılır
        /// - Müşteri iletişiminde referans olarak verilir
        ///
        /// FORMAT: "S-YYYY-NNNNN"
        /// - S: Sale prefix'i
        /// - YYYY: Yıl (2024)
        /// - NNNNN: Sıra numarası (00001'den başlar)
        /// - Örn: "S-2024-00001", "S-2024-12345"
        ///
        /// OTOMATİK OLUŞTURMA:
        /// - Service katmanında oluşturulur
        /// - Yıl değişiminde numara sıfırlanır
        /// - Thread-safe sequence generator kullanılmalı
        /// </summary>
        public string SaleNumber { get; set; } = null!;

        /// <summary>
        /// Satış Tarihi ve Saati
        ///
        /// AÇIKLAMA:
        /// - Satışın gerçekleştiği tarih ve saat
        /// - Fatura tarihi olarak kullanılır
        /// - Raporlama ve analiz için kritik
        ///
        /// VERİTABANI:
        /// - datetime2(7) olarak saklanır
        /// - Milisaniye hassasiyeti var
        ///
        /// RAPORLAMA:
        /// - Günlük/Haftalık/Aylık satış raporları
        /// - Saat bazlı yoğunluk analizi
        /// - Kasa kapanış raporları
        /// </summary>
        public DateTime SaleDate { get; set; }

        // ====================================================================
        // İLİŞKİSEL ALANLAR (FOREIGN KEYS)
        // ====================================================================

        /// <summary>
        /// Müşteri ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Satışın yapıldığı müşteri
        /// - Customers tablosundaki Id ile eşleşir
        ///
        /// NOT:
        /// - Anonim satışlar için nullable yapılabilir
        /// - Şu anki tasarımda opsiyonel (int?)
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Çalışan ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Satışı yapan personel
        /// - Employees tablosundaki Id ile eşleşir
        /// - Performans değerlendirmesi için kullanılır
        ///
        /// PRİM HESABI:
        /// - Her satış çalışana atanır
        /// - Ay sonu satış toplamı prim hesabında kullanılır
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        /// Mağaza ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Satışın yapıldığı mağaza
        /// - Stores tablosundaki Id ile eşleşir
        /// - Mağaza bazlı raporlama için
        ///
        /// STOK İLİŞKİSİ:
        /// - Satış yapıldığında o mağazanın stoğu düşer
        /// - Merkezi stok yerine mağaza bazlı stok varsa
        /// </summary>
        public int StoreId { get; set; }

        // ====================================================================
        // SATIŞ DURUMU VE ÖDEME
        // ====================================================================

        /// <summary>
        /// Satış Durumu (Enum)
        ///
        /// AÇIKLAMA:
        /// - Satışın mevcut durumunu belirtir
        /// - SaleStatus enum'undan değer alır
        ///
        /// DURUM DEĞERLERİ:
        /// - Beklemede (0): Satış başlatıldı, ödeme alınmadı
        /// - Tamamlandi (1): Ödeme alındı, satış kapandı
        /// - IptalEdildi (2): Satış iptal edildi
        /// - IadeEdildi (3): Ürün iade alındı
        ///
        /// VARSAYILAN DEĞER:
        /// - "= SaleStatus.Beklemede" ile başlar
        /// - Ödeme alındığında Tamamlandi olur
        ///
        /// WORKFLOW:
        /// Beklemede → Tamamlandi → (opsiyonel) IadeEdildi
        /// Beklemede → IptalEdildi
        /// </summary>
        public SaleStatus Status { get; set; } = SaleStatus.Beklemede;

        /// <summary>
        /// Ödeme Türü (Enum)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin kullandığı ödeme yöntemi
        /// - PaymentType enum'undan değer alır
        ///
        /// ÖDEME TÜRLERİ:
        /// - Nakit: Peşin nakit ödeme
        /// - KrediKarti: Tek çekim kredi kartı
        /// - Taksitli: Kredi kartı taksitli
        /// - Havale: Banka havalesi/EFT
        ///
        /// RAPORLAMA:
        /// - Ödeme türü bazlı satış dağılımı
        /// - Nakit akış planlaması (vadeli satışlar)
        /// </summary>
        public PaymentType PaymentType { get; set; }

        // ====================================================================
        // TUTAR ALANLARI
        // ====================================================================

        /// <summary>
        /// Ara Toplam (KDV Hariç)
        ///
        /// AÇIKLAMA:
        /// - Tüm ürünlerin toplam tutarı (KDV hariç)
        /// - SaleDetail'lerin toplamı
        /// - Subtotal = Σ(Quantity × UnitPrice)
        ///
        /// HESAPLAMA:
        /// var subtotal = saleDetails.Sum(d => d.Quantity * d.UnitPrice);
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// KDV Tutarı
        ///
        /// AÇIKLAMA:
        /// - Toplam KDV (Katma Değer Vergisi) tutarı
        /// - Türkiye'de genel KDV oranı %20 (2024)
        /// - TaxAmount = Subtotal × 0.20
        ///
        /// KDV ORANLARI (2024 TÜRKİYE):
        /// - Genel: %20 (elektronik ürünler dahil)
        /// - İndirimli: %10 (temel gıda)
        /// - Süper İndirimli: %1 (gazete, dergi)
        ///
        /// NOT:
        /// - Ürün bazlı farklı KDV oranları için
        ///   SaleDetail'de TaxRate alanı eklenebilir
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// İndirim Tutarı
        ///
        /// AÇIKLAMA:
        /// - Satışa uygulanan toplam indirim
        /// - Manuel indirim veya kampanya indirimi
        /// - Varsayılan: 0 (indirim yok)
        ///
        /// İNDİRİM TÜRLERİ:
        /// - Yüzdelik indirim: %10 indirim → Subtotal × 0.10
        /// - Sabit tutar indirim: 500 TL indirim
        /// - Kupon kodu indirimi
        /// - Sadakat puanı kullanımı
        ///
        /// "= 0" VARSAYILAN:
        /// - Property initializer ile 0 atanır
        /// - İndirim yapılmazsa 0 kalır
        /// </summary>
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Genel Toplam (Ödenecek Tutar)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin ödeyeceği son tutar
        /// - TotalAmount = Subtotal + TaxAmount - DiscountAmount
        ///
        /// ÖRNEK HESAPLAMA:
        /// - Ürünler: 10.000 TL (Subtotal)
        /// - KDV %20: 2.000 TL (TaxAmount)
        /// - İndirim: 500 TL (DiscountAmount)
        /// - TOPLAM: 11.500 TL (TotalAmount)
        ///
        /// TUTARSIZLIK KONTROLÜ:
        /// - TotalAmount her zaman hesaplanarak doğrulanmalı
        /// - if (sale.TotalAmount != subtotal + tax - discount) → HATA
        /// </summary>
        public decimal TotalAmount { get; set; }

        // ====================================================================
        // EK BİLGİLER
        // ====================================================================

        /// <summary>
        /// Kasa Numarası (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Satışın yapıldığı kasa terminali
        /// - Bir mağazada birden fazla kasa olabilir
        /// - Örn: "KASA-01", "KASA-02"
        ///
        /// NULLABLE (string?):
        /// - Online satışlarda null olabilir
        /// - Geçmişten aktarılan verilerde boş olabilir
        ///
        /// KULLANIM:
        /// - Kasa bazlı Z raporu
        /// - Kasa sorumlusu takibi
        /// - Kasa açığı/fazlası kontrolü
        /// </summary>
        public string? CashRegisterNumber { get; set; }

        /// <summary>
        /// Satış Notu (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Satış hakkında ek bilgi/açıklama
        /// - Personel notu veya müşteri talebi
        ///
        /// ÖRNEK NOTLAR:
        /// - "Hediye paketi istendi"
        /// - "Müşteri 17:00'da gelip alacak"
        /// - "Fatura adresi farklı"
        /// </summary>
        public string? Notes { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Müşteri (Navigation Property)
        ///
        /// İLİŞKİ: Sale (N) → Customer (1)
        /// - Birden fazla satış aynı müşteriye yapılabilir
        ///
        /// KULLANIM:
        /// var customerName = sale.Customer?.FullName;
        /// var customerPhone = sale.Customer?.Phone;
        /// </summary>
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Satışı Yapan Çalışan (Navigation Property)
        ///
        /// İLİŞKİ: Sale (N) → Employee (1)
        /// </summary>
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// Satışın Yapıldığı Mağaza (Navigation Property)
        ///
        /// İLİŞKİ: Sale (N) → Store (1)
        /// </summary>
        public virtual Store Store { get; set; } = null!;

        /// <summary>
        /// Satış Detayları (Collection Navigation Property)
        ///
        /// İLİŞKİ: Sale (1) → SaleDetail (N)
        /// - Bir satışta birden fazla ürün olabilir
        /// - MASTER-DETAIL ilişki
        ///
        /// KULLANIM:
        /// // Satıştaki ürün sayısı
        /// var itemCount = sale.SaleDetails.Sum(d => d.Quantity);
        ///
        /// // Satılan ürün çeşidi
        /// var productCount = sale.SaleDetails.Count;
        ///
        /// // En pahalı ürün
        /// var mostExpensive = sale.SaleDetails
        ///     .OrderByDescending(d => d.UnitPrice)
        ///     .First();
        ///
        /// EAGER LOADING:
        /// _context.Sales
        ///     .Include(s => s.SaleDetails)
        ///         .ThenInclude(d => d.Product)
        ///     .ToList();
        /// </summary>
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
