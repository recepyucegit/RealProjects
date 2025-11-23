// ============================================================================
// Expense.cs - Gider Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın işletme giderlerini takip eden entity.
// Kira, elektrik, maaş, malzeme gibi tüm giderleri kapsar.
//
// GİDER KATEGORİLERİ:
// - Kira Giderleri (mağaza kiraları)
// - Personel Giderleri (maaş, prim, SGK)
// - Enerji Giderleri (elektrik, doğalgaz, su)
// - İletişim Giderleri (telefon, internet)
// - Bakım-Onarım Giderleri
// - Pazarlama Giderleri (reklam, kampanya)
// - Vergi ve Harçlar
// - Diğer İşletme Giderleri
//
// İŞ KURALLARI:
// - Her gidere benzersiz numara verilir
// - Dövizli giderler için kur bilgisi tutulur
// - Ödeme durumu ve tarihi takip edilir
// - Mağaza bazlı gider raporlaması yapılır
// ============================================================================

using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Gider Entity Sınıfı
    ///
    /// FİNANSAL YÖNETİM:
    /// - Nakit akış takibi
    /// - Bütçe kontrolü
    /// - Kar/Zarar hesaplama
    /// - Maliyet analizi
    ///
    /// MUHASEBE ENTEGRASYONU:
    /// - E-fatura ile eşleşme (DocumentNumber)
    /// - Gider kategorileri vergi beyanında kullanılır
    /// - KDV mahsuplaşması için gider faturaları önemli
    /// </summary>
    public class Expense : BaseEntity
    {
        // ====================================================================
        // GİDER TANIMLAYICI
        // ====================================================================

        /// <summary>
        /// Gider Numarası (Benzersiz)
        ///
        /// AÇIKLAMA:
        /// - Her gider kaydına verilen benzersiz referans
        /// - Takip ve raporlama için kullanılır
        ///
        /// FORMAT: "G-YYYY-NNNNN"
        /// - G: Gider prefix'i (Expense)
        /// - YYYY: Yıl (2024)
        /// - NNNNN: Sıra numarası
        /// - Örn: "G-2024-00001", "G-2024-00125"
        ///
        /// OTOMATİK OLUŞTURMA:
        /// - Service katmanında sequence ile oluşturulur
        /// </summary>
        public string ExpenseNumber { get; set; } = null!;

        /// <summary>
        /// Gider Tarihi
        ///
        /// AÇIKLAMA:
        /// - Giderin gerçekleştiği/fatura tarihi
        /// - Ödeme tarihi farklı olabilir (vadeli ödemeler)
        ///
        /// RAPORLAMA:
        /// - Aylık/Yıllık gider raporları bu tarihe göre
        /// - Dönemsel karşılaştırma için
        /// </summary>
        public DateTime ExpenseDate { get; set; }

        // ====================================================================
        // GİDER TİPİ VE SINIFLANDIRMA
        // ====================================================================

        /// <summary>
        /// Gider Türü (Enum)
        ///
        /// AÇIKLAMA:
        /// - Giderin kategorisi
        /// - ExpenseType enum'undan değer alır
        ///
        /// ENUM DEĞERLERİ:
        /// - Kira: Mağaza/ofis kira giderleri
        /// - Elektrik: Elektrik faturaları
        /// - Su: Su faturaları
        /// - Dogalgaz: Doğalgaz/ısınma giderleri
        /// - Internet: İnternet ve iletişim
        /// - CalisanOdemesi: Maaş, prim, avans
        /// - Bakim: Bakım-onarım giderleri
        /// - Diger: Sınıflandırılamayan diğer giderler
        ///
        /// RAPORLAMA:
        /// - Gider türü bazlı analiz
        /// - En yüksek gider kalemi tespiti
        /// - Bütçe planlaması
        /// </summary>
        public ExpenseType ExpenseType { get; set; }

        // ====================================================================
        // İLİŞKİSEL ALANLAR (FOREIGN KEYS)
        // ====================================================================

        /// <summary>
        /// Mağaza ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Giderin ait olduğu mağaza
        /// - Stores tablosundaki Id ile eşleşir
        /// - Mağaza bazlı maliyet takibi için
        ///
        /// MERKEZ GİDERLER:
        /// - Genel müdürlük giderleri için özel mağaza kaydı
        /// - Veya StoreId nullable yapılabilir
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Çalışan ID (Foreign Key - Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Çalışana yapılan ödemeler için kullanılır
        /// - Sadece ExpenseType = CalisanOdemesi ise dolu
        /// - Maaş, prim, avans ödemeleri
        ///
        /// NULLABLE (int?):
        /// - Çalışan ödemesi değilse null
        /// - Kira, elektrik vs. için null
        ///
        /// KULLANIM:
        /// if (expense.ExpenseType == ExpenseType.CalisanOdemesi)
        ///     var employeeName = expense.Employee?.FullName;
        /// </summary>
        public int? EmployeeId { get; set; }

        // ====================================================================
        // TUTAR BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Gider Tutarı (Orijinal Para Birimi)
        ///
        /// AÇIKLAMA:
        /// - Giderin orijinal tutarı
        /// - Currency alanındaki para birimi cinsinden
        ///
        /// ÖRNEK:
        /// - TL gider: Amount=1000, Currency=TRY
        /// - USD gider: Amount=100, Currency=USD
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Para Birimi (Enum)
        ///
        /// AÇIKLAMA:
        /// - Giderin para birimi
        /// - Currency enum'undan değer alır
        /// - Varsayılan: TRY (Türk Lirası)
        ///
        /// DÖVİZLİ GİDERLER:
        /// - İthal ürün ödemeleri (USD, EUR)
        /// - Yurtdışı hizmet alımları
        /// - Lisans ve yazılım ücretleri
        /// </summary>
        public Currency Currency { get; set; } = Currency.TRY;

        /// <summary>
        /// Döviz Kuru (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Dövizli giderlerde TL'ye çevrim kuru
        /// - TRY ise null veya 1
        /// - TCMB kurundan alınabilir
        ///
        /// ÖRNEK:
        /// - USD gider: ExchangeRate = 32.50
        /// - EUR gider: ExchangeRate = 35.20
        /// </summary>
        public decimal? ExchangeRate { get; set; }

        /// <summary>
        /// TL Karşılığı (Hesaplanmış)
        ///
        /// AÇIKLAMA:
        /// - Giderin Türk Lirası karşılığı
        /// - Amount × ExchangeRate (veya Amount eğer TRY)
        /// - Raporlama için standart birim
        ///
        /// HESAPLAMA:
        /// AmountInTRY = Currency == TRY
        ///     ? Amount
        ///     : Amount × ExchangeRate.Value;
        ///
        /// NEDEN SAKLANIR?
        /// - Performans: Her seferinde hesaplama yapılmaz
        /// - Tutarlılık: Kur değişse bile kayıt değişmez
        /// - Raporlama: TL bazlı toplamlar kolayca alınır
        /// </summary>
        public decimal AmountInTRY { get; set; }

        // ====================================================================
        // EK BİLGİLER
        // ====================================================================

        /// <summary>
        /// Gider Açıklaması
        ///
        /// AÇIKLAMA:
        /// - Gider hakkında detaylı bilgi
        /// - Ne için yapıldığı, kim tarafından onaylandığı
        ///
        /// ÖRNEKLER:
        /// - "Ocak 2024 mağaza kirası"
        /// - "Klima bakım-onarım ücreti"
        /// - "Mehmet Yılmaz - Ocak maaşı"
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Fatura/Evrak Numarası (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Gidere ait fatura/makbuz numarası
        /// - Muhasebe kaydı ve denetim için
        /// - E-fatura numarası da olabilir
        ///
        /// ÖRNEKLER:
        /// - "GIB2024000123456" (e-fatura)
        /// - "F-12345" (kağıt fatura)
        /// - "M-2024-001" (makbuz)
        /// </summary>
        public string? DocumentNumber { get; set; }

        // ====================================================================
        // ÖDEME DURUMU
        // ====================================================================

        /// <summary>
        /// Ödeme Yapıldı mı?
        ///
        /// AÇIKLAMA:
        /// - true: Ödeme yapıldı
        /// - false: Henüz ödenmedi (borç)
        ///
        /// VARSAYILAN:
        /// - "= false" ile ödenmemiş olarak başlar
        /// - Ödeme sonrası true yapılır
        ///
        /// NAKİT AKIŞ YÖNETİMİ:
        /// - Ödenmemiş giderler = Borç
        /// - Vadesi gelen ödemeler listesi
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Ödeme Tarihi (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Ödemenin yapıldığı tarih
        /// - IsPaid = false ise null
        /// - Planlanan ödeme tarihi için ayrı alan eklenebilir
        ///
        /// NULLABLE (DateTime?):
        /// - Ödeme yapılmadıysa null
        /// - Ödeme yapıldığında set edilir
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// Vade Tarihi (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Giderin ödenmesi gereken son tarih
        /// - Vadeli ödemeler için kullanılır
        /// - Nakit akış planlaması için önemli
        ///
        /// NULLABLE (DateTime?):
        /// - Peşin ödemelerde null olabilir
        /// - Vadeli ödemelerde set edilir
        /// </summary>
        public DateTime? DueDate { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Giderin Ait Olduğu Mağaza (Navigation Property)
        ///
        /// İLİŞKİ: Expense (N) → Store (1)
        /// </summary>
        public virtual Store Store { get; set; } = null!;

        /// <summary>
        /// İlişkili Çalışan (Navigation Property - Opsiyonel)
        ///
        /// İLİŞKİ: Expense (N) → Employee (1)
        /// - Sadece çalışan ödemeleri için
        ///
        /// NULLABLE (Employee?):
        /// - Çalışan ödemesi değilse null
        /// - virtual Employee? = null reference olabilir
        /// </summary>
        public virtual Employee? Employee { get; set; }
    }
}
