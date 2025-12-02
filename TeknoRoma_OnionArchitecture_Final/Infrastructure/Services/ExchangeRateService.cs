// ===================================================================================
// TEKNOROMA - DOVIZ KURU SERVISI (ExchangeRateService.cs)
// ===================================================================================
//
// Bu dosya doviz kuru islemleri icin Infrastructure katmani implementasyonudur.
// TCMB (Turkiye Cumhuriyet Merkez Bankasi) API'si ile entegrasyon saglar.
//
// ONION ARCHITECTURE PRENSIBI:
// - Application katmaninda INTERFACE (IExchangeRateService) tanimlandi
// - Infrastructure katmaninda IMPLEMENTASYON (bu dosya) yer alir
// - Dis bagimliliklklar (HTTP, API) her zaman Infrastructure'da olmali
//
// TCMB API BILGISI:
// URL: https://www.tcmb.gov.tr/kurlar/today.xml
// Format: XML
// Guncelleme: Her is gunu saat 15:30'da
// Hafta sonu: Cuma gunu kurlari gecerli
//
// CACHE STRATEJISI:
// - Kurlar gunluk degisir, surekli API cagrisi gereksiz
// - MemoryCache ile 1 saatlik cache
// - TCMB erisim hatalarinda son bilinen kur kullanilir
//
// ===================================================================================

using Application.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Xml.Linq;

namespace Infrastructure.Services
{
    /// <summary>
    /// Doviz Kuru Servisi Implementasyonu
    ///
    /// TCMB ENTEGRASYONU:
    /// Turkiye Cumhuriyet Merkez Bankasi'ndan guncel kurlari ceker
    ///
    /// KULLANIM ALANLARI:
    /// - Tedarikci odemeleri (USD/EUR ile alim)
    /// - Fiyat karsilastirma
    /// - Coklu para birimi destegi
    /// </summary>
    public class ExchangeRateService : IExchangeRateService
    {
        // =========================================================================
        // BAGIMLILIKLAR (Dependencies)
        // =========================================================================

        /// <summary>
        /// HTTP istemcisi - TCMB API cagrilari icin
        ///
        /// BEST PRACTICE:
        /// HttpClient singleton olarak kullanilmali (socket exhaustion onleme)
        /// IHttpClientFactory ile inject edilmesi onerililr
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Bellek cache'i - Kurlari gecici saklamak icin
        ///
        /// PERFORMANS:
        /// Her doviz sorgusu icin API cagrisi yapmak yerine
        /// cache'den okuma yapariz (1 saat gecerli)
        /// </summary>
        private readonly IMemoryCache _cache;

        // =========================================================================
        // SABITLER (Constants)
        // =========================================================================

        /// <summary>
        /// TCMB gunluk kur XML adresi
        /// Her is gunu 15:30'da guncellenir
        /// </summary>
        private const string TcmbUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";

        /// <summary>
        /// Cache anahtari - USD kuru icin
        /// </summary>
        private const string UsdCacheKey = "ExchangeRate_USD";

        /// <summary>
        /// Cache anahtari - EUR kuru icin
        /// </summary>
        private const string EurCacheKey = "ExchangeRate_EUR";

        /// <summary>
        /// Cache suresi - 1 saat
        /// Kurlar gunluk degistigi icin 1 saat makul bir sure
        /// </summary>
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        /// <summary>
        /// Fallback USD kuru - TCMB erisilemezse kullanilir
        /// UYARI: Bu deger guncellenmeli veya veritabanindan alinmali
        /// </summary>
        private const decimal FallbackUsdRate = 32.50m;

        /// <summary>
        /// Fallback EUR kuru - TCMB erisilemezse kullanilir
        /// </summary>
        private const decimal FallbackEurRate = 35.50m;

        // =========================================================================
        // CONSTRUCTOR
        // =========================================================================

        /// <summary>
        /// ExchangeRateService constructor
        ///
        /// DEPENDENCY INJECTION:
        /// HttpClient ve IMemoryCache DI container'dan inject edilir
        ///
        /// KAYIT ORNEGI (Program.cs):
        /// <code>
        /// services.AddHttpClient();
        /// services.AddMemoryCache();
        /// services.AddScoped&lt;IExchangeRateService, ExchangeRateService&gt;();
        /// </code>
        /// </summary>
        /// <param name="httpClient">HTTP istemcisi</param>
        /// <param name="cache">Bellek cache'i</param>
        public ExchangeRateService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // =========================================================================
        // PUBLIC METODLAR - Interface Implementasyonu
        // =========================================================================

        /// <summary>
        /// USD/TRY kurunu getirir
        ///
        /// ISLEM ADIM ADIM:
        /// 1. Cache'de var mi kontrol et
        /// 2. Yoksa TCMB'den cek
        /// 3. Cache'e kaydet
        /// 4. Dondur
        ///
        /// HATA DURUMU:
        /// TCMB erisilemezse fallback deger doner
        /// </summary>
        public async Task<decimal> GetUsdRateAsync()
        {
            // Cache'den okumaya calis
            if (_cache.TryGetValue(UsdCacheKey, out decimal cachedRate))
            {
                return cachedRate;
            }

            // Cache'de yok, TCMB'den cek
            var rate = await FetchRateFromTcmbAsync("USD");

            // Cache'e kaydet
            _cache.Set(UsdCacheKey, rate, CacheDuration);

            return rate;
        }

        /// <summary>
        /// EUR/TRY kurunu getirir
        ///
        /// USD ile ayni mantik:
        /// Cache -> TCMB -> Fallback
        /// </summary>
        public async Task<decimal> GetEurRateAsync()
        {
            if (_cache.TryGetValue(EurCacheKey, out decimal cachedRate))
            {
                return cachedRate;
            }

            var rate = await FetchRateFromTcmbAsync("EUR");
            _cache.Set(EurCacheKey, rate, CacheDuration);

            return rate;
        }

        /// <summary>
        /// Dovizi TL'ye cevirir
        ///
        /// DESTEKLENEN PARA BIRIMLERI:
        /// - USD (Amerikan Dolari)
        /// - EUR (Euro)
        /// - TRY (Turk Lirasi - direkt doner)
        ///
        /// ORNEK:
        /// ConvertToTryAsync(100, "USD") -> 100 * 32.45 = 3245 TL
        /// </summary>
        /// <param name="amount">Cevirilecek tutar</param>
        /// <param name="currency">Para birimi kodu (USD, EUR, TRY)</param>
        /// <returns>TL karsiligi</returns>
        /// <exception cref="ArgumentException">Desteklenmeyen para birimi</exception>
        public async Task<decimal> ConvertToTryAsync(decimal amount, string currency)
        {
            // Para birimi kodunu normalize et (buyuk harf)
            var normalizedCurrency = currency?.ToUpperInvariant() ?? string.Empty;

            // Para birimine gore kur al ve cevir
            return normalizedCurrency switch
            {
                "TRY" => amount, // Zaten TL, ceviri gerekmez
                "TL" => amount,  // TL kisaltmasi da kabul edilir
                "USD" => amount * await GetUsdRateAsync(),
                "EUR" => amount * await GetEurRateAsync(),
                _ => throw new ArgumentException(
                    $"Desteklenmeyen para birimi: {currency}. " +
                    $"Desteklenen: USD, EUR, TRY",
                    nameof(currency))
            };
        }

        // =========================================================================
        // PRIVATE METODLAR - Yardimci Fonksiyonlar
        // =========================================================================

        /// <summary>
        /// TCMB XML API'sinden doviz kurunu ceker
        ///
        /// XML YAPISI:
        /// <code>
        /// &lt;Tarih_Date&gt;
        ///   &lt;Currency CurrencyCode="USD"&gt;
        ///     &lt;ForexBuying&gt;32.1234&lt;/ForexBuying&gt;
        ///     &lt;ForexSelling&gt;32.4567&lt;/ForexSelling&gt;
        ///   &lt;/Currency&gt;
        /// &lt;/Tarih_Date&gt;
        /// </code>
        ///
        /// SATIS KURU KULLANILIR:
        /// Alis kuru degil satis kuru alinir (ForexSelling)
        /// Cunku doviz alirken satis kurundan aliriz
        /// </summary>
        /// <param name="currencyCode">Para birimi kodu (USD, EUR)</param>
        /// <returns>TCMB satis kuru</returns>
        private async Task<decimal> FetchRateFromTcmbAsync(string currencyCode)
        {
            try
            {
                // TCMB API'sine istek at
                var response = await _httpClient.GetStringAsync(TcmbUrl);

                // XML'i parse et
                var xml = XDocument.Parse(response);

                // Ilgili para birimini bul
                // XPath: //Currency[@CurrencyCode='USD']
                var currency = xml.Descendants("Currency")
                    .FirstOrDefault(c => c.Attribute("CurrencyCode")?.Value == currencyCode);

                if (currency == null)
                {
                    // Para birimi bulunamadi, fallback kullan
                    return GetFallbackRate(currencyCode);
                }

                // Satis kurunu al (ForexSelling)
                var sellingRateStr = currency.Element("ForexSelling")?.Value;

                if (string.IsNullOrEmpty(sellingRateStr))
                {
                    return GetFallbackRate(currencyCode);
                }

                // ⚠️ KRİTİK FIX: TCMB Parsing Sorunu
                // =====================================
                // SORUN:
                // TCMB XML'den gelen değer: "34,5678" (virgül ondalık ayracı)
                // NumberStyles.Any ile parse edilince: 345678 (virgül binlik ayraç sanılıyor!)
                //
                // ÇÖZÜM:
                // Virgülü noktaya çevir ve InvariantCulture ile parse et
                // "34,5678" → "34.5678" → 34.5678 ✅
                //
                // NEDEN INVARIANTCULTURE?
                // - Nokta her zaman ondalık ayracı
                // - Binlik ayraç yok
                // - Tutarlı ve güvenilir parsing
                //
                // ÖNCEKİ KOD (HATALI):
                // decimal.TryParse(sellingRateStr, NumberStyles.Any, new CultureInfo("tr-TR"), out decimal rate)
                //
                // YENİ KOD (DOĞRU):
                var normalizedRateStr = sellingRateStr.Replace(',', '.'); // Virgül → Nokta

                if (decimal.TryParse(
                    normalizedRateStr,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, // Sadece ondalık nokta
                    CultureInfo.InvariantCulture, // Kültürden bağımsız
                    out decimal rate))
                {
                    return rate;
                }

                // Parse basarisiz, fallback kullan
                return GetFallbackRate(currencyCode);
            }
            catch (HttpRequestException)
            {
                // Ag hatasi - TCMB erisilemez
                // Fallback kur kullan
                return GetFallbackRate(currencyCode);
            }
            catch (Exception)
            {
                // Diger hatalar (XML parse vs.)
                return GetFallbackRate(currencyCode);
            }
        }

        /// <summary>
        /// Fallback (yedek) kur deger dondurur
        ///
        /// KULLANIM DURUMU:
        /// - TCMB API erisilemez
        /// - XML parse hatasi
        /// - Beklenmeyen format
        ///
        /// UYARI:
        /// Fallback degerler sabit tanimli!
        /// Prod ortamda veritabanindan son bilinen kur alinmali
        /// </summary>
        private static decimal GetFallbackRate(string currencyCode)
        {
            return currencyCode switch
            {
                "USD" => FallbackUsdRate,
                "EUR" => FallbackEurRate,
                _ => 1m // Bilinmeyen para birimi icin 1:1
            };
        }
    }
}
