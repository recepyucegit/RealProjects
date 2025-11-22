// ============================================================================
// Currency.cs - Para Birimi Enum
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın finansal işlemlerinde kullanılan para birimlerini tanımlar.
// Özellikle gider kayıtlarında dövizli işlemler için gereklidir.
//
// NEDEN PARA BİRİMİ TAKİBİ?
// - Tedarikçi ödemeleri dövizle yapılabilir (ithalat)
// - Yazılım lisansları genellikle USD/EUR
// - Kur farkı takibi ve raporlama
// - Muhasebe standartlarına uyum
//
// ISO 4217 STANDARDI:
// - Para birimleri için uluslararası standart
// - 3 harfli kodlar kullanılır
// - TRY: Turkish Lira (949)
// - USD: US Dollar (840)
// - EUR: Euro (978)
//
// KULLANIM SENARYOLARI:
// 1. Gider kaydı: Expense.Currency = Currency.USD
// 2. Kur çevrimi: Expense.AmountInTRY = Amount × ExchangeRate
// 3. Raporlama: Tüm giderler TRY'ye çevrilerek toplanır
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Para Birimi Enum'u
    ///
    /// FİNANSAL YÖNETİM:
    /// - Çoklu para birimi desteği
    /// - Döviz kuru takibi
    /// - TL bazlı konsolidasyon
    ///
    /// EXPENSE ENTITY İLE KULLANIM:
    /// <code>
    /// var expense = new Expense
    /// {
    ///     Amount = 100,           // Orijinal tutar
    ///     Currency = Currency.USD, // Para birimi
    ///     ExchangeRate = 32.50m,  // TCMB kuru
    ///     AmountInTRY = 3250m     // 100 × 32.50 = 3250 TL
    /// };
    /// </code>
    /// </summary>
    public enum Currency
    {
        // ====================================================================
        // YEREL PARA BİRİMİ
        // ====================================================================

        /// <summary>
        /// Türk Lirası (Turkish Lira)
        ///
        /// ISO KODU: TRY (Turkish lira - 949)
        ///
        /// AÇIKLAMA:
        /// - Varsayılan para birimi
        /// - Çoğu işlem TRY cinsindendir
        /// - Raporlama birimi (tüm dövizler TRY'ye çevrilir)
        ///
        /// KULLANIM:
        /// <code>
        /// // TRY giderinde ExchangeRate kullanılmaz
        /// var expense = new Expense
        /// {
        ///     Amount = 1000m,
        ///     Currency = Currency.TRY,
        ///     ExchangeRate = null,  // veya 1
        ///     AmountInTRY = 1000m   // Aynı değer
        /// };
        /// </code>
        ///
        /// SEMBOL: ₺ (Turkish Lira Sign, U+20BA)
        /// </summary>
        TRY = 1,

        // ====================================================================
        // YABANCI PARA BİRİMLERİ
        // ====================================================================

        /// <summary>
        /// Amerikan Doları (US Dollar)
        ///
        /// ISO KODU: USD (US Dollar - 840)
        ///
        /// AÇIKLAMA:
        /// - En yaygın kullanılan yabancı para
        /// - Yazılım lisansları genellikle USD
        /// - Teknoloji ürünleri ithalatı
        ///
        /// KULLANIM ÖRNEKLERİ:
        /// - Microsoft Office lisansı: 99.99 USD
        /// - Adobe Creative Cloud: 54.99 USD/ay
        /// - Sunucu barındırma: 50 USD/ay
        ///
        /// KUR ÇEVRİMİ:
        /// <code>
        /// // USD → TRY çevrimi
        /// decimal usdAmount = 100;
        /// decimal exchangeRate = 32.50m; // TCMB satış kuru
        /// decimal tryAmount = usdAmount * exchangeRate; // 3250 TL
        /// </code>
        ///
        /// SEMBOL: $ (Dollar Sign, U+0024)
        /// </summary>
        USD = 2,

        /// <summary>
        /// Euro (European Union Currency)
        ///
        /// ISO KODU: EUR (Euro - 978)
        ///
        /// AÇIKLAMA:
        /// - Avrupa Birliği para birimi
        /// - Avrupa'dan ithalat ödemeleri
        /// - Bazı uluslararası hizmetler
        ///
        /// KULLANIM ÖRNEKLERİ:
        /// - Avrupa tedarikçi ödemeleri
        /// - GDPR danışmanlık hizmetleri
        /// - Avrupa fuarları katılım ücreti
        ///
        /// KUR ÇEVRİMİ:
        /// <code>
        /// // EUR → TRY çevrimi
        /// decimal eurAmount = 100;
        /// decimal exchangeRate = 35.20m; // TCMB satış kuru
        /// decimal tryAmount = eurAmount * exchangeRate; // 3520 TL
        /// </code>
        ///
        /// SEMBOL: € (Euro Sign, U+20AC)
        /// </summary>
        EUR = 3
    }
}

// ============================================================================
// EK BİLGİLER VE BEST PRACTICES
// ============================================================================
//
// KUR ALIMI İÇİN ÖNERİLER:
// 1. TCMB Web Servisi: https://www.tcmb.gov.tr/kurlar/today.xml
// 2. Günlük otomatik güncelleme
// 3. Geçmiş kurları saklama (tarihsel raporlar için)
//
// GELİŞTİRME ÖNERİSİ:
// Eğer daha fazla para birimi gerekirse:
// <code>
// public enum Currency
// {
//     TRY = 1,
//     USD = 2,
//     EUR = 3,
//     GBP = 4,  // İngiliz Sterlini
//     CHF = 5,  // İsviçre Frangı
//     JPY = 6,  // Japon Yeni
//     CNY = 7   // Çin Yuanı
// }
// </code>
//
// FORMATLAMA:
// <code>
// // Para birimi formatında gösterim
// string formatted = currency switch
// {
//     Currency.TRY => $"₺{amount:N2}",
//     Currency.USD => $"${amount:N2}",
//     Currency.EUR => $"€{amount:N2}",
//     _ => $"{amount:N2}"
// };
// // Çıktı: "₺1.000,00", "$1,000.00", "€1.000,00"
// </code>
// ============================================================================
