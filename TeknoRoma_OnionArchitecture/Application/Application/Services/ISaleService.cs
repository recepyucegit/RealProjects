using Application.DTOs.SaleDTO;
using Application.DTOs.SaleDTO;

namespace Application.Services
{
    /// <summary>
    /// Sale Service Interface
    /// Satış iş mantığı koordinasyonu
    /// 
    /// ÖZEL İŞ MANTIĞI:
    /// 1. Stok kontrolü
    /// 2. Fiyat hesaplama (KDV, indirim)
    /// 3. Satış numarası oluşturma
    /// 4. Durum yönetimi (Beklemede → Hazirlaniyor → Tamamlandi)
    /// 5. Prim hesaplama
    /// </summary>
    public interface ISaleService
    {
        // ====== QUERY OPERATIONS ======

        /// <summary>
        /// Tüm satışları getirir
        /// </summary>
        Task<IEnumerable<SaleDTO>> GetAllSalesAsync();

        /// <summary>
        /// ID'ye göre satış getirir (Detayları ile birlikte)
        /// </summary>
        Task<SaleDTO> GetSaleByIdAsync(int id);

        /// <summary>
        /// Satış numarasına göre getirir
        /// NEDEN?
        /// - Fahri Cepçi: "Satış numarasını girip görebilmeliyim"
        /// </summary>
        Task<SaleDTO> GetSaleBySaleNumberAsync(string saleNumber);

        /// <summary>
        /// Müşterinin satışlarını getirir
        /// </summary>
        Task<IEnumerable<SaleDTO>> GetSalesByCustomerAsync(int customerId);

        /// <summary>
        /// Çalışanın satışlarını getirir
        /// </summary>
        Task<IEnumerable<SaleDTO>> GetSalesByEmployeeAsync(int employeeId);

        /// <summary>
        /// Bugünkü satışları getirir
        /// </summary>
        Task<IEnumerable<SaleDTO>> GetTodaysSalesAsync();

        /// <summary>
        /// Bu ayın satışlarını getirir
        /// </summary>
        Task<IEnumerable<SaleDTO>> GetThisMonthsSalesAsync();

        /// <summary>
        /// Duruma göre satışları getirir
        /// NEDEN?
        /// - Kerim Zulacı: "Hazirlaniyor durumundaki satışları görmeliyim"
        /// </summary>
        Task<IEnumerable<SaleDTO>> GetSalesByStatusAsync(Domain.Enums.SaleStatus status);


        // ====== COMMAND OPERATIONS ======

        /// <summary>
        /// Yeni satış oluşturur
        /// 
        /// İŞ MANTIĞI:
        /// 1. Satış numarası otomatik oluştur (S-2024-00001)
        /// 2. Tüm ürünlerin stok kontrolünü yap
        /// 3. Fiyat hesaplamalarını yap (Ara toplam, KDV, İndirim, Genel toplam)
        /// 4. Satış kaydını oluştur
        /// 5. Satış detaylarını oluştur
        /// 6. Stokları azalt
        /// 7. Status = Beklemede olarak ayarla
        /// </summary>
        Task<SaleDTO> CreateSaleAsync(CreateSaleDTO dto);

        /// <summary>
        /// Satışı günceller
        /// DİKKAT: Sadece belirli durumlarda güncellenebilir
        /// </summary>
        Task<SaleDTO> UpdateSaleAsync(UpdateSaleDTO dto);

        /// <summary>
        /// Satışı iptal eder
        /// 
        /// İŞ MANTIĞI:
        /// 1. Satış Tamamlandi durumundaysa iptal edilemez
        /// 2. Status = Iptal yap
        /// 3. Stokları geri ekle
        /// 4. İptal nedeni kaydet
        /// </summary>
        Task<bool> CancelSaleAsync(int saleId, string reason);

        /// <summary>
        /// Satış durumunu günceller
        /// 
        /// İŞ AKIŞI:
        /// Beklemede → Hazirlaniyor (Ödeme yapıldı)
        /// Hazirlaniyor → Tamamlandi (Ürünler teslim edildi)
        /// </summary>
        Task<bool> UpdateSaleStatusAsync(int saleId, Domain.Enums.SaleStatus newStatus);

        /// <summary>
        /// Ödeme onaylar ve durumu Hazirlaniyor yapar
        /// NEDEN?
        /// - Gül Satar: "Ödemeyi onayladığım anda depo bilgilendirilmeli"
        /// </summary>
        Task<bool> ConfirmPaymentAsync(int saleId);

        /// <summary>
        /// Teslimatı tamamlar ve durumu Tamamlandi yapar
        /// NEDEN?
        /// - Kerim Zulacı: "Ürünleri kasaya getirdikten sonra tamamlandı işaretlemeliyim"
        /// </summary>
        Task<bool> CompleteSaleAsync(int saleId);


        // ====== BUSINESS CALCULATIONS ======

        /// <summary>
        /// Çalışanın aylık satış toplamını hesaplar
        /// </summary>
        Task<decimal> GetEmployeeMonthlySalesAsync(int employeeId, int year, int month);

        /// <summary>
        /// Çalışanın prim tutarını hesaplar
        /// 
        /// İŞ KURALI:
        /// - Satış kotası: 10.000 TL
        /// - Kotayı geçerse: (Toplam Satış - 10.000) * 0.10
        /// - Örn: 15.000 TL satış → (15.000 - 10.000) * 0.10 = 500 TL prim
        /// </summary>
        Task<decimal> CalculateEmployeeCommissionAsync(int employeeId, int year, int month);

        /// <summary>
        /// Mağazanın aylık satış toplamını hesaplar
        /// </summary>
        Task<decimal> GetStoreMonthlySalesAsync(int storeId, int year, int month);
    }
}