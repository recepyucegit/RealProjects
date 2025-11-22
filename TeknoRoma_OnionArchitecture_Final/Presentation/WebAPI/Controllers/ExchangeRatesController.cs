// ===================================================================================
// TEKNOROMA - EXCHANGE RATES CONTROLLER
// ===================================================================================
//
// Doviz kuru sorgulama icin API endpoint'leri.
// TCMB entegrasyonu.
//
// TEKNOROMA GEREKSINIMLERI:
// - Guncel USD/TRY ve EUR/TRY kurlari
// - Urun fiyatini doviz olarak gosterme
// - Gecmis tarih kurlari ile rapor (Muhasebe icin)
//
// SENARYO (Gul, Fahri - Satis):
// "Musterilerime guncel doviz kuru uzerinden urunun
// dolar ve euro fiyatini soyleyebilmeliyim"
//
// SENARYO (Feyza - Muhasebe):
// "10.10.2006 tarihindeki giris cikislarla birlikte
// o tarihteki doviz kurunu da Dolar ve Euro olarak gorebilmek istiyorum"
//
// ===================================================================================

using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Doviz Kuru API Controller
    ///
    /// TCMB guncel kurlar, doviz cevirme
    /// </summary>
    [Authorize]
    public class ExchangeRatesController : BaseApiController
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeRatesController> _logger;

        public ExchangeRatesController(
            IExchangeRateService exchangeRateService,
            ILogger<ExchangeRatesController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        /// <summary>
        /// Guncel doviz kurlari
        /// </summary>
        /// <remarks>
        /// TCMB'den alinan guncel USD ve EUR kurlari.
        /// 1 saat cache'lenir.
        /// </remarks>
        [HttpGet("current")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetCurrentRates()
        {
            var usdRate = await _exchangeRateService.GetUsdRateAsync();
            var eurRate = await _exchangeRateService.GetEurRateAsync();

            _logger.LogInformation("Doviz kuru sorgusu: USD={UsdRate}, EUR={EurRate}",
                usdRate, eurRate);

            return Success(new
            {
                USD = new
                {
                    Code = "USD",
                    Name = "Amerikan Dolari",
                    Rate = usdRate,
                    Symbol = "$"
                },
                EUR = new
                {
                    Code = "EUR",
                    Name = "Euro",
                    Rate = eurRate,
                    Symbol = "€"
                },
                BaseCurrency = "TRY",
                Source = "TCMB",
                UpdatedAt = DateTime.Now
            });
        }

        /// <summary>
        /// USD/TRY kuru
        /// </summary>
        [HttpGet("usd")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        public async Task<IActionResult> GetUsdRate()
        {
            var rate = await _exchangeRateService.GetUsdRateAsync();
            return Success(new { Currency = "USD", Rate = rate });
        }

        /// <summary>
        /// EUR/TRY kuru
        /// </summary>
        [HttpGet("eur")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        public async Task<IActionResult> GetEurRate()
        {
            var rate = await _exchangeRateService.GetEurRateAsync();
            return Success(new { Currency = "EUR", Rate = rate });
        }

        /// <summary>
        /// Doviz cevirme
        /// </summary>
        /// <remarks>
        /// Belirtilen tutari TL'ye cevirir.
        ///
        /// DESTEKLENEN PARA BIRIMLERI:
        /// - USD (Amerikan Dolari)
        /// - EUR (Euro)
        /// - TRY (Turk Lirasi - degisim yok)
        ///
        /// ORNEK:
        /// /convert?amount=100&amp;currency=USD
        /// 100 USD -> 3245 TL (kura bagli)
        /// </remarks>
        /// <param name="amount">Cevirilecek tutar</param>
        /// <param name="currency">Para birimi (USD, EUR)</param>
        [HttpGet("convert")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Convert(
            [FromQuery] decimal amount,
            [FromQuery] string currency)
        {
            if (amount <= 0)
                return BadRequestResponse("Tutar sifirdan buyuk olmali");

            if (string.IsNullOrEmpty(currency))
                return BadRequestResponse("Para birimi belirtilmeli");

            try
            {
                var tryAmount = await _exchangeRateService.ConvertToTryAsync(amount, currency);

                decimal rate = currency.ToUpperInvariant() switch
                {
                    "USD" => await _exchangeRateService.GetUsdRateAsync(),
                    "EUR" => await _exchangeRateService.GetEurRateAsync(),
                    _ => 1
                };

                return Success(new
                {
                    OriginalAmount = amount,
                    OriginalCurrency = currency.ToUpperInvariant(),
                    ConvertedAmount = tryAmount,
                    ConvertedCurrency = "TRY",
                    ExchangeRate = rate,
                    ConvertedAt = DateTime.Now
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Tutari tum para birimlerinde goster
        /// </summary>
        /// <remarks>
        /// Verilen TL tutarini USD ve EUR'ya cevirir.
        /// Fiyat gosterimi icin kullanilir.
        ///
        /// SENARYO (Fahri - Mobil Satis):
        /// "Bazi merakli musterilerime fiyat bilgisini,
        /// guncel doviz kuru uzerinden Dolar ve Euro olarak sunabilmeliyim"
        /// </remarks>
        /// <param name="amountInTry">TL tutari</param>
        [HttpGet("display-all")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> DisplayInAllCurrencies([FromQuery] decimal amountInTry)
        {
            if (amountInTry <= 0)
                return BadRequestResponse("Tutar sifirdan buyuk olmali");

            var usdRate = await _exchangeRateService.GetUsdRateAsync();
            var eurRate = await _exchangeRateService.GetEurRateAsync();

            return Success(new
            {
                TRY = new
                {
                    Amount = amountInTry,
                    Formatted = $"{amountInTry:N2} TL"
                },
                USD = new
                {
                    Amount = Math.Round(amountInTry / usdRate, 2),
                    Formatted = $"${amountInTry / usdRate:N2}",
                    Rate = usdRate
                },
                EUR = new
                {
                    Amount = Math.Round(amountInTry / eurRate, 2),
                    Formatted = $"€{amountInTry / eurRate:N2}",
                    Rate = eurRate
                },
                UpdatedAt = DateTime.Now
            });
        }
    }
}
