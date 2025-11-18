using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Sale Repository Interface
    /// Satış işlemleri için özel metodlar
    /// </summary>
    public interface ISaleRepository : IRepository<Sale>
    {
        /// <summary>
        /// Satış numarasına göre satış bulur
        /// NEDEN?
        /// - Fahri Cepçi: "Müşteriye satış numarası vereceğim, kasada o numarayı girip görebilmeliler"
        /// - Unique field olduğu için özel metod
        /// </summary>
        Task<Sale> GetBySaleNumberAsync(string saleNumber);

        /// <summary>
        /// Satışı detayları ile birlikte getirir (Eager Loading)
        /// NEDEN?
        /// - Sale + SaleDetails + Product bilgilerini birlikte getirmek
        /// - N+1 Query problemini önler
        /// - Performans için include yapıyoruz
        /// </summary>
        Task<Sale> GetWithDetailsAsync(int saleId);

        /// <summary>
        /// Müşterinin tüm satışlarını getirir
        /// Müşteri geçmişi için
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// Çalışanın yaptığı satışları getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Hangi çalışanım bu ay hangi ürünleri ne kadar satmış"
        /// - Performans değerlendirmesi için
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Mağazanın satışlarını getirir
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Tarih aralığındaki satışları getirir
        /// NEDEN?
        /// - Aylık, haftalık raporlar için
        /// - "Bu ay ne kadar satış yaptık?"
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Duruma göre satışları getirir
        /// NEDEN?
        /// - Kerim Zulacı: "Hazirlaniyor durumundaki satışları görmem lazım"
        /// - Depo için bekleyen işler listesi
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByStatusAsync(SaleStatus status);

        /// <summary>
        /// Bugünkü satışları getirir
        /// Günlük rapor için kısayol metod
        /// </summary>
        Task<IReadOnlyList<Sale>> GetTodaysSalesAsync();

        /// <summary>
        /// Bu ayın satışlarını getirir
        /// Aylık rapor için kısayol metod
        /// </summary>
        Task<IReadOnlyList<Sale>> GetThisMonthsSalesAsync();

        /// <summary>
        /// Çalışanın aylık satış toplamını hesaplar
        /// NEDEN?
        /// - Gül Satar: "Satış kotasını geçmiş mi, ne kadar prim haketmiş"
        /// - Prime ne kadar yaklaştım görmek istiyoruz
        /// </summary>
        Task<decimal> GetEmployeeMonthlySalesTotalAsync(int employeeId, int year, int month);

        /// <summary>
        /// Mağazanın günlük satış toplamını hesaplar
        /// </summary>
        Task<decimal> GetStoreDailySalesTotalAsync(int storeId, DateTime date);

        /// <summary>
        /// Mağazanın aylık satış toplamını hesaplar
        /// </summary>
        Task<decimal> GetStoreMonthlySalesTotalAsync(int storeId, int year, int month);

        /// <summary>
        /// En çok satan ürünleri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "En çok satılan 10 ürün"
        /// - SaleDetail tablosundan group by ile hesaplanır
        /// </summary>
        Task<IReadOnlyList<Product>> GetTopSellingProductsAsync(int count, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Ürünün satış sayısını getirir
        /// Belirli bir tarih aralığında
        /// </summary>
        Task<int> GetProductSalesCountAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Yeni satış numarası oluşturur
        /// Format: S-2024-00001, S-2024-00002
        /// </summary>
        Task<string> GenerateSaleNumberAsync();
    }
}