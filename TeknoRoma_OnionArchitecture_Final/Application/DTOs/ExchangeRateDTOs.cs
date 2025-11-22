// ===================================================================================
// TEKNOROMA - DOVIZ KURU DTO DOSYASI (ExchangeRateDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, doviz kuru (Exchange Rate) ile ilgili DTO'lari icerir.
// TeknoRoma'nin yabanci para birimleriyle islem yapmasi icin kullanilir.
//
// DOVIZ KURULARI NEDEN GEREKLI?
// -----------------------------
// 1. Yabanci tedarikcilerden USD/EUR cinsinden mal alimi
// 2. Fiyat karsilastirma (TL vs USD/EUR)
// 3. Finansal raporlama (tum islemleri TL'ye cevirme)
// 4. Dovizli borc/alacak takibi
//
// VERİ KAYNAGI
// ------------
// Doviz kurlari genellikle TCMB (Turkiye Cumhuriyet Merkez Bankasi)
// veya ozel API servislerinden alinir.
//
// TCMB Doviz Kuru API:
// - Gunluk kurlar yayinlanir
// - Alis (Buying) ve Satis (Selling) kurlari vardir
// - XML veya JSON formatinda alinabilir
//
// DESTEKLENEN PARA BIRIMLERI (Currency enum):
// ------------------------------------------
// - TRY: Turk Lirasi (ana para birimi)
// - USD: Amerikan Dolari
// - EUR: Euro
// - GBP: Ingiliz Sterlini (opsiyonel)
//
// DTO TURLERI
// -----------
// 1. ExchangeRateDto: Tek bir para birimi kur bilgisi
// 2. CurrencyConversionDto: Para birimi cevirme sonucu
// 3. ExchangeRatesResponseDto: Tum kurlarin toplu yaniti
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/IExchangeRateService.cs
// - GetCurrentRatesAsync()
// - ConvertAsync(amount, from, to)
// - GetRateAsync(currency)
//
// ===================================================================================

using Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region DOVIZ KURU DTO'SU

/// <summary>
/// Tek bir para birimi icin doviz kuru bilgisi.
///
/// ACIKLAMA:
/// ---------
/// TCMB veya baska kaynaklardan alinan guncel kur bilgisini tasir.
/// Her para birimi icin alis ve satis kurlari icerir.
///
/// ALIS vs SATIS KURU:
/// -------------------
/// - Alis Kuru (Buying): Banka sizden doviz ALIRKEN uygular
/// - Satis Kuru (Selling): Banka size doviz SATARKEN uygular
/// - Satis kuru her zaman alis kurundan yuksektir (banka kari)
///
/// KULLANIM:
/// ---------
/// - Tedarikci odemelerinde Satis kuru kullanilir (biz doviz aliyoruz)
/// - Raporlamada genelde Alis kuru veya ortalama kullanilir
///
/// UI KULLANIMI:
/// -------------
/// - Dashboard doviz karti
/// - Tedarikci islem formunda kur bilgisi
/// - Finansal raporlarda kur donusumleri
///
/// ORNEK:
/// ------
/// var usdRate = await _exchangeRateService.GetRateAsync(Currency.USD);
/// Console.WriteLine($"USD Alis: {usdRate.BuyingRate}");
/// Console.WriteLine($"USD Satis: {usdRate.SellingRate}");
/// Console.WriteLine($"Guncelleme: {usdRate.UpdatedAt}");
/// </summary>
public class ExchangeRateDto
{
    /// <summary>
    /// Para birimi (enum).
    /// </summary>
    public Currency Currency { get; set; }

    /// <summary>
    /// Para birimi kodu (3 harfli ISO 4217).
    /// Ornek: "USD", "EUR", "TRY"
    /// UI'da kisa gosterim icin.
    /// </summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// Para birimi tam adi.
    /// Ornek: "Amerikan Dolari", "Euro", "Turk Lirasi"
    /// UI'da aciklayici metin olarak.
    /// </summary>
    public string CurrencyName { get; set; } = null!;

    /// <summary>
    /// Alis kuru (TL cinsinden).
    /// 1 birim yabanci para = X TL (alis)
    /// Banka sizden doviz alirken bu kurdan alir.
    /// </summary>
    public decimal BuyingRate { get; set; }

    /// <summary>
    /// Satis kuru (TL cinsinden).
    /// 1 birim yabanci para = X TL (satis)
    /// Banka size doviz satarken bu kurdan satar.
    /// </summary>
    public decimal SellingRate { get; set; }

    /// <summary>
    /// Son guncelleme zamani.
    /// Kurun ne zaman alindigini gosterir.
    /// Eski kur uyarisi vermek icin kontrol edilir.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

#endregion

#region DOVIZ CEVIRME DTO'SU

/// <summary>
/// Para birimi cevirme isleminin sonucu.
///
/// ACIKLAMA:
/// ---------
/// Bir miktarin baska para birimine cevrilmis halini gosterir.
/// Tedarikci islemlerinde ve raporlamada kullanilir.
///
/// KULLANIM SENARYOLARI:
/// ---------------------
/// 1. Tedarikci faturasi: 1000 USD kac TL?
/// 2. Raporlama: Tum islemleri TL'ye cevirme
/// 3. Fiyat karsilastirma: Bu urun USD'de kac?
///
/// ORNEK:
/// ------
/// var result = await _exchangeRateService.ConvertAsync(1000, Currency.USD, Currency.TRY);
/// Console.WriteLine($"{result.OriginalAmount} {result.FromCurrency}");
/// Console.WriteLine($"= {result.ConvertedAmount} {result.ToCurrency}");
/// Console.WriteLine($"Kur: {result.ExchangeRate}");
/// // Cikti: 1000 USD = 32500 TRY (Kur: 32.50)
/// </summary>
public class CurrencyConversionDto
{
    /// <summary>
    /// Kaynak para birimi.
    /// Cevrilen miktar bu para biriminde.
    /// </summary>
    public Currency FromCurrency { get; set; }

    /// <summary>
    /// Hedef para birimi.
    /// Sonuc bu para biriminde.
    /// </summary>
    public Currency ToCurrency { get; set; }

    /// <summary>
    /// Orijinal miktar.
    /// Cevrilen tutar.
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Cevrilmis miktar.
    /// Hesaplanan sonuc.
    /// Formul: OriginalAmount * ExchangeRate
    /// </summary>
    public decimal ConvertedAmount { get; set; }

    /// <summary>
    /// Kullanilan doviz kuru.
    /// 1 FromCurrency = X ToCurrency
    /// </summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Cevirme tarihi/saati.
    /// Hangi kurun kullanildigini belirler.
    /// </summary>
    public DateTime ConversionDate { get; set; }
}

#endregion

#region TOPLU KUR YANITI DTO'SU

/// <summary>
/// Tum doviz kurlarinin toplu yaniti.
///
/// ACIKLAMA:
/// ---------
/// Bir API cagrisinda tum guncel kurlari getirir.
/// Dashboard veya kur tablosu icin kullanilir.
///
/// UI KULLANIMI:
/// -------------
/// - Doviz kuru tablosu
/// - Dashboard doviz widget'i
/// - Kur guncelleme bildirimi
///
/// ORNEK:
/// ------
/// var rates = await _exchangeRateService.GetAllRatesAsync();
/// Console.WriteLine($"Tarih: {rates.Date:d}");
/// Console.WriteLine($"Kaynak: {rates.Source}");
/// foreach (var rate in rates.Rates)
/// {
///     Console.WriteLine($"{rate.CurrencyCode}: Alis={rate.BuyingRate}, Satis={rate.SellingRate}");
/// }
/// </summary>
public class ExchangeRatesResponseDto
{
    /// <summary>
    /// Kurlarin tarihi.
    /// TCMB gunluk kur yayinlar.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Veri kaynagi.
    /// Varsayilan: "TCMB" (Turkiye Cumhuriyet Merkez Bankasi)
    /// Alternatif: "Forex API", "XE.com" vb.
    /// </summary>
    public string Source { get; set; } = "TCMB";

    /// <summary>
    /// Para birimi kurlarinin listesi.
    /// Her para birimi icin ayri ExchangeRateDto.
    /// </summary>
    public List<ExchangeRateDto> Rates { get; set; } = new();
}

#endregion
