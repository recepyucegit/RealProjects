using TeknoRoma.Business.DTOs;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Abstract;

/// <summary>
/// Sale Service Interface - Satış işlemleri
/// Gül Satar'ın (Kasa Satış Temsilcisi) kullandığı servis
///
/// EN KOMPLEKS SERVİS!
///
/// SORUMLULUKLAR:
/// 1. CRUD İşlemleri (Master-Detail ilişkisi)
/// 2. Transaction yönetimi (Sale + SaleDetails + Stock Update)
/// 3. Otomatik SaleNumber oluşturma (S-2024-00001)
/// 4. Stok kontrolü ve güncelleme
/// 5. Fiyat hesaplamaları (indirim, kargo, toplam)
/// 6. Satış durumu yönetimi (Beklemede → Hazirlaniyor → Tamamlandi)
/// 7. Satış raporları ve analizleri
///
/// TRANSACTION KURALLARI:
/// CreateSale işlemi atomik olmalı:
/// 1. Sale kaydı oluşturulur
/// 2. Her SaleDetail için:
///    a. SaleDetail kaydı oluşturulur
///    b. Product.Stock -= Quantity
///    c. Stok negatife düşmemeli (kontrol!)
///    d. StockStatus güncellenmeli (Yeterli → Azaliyor → Tukendi)
/// 3. TotalAmount hesaplanır
/// 4. Hata olursa TÜM işlemler geri alınır (Rollback)
///
/// İŞ KURALLARI:
/// - IsPaid = true ise PaymentDate zorunlu
/// - Status = Tamamlandi ise IsPaid = true olmalı
/// - Status = Iptal ise stoklar geri eklenmeli
/// - SaleDetails en az 1 satır içermeli
/// </summary>
public interface ISaleService
{
    // ====== CRUD OPERATIONS ======

    /// <summary>
    /// Tüm satışları getirir
    /// </summary>
    /// <param name="includeDeleted">Silinmiş kayıtlar da dahil edilsin mi?</param>
    /// <returns>Satış özet listesi</returns>
    Task<IEnumerable<SaleSummaryDto>> GetAllSalesAsync(bool includeDeleted = false);

    /// <summary>
    /// ID'ye göre satış getirir (detaylar ile birlikte)
    /// </summary>
    /// <param name="id">Satış ID</param>
    /// <returns>Satış bilgileri + SaleDetails veya null</returns>
    Task<SaleDto?> GetSaleByIdAsync(int id);

    /// <summary>
    /// Satış numarasına göre getirir
    /// KULLANIM: Gül Satar: "S-2024-00156 numaralı satış"
    /// </summary>
    /// <param name="saleNumber">Satış numarası</param>
    /// <returns>Satış bilgileri veya null</returns>
    Task<SaleDto?> GetSaleByNumberAsync(string saleNumber);

    /// <summary>
    /// Yeni satış oluşturur
    ///
    /// TRANSACTION İŞLEMLERİ:
    /// BEGIN TRANSACTION
    ///   1. SaleNumber oluştur: S-{Yıl}-{5 haneli sıra}
    ///   2. Sale kaydı ekle (Status = Beklemede)
    ///   3. Her SaleDetail için:
    ///      a. Ürün var mı kontrol et
    ///      b. Stok yeterli mi kontrol et (Product.Stock >= Quantity)
    ///      c. SaleDetail kaydı ekle
    ///      d. Product.Stock -= Quantity
    ///      e. Product.StockStatus güncelle:
    ///         - Stock <= 0: Tukendi
    ///         - Stock <= CriticalStockLevel: Azaliyor
    ///         - Stock > CriticalStockLevel: Yeterli
    ///   4. TotalAmount hesapla:
    ///      - SaleDetails toplamı
    ///      + ShippingCost
    ///      - DiscountAmount
    ///   5. IsPaid = true ise PaymentDate = DateTime.Now
    /// COMMIT TRANSACTION
    ///
    /// HATA SENARYOLARI:
    /// - Stok yetersiz: "X ürünü için stok yetersiz (Mevcut: 5, İstenen: 10)"
    /// - Ürün bulunamadı: "Y ürünü bulunamadı"
    /// - Transaction hatası: Tüm işlemler geri alınır
    ///
    /// BİLDİRİM:
    /// - Durna Sabit'e bildirim (Depo): "Yeni sipariş hazırlanacak"
    /// - Stok azalıyorsa bildirim: "X ürünü kritik seviyeye düştü"
    /// </summary>
    /// <param name="createSaleDto">Oluşturulacak satış bilgileri</param>
    /// <returns>Oluşturulan satış bilgileri</returns>
    Task<SaleDto?> CreateSaleAsync(CreateSaleDto createSaleDto);

    /// <summary>
    /// Satış bilgilerini günceller (SINIRLI)
    ///
    /// DEĞİŞTİRİLEBİLİRLER:
    /// - Status
    /// - IsPaid / PaymentDate
    /// - Notes
    ///
    /// DEĞİŞTİRİLEMEZLER:
    /// - SaleNumber
    /// - SaleDetails (ürünler, miktarlar, fiyatlar)
    /// - CustomerId
    /// - TotalAmount
    ///
    /// NEDEN?
    /// - Muhasebe tutarlılığı
    /// - Stok tutarlılığı
    /// - Fatura kesilmiş olabilir
    ///
    /// ÖZEL KURALLAR:
    /// - Status = Iptal ise CancelSale metodunu kullan (stok iadesi için)
    /// - Status = Tamamlandi ise IsPaid = true olmalı
    /// </summary>
    /// <param name="updateSaleDto">Güncellenecek satış bilgileri</param>
    /// <returns>Güncellenen satış bilgileri veya null</returns>
    Task<SaleDto?> UpdateSaleAsync(UpdateSaleDto updateSaleDto);

    /// <summary>
    /// Satışı siler (Soft Delete)
    /// NOT: Gerçekte satış silinmez, iptal edilir (CancelSale kullanılmalı)
    /// </summary>
    /// <param name="id">Silinecek satış ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> DeleteSaleAsync(int id);


    // ====== BUSINESS LOGIC METHODS ======

    /// <summary>
    /// Mağazanın satışlarını getirir
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="status">Durum filtresi (opsiyonel)</param>
    /// <returns>Mağazanın satışları</returns>
    Task<IEnumerable<SaleSummaryDto>> GetSalesByStoreAsync(int storeId, DateTime? startDate = null, DateTime? endDate = null, SaleStatus? status = null);

    /// <summary>
    /// Müşterinin satışlarını getirir
    /// KULLANIM: Müşteri geçmişi, sadakat analizi
    /// </summary>
    /// <param name="customerId">Müşteri ID</param>
    /// <returns>Müşterinin satışları</returns>
    Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId);

    /// <summary>
    /// Çalışanın satışlarını getirir
    /// KULLANIM: Gül Satar'ın performans takibi, prim hesabı
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <returns>Çalışanın satışları</returns>
    Task<IEnumerable<SaleSummaryDto>> GetSalesByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Bekleyen siparişleri getirir (Status = Beklemede veya Hazirlaniyor)
    /// KULLANIM: Durna Sabit'in (Depo) iş listesi
    /// </summary>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Bekleyen siparişler</returns>
    Task<IEnumerable<SaleSummaryDto>> GetPendingSalesAsync(int? storeId = null);

    /// <summary>
    /// Ödenmemiş satışları getirir (IsPaid = false)
    /// KULLANIM: Feyza Paragöz'ün (Muhasebe) tahsilat takibi
    /// </summary>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Ödenmemiş satışlar</returns>
    Task<IEnumerable<SaleSummaryDto>> GetUnpaidSalesAsync(int? storeId = null);

    /// <summary>
    /// Satışı iptal eder ve stokları geri ekler
    ///
    /// TRANSACTION İŞLEMLERİ:
    /// BEGIN TRANSACTION
    ///   1. Sale.Status = Iptal
    ///   2. Her SaleDetail için:
    ///      a. Product.Stock += Quantity (stok iadesi)
    ///      b. Product.StockStatus güncelle
    ///   3. Sale.Notes'a iptal sebebi ekle
    /// COMMIT TRANSACTION
    ///
    /// KURALLER:
    /// - Sadece Status = Beklemede veya Hazirlaniyor olanlar iptal edilebilir
    /// - Status = Tamamlandi olanlar iptal edilemez (iade işlemi başlatılmalı)
    /// </summary>
    /// <param name="saleId">Satış ID</param>
    /// <param name="cancelReason">İptal sebebi</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> CancelSaleAsync(int saleId, string cancelReason);

    /// <summary>
    /// Satışı ödendi olarak işaretler
    ///
    /// GÜNCELLENİR:
    /// - IsPaid = true
    /// - PaymentDate = DateTime.Now
    /// - Status = Hazirlaniyor (eğer Beklemede ise)
    ///
    /// BİLDİRİM:
    /// - Durna Sabit'e bildirim: "Sipariş hazırlanabilir"
    /// </summary>
    /// <param name="saleId">Satış ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> MarkSaleAsPaidAsync(int saleId);

    /// <summary>
    /// Satışı tamamlandı olarak işaretler
    ///
    /// KURALLER:
    /// - IsPaid = true olmalı
    /// - Status = Hazirlaniyor olmalı
    ///
    /// GÜNCELLENİR:
    /// - Status = Tamamlandi
    ///
    /// BİLDİRİM:
    /// - Müşteriye teslimat bildirimi (SMS/Email)
    /// </summary>
    /// <param name="saleId">Satış ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> CompleteSaleAsync(int saleId);

    /// <summary>
    /// Satış toplamını hesaplar (doğrulama için)
    /// </summary>
    /// <param name="saleDetails">Satış detayları</param>
    /// <param name="shippingCost">Kargo ücreti</param>
    /// <param name="discountAmount">İndirim tutarı</param>
    /// <returns>Toplam tutar</returns>
    decimal CalculateTotalAmount(List<CreateSaleDetailDto> saleDetails, decimal shippingCost, decimal discountAmount);

    /// <summary>
    /// Günlük satış raporu
    /// KULLANIM: Gül Satar ve Haluk Bey'in günlük özeti
    ///
    /// İÇERİK:
    /// - Toplam satış adedi
    /// - Toplam satış tutarı
    /// - Ortalama sepet tutarı
    /// - En çok satan ürünler (top 5)
    /// - Ödeme yöntemlerine göre dağılım
    /// - Saatlik satış grafiği
    /// </summary>
    /// <param name="date">Tarih</param>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Günlük satış raporu</returns>
    Task<object> GetDailySalesReportAsync(DateTime date, int? storeId = null);

    /// <summary>
    /// Aylık satış raporu
    /// KULLANIM: Haluk Bey'in aylık değerlendirmesi
    ///
    /// İÇERİK:
    /// - Aylık satış trendi (günlük grafik)
    /// - Kategori bazında satış dağılımı
    /// - Mağaza bazında karşılaştırma
    /// - Çalışan performansları
    /// - Hedef/Gerçekleşme karşılaştırması
    /// </summary>
    /// <param name="year">Yıl</param>
    /// <param name="month">Ay</param>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Aylık satış raporu</returns>
    Task<object> GetMonthlySalesReportAsync(int year, int month, int? storeId = null);

    /// <summary>
    /// En çok satan ürünler raporu
    /// KULLANIM: Haluk Bey: "Hangi ürünler çok satıyor?"
    /// </summary>
    /// <param name="topCount">Kaç ürün (default: 10)</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>En çok satan ürünler listesi</returns>
    Task<object> GetTopSellingProductsAsync(int topCount, DateTime startDate, DateTime endDate, int? storeId = null);

    /// <summary>
    /// Çalışan satış performansı
    /// KULLANIM: Prim hesabı, performans değerlendirmesi
    ///
    /// İÇERİK:
    /// - Toplam satış tutarı
    /// - Satış kotası
    /// - Gerçekleşme oranı
    /// - Ortalama sepet tutarı
    /// - Müşteri sayısı
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Çalışan performans raporu</returns>
    Task<object> GetEmployeeSalesPerformanceAsync(int employeeId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Mağaza toplam satışını hesaplar (belirli tarih aralığında)
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Toplam satış tutarı (TL)</returns>
    Task<decimal> GetTotalSalesAmountAsync(int storeId, DateTime startDate, DateTime endDate);
}
