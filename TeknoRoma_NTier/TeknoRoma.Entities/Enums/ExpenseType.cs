namespace TeknoRoma.Entities.Enums;

/// <summary>
/// Gider Türü
/// Feyza Paragöz'ün (Muhasebe Temsilcisi) takip ettiği gider kategorileri
/// Haluk Bey'in (Şube Müdürü) istediği gider raporu için kategorilendirme
///
/// NEDEN? Her gider türü için ayrı raporlama gerekiyor
/// - Çalışan ödemeleri: Personel giderleri
/// - Teknik altyapı: IT altyapı maliyetleri
/// - Faturalar: Kira, elektrik, su, internet
/// - Diğer giderler: Kırtasiye, temizlik vb.
/// </summary>
public enum ExpenseType
{
    /// <summary>
    /// Çalışan maaşları ve primleri
    /// En büyük gider kalemi
    /// </summary>
    CalisanOdemesi = 1,

    /// <summary>
    /// Sunucu, network, donanım, yazılım lisansları
    /// IT altyapı giderleri
    /// </summary>
    TeknikaltyapiGideri = 2,

    /// <summary>
    /// Kira, elektrik, su, doğalgaz, internet faturaları
    /// Sabit giderler
    /// </summary>
    Fatura = 3,

    /// <summary>
    /// Kırtasiye, temizlik, güvenlik, reklam vb.
    /// Diğer operasyonel giderler
    /// </summary>
    DigerGider = 4
}
