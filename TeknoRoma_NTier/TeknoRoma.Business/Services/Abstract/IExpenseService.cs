using TeknoRoma.Business.DTOs;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Abstract;

/// <summary>
/// Expense Service Interface - Gider yönetimi işlemleri
/// Feyza Paragöz'ün (Muhasebe) kullandığı servis
///
/// SORUMLULUKLAR:
/// 1. CRUD İşlemleri
/// 2. Çoklu para birimi yönetimi (TRY, USD, EUR)
/// 3. Döviz kuru hesaplaması ve API entegrasyonu
/// 4. Otomatik ExpenseNumber oluşturma (G-2024-00001)
/// 5. Ödeme durumu takibi ve vade kontrolü
/// 6. Gider raporları (mağaza, tip, tarih bazlı)
///
/// ÖNEMLİ ÖZELLİKLER:
/// - Currency = TRY ise: ExchangeRate = 1, AmountInTRY = Amount
/// - Currency = USD/EUR ise: ExchangeRate API'den çekilir veya manuel girilir
/// - AmountInTRY otomatik hesaplanır: Amount * ExchangeRate
/// - ExpenseNumber benzersiz olmalı
///
/// İŞ KURALLARI:
/// - ExpenseType = CalisanOdemesi ise EmployeeId zorunlu
/// - IsPaid = true ise PaymentDate zorunlu
/// - Geçmiş tarihli döviz kuru değiştirilemez (tarihi doğruluk)
/// </summary>
public interface IExpenseService
{
    // ====== CRUD OPERATIONS ======

    /// <summary>
    /// Tüm giderleri getirir
    /// </summary>
    /// <param name="includeDeleted">Silinmiş kayıtlar da dahil edilsin mi?</param>
    /// <returns>Gider listesi</returns>
    Task<IEnumerable<ExpenseSummaryDto>> GetAllExpensesAsync(bool includeDeleted = false);

    /// <summary>
    /// ID'ye göre gider getirir
    /// </summary>
    /// <param name="id">Gider ID</param>
    /// <returns>Gider bilgileri veya null</returns>
    Task<ExpenseDto?> GetExpenseByIdAsync(int id);

    /// <summary>
    /// Gider numarasına göre getirir
    /// KULLANIM: Feyza Paragöz: "G-2024-00123 numaralı gideri bul"
    /// </summary>
    /// <param name="expenseNumber">Gider numarası</param>
    /// <returns>Gider bilgileri veya null</returns>
    Task<ExpenseDto?> GetExpenseByNumberAsync(string expenseNumber);

    /// <summary>
    /// Yeni gider oluşturur
    ///
    /// OTOMATIK İŞLEMLER:
    /// 1. ExpenseNumber oluşturulur: G-{Yıl}-{5 haneli sıra}
    /// 2. Currency = TRY ise ExchangeRate = 1
    /// 3. Currency = USD/EUR ise:
    ///    - ExchangeRate verilmişse kullan
    ///    - Verilmemişse API'den çek (TCMB, CentralBank API)
    /// 4. AmountInTRY hesapla: Amount * ExchangeRate
    ///
    /// VALIDASYON:
    /// - ExpenseType = CalisanOdemesi ise EmployeeId kontrolü
    /// - IsPaid = true ise PaymentDate kontrolü
    /// - Mağaza varlığı kontrolü
    /// </summary>
    /// <param name="createExpenseDto">Oluşturulacak gider bilgileri</param>
    /// <returns>Oluşturulan gider bilgileri</returns>
    Task<ExpenseDto?> CreateExpenseAsync(CreateExpenseDto createExpenseDto);

    /// <summary>
    /// Gider bilgilerini günceller
    ///
    /// ÖZEL KURALLAR:
    /// - ExpenseNumber değiştirilemez
    /// - Currency değişirse AmountInTRY yeniden hesaplanır
    /// - ExchangeRate değişirse AmountInTRY yeniden hesaplanır
    /// </summary>
    /// <param name="updateExpenseDto">Güncellenecek gider bilgileri</param>
    /// <returns>Güncellenen gider bilgileri veya null</returns>
    Task<ExpenseDto?> UpdateExpenseAsync(UpdateExpenseDto updateExpenseDto);

    /// <summary>
    /// Gideri siler (Soft Delete)
    /// </summary>
    /// <param name="id">Silinecek gider ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> DeleteExpenseAsync(int id);


    // ====== BUSINESS LOGIC METHODS ======

    /// <summary>
    /// Mağazanın giderlerini getirir
    /// KULLANIM: Haluk Bey: "İstanbul Kadıköy mağazasının giderleri"
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <returns>Mağaza giderleri</returns>
    Task<IEnumerable<ExpenseSummaryDto>> GetExpensesByStoreAsync(int storeId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gider tipine göre getirir
    /// KULLANIM: Feyza Paragöz: "Tüm maaş ödemeleri"
    /// </summary>
    /// <param name="expenseType">Gider tipi</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <returns>Bu tipteki giderler</returns>
    Task<IEnumerable<ExpenseSummaryDto>> GetExpensesByTypeAsync(ExpenseType expenseType, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Çalışanın maaş ödemelerini getirir
    /// KULLANIM: "Gül Satar'ın maaş geçmişi"
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <returns>Maaş ödemeleri</returns>
    Task<IEnumerable<ExpenseDto>> GetEmployeeExpensesAsync(int employeeId);

    /// <summary>
    /// Bekleyen (ödenmemiş) giderleri getirir
    /// KULLANIM: Feyza Paragöz dashboard'unda "Bugün ödenecekler"
    /// </summary>
    /// <param name="storeId">Mağaza ID (opsiyonel, null ise tüm mağazalar)</param>
    /// <returns>Bekleyen giderler</returns>
    Task<IEnumerable<ExpenseSummaryDto>> GetPendingExpensesAsync(int? storeId = null);

    /// <summary>
    /// Vadesi geçmiş giderleri getirir
    /// KULLANIM: "Ödemesi gecikmiş faturalar" (kırmızı uyarı)
    /// </summary>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Vadesi geçmiş giderler</returns>
    Task<IEnumerable<ExpenseSummaryDto>> GetOverdueExpensesAsync(int? storeId = null);

    /// <summary>
    /// Gideri ödendi olarak işaretler
    /// KULLANIM: Feyza Paragöz ödeme yaptıktan sonra
    ///
    /// GÜNCELLENİR:
    /// - IsPaid = true
    /// - PaymentDate = şimdi
    /// - PaymentMethod
    /// </summary>
    /// <param name="expenseId">Gider ID</param>
    /// <param name="paymentMethod">Ödeme yöntemi</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> MarkAsPaidAsync(int expenseId, PaymentType paymentMethod);

    /// <summary>
    /// Güncel döviz kurunu API'den çeker
    /// KULLANIM: Frontend'de Currency seçilince otomatik çağrılır
    ///
    /// API KAYNAKLARI:
    /// - TCMB (Türkiye Cumhuriyet Merkez Bankası) API
    /// - Fixer.io API (Yedek)
    /// - ExchangeRate-API (Yedek)
    /// </summary>
    /// <param name="currency">Para birimi (USD veya EUR)</param>
    /// <returns>Güncel döviz kuru (TRY karşılığı)</returns>
    Task<decimal> GetCurrentExchangeRateAsync(Currency currency);

    /// <summary>
    /// Aylık gider raporu
    /// Feyza Paragöz ve Haluk Bey için
    ///
    /// İÇERİK:
    /// - Gider tiplerine göre dağılım (Pasta grafik)
    /// - Mağazalara göre dağılım
    /// - Aylık trend grafiği
    /// - Toplam gider (TL)
    /// - En büyük gider kalemleri
    /// </summary>
    /// <param name="year">Yıl</param>
    /// <param name="month">Ay</param>
    /// <param name="storeId">Mağaza ID (opsiyonel, null ise tüm mağazalar)</param>
    /// <returns>Aylık gider raporu</returns>
    Task<object> GetMonthlyExpenseReportAsync(int year, int month, int? storeId = null);

    /// <summary>
    /// Yıllık gider raporu
    /// Haluk Bey'in yıl sonu değerlendirmesi için
    /// </summary>
    /// <param name="year">Yıl</param>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Yıllık gider raporu</returns>
    Task<object> GetYearlyExpenseReportAsync(int year, int? storeId = null);

    /// <summary>
    /// Mağaza toplam giderini hesaplar (belirli tarih aralığında)
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Toplam gider (TL)</returns>
    Task<decimal> GetTotalExpenseAsync(int storeId, DateTime startDate, DateTime endDate);
}
