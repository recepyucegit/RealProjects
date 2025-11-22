// ============================================================================
// IExchangeRateService.cs - Döviz Kuru Servis Interface
// ============================================================================
// AÇIKLAMA:
// Döviz kuru çevirisi için servis interface'i.
// TCMB (Türkiye Cumhuriyet Merkez Bankası) entegrasyonu.
//
// KULLANIM ALANLARI:
// 1. Tedarikçi ödemesi (USD/EUR ile alım)
// 2. Fiyat karşılaştırma (Apple Store USD fiyatı)
// 3. Çoklu para birimi desteği
//
// TCMB API:
// https://www.tcmb.gov.tr/kurlar/today.xml
// Günlük kurlar XML formatında yayınlanır
//
// CACHE STRATEJİSİ:
// Kurlar günde 1 kez güncellenir
// Performans için cache kullanılmalı (1 saat)
// ============================================================================

namespace Application.Services
{
    /// <summary>
    /// Döviz Kuru Servis Interface
    ///
    /// DÖVİZ ÇEVİRİSİ: USD/EUR -> TRY
    /// TCMB ENTEGRASYONU: Resmi kurlar
    ///
    /// IMPLEMENTASYON NOTU:
    /// Infrastructure katmanında HttpClient ile TCMB API'ye bağlanılır
    /// </summary>
    public interface IExchangeRateService
    {
        /// <summary>
        /// USD/TRY Kuru
        ///
        /// AMERİKAN DOLARI: En yaygın döviz
        /// TCMB SATIŞ KURU kullanılır (alış değil)
        ///
        /// ÖRNEK:
        /// var usdKur = await GetUsdRateAsync(); // 32.45
        /// var tlKarsiligi = 100 * usdKur; // 100 USD = 3245 TL
        ///
        /// CACHE: Sonuç cache'lenebilir (1 saat)
        /// HATA: TCMB erişilemezse son bilinen kur döner
        /// </summary>
        Task<decimal> GetUsdRateAsync();

        /// <summary>
        /// EUR/TRY Kuru
        ///
        /// EURO: Avrupa tedarikçileri için
        /// TCMB SATIŞ KURU kullanılır
        ///
        /// ÖRNEK:
        /// var eurKur = await GetEurRateAsync(); // 35.20
        /// var tlKarsiligi = 500 * eurKur; // 500 EUR = 17600 TL
        /// </summary>
        Task<decimal> GetEurRateAsync();

        /// <summary>
        /// Dövizi TL'ye Çevir
        ///
        /// GENEL ÇEVİRİ METODU:
        /// Herhangi bir para biriminden TL'ye çevirme
        ///
        /// PARAMETRELER:
        /// - amount: Çevrilecek tutar
        /// - currency: Para birimi kodu ("USD", "EUR")
        ///
        /// ÖRNEK KULLANIM:
        /// // Tedarikçi faturası 1000 USD
        /// var tlTutar = await ConvertToTryAsync(1000, "USD");
        /// // SupplierTransaction.TotalAmountTry = tlTutar
        ///
        /// DESTEKLENMEYEn PARA BİRİMİ:
        /// ArgumentException fırlatılır
        ///
        /// GENİŞLETİLEBİLİRLİK:
        /// - GBP (İngiliz Sterlini)
        /// - CHF (İsviçre Frangı)
        /// - JPY (Japon Yeni)
        /// gibi para birimleri eklenebilir
        /// </summary>
        Task<decimal> ConvertToTryAsync(decimal amount, string currency);
    }
}
