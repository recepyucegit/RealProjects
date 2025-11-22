// ============================================================================
// Gender.cs - Cinsiyet Enum
// ============================================================================
// AÇIKLAMA:
// Müşteri ve çalışan kayıtlarında cinsiyet bilgisini saklamak için kullanılır.
// Demografik analiz ve pazarlama stratejileri için önemlidir.
//
// NEDEN CİNSİYET TAKİBİ?
// 1. Demografik Analiz:
//    - Müşteri profilini anlama
//    - Hedef kitle belirleme
//
// 2. Pazarlama:
//    - Cinsiyete özel kampanyalar
//    - Ürün önerileri
//    - E-posta pazarlama segmentasyonu
//
// 3. Raporlama:
//    - Satış dağılımı analizi
//    - Müşteri davranış analizi
//
// KVKK (KİŞİSEL VERİLERİN KORUNMASI KANUNU):
// - Cinsiyet "özel nitelikli kişisel veri" DEĞİLDİR
// - Ancak yine de açık rıza ile toplanmalı
// - "Belirtilmemis" seçeneği KVKK uyumu için önemli
//
// ÖNEMLİ NOT:
// - Müşteri cinsiyet vermek zorunda değil
// - "Belirtilmemis" varsayılan olabilir
// - Zorlamamak müşteri deneyimini iyileştirir
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Cinsiyet Enum'u
    ///
    /// DEMOGRAFİK ANALİZ:
    /// - Müşteri profilleme
    /// - Satış analizi
    /// - Pazarlama segmentasyonu
    ///
    /// KULLANIM ÖRNEKLERİ:
    /// <code>
    /// // Müşteri kaydında
    /// var customer = new Customer
    /// {
    ///     FirstName = "Ayşe",
    ///     Gender = Gender.Kadin
    /// };
    ///
    /// // Raporlama
    /// var genderDistribution = customers
    ///     .GroupBy(c => c.Gender)
    ///     .Select(g => new { Gender = g.Key, Count = g.Count() });
    ///
    /// // Kampanya filtreleme
    /// var femaleCustomers = customers
    ///     .Where(c => c.Gender == Gender.Kadin)
    ///     .ToList();
    /// </code>
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Erkek
        ///
        /// AÇIKLAMA:
        /// - Erkek müşteri/çalışan
        ///
        /// PAZARLAMA KULLANIMI:
        /// - Erkek müşterilere özel kampanyalar
        /// - Babalar günü kampanyası
        /// - Erkek giyim/aksesuar önerileri
        ///
        /// ÖRNEK SORGU:
        /// <code>
        /// var maleCustomersSales = await _context.Sales
        ///     .Where(s => s.Customer.Gender == Gender.Erkek)
        ///     .SumAsync(s => s.TotalAmount);
        /// </code>
        /// </summary>
        Erkek = 1,

        /// <summary>
        /// Kadın
        ///
        /// AÇIKLAMA:
        /// - Kadın müşteri/çalışan
        ///
        /// PAZARLAMA KULLANIMI:
        /// - Kadın müşterilere özel kampanyalar
        /// - Anneler günü kampanyası
        /// - Kadın ürünleri önerileri
        ///
        /// İSTATİSTİK:
        /// <code>
        /// // Kadın müşteri yüzdesi
        /// var totalCustomers = customers.Count();
        /// var femaleCustomers = customers.Count(c => c.Gender == Gender.Kadin);
        /// var femalePercent = (femaleCustomers * 100.0) / totalCustomers;
        /// </code>
        /// </summary>
        Kadin = 2,

        /// <summary>
        /// Belirtilmemiş
        ///
        /// AÇIKLAMA:
        /// - Müşteri/çalışan cinsiyet bilgisi vermek istemedi
        /// - Veya henüz kayıt edilmedi
        ///
        /// NEDEN ÖNEMLİ?
        /// 1. KVKK Uyumu:
        ///    - Kişi veri paylaşmak zorunda değil
        ///    - "Paylaşmak istemiyorum" seçeneği
        ///
        /// 2. Kullanıcı Deneyimi:
        ///    - Zorlamamak müşteri memnuniyetini artırır
        ///    - Hızlı kayıt için atlayabilir
        ///
        /// 3. Veri Kalitesi:
        ///    - Bilinmeyen/eksik veri için
        ///    - NULL yerine anlamlı değer
        ///
        /// VARSAYILAN DEĞER OLARAK:
        /// <code>
        /// // Customer entity'sinde
        /// public Gender? Gender { get; set; }  // NULL olabilir
        /// // veya
        /// public Gender Gender { get; set; } = Gender.Belirtilmemis;
        /// </code>
        ///
        /// RAPORLARDA:
        /// <code>
        /// // Belirtilmemiş olanları hariç tut
        /// var genderStats = customers
        ///     .Where(c => c.Gender != Gender.Belirtilmemis)
        ///     .GroupBy(c => c.Gender);
        /// </code>
        /// </summary>
        Belirtilmemis = 3
    }
}

// ============================================================================
// EK BİLGİLER
// ============================================================================
//
// ULUSLARARASI STANDARTLAR:
// ISO 5218 - Codes for the representation of human sexes
// - 0: Not known (bilinmiyor)
// - 1: Male (erkek)
// - 2: Female (kadın)
// - 9: Not applicable (uygulanamaz)
//
// MODERN YAKLAŞIMLAR:
// Bazı sistemlerde daha kapsamlı seçenekler sunulur:
// - Non-binary
// - Prefer not to say
// - Self-describe
//
// ŞU ANKİ TASARIM:
// TEKNOROMA'nın ihtiyaçları için 3 seçenek yeterli.
// Gerekirse genişletilebilir.
//
// ÖNERİ:
// UI'da "Belirtmek istemiyorum" gibi kullanıcı dostu bir metin gösterilebilir.
// ============================================================================
