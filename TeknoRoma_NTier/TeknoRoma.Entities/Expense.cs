using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Entities;

/// <summary>
/// Gider Entity
/// Feyza Paragöz'ün (Muhasebe Temsilcisi) yönettiği giderler
///
/// Haluk Bey'in istediği Gider Raporu kapsar:
/// - Çalışan maaş ödemeleri (en büyük gider kalemi)
/// - Teknik altyapı giderleri (sunucu, network, yazılım lisansları)
/// - Faturalar (kira, elektrik, su, internet)
/// - Diğer giderler (kırtasiye, temizlik, reklam vb.)
/// </summary>
public class Expense : BaseEntity
{
    /// <summary>
    /// Gider Numarası (Benzersiz)
    /// Format: "G-2024-00001"
    /// Feyza Paragöz'ün takip için kullandığı numara
    /// </summary>
    public string ExpenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gider Tarihi
    /// Hangi ay/yıla ait gider (raporlama için kritik)
    /// </summary>
    public DateTime ExpenseDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Gider Türü
    /// CalisanOdemesi, TeknikaltyapiGideri, Fatura, DigerGider
    /// NEDEN? Gider kategorilerini ayırarak raporlama yapıyoruz
    /// </summary>
    public ExpenseType ExpenseType { get; set; }

    /// <summary>
    /// Hangi mağaza? (Foreign Key)
    /// Her mağaza kendi giderlerini takip eder
    /// Haluk Bey: "Hangi mağazanın giderleri fazla"
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Hangi çalışana ödeme? (Foreign Key)
    /// Sadece ExpenseType = CalisanOdemesi ise dolu
    /// Maaş ödemelerinde ilgili çalışan
    /// Null olabilir (diğer gider türlerinde çalışan yok)
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// Gider Tutarı
    /// Ödenecek veya ödenen miktar
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para Birimi
    /// TRY, USD, EUR
    /// Feyza Paragöz: "O tarihteki döviz kurunu da görmek istiyorum"
    /// NEDEN? Bazı giderler döviz cinsinden (yazılım lisansları, sunucu kiralama)
    /// </summary>
    public Currency Currency { get; set; } = Currency.TRY;

    /// <summary>
    /// Döviz Kuru (sadece USD ve EUR için)
    /// O tarihteki kur
    /// Null ise TRY (varsayılan kur = 1)
    /// NEDEN? Geçmiş giderlerin bugünkü TL karşılığını görmek için
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// TL karşılığı
    /// Hesaplama: Amount * ExchangeRate (veya Amount * 1 if TRY)
    /// Currency = TRY ise Amount ile aynı
    /// NEDEN? Tüm giderleri TL cinsinden toplayıp rapor vermek için
    /// </summary>
    public decimal AmountInTRY { get; set; }

    /// <summary>
    /// Gider Açıklaması
    /// "Ocak 2024 Maaşı - Recep Öztürk"
    /// "Elektrik Faturası - Aralık 2023"
    /// "Sunucu Bakım Ücreti"
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Fatura/Evrak Numarası
    /// Opsiyonel - Fatura varsa numarası
    /// Muhasebe arşivi için önemli
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Ödeme yapıldı mı?
    /// false: Bekleyen ödeme
    /// true: Ödeme yapıldı
    /// NEDEN? Nakit akışı yönetimi için kritik
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Ödeme Tarihi
    /// Null olabilir (henüz ödeme yapılmadıysa)
    /// NEDEN? Vade takibi için
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Ödeme Yöntemi
    /// Nakit, KrediKarti, Havale, Cek
    /// </summary>
    public PaymentType? PaymentMethod { get; set; }


    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Giderin ait olduğu mağaza
    /// Many-to-One ilişki
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// Giderin ait olduğu çalışan (maaş ödemelerinde)
    /// Many-to-One ilişki
    /// Null olabilir (çalışan maaşı olmayan giderlerde)
    /// </summary>
    public virtual Employee? Employee { get; set; }
}
