// ===================================================================================
// TEKNOROMA MVC - DASHBOARD VIEW MODEL
// ===================================================================================
//
// Ana sayfa dashboard'u icin view model.
// Tum ozet bilgileri tek bir modelde toplar.
//
// ===================================================================================

namespace WebMVC.Models
{
    /// <summary>
    /// Dashboard View Model
    ///
    /// OZET BILGILER:
    /// - Satis istatistikleri
    /// - Stok uyarilari
    /// - Teknik servis talepleri
    /// - Finansal ozet
    /// - Doviz kurlari
    /// </summary>
    public class DashboardViewModel
    {
        // =====================================================================
        // SATIS ISTATISTIKLERI
        // =====================================================================

        /// <summary>
        /// Bugunun toplam satis tutari
        /// </summary>
        public decimal TodaySalesTotal { get; set; }

        /// <summary>
        /// Bu ayin toplam satis tutari
        /// </summary>
        public decimal MonthlySalesTotal { get; set; }

        /// <summary>
        /// Bekleyen satis sayisi (Depo icin)
        /// </summary>
        public int PendingSalesCount { get; set; }

        // =====================================================================
        // STOK BILGILERI
        // =====================================================================

        /// <summary>
        /// Kritik stok seviyesindeki urun sayisi
        /// </summary>
        public int LowStockProductCount { get; set; }

        // =====================================================================
        // TEKNIK SERVIS
        // =====================================================================

        /// <summary>
        /// Acik teknik servis talep sayisi
        /// </summary>
        public int OpenTicketsCount { get; set; }

        // =====================================================================
        // FINANSAL
        // =====================================================================

        /// <summary>
        /// Odenmemis gider sayisi
        /// </summary>
        public int UnpaidExpensesCount { get; set; }

        // =====================================================================
        // DOVIZ KURLARI
        // =====================================================================

        /// <summary>
        /// USD/TRY kuru
        /// </summary>
        public decimal UsdRate { get; set; }

        /// <summary>
        /// EUR/TRY kuru
        /// </summary>
        public decimal EurRate { get; set; }

        // =====================================================================
        // CHART VERILERI
        // =====================================================================

        /// <summary>
        /// Son 7 gunluk satis verileri (Chart.js icin)
        /// </summary>
        public List<DailySalesData> Last7DaysSales { get; set; } = new();

        /// <summary>
        /// Kategori bazli satis dagilimi (Chart.js icin)
        /// </summary>
        public List<CategorySalesData> CategorySales { get; set; } = new();

        /// <summary>
        /// Son satislar listesi
        /// </summary>
        public List<RecentSaleData> RecentSales { get; set; } = new();

        /// <summary>
        /// Kritik stok urunleri
        /// </summary>
        public List<LowStockProductData> LowStockProducts { get; set; } = new();

        // =====================================================================
        // META BILGILER
        // =====================================================================

        /// <summary>
        /// Son guncelleme zamani
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    // =========================================================================
    // YARDIMCI SINIFLAR
    // =========================================================================

    /// <summary>
    /// Gunluk satis verisi
    /// </summary>
    public class DailySalesData
    {
        public string Date { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int SaleCount { get; set; }
    }

    /// <summary>
    /// Kategori satis verisi
    /// </summary>
    public class CategorySalesData
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// Son satis verisi
    /// </summary>
    public class RecentSaleData
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }

    /// <summary>
    /// Kritik stok urun verisi
    /// </summary>
    public class LowStockProductData
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
