using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Döviz Kuru Servisi Interface
///
/// KULLANIM SENARYOLARI:
/// 1. Gül Satar: "Müşterilerime güncel döviz kuru üzerinden ürünün dolar ve euro fiyatını söyleyebilmeliyim"
/// 2. Fahri Cepçi: "Bazı meraklı müşterilerime fiyat bilgisini Dolar ve Euro olarak sunabilmeliyim"
/// 3. Feyza Paragöz: "10.10.2006 tarihindeki döviz kurunu da görebilmek istiyorum"
///
/// VERİ KAYNAKLARI:
/// - TCMB (Türkiye Cumhuriyet Merkez Bankası) XML API
/// - Yedek: ExchangeRate-API veya Fixer.io
///
/// ÖNEMLİ:
/// - Güncel kur için: GetCurrentRateAsync
/// - Geçmiş kur için: GetHistoricalRateAsync (Muhasebe raporları için)
/// - Cache mekanizması: Aynı gün için tekrar API'ye gitme
/// </summary>
public interface IExchangeRateService
{
    /// <summary>
    /// Güncel döviz kurunu getirir
    /// Gül Satar ve Fahri Cepçi satış anında kullanır
    /// </summary>
    /// <param name="currency">Para birimi (USD, EUR)</param>
    /// <returns>1 birim döviz = X TL</returns>
    Task<decimal> GetCurrentRateAsync(Currency currency);

    /// <summary>
    /// Geçmiş tarihli döviz kurunu getirir
    /// Feyza Paragöz muhasebe raporlarında kullanır
    /// </summary>
    /// <param name="currency">Para birimi</param>
    /// <param name="date">Tarih</param>
    /// <returns>O tarihteki kur</returns>
    Task<decimal> GetHistoricalRateAsync(Currency currency, DateTime date);

    /// <summary>
    /// TL tutarını dövize çevirir
    /// </summary>
    /// <param name="amountInTRY">TL tutarı</param>
    /// <param name="targetCurrency">Hedef para birimi</param>
    /// <returns>Döviz tutarı</returns>
    Task<decimal> ConvertFromTRYAsync(decimal amountInTRY, Currency targetCurrency);

    /// <summary>
    /// Döviz tutarını TL'ye çevirir
    /// </summary>
    /// <param name="amount">Döviz tutarı</param>
    /// <param name="sourceCurrency">Kaynak para birimi</param>
    /// <returns>TL tutarı</returns>
    Task<decimal> ConvertToTRYAsync(decimal amount, Currency sourceCurrency);

    /// <summary>
    /// Ürün fiyatını tüm para birimlerinde gösterir
    /// Satış ekranında kullanılır
    /// </summary>
    /// <param name="priceInTRY">TL fiyatı</param>
    /// <returns>TRY, USD, EUR fiyatları</returns>
    Task<ProductPriceInCurrenciesDto> GetPriceInAllCurrenciesAsync(decimal priceInTRY);

    /// <summary>
    /// Güncel tüm kurları getirir (Dashboard için)
    /// </summary>
    Task<ExchangeRatesDto> GetAllCurrentRatesAsync();
}

/// <summary>
/// Ürün Fiyatı Tüm Para Birimlerinde
/// </summary>
public class ProductPriceInCurrenciesDto
{
    public decimal PriceInTRY { get; set; }
    public decimal PriceInUSD { get; set; }
    public decimal PriceInEUR { get; set; }
    public decimal USDRate { get; set; }
    public decimal EURRate { get; set; }
    public DateTime RateDate { get; set; }
}

/// <summary>
/// Tüm Döviz Kurları
/// </summary>
public class ExchangeRatesDto
{
    public decimal USDRate { get; set; }
    public decimal EURRate { get; set; }
    public decimal GBPRate { get; set; }
    public DateTime LastUpdated { get; set; }
}
