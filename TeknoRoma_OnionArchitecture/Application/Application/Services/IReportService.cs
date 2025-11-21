using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Rapor Servisi Interface - Haluk Bey'in (Şube Müdürü) İstekleri
///
/// TÜM RAPORLAR:
/// 1. Stok Takip Raporu - Ürün stok durumları, kritik seviyeler
/// 2. Satış Takip Raporu - Çalışan satışları, prim hesaplama, kota kontrolü
/// 3. Tedarikçi Hareket Raporu - Hangi tedarikçiden ne kadar alım
/// 4. Ürün Liste Raporu - Kategorilere göre ürünler, satılmayan ürünler
/// 5. Gider Raporu - Çalışan ödemeleri, faturalar, diğer giderler
/// 6. Çapraz Satış Raporu - Birlikte satılan ürünler
/// 7. Müşteri Demografik Raporu - Yaş, cinsiyet analizi
///
/// EXPORT:
/// - Excel formatında (.xlsx)
/// - PDF formatında (.pdf)
/// - Yazıcıdan çıktı alınabilir
/// </summary>
public interface IReportService
{
    // ====== STOK TAKİP RAPORU ======

    /// <summary>
    /// Stok Takip Raporu
    /// Haluk Bey: "Ürünlerin stok durumları, kritik seviyenin altına düşen ürünler"
    /// </summary>
    Task<StockReportDto> GetStockReportAsync(int? storeId = null);

    /// <summary>
    /// Kritik Stok Raporu
    /// Sadece kritik seviyenin altına düşen ürünler
    /// </summary>
    Task<IEnumerable<CriticalStockItemDto>> GetCriticalStockReportAsync(int? storeId = null);

    /// <summary>
    /// Stok olmayan ürünler raporu
    /// </summary>
    Task<IEnumerable<OutOfStockItemDto>> GetOutOfStockReportAsync(int? storeId = null);


    // ====== SATIŞ TAKİP RAPORU ======

    /// <summary>
    /// Çalışan Satış Performans Raporu
    /// Haluk Bey: "Hangi çalışanım, bu ay hangi ürünleri ne kadar satmış"
    ///
    /// İÇERİK:
    /// - Çalışan adı
    /// - Toplam satış tutarı
    /// - Satış kotası (10.000 TL)
    /// - Kota gerçekleşme oranı
    /// - Prim tutarı (kota üstü %10)
    /// </summary>
    Task<IEnumerable<EmployeeSalesPerformanceDto>> GetEmployeeSalesPerformanceReportAsync(int year, int month, int? storeId = null);

    /// <summary>
    /// En Çok Satan 10 Ürün Raporu
    /// Haluk Bey: "En çok satılan 10 ürün, tedarikçisi ve müşteri kitlesi"
    ///
    /// İÇERİK:
    /// - Ürün adı
    /// - Satış adedi
    /// - Toplam satış tutarı
    /// - Tedarikçi adı
    /// - Müşteri demografisi (yaş ortalaması, cinsiyet dağılımı)
    /// </summary>
    Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsReportAsync(int topCount, DateTime startDate, DateTime endDate, int? storeId = null);

    /// <summary>
    /// Çapraz Satış Raporu (Market Basket Analysis)
    /// Haluk Bey: "En çok satılan 10 ürün ve bunların yanında en çok satılan ürünler"
    ///
    /// ÖRNEK:
    /// iPhone 15 alan müşteriler genellikle şunları da alıyor:
    /// - AirPods Pro (%45)
    /// - Silikon Kılıf (%38)
    /// - Şarj Kablosu (%32)
    /// </summary>
    Task<IEnumerable<CrossSellingReportDto>> GetCrossSellingReportAsync(int topProductCount, DateTime startDate, DateTime endDate, int? storeId = null);


    // ====== TEDARİKÇİ HAREKET RAPORU ======

    /// <summary>
    /// Tedarikçi Hareket Raporu
    /// Haluk Bey: "Hangi tedarikçiden bu ay hangi ürünleri ne kadar almışız"
    ///
    /// İÇERİK:
    /// - Tedarikçi adı
    /// - Alınan ürünler listesi
    /// - Her ürün için miktar ve tutar
    /// - Toplam alım tutarı
    /// </summary>
    Task<IEnumerable<SupplierTransactionReportDto>> GetSupplierTransactionReportAsync(int year, int month, int? supplierId = null, int? storeId = null);


    // ====== ÜRÜN LİSTE RAPORU ======

    /// <summary>
    /// Kategorilere Göre Ürün Liste Raporu
    /// Haluk Bey: "Kategorilere göre ürünlerin isimleri, fiyatları ve adetleri"
    /// </summary>
    Task<IEnumerable<CategoryProductListDto>> GetProductListByCategoryReportAsync(int? storeId = null);

    /// <summary>
    /// Satılmayan (Eskiden Satılan) Ürünler Raporu
    /// Haluk Bey: "Şuanda satmadığımız eskiden sattığımız ürünler"
    ///
    /// KRİTER:
    /// - Son 90 günde hiç satılmamış
    /// - Stokta var
    /// - Aktif ürün
    ///
    /// NOT: "Bu ürünleri hızlı bir şekilde elimizden çıkarmamız gerekiyor"
    /// </summary>
    Task<IEnumerable<UnsoldProductDto>> GetUnsoldProductsReportAsync(int daysSinceLastSale = 90, int? storeId = null);


    // ====== GİDER RAPORU ======

    /// <summary>
    /// Gider Raporu
    /// Haluk Bey: "Çalışan ödemeleri, Teknik altyapı giderleri, Faturalar, Diğer Giderler"
    ///
    /// İÇERİK:
    /// - Gider kategorisine göre gruplandırma
    /// - Her kategori için toplam tutar
    /// - Detaylı gider listesi
    /// - Döviz kuru bilgisi (o tarihteki)
    /// </summary>
    Task<ExpenseReportDto> GetExpenseReportAsync(int year, int month, int? storeId = null);


    // ====== MÜŞTERİ DEMOGRAFİK RAPORU ======

    /// <summary>
    /// Müşteri Demografik Raporu
    /// Haluk Bey: "Müşteri kitlesi yaş, cinsiyet gibi"
    ///
    /// İÇERİK:
    /// - Yaş gruplarına göre dağılım
    /// - Cinsiyet dağılımı
    /// - Şehir dağılımı
    /// - En çok alışveriş yapan müşteri segmenti
    /// </summary>
    Task<CustomerDemographicsDto> GetCustomerDemographicsReportAsync(DateTime startDate, DateTime endDate, int? storeId = null);


    // ====== ÖZET DASHBOARD RAPORU ======

    /// <summary>
    /// Dashboard Özet Raporu
    /// Tek bakışta tüm önemli metrikler
    /// </summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? storeId = null);
}


// ====== DTO'LAR ======

/// <summary>
/// Stok Raporu DTO
/// </summary>
public class StockReportDto
{
    public int TotalProducts { get; set; }
    public int InStockProducts { get; set; }
    public int CriticalStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public IEnumerable<StockItemDto> Items { get; set; } = new List<StockItemDto>();
}

public class StockItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int UnitsInStock { get; set; }
    public int CriticalStockLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal StockValue { get; set; }
    public string StockStatus { get; set; } = string.Empty;
}

public class CriticalStockItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int UnitsInStock { get; set; }
    public int CriticalStockLevel { get; set; }
    public int ShortageAmount { get; set; } // Eksik miktar
}

public class OutOfStockItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? LastSaleDate { get; set; }
    public int LastOrderQuantity { get; set; }
}

/// <summary>
/// Çalışan Satış Performans DTO
/// </summary>
public class EmployeeSalesPerformanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal SalesQuota { get; set; } // 10.000 TL
    public decimal TotalSales { get; set; }
    public int SaleCount { get; set; }
    public decimal QuotaAchievementRate { get; set; } // Yüzde
    public decimal SalesAboveQuota { get; set; } // Kota üstü satış
    public decimal CommissionAmount { get; set; } // Prim = Kota üstü * %10
    public bool IsQuotaAchieved { get; set; }
}

/// <summary>
/// En Çok Satan Ürün DTO
/// </summary>
public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public CustomerDemographicsSummaryDto CustomerDemographics { get; set; } = new();
}

public class CustomerDemographicsSummaryDto
{
    public decimal AverageAge { get; set; }
    public decimal MalePercentage { get; set; }
    public decimal FemalePercentage { get; set; }
    public string TopCity { get; set; } = string.Empty;
}

/// <summary>
/// Çapraz Satış Raporu DTO
/// </summary>
public class CrossSellingReportDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSales { get; set; }
    public IEnumerable<CrossSellingItemDto> FrequentlyBoughtTogether { get; set; } = new List<CrossSellingItemDto>();
}

public class CrossSellingItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CoOccurrenceCount { get; set; }
    public decimal CoOccurrencePercentage { get; set; }
}

/// <summary>
/// Tedarikçi Hareket Raporu DTO
/// </summary>
public class SupplierTransactionReportDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public int TotalTransactionCount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public IEnumerable<SupplierProductPurchaseDto> ProductPurchases { get; set; } = new List<SupplierProductPurchaseDto>();
}

public class SupplierProductPurchaseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Kategori Ürün Liste DTO
/// </summary>
public class CategoryProductListDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public decimal TotalStockValue { get; set; }
    public IEnumerable<ProductListItemDto> Products { get; set; } = new List<ProductListItemDto>();
}

public class ProductListItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Satılmayan Ürün DTO
/// </summary>
public class UnsoldProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
    public decimal StockValue { get; set; }
    public int DaysSinceLastSale { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public string RecommendedAction { get; set; } = string.Empty; // İndirim önerisi
}

/// <summary>
/// Gider Raporu DTO
/// </summary>
public class ExpenseReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal EmployeePayments { get; set; }
    public decimal TechnicalInfrastructure { get; set; }
    public decimal Bills { get; set; }
    public decimal OtherExpenses { get; set; }
    public IEnumerable<ExpenseDetailDto> ExpenseDetails { get; set; } = new List<ExpenseDetailDto>();
}

public class ExpenseDetailDto
{
    public int ExpenseId { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string ExpenseType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? ExchangeRate { get; set; }
    public decimal AmountInTRY { get; set; }
    public bool IsPaid { get; set; }
}

/// <summary>
/// Müşteri Demografik DTO
/// </summary>
public class CustomerDemographicsDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal AverageAge { get; set; }
    public IEnumerable<AgeGroupDto> AgeGroups { get; set; } = new List<AgeGroupDto>();
    public IEnumerable<GenderDistributionDto> GenderDistribution { get; set; } = new List<GenderDistributionDto>();
    public IEnumerable<CityDistributionDto> CityDistribution { get; set; } = new List<CityDistributionDto>();
}

public class AgeGroupDto
{
    public string AgeRange { get; set; } = string.Empty; // "18-25", "26-35", vb.
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class GenderDistributionDto
{
    public string Gender { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class CityDistributionDto
{
    public string City { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Dashboard Özet DTO
/// </summary>
public class DashboardSummaryDto
{
    public DateTime ReportDate { get; set; } = DateTime.Now;

    // Satış özeti
    public decimal TodaysSales { get; set; }
    public int TodaysSaleCount { get; set; }
    public decimal ThisMonthsSales { get; set; }
    public int ThisMonthsSaleCount { get; set; }

    // Stok özeti
    public int TotalProducts { get; set; }
    public int CriticalStockCount { get; set; }
    public int OutOfStockCount { get; set; }

    // Çalışan özeti
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }

    // Teknik servis özeti
    public int OpenTechnicalServices { get; set; }
    public int CriticalTechnicalServices { get; set; }

    // Gider özeti
    public decimal ThisMonthsExpenses { get; set; }
    public int UnpaidExpensesCount { get; set; }
}
