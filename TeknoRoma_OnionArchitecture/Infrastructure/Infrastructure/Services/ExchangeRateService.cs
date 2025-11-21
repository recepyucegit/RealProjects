using Application.Services;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Infrastructure.Services;

/// <summary>
/// Döviz Kuru Servisi Implementasyonu
///
/// VERİ KAYNAĞLARI:
/// 1. TCMB XML API (Birincil)
/// 2. Statik değerler (API erişilemezse)
///
/// CACHE:
/// - IMemoryCache kullanılır
/// - Aynı gün için tekrar API'ye gidilmez
/// - Cache süresi: 1 saat
///
/// TCMB API:
/// - URL: https://www.tcmb.gov.tr/kurlar/today.xml
/// - Günlük güncellenir (hafta içi)
/// - Hafta sonu için Cuma kurları geçerli
/// </summary>
public class ExchangeRateService : IExchangeRateService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly HttpClient _httpClient;

    // TCMB API URL'leri
    private const string TCMB_TODAY_URL = "https://www.tcmb.gov.tr/kurlar/today.xml";
    private const string TCMB_HISTORY_URL = "https://www.tcmb.gov.tr/kurlar/{0}/{1}.xml"; // YYYYMM/DDMMYYYY

    // Cache key'leri
    private const string CACHE_KEY_TODAY = "ExchangeRates_Today";
    private const string CACHE_KEY_DATE = "ExchangeRates_{0}"; // Tarih bazlı

    // Fallback değerleri (API erişilemezse)
    private static readonly Dictionary<Currency, decimal> FallbackRates = new()
    {
        { Currency.USD, 34.50m },
        { Currency.EUR, 37.20m },
        { Currency.TRY, 1m }
    };

    public ExchangeRateService(IMemoryCache cache, ILogger<ExchangeRateService> logger)
    {
        _cache = cache;
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<decimal> GetCurrentRateAsync(Currency currency)
    {
        if (currency == Currency.TRY)
            return 1m;

        try
        {
            var rates = await GetAllCurrentRatesAsync();
            return currency switch
            {
                Currency.USD => rates.USDRate,
                Currency.EUR => rates.EURRate,
                _ => 1m
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Döviz kuru alınamadı, fallback kullanılıyor");
            return FallbackRates.GetValueOrDefault(currency, 1m);
        }
    }

    public async Task<decimal> GetHistoricalRateAsync(Currency currency, DateTime date)
    {
        if (currency == Currency.TRY)
            return 1m;

        // Bugünse güncel kuru getir
        if (date.Date == DateTime.Today)
            return await GetCurrentRateAsync(currency);

        var cacheKey = string.Format(CACHE_KEY_DATE, date.ToString("yyyyMMdd"));

        if (_cache.TryGetValue(cacheKey, out ExchangeRatesDto? cachedRates) && cachedRates != null)
        {
            return currency == Currency.USD ? cachedRates.USDRate : cachedRates.EURRate;
        }

        try
        {
            // TCMB geçmiş kur URL'i oluştur
            var yearMonth = date.ToString("yyyyMM");
            var dayMonthYear = date.ToString("ddMMyyyy");
            var url = string.Format(TCMB_HISTORY_URL, yearMonth, dayMonthYear);

            var rates = await FetchRatesFromTcmbAsync(url);

            // Cache'e kaydet (7 gün)
            _cache.Set(cacheKey, rates, TimeSpan.FromDays(7));

            return currency == Currency.USD ? rates.USDRate : rates.EURRate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Geçmiş döviz kuru alınamadı: {Date}", date);
            return FallbackRates.GetValueOrDefault(currency, 1m);
        }
    }

    public async Task<decimal> ConvertFromTRYAsync(decimal amountInTRY, Currency targetCurrency)
    {
        if (targetCurrency == Currency.TRY)
            return amountInTRY;

        var rate = await GetCurrentRateAsync(targetCurrency);
        return Math.Round(amountInTRY / rate, 2);
    }

    public async Task<decimal> ConvertToTRYAsync(decimal amount, Currency sourceCurrency)
    {
        if (sourceCurrency == Currency.TRY)
            return amount;

        var rate = await GetCurrentRateAsync(sourceCurrency);
        return Math.Round(amount * rate, 2);
    }

    public async Task<ProductPriceInCurrenciesDto> GetPriceInAllCurrenciesAsync(decimal priceInTRY)
    {
        var rates = await GetAllCurrentRatesAsync();

        return new ProductPriceInCurrenciesDto
        {
            PriceInTRY = priceInTRY,
            PriceInUSD = Math.Round(priceInTRY / rates.USDRate, 2),
            PriceInEUR = Math.Round(priceInTRY / rates.EURRate, 2),
            USDRate = rates.USDRate,
            EURRate = rates.EURRate,
            RateDate = rates.LastUpdated
        };
    }

    public async Task<ExchangeRatesDto> GetAllCurrentRatesAsync()
    {
        // Cache'den kontrol et
        if (_cache.TryGetValue(CACHE_KEY_TODAY, out ExchangeRatesDto? cachedRates) && cachedRates != null)
        {
            return cachedRates;
        }

        try
        {
            var rates = await FetchRatesFromTcmbAsync(TCMB_TODAY_URL);

            // 1 saat cache'le
            _cache.Set(CACHE_KEY_TODAY, rates, TimeSpan.FromHours(1));

            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TCMB'den kur alınamadı, fallback kullanılıyor");

            // Fallback değerleri
            return new ExchangeRatesDto
            {
                USDRate = FallbackRates[Currency.USD],
                EURRate = FallbackRates[Currency.EUR],
                GBPRate = 43.50m, // Fallback GBP
                LastUpdated = DateTime.Now
            };
        }
    }

    /// <summary>
    /// TCMB XML API'sinden kurları çeker
    /// </summary>
    private async Task<ExchangeRatesDto> FetchRatesFromTcmbAsync(string url)
    {
        _logger.LogInformation("TCMB'den döviz kuru alınıyor: {Url}", url);

        var response = await _httpClient.GetStringAsync(url);
        var xml = XDocument.Parse(response);

        decimal usdRate = 1m, eurRate = 1m, gbpRate = 1m;

        var currencies = xml.Descendants("Currency");

        foreach (var currency in currencies)
        {
            var code = currency.Attribute("Kod")?.Value;
            var forexBuying = currency.Element("ForexBuying")?.Value;

            if (!string.IsNullOrEmpty(forexBuying) && decimal.TryParse(forexBuying.Replace(".", ","), out decimal rate))
            {
                switch (code)
                {
                    case "USD":
                        usdRate = rate;
                        break;
                    case "EUR":
                        eurRate = rate;
                        break;
                    case "GBP":
                        gbpRate = rate;
                        break;
                }
            }
        }

        _logger.LogInformation("Döviz kurları alındı - USD: {USD}, EUR: {EUR}, GBP: {GBP}",
            usdRate, eurRate, gbpRate);

        return new ExchangeRatesDto
        {
            USDRate = usdRate,
            EURRate = eurRate,
            GBPRate = gbpRate,
            LastUpdated = DateTime.Now
        };
    }
}
