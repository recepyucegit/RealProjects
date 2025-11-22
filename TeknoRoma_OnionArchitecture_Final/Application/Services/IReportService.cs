// ============================================================================
// IReportService.cs - Rapor Servis Interface
// ============================================================================
// AÇIKLAMA:
// Yönetici dashboard ve raporlama için özel servis.
// Birden fazla entity'den veri toplayarak anlamlı raporlar üretir.
//
// RAPOR TÜRLERİ:
// 1. Satış Raporları - Günlük/Aylık/Yıllık ciro
// 2. Ürün Raporları - En çok satanlar, düşük stok
// 3. Çalışan Raporları - Performans analizi
// 4. Gider Raporları - Maliyet analizi
// 5. Mağaza Raporları - Şube karşılaştırma
//
// PERFORMANS:
// Raporlar optimize SQL sorguları ile çalışır
// GROUP BY, SUM, COUNT gibi aggregate fonksiyonlar kullanılır
// Büyük veri setlerinde index kritik!
// ============================================================================

namespace Application.Services
{
    /// <summary>
    /// Rapor Servis Interface
    ///
    /// YÖNETİCİ DASHBOARD: Özet bilgiler, KPI'lar
    /// KARAR DESTEK: Veri bazlı karar alma
    /// </summary>
    public interface IReportService
    {
        // ========================================================================
        // SATIŞ RAPORLARI
        // ========================================================================

        /// <summary>
        /// Günlük Satış Toplamı
        ///
        /// KASA RAPORU: Gün sonu ciro
        /// KARŞILAŞTIRMA: Dün vs bugün
        ///
        /// ÖRNEK:
        /// var bugunSatis = await GetDailySalesAsync(DateTime.Today);
        /// var dunSatis = await GetDailySalesAsync(DateTime.Today.AddDays(-1));
        /// var artis = ((bugunSatis - dunSatis) / dunSatis) * 100; // Yüzde değişim
        /// </summary>
        Task<decimal> GetDailySalesAsync(DateTime date, int? storeId = null);

        /// <summary>
        /// Aylık Satış Toplamı
        ///
        /// PERFORMANS RAPORU: Ay bazlı trend
        /// HEDEF TAKİBİ: Aylık hedef karşılaştırma
        /// </summary>
        Task<decimal> GetMonthlySalesAsync(int year, int month, int? storeId = null);

        /// <summary>
        /// Yıllık Satış Toplamı
        ///
        /// FİNANSAL ÖZET: Yıllık ciro
        /// YIL KARŞILAŞTIRMA: 2023 vs 2024
        /// </summary>
        Task<decimal> GetYearlySalesAsync(int year, int? storeId = null);

        // ========================================================================
        // ÜRÜN RAPORLARI
        // ========================================================================

        /// <summary>
        /// En Çok Satan Ürünler
        ///
        /// DASHBOARD WİDGET: Top 10 ürün listesi
        /// STOK PLANLAMA: Çok satan ürünlerin stok durumu
        ///
        /// SIRALAMA: TotalAmount (satış tutarı) veya TotalQuantity (adet)
        ///
        /// ÖRNEK:
        /// // Bu ayın en çok satan 10 ürünü
        /// var top10 = await GetTopSellingProductsAsync(10,
        ///     startDate: new DateTime(2024, 1, 1),
        ///     endDate: new DateTime(2024, 1, 31));
        /// </summary>
        Task<IEnumerable<TopSellingProductReport>> GetTopSellingProductsAsync(int count, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Düşük Stok Raporu
        ///
        /// STOK UYARISI: Kritik seviyedeki ürünler
        /// SATIN ALMA: Sipariş verilmesi gereken ürünler
        ///
        /// KRİTER: CurrentStock &lt;= CriticalStockLevel
        /// </summary>
        Task<IEnumerable<LowStockProductReport>> GetLowStockReportAsync();

        // ========================================================================
        // ÇALIŞAN RAPORLARI
        // ========================================================================

        /// <summary>
        /// Çalışan Satış Raporu
        ///
        /// PRİM HESAPLAMA: Personel performansı
        /// PERFORMANS DEĞERLENDİRME: En çok satan personel
        ///
        /// İÇERİK:
        /// - Personel adı
        /// - Satış adedi
        /// - Toplam satış tutarı
        ///
        /// ÖRNEK:
        /// var ocakPerformans = await GetEmployeeSalesReportAsync(
        ///     new DateTime(2024, 1, 1),
        ///     new DateTime(2024, 1, 31));
        /// </summary>
        Task<IEnumerable<EmployeeSalesReport>> GetEmployeeSalesReportAsync(DateTime startDate, DateTime endDate, int? storeId = null);

        // ========================================================================
        // GİDER RAPORLARI
        // ========================================================================

        /// <summary>
        /// Aylık Toplam Gider
        ///
        /// KAR/ZARAR: Kar = Satış - Gider
        /// BÜTÇE TAKİBİ: Planlanan vs gerçekleşen
        /// </summary>
        Task<decimal> GetMonthlyExpensesAsync(int year, int month, int? storeId = null);

        /// <summary>
        /// Kategori Bazlı Gider Raporu
        ///
        /// MALİYET ANALİZİ: Hangi kategoride ne kadar harcandı?
        /// BÜTÇE PLANLAMASI: Kategori bazlı bütçe oluşturma
        ///
        /// ÇIKTI:
        /// - ExpenseType: Kira, Fatura, Maaş, vb.
        /// - TotalAmount: O kategorideki toplam tutar
        /// - Count: Kayıt sayısı
        ///
        /// GÖRSEL: Pasta grafiği için ideal
        /// </summary>
        Task<IEnumerable<ExpenseByTypeReport>> GetExpensesByTypeAsync(int year, int month, int? storeId = null);

        // ========================================================================
        // MAĞAZA RAPORLARI
        // ========================================================================

        /// <summary>
        /// Mağaza Satış Karşılaştırması
        ///
        /// ŞUBE PERFORMANSI: Hangi mağaza ne kadar satıyor?
        /// SIRALAMALI: En çok satan mağazadan başlayarak
        ///
        /// ÇIKTI:
        /// - StoreName: Mağaza adı
        /// - TotalSales: Toplam satış tutarı
        /// - SaleCount: Satış adedi
        ///
        /// GÖRSEL: Bar chart için ideal
        /// </summary>
        Task<IEnumerable<StoreSalesReport>> GetStoreSalesComparisonAsync(DateTime startDate, DateTime endDate);
    }

    // ========================================================================
    // RAPOR DTO'LARI (Data Transfer Objects)
    // ========================================================================
    // AÇIKLAMA:
    // Raporlama için özel DTO sınıfları.
    // Entity'lerden farklı olarak sadece raporda gerekli alanları içerir.
    //
    // NEDEN DTO?
    // - Performans: Sadece gerekli alanlar çekilir
    // - Güvenlik: Hassas veriler dışarı sızmaz
    // - Esneklik: Rapor formatı entity'den bağımsız
    // ========================================================================

    /// <summary>
    /// En Çok Satan Ürün Rapor DTO
    ///
    /// SQL KARŞILIĞI:
    /// SELECT ProductId, ProductName, SUM(Quantity), SUM(LineTotal)
    /// FROM SaleDetails
    /// GROUP BY ProductId, ProductName
    /// ORDER BY SUM(LineTotal) DESC
    /// </summary>
    public class TopSellingProductReport
    {
        /// <summary>Ürün ID - Detay sayfasına link için</summary>
        public int ProductId { get; set; }

        /// <summary>Ürün Adı - Raporda gösterim için</summary>
        public string ProductName { get; set; } = null!;

        /// <summary>Toplam Satılan Adet - SUM(Quantity)</summary>
        public int TotalQuantity { get; set; }

        /// <summary>Toplam Satış Tutarı - SUM(LineTotal)</summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Düşük Stok Rapor DTO
    ///
    /// KRİTİK STOK UYARISI için kullanılır
    /// Dashboard'da kırmızı uyarı badge'i
    /// </summary>
    public class LowStockProductReport
    {
        /// <summary>Ürün ID - Detay sayfasına link için</summary>
        public int ProductId { get; set; }

        /// <summary>Ürün Adı - Raporda gösterim</summary>
        public string ProductName { get; set; } = null!;

        /// <summary>Mevcut Stok - Product.CurrentStock</summary>
        public int CurrentStock { get; set; }

        /// <summary>Kritik Seviye - Product.CriticalStockLevel</summary>
        public int CriticalLevel { get; set; }
    }

    /// <summary>
    /// Çalışan Satış Rapor DTO
    ///
    /// PRİM HESAPLAMA için kullanılır
    /// </summary>
    public class EmployeeSalesReport
    {
        /// <summary>Çalışan ID - Detay sayfasına link</summary>
        public int EmployeeId { get; set; }

        /// <summary>Çalışan Adı - FirstName + " " + LastName</summary>
        public string EmployeeName { get; set; } = null!;

        /// <summary>Satış Adedi - COUNT(SaleId)</summary>
        public int SaleCount { get; set; }

        /// <summary>Toplam Satış Tutarı - SUM(TotalAmount)</summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Gider Türü Bazlı Rapor DTO
    ///
    /// PASTA GRAFİĞİ için ideal format
    /// Kategori bazlı maliyet dağılımı
    /// </summary>
    public class ExpenseByTypeReport
    {
        /// <summary>Gider Türü - ExpenseType.ToString()</summary>
        public string ExpenseType { get; set; } = null!;

        /// <summary>Toplam Tutar - SUM(Amount)</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Kayıt Sayısı - COUNT(*)</summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Mağaza Satış Rapor DTO
    ///
    /// BAR CHART için ideal format
    /// Şube karşılaştırma grafiği
    /// </summary>
    public class StoreSalesReport
    {
        /// <summary>Mağaza ID - Detay sayfasına link</summary>
        public int StoreId { get; set; }

        /// <summary>Mağaza Adı - Store.Name</summary>
        public string StoreName { get; set; } = null!;

        /// <summary>Toplam Satış Tutarı - SUM(TotalAmount)</summary>
        public decimal TotalSales { get; set; }

        /// <summary>Satış Adedi - COUNT(SaleId)</summary>
        public int SaleCount { get; set; }
    }
}
