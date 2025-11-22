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
        // META BILGILER
        // =====================================================================

        /// <summary>
        /// Son guncelleme zamani
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
