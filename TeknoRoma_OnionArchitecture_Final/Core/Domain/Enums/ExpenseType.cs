// ============================================================================
// ExpenseType.cs - Gider Türü Enum
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın işletme giderlerini kategorize eden enum.
// Muhasebe, raporlama ve bütçe yönetimi için kullanılır.
//
// GİDER KATEGORİZASYONU NEDİR?
// - Giderler türlerine göre gruplanır
// - Her kategori farklı muhasebe hesabına kaydedilir
// - Raporlarda kategori bazlı analiz yapılır
// - Bütçe planlaması kategorilere göre yapılır
//
// MUHASEBE HESAP PLANI EŞLEŞMESİ:
// - CalisanOdemesi → 760 Personel Giderleri
// - TeknikAltyapiGideri → 770 Genel Yönetim Giderleri
// - Fatura → 730 İşletme Giderleri
// - DigerGider → 780 Diğer Olağan Giderler
//
// RAPORLAMA:
// - Aylık gider analizi
// - Yıllık bütçe karşılaştırması
// - Mağaza bazlı maliyet analizi
// - Gider kalemi trend takibi
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Gider Türü Enum'u
    ///
    /// FİNANSAL YÖNETİM:
    /// - Gider sınıflandırması
    /// - Bütçe kontrolü
    /// - Maliyet analizi
    ///
    /// İŞ GEREKSİNİMİ (Haluk Bey):
    /// "Gider raporlarını kategorilere göre görmek istiyorum.
    ///  Personel giderleri, faturalar ve diğerleri ayrı ayrı olsun."
    ///
    /// KULLANIM ÖRNEĞİ:
    /// <code>
    /// var expense = new Expense
    /// {
    ///     Description = "Ocak 2024 - Elektrik Faturası",
    ///     ExpenseType = ExpenseType.Fatura,
    ///     Amount = 5000m,
    ///     StoreId = 1
    /// };
    /// </code>
    /// </summary>
    public enum ExpenseType
    {
        // ====================================================================
        // PERSONEL GİDERLERİ
        // ====================================================================

        /// <summary>
        /// Çalışan Ödemesi - Maaş ve Personel Giderleri
        ///
        /// AÇIKLAMA:
        /// - Çalışan maaşları
        /// - Prim ve ikramiyeler
        /// - SGK işveren payı
        /// - Yemek ve yol yardımı
        ///
        /// İLİŞKİLİ ALAN:
        /// - Expense.EmployeeId bu tür giderlerde dolu olmalı
        /// - Hangi çalışana yapıldığı bilinir
        ///
        /// ÖRNEK GİDERLER:
        /// - "Mehmet Yılmaz - Ocak 2024 Maaşı"
        /// - "Ayşe Demir - Satış Primi Q1"
        /// - "SGK Ödemeleri Ocak 2024"
        ///
        /// MUHASEBE:
        /// - 760 Personel Giderleri hesabına kaydedilir
        /// - Bordro ile eşleştirilir
        ///
        /// RAPORLAMA:
        /// <code>
        /// // Toplam personel giderleri
        /// var personnelExpenses = expenses
        ///     .Where(e => e.ExpenseType == ExpenseType.CalisanOdemesi)
        ///     .Sum(e => e.AmountInTRY);
        ///
        /// // Çalışan başına maliyet
        /// var costPerEmployee = personnelExpenses / employeeCount;
        /// </code>
        ///
        /// BÜTÇE TAKİBİ:
        /// - Personel giderleri genellikle en büyük kalem
        /// - Aylık sabit gider olarak planlanır
        /// - Prim/ikramiye dönemsel değişkenlik gösterir
        /// </summary>
        CalisanOdemesi = 1,

        // ====================================================================
        // TEKNİK GİDERLER
        // ====================================================================

        /// <summary>
        /// Teknik Altyapı Gideri - IT ve Teknoloji Giderleri
        ///
        /// AÇIKLAMA:
        /// - Sunucu ve hosting giderleri
        /// - Yazılım lisansları
        /// - Donanım bakım-onarımı
        /// - IT danışmanlık hizmetleri
        ///
        /// ÖRNEK GİDERLER:
        /// - "Azure Sunucu - Ocak 2024" (USD)
        /// - "Microsoft 365 Lisansı - Yıllık" (USD)
        /// - "POS Cihazı Tamiri"
        /// - "Ağ Altyapısı Yenileme"
        ///
        /// DÖVİZLİ GİDERLER:
        /// - Bu kategori sıklıkla USD/EUR cinsindendir
        /// - Expense.Currency ve ExchangeRate kullanılır
        ///
        /// <code>
        /// var techExpense = new Expense
        /// {
        ///     Description = "Azure App Service - Ocak 2024",
        ///     ExpenseType = ExpenseType.TeknikAltyapiGideri,
        ///     Amount = 150m,
        ///     Currency = Currency.USD,
        ///     ExchangeRate = 32.50m,
        ///     AmountInTRY = 4875m  // 150 × 32.50
        /// };
        /// </code>
        ///
        /// MUHASEBE:
        /// - 770 Genel Yönetim Giderleri
        /// - Bazı yazılımlar aktifleştirilerek amortisman ayrılabilir
        ///
        /// BÜTÇE PLANLAMASI:
        /// - Yıllık lisans yenilemeleri önceden planlanmalı
        /// - Teknoloji yatırımları uzun vadeli değerlendirilmeli
        /// </summary>
        TeknikAltyapiGideri = 2,

        // ====================================================================
        // İŞLETME GİDERLERİ
        // ====================================================================

        /// <summary>
        /// Fatura - Elektrik, Su, Doğalgaz, İnternet
        ///
        /// AÇIKLAMA:
        /// - Elektrik faturaları
        /// - Su faturaları
        /// - Doğalgaz/ısınma giderleri
        /// - İnternet ve telefon faturaları
        ///
        /// ÖRNEK GİDERLER:
        /// - "Kadıköy Mağaza - Elektrik Ocak 2024"
        /// - "Bornova Mağaza - Doğalgaz Şubat 2024"
        /// - "Merkez - Turkcell Kurumsal Hat"
        ///
        /// ÖZELLİKLER:
        /// - Genellikle aylık periyodik giderler
        /// - Mağaza bazında ayrı kaydedilir
        /// - StoreId ile ilişkilendirilir
        ///
        /// MAĞAZA BAZLI TAKİP:
        /// <code>
        /// // Mağaza bazında fatura giderleri
        /// var utilityByStore = expenses
        ///     .Where(e => e.ExpenseType == ExpenseType.Fatura)
        ///     .GroupBy(e => e.Store.Name)
        ///     .Select(g => new
        ///     {
        ///         Store = g.Key,
        ///         Total = g.Sum(e => e.AmountInTRY)
        ///     });
        /// </code>
        ///
        /// MEVSİMSEL ANALİZ:
        /// <code>
        /// // Yaz vs Kış fatura karşılaştırması
        /// var summerBills = expenses
        ///     .Where(e => e.ExpenseType == ExpenseType.Fatura)
        ///     .Where(e => e.ExpenseDate.Month >= 6 && e.ExpenseDate.Month <= 8);
        ///
        /// var winterBills = expenses
        ///     .Where(e => e.ExpenseType == ExpenseType.Fatura)
        ///     .Where(e => e.ExpenseDate.Month >= 12 || e.ExpenseDate.Month <= 2);
        /// </code>
        ///
        /// MUHASEBE:
        /// - 730 İşletme Giderleri
        /// - E-fatura numarası DocumentNumber'da saklanır
        /// </summary>
        Fatura = 3,

        // ====================================================================
        // DİĞER GİDERLER
        // ====================================================================

        /// <summary>
        /// Diğer Gider - Kategorize Edilemeyen Giderler
        ///
        /// AÇIKLAMA:
        /// - Yukarıdaki kategorilere girmeyen giderler
        /// - Nadir veya tek seferlik harcamalar
        /// - Genel işletme giderleri
        ///
        /// ÖRNEK GİDERLER:
        /// - "Kırtasiye Malzemeleri"
        /// - "Temizlik Hizmeti"
        /// - "Fuar Katılım Ücreti"
        /// - "Araç Yakıt Gideri"
        /// - "Kira Giderleri"
        /// - "Sigorta Primleri"
        /// - "Reklam ve Pazarlama"
        ///
        /// DİKKAT:
        /// - Bu kategori çok büyürse yeni kategoriler eklenmeli
        /// - "Diğer" her zaman en küçük kategori olmalı
        ///
        /// ANALİZ:
        /// <code>
        /// // Diğer giderlerin detaylı analizi
        /// var otherExpenses = expenses
        ///     .Where(e => e.ExpenseType == ExpenseType.DigerGider)
        ///     .OrderByDescending(e => e.AmountInTRY)
        ///     .Take(10);
        ///
        /// // Eğer "Diğer" çok yüksekse kategori önerisi
        /// var otherTotal = expenses
        ///     .Where(e => e.ExpenseType == ExpenseType.DigerGider)
        ///     .Sum(e => e.AmountInTRY);
        ///
        /// var grandTotal = expenses.Sum(e => e.AmountInTRY);
        ///
        /// if (otherTotal / grandTotal > 0.2m) // %20'den fazlaysa
        /// {
        ///     // Uyarı: Diğer kategorisi çok yüksek, alt kategori ekleyin
        /// }
        /// </code>
        ///
        /// MUHASEBE:
        /// - 780 Diğer Olağan Giderler
        /// - Description alanında detay belirtilmeli
        /// </summary>
        DigerGider = 4
    }
}

// ============================================================================
// EK BİLGİLER VE GENİŞLETME ÖNERİLERİ
// ============================================================================
//
// GELİŞTİRME ÖNERİSİ:
// Daha detaylı gider takibi için kategori genişletilebilir:
// <code>
// public enum ExpenseType
// {
//     // Personel
//     Maas = 1,
//     Prim = 2,
//     SGK = 3,
//
//     // Teknik
//     Yazilim = 10,
//     Donanim = 11,
//     Hosting = 12,
//
//     // Faturalar
//     Elektrik = 20,
//     Su = 21,
//     Dogalgaz = 22,
//     Internet = 23,
//     Telefon = 24,
//
//     // İşletme
//     Kira = 30,
//     Sigorta = 31,
//     Temizlik = 32,
//     Guvenlik = 33,
//
//     // Pazarlama
//     Reklam = 40,
//     Promosyon = 41,
//
//     // Diğer
//     Diger = 99
// }
// </code>
//
// RAPORLAMA SORGUSU:
// <code>
// // Gider türü bazında aylık rapor
// var monthlyReport = expenses
//     .Where(e => e.ExpenseDate.Month == DateTime.Now.Month)
//     .GroupBy(e => e.ExpenseType)
//     .Select(g => new
//     {
//         Type = g.Key,
//         Count = g.Count(),
//         Total = g.Sum(e => e.AmountInTRY),
//         Average = g.Average(e => e.AmountInTRY)
//     })
//     .OrderByDescending(x => x.Total);
// </code>
//
// BÜTÇE KARŞILAŞTIRMA:
// <code>
// // Bütçe vs Gerçekleşen karşılaştırması
// var comparison = new
// {
//     Personnel = new
//     {
//         Budget = 100000m,
//         Actual = expenses
//             .Where(e => e.ExpenseType == ExpenseType.CalisanOdemesi)
//             .Sum(e => e.AmountInTRY)
//     },
//     // ... diğer kategoriler
// };
// </code>
// ============================================================================
