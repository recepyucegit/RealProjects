// ===================================================================================
// TEKNOROMA - ORTAK DTO DOSYASI (CommonDTOs.cs)
// ===================================================================================
//
// DTO (Data Transfer Object) NEDİR?
// ----------------------------------
// DTO, "Veri Transfer Nesnesi" anlamına gelir. Katmanlar arasında veri taşımak için
// kullanılan basit sınıflardır. DTO'lar:
//
// 1. Entity'lerden farklıdır: Entity'ler veritabanı tablolarını temsil ederken,
//    DTO'lar sadece veri taşıma amacı güder.
//
// 2. Güvenlik sağlar: Hassas bilgileri (şifreler, dahili ID'ler) dışarıya vermez.
//
// 3. Performans artırır: Sadece gerekli alanları taşır, gereksiz veri transferi olmaz.
//
// 4. Esneklik sağlar: Entity yapısı değişse bile API yanıtları aynı kalabilir.
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, tüm uygulama genelinde kullanılan ORTAK DTO'ları içerir:
// - Sayfalama (pagination) için genel yapılar
// - API yanıt formatları
// - Arama/filtreleme parametreleri
// - Dashboard istatistikleri
//
// Bu DTO'lar "generic" yapıdadır, yani herhangi bir veri tipiyle kullanılabilir.
//
// İLİŞKİLİ SERVİSLER
// ------------------
// Bu DTO'lar tüm servisler tarafından kullanılır:
// - IProductService, ISaleService vb. -> PaginatedResult<T> döner
// - Tüm Controller'lar -> ApiResponse<T> kullanır
// - Dashboard servisi -> DashboardSummaryDto kullanır
//
// ÖRNEK KULLANIM
// --------------
// Controller'da:
//   var products = await _productService.GetAllAsync(new PaginationRequest { PageNumber = 1, PageSize = 20 });
//   return ApiResponse<PaginatedResult<ProductDto>>.SuccessResponse(products, "Ürünler listelendi");
//
// ===================================================================================

namespace TeknoRoma.Application.DTOs;

#region SAYFALAMA (PAGINATION) DTO'LARI

/// <summary>
/// Sayfalanmış sonuç sarmalayıcısı (Generic Paginated Result Wrapper).
///
/// AÇIKLAMA:
/// ---------
/// Büyük veri setlerini sayfalara bölerek döndürmek için kullanılır.
/// Generic yapıda olduğu için her türlü entity listesi ile çalışabilir.
///
/// NEDEN KULLANILIR?
/// ----------------
/// - Performans: 10.000 ürün yerine sadece 20 ürün döner
/// - Kullanıcı deneyimi: Sayfa sayfa gezinme imkanı
/// - Bellek tasarrufu: Hem sunucu hem istemci tarafında
///
/// UI KULLANIMI:
/// -------------
/// - Items: DataGrid veya liste kontrolüne bağlanır
/// - TotalCount: "Toplam X kayıt" şeklinde gösterilir
/// - HasPreviousPage/HasNextPage: Önceki/Sonraki butonlarını aktif/pasif yapar
/// - TotalPages: Sayfa numaraları için kullanılır
///
/// ÖRNEK:
/// ------
/// var result = new PaginatedResult&lt;ProductDto&gt; {
///     Items = productList,
///     TotalCount = 150,
///     PageNumber = 2,
///     PageSize = 20
/// };
/// // TotalPages = 8, HasPreviousPage = true, HasNextPage = true
/// </summary>
/// <typeparam name="T">Sayfalanacak öğelerin tipi (örn: ProductDto, CustomerDto)</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Mevcut sayfadaki öğe listesi.
    /// UI'da DataGrid, ListView veya Card listesi olarak gösterilir.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Toplam kayıt sayısı (tüm sayfalardaki).
    /// UI'da "150 kayıt bulundu" şeklinde bilgi olarak gösterilir.
    /// Sayfa hesaplamaları için kullanılır.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Mevcut sayfa numarası (1'den başlar).
    /// UI'da aktif sayfa olarak vurgulanır.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Sayfa başına kayıt sayısı.
    /// Genellikle 10, 20, 50 veya 100 olarak ayarlanır.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Toplam sayfa sayısı (hesaplanmış - computed property).
    /// TotalCount / PageSize formülüyle hesaplanır, yukarı yuvarlanır.
    /// UI'da sayfa numaraları gösterilirken kullanılır.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Önceki sayfa var mı? (1. sayfada değilsek true).
    /// UI'da "Önceki" butonunun aktif/pasif durumunu belirler.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Sonraki sayfa var mı? (son sayfada değilsek true).
    /// UI'da "Sonraki" butonunun aktif/pasif durumunu belirler.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

#endregion

#region API YANIT DTO'LARI

/// <summary>
/// Genel API yanıt sarmalayıcısı (Generic API Response Wrapper).
///
/// AÇIKLAMA:
/// ---------
/// Tüm API yanıtlarını standart bir formatta döndürmek için kullanılır.
/// Frontend geliştiriciler her zaman aynı yapıyı bekleyebilir.
///
/// NEDEN KULLANILIR?
/// ----------------
/// - Tutarlılık: Her endpoint aynı formatta yanıt verir
/// - Hata yönetimi: Success=false ile hatalar standart şekilde bildirilir
/// - Ek bilgi: Message alanı ile kullanıcıya bilgi verilebilir
///
/// UI KULLANIMI:
/// -------------
/// - Success: İşlem başarılı mı kontrolü
/// - Data: Asıl veri (ürün, müşteri listesi vb.)
/// - Message: Toast/Snackbar mesajı olarak gösterilir
/// - Errors: Validasyon hatalarını liste olarak gösterir
///
/// ÖRNEK:
/// ------
/// // Başarılı yanıt
/// return ApiResponse&lt;ProductDto&gt;.SuccessResponse(product, "Ürün kaydedildi");
///
/// // Hata yanıtı
/// return ApiResponse&lt;ProductDto&gt;.ErrorResponse("Ürün bulunamadı", new List&lt;string&gt; { "ID geçersiz" });
/// </summary>
/// <typeparam name="T">Döndürülecek verinin tipi</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// İşlem başarılı mı?
    /// true: İşlem başarıyla tamamlandı, Data alanında sonuç var
    /// false: Hata oluştu, Message ve Errors alanlarını kontrol et
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Kullanıcıya gösterilecek mesaj.
    /// Başarı: "Ürün başarıyla kaydedildi"
    /// Hata: "Ürün kaydedilemedi"
    /// UI'da toast/snackbar olarak gösterilir.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Döndürülen veri.
    /// Success=true ise dolu, Success=false ise null olabilir.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Hata listesi.
    /// Validasyon hataları: ["Ad alanı zorunludur", "Fiyat 0'dan büyük olmalı"]
    /// UI'da hata listesi olarak gösterilir.
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Başarılı yanıt oluşturur (Factory Method Pattern).
    ///
    /// KULLANIM:
    /// return ApiResponse&lt;ProductDto&gt;.SuccessResponse(product, "Ürün kaydedildi");
    /// </summary>
    /// <param name="data">Döndürülecek veri</param>
    /// <param name="message">Opsiyonel başarı mesajı</param>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    /// <summary>
    /// Hata yanıtı oluşturur (Factory Method Pattern).
    ///
    /// KULLANIM:
    /// return ApiResponse&lt;ProductDto&gt;.ErrorResponse("İşlem başarısız", errors);
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errors">Opsiyonel hata detayları listesi</param>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}

/// <summary>
/// Veri döndürmeyen işlemler için basit sonuç sınıfı.
///
/// AÇIKLAMA:
/// ---------
/// Delete, Update gibi veri döndürmeye gerek olmayan işlemler için kullanılır.
/// ApiResponse&lt;T&gt;'nin data'sız versiyonudur.
///
/// ÖRNEK KULLANIM:
/// --------------
/// // Silme işlemi
/// public async Task&lt;OperationResult&gt; DeleteProductAsync(int id) {
///     if (product == null)
///         return OperationResult.ErrorResult("Ürün bulunamadı");
///     await _repository.DeleteAsync(product);
///     return OperationResult.SuccessResult("Ürün silindi");
/// }
/// </summary>
public class OperationResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// İşlem sonucu mesajı.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Varsa hata detayları.
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Başarılı sonuç oluşturur.
    /// </summary>
    public static OperationResult SuccessResult(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    /// <summary>
    /// Hata sonucu oluşturur.
    /// </summary>
    public static OperationResult ErrorResult(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}

#endregion

#region SAYFALAMA VE ARAMA İSTEK DTO'LARI

/// <summary>
/// Sayfalama isteği parametreleri.
///
/// AÇIKLAMA:
/// ---------
/// API'ye yapılan liste isteklerinde sayfalama bilgilerini taşır.
/// Controller'dan Service katmanına geçirilir.
///
/// GÜVENLİK:
/// ---------
/// - PageNumber < 1 ise otomatik 1 yapılır (negatif sayfa yok)
/// - PageSize 1-100 arasında sınırlandırılmıştır (aşırı yük engellenir)
///
/// ÖRNEK İSTEK:
/// ------------
/// GET /api/products?PageNumber=2&PageSize=20&SortBy=ProductName&SortDescending=false
/// </summary>
public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    /// <summary>
    /// İstenen sayfa numarası (1'den başlar).
    /// Minimum 1 olarak sınırlandırılmıştır.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Sayfa başına kayıt sayısı (varsayılan: 10).
    /// 1-100 arasında sınırlandırılmıştır.
    /// Performans için 100'den fazla kayıt tek seferde çekilemez.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
    }

    /// <summary>
    /// Sıralama yapılacak alan adı.
    /// Örnek: "ProductName", "CreatedDate", "Price"
    /// UI'daki tablo başlıklarına tıklandığında değişir.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Azalan sıralama mı? (true: Z-A veya 9-0, false: A-Z veya 0-9)
    /// UI'daki sıralama okuna göre değişir.
    /// </summary>
    public bool SortDescending { get; set; }
}

/// <summary>
/// Arama/filtreleme parametreleri (PaginationRequest'i genişletir).
///
/// AÇIKLAMA:
/// ---------
/// Sayfalama + arama + tarih aralığı filtreleme yeteneklerini birleştirir.
/// PaginationRequest'ten kalıtım alarak sayfalama özelliklerini de içerir.
///
/// ÖRNEK İSTEK:
/// ------------
/// GET /api/sales?SearchTerm=Ahmet&StartDate=2024-01-01&EndDate=2024-12-31&PageNumber=1&PageSize=20
/// </summary>
public class SearchRequest : PaginationRequest
{
    /// <summary>
    /// Arama terimi.
    /// İsim, barkod, açıklama gibi metin alanlarında arama yapar.
    /// UI'daki arama kutusundan gelir.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filtreleme başlangıç tarihi.
    /// Bu tarihten sonraki kayıtlar getirilir.
    /// UI'daki "Başlangıç Tarihi" date picker'dan gelir.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filtreleme bitiş tarihi.
    /// Bu tarihten önceki kayıtlar getirilir.
    /// UI'daki "Bitiş Tarihi" date picker'dan gelir.
    /// </summary>
    public DateTime? EndDate { get; set; }
}

#endregion

#region YARDIMCI DTO'LAR

/// <summary>
/// Tarih aralığı bilgisi.
///
/// AÇIKLAMA:
/// ---------
/// Raporlama için tarih aralığı belirlemede kullanılır.
/// Özellikle satış, gider ve performans raporlarında kullanılır.
///
/// ÖRNEK:
/// ------
/// var range = new DateRangeDto { StartDate = DateTime.Today.AddMonths(-1), EndDate = DateTime.Today };
/// var report = await _reportService.GetSalesReport(range);
/// </summary>
public class DateRangeDto
{
    /// <summary>
    /// Raporun başlangıç tarihi.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Raporun bitiş tarihi.
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Dropdown/ComboBox listesi için öğe.
///
/// AÇIKLAMA:
/// ---------
/// UI'daki açılır listeleri (dropdown, combobox) doldurmak için kullanılır.
/// Her türlü seçim listesi için generic bir yapı sağlar.
///
/// UI KULLANIMI:
/// -------------
/// - Id: Seçilen değer (value)
/// - Text: Görüntülenen metin
/// - IsSelected: Varsayılan seçili mi?
///
/// ÖRNEK:
/// ------
/// // Kategori dropdown'ı için
/// var categories = await _categoryService.GetSelectListAsync();
/// // Dönen veri: [{ Id: 1, Text: "Bilgisayar" }, { Id: 2, Text: "Telefon" }]
/// </summary>
public class SelectItemDto
{
    /// <summary>
    /// Öğenin benzersiz ID'si.
    /// Form submit edildiğinde bu değer gönderilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kullanıcıya gösterilen metin.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Varsayılan olarak seçili mi?
    /// Edit formlarında mevcut değeri seçili göstermek için.
    /// </summary>
    public bool IsSelected { get; set; }
}

/// <summary>
/// Dashboard istatistik kartı için öğe.
///
/// AÇIKLAMA:
/// ---------
/// Dashboard'daki istatistik kartlarını (KPI kartları) doldurmak için kullanılır.
/// Değer + değişim yüzdesi + pozitif/negatif trend bilgisi içerir.
///
/// UI KULLANIMI:
/// -------------
/// - Label: Kart başlığı ("Günlük Satış", "Toplam Müşteri")
/// - Value: Sayısal değer (1500.00)
/// - FormattedValue: Formatlanmış değer ("1.500,00 TL")
/// - ChangePercent: Değişim yüzdesi (15.5)
/// - IsPositiveChange: Yeşil yukarı ok / Kırmızı aşağı ok
///
/// ÖRNEK:
/// ------
/// new StatisticItemDto {
///     Label = "Günlük Satış",
///     Value = 15000,
///     FormattedValue = "15.000 TL",
///     ChangePercent = 12.5m,
///     IsPositiveChange = true  // Yeşil ok
/// }
/// </summary>
public class StatisticItemDto
{
    /// <summary>
    /// İstatistiğin etiketi/başlığı.
    /// Örnek: "Günlük Satış", "Aylık Gelir", "Stok Değeri"
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// Sayısal değer.
    /// Hesaplamalar için ham değer olarak kullanılır.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Formatlanmış değer (UI'da görüntülenen).
    /// Örnek: "15.000,00 TL", "%85", "1.250 adet"
    /// </summary>
    public string? FormattedValue { get; set; }

    /// <summary>
    /// Önceki döneme göre değişim yüzdesi.
    /// Örnek: 15.5 = %15.5 artış, -8.2 = %8.2 düşüş
    /// </summary>
    public decimal? ChangePercent { get; set; }

    /// <summary>
    /// Değişim pozitif mi?
    /// true: Yeşil yukarı ok (artış)
    /// false: Kırmızı aşağı ok (düşüş)
    /// </summary>
    public bool IsPositiveChange { get; set; }
}

#endregion

#region DASHBOARD DTO'LARI

/// <summary>
/// Ana dashboard özet bilgileri.
///
/// AÇIKLAMA:
/// ---------
/// Uygulamanın ana sayfasındaki dashboard'u doldurmak için kullanılır.
/// Tüm önemli metrikleri tek bir API çağrısıyla getirir.
///
/// UI KULLANIMI:
/// -------------
/// Dashboard sayfasında farklı kartlar ve grafikler bu verilerle doldurulur:
/// - KPI kartları: Günlük/aylık satış, müşteri sayısı vb.
/// - Uyarı kartları: Kritik stok, bekleyen servisler
/// - Grafik: Son 7 günlük satış trendi
/// - Tablo: En çok satan ürünler
///
/// PERFORMANS:
/// -----------
/// Tek API çağrısıyla tüm dashboard verileri gelir.
/// Multiple API çağrısı yapmak yerine bu yaklaşım tercih edilir.
///
/// ÖRNEK KULLANIM:
/// ---------------
/// // Controller'da
/// var dashboard = await _dashboardService.GetSummaryAsync();
///
/// // Vue/React'ta
/// &lt;StatCard title="Günlük Satış" :value="dashboard.todaySales" /&gt;
/// </summary>
public class DashboardSummaryDto
{
    /// <summary>
    /// Bugünkü toplam satış tutarı.
    /// KPI kartında "Bugünkü Satış: 15.000 TL" şeklinde gösterilir.
    /// </summary>
    public decimal TodaySales { get; set; }

    /// <summary>
    /// Bu ayki toplam satış tutarı.
    /// KPI kartında "Aylık Satış: 450.000 TL" şeklinde gösterilir.
    /// </summary>
    public decimal MonthlySales { get; set; }

    /// <summary>
    /// Toplam müşteri sayısı.
    /// KPI kartında "Toplam Müşteri: 1.250" şeklinde gösterilir.
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// Toplam ürün çeşidi sayısı.
    /// KPI kartında "Ürün Çeşidi: 580" şeklinde gösterilir.
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Kritik stok seviyesindeki ürün sayısı.
    /// Uyarı kartında kırmızı ikon ile "5 ürün kritik stokta!" şeklinde gösterilir.
    /// Tıklandığında düşük stok listesine yönlendirir.
    /// </summary>
    public int LowStockProducts { get; set; }

    /// <summary>
    /// Bekleyen teknik servis kayıt sayısı.
    /// Uyarı kartında "12 bekleyen servis" şeklinde gösterilir.
    /// </summary>
    public int PendingTechnicalServices { get; set; }

    /// <summary>
    /// Bu ayki toplam gider tutarı.
    /// KPI kartında "Aylık Gider: 180.000 TL" şeklinde gösterilir.
    /// </summary>
    public decimal MonthlyExpenses { get; set; }

    /// <summary>
    /// Net kar (Aylık Satış - Aylık Gider).
    /// KPI kartında yeşil/kırmızı renkle "Net Kar: 270.000 TL" şeklinde gösterilir.
    /// </summary>
    public decimal NetProfit { get; set; }

    /// <summary>
    /// Son 7 günlük satış özeti (grafik verisi).
    /// Çizgi grafiği veya bar chart ile görselleştirilir.
    /// Her gün için tarih, satış sayısı ve toplam tutar içerir.
    /// </summary>
    public List<DailySalesSummaryDto> Last7DaysSales { get; set; } = new();

    /// <summary>
    /// En çok satan ürünler listesi.
    /// "Top 5 Ürün" tablosu veya listesi olarak gösterilir.
    /// </summary>
    public List<StatisticItemDto> TopSellingProducts { get; set; } = new();
}

#endregion
