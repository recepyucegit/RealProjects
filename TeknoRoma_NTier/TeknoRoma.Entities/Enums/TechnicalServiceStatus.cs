namespace TeknoRoma.Entities.Enums;

/// <summary>
/// Teknik Servis Durumu
/// Özgün Kablocu'nun (Teknik Servis Temsilcisi) istediği takip sistemi
/// İş Akışı: Acik → Islemde → Tamamlandi/Cozulemedi
///
/// NEDEN?
/// - Sorunların takibi için
/// - SLA (Service Level Agreement) kontrolü
/// - Performans metrikleri (ortalama çözüm süresi)
/// </summary>
public enum TechnicalServiceStatus
{
    /// <summary>
    /// Sorun bildirildi, henüz üzerine düşülmedi
    /// Bekleyen sorunlar listesinde
    /// </summary>
    Acik = 1,

    /// <summary>
    /// Teknik servis sorunu çözmeye çalışıyor
    /// Özgün Kablocu veya ekibi üzerinde çalışıyor
    /// </summary>
    Islemde = 2,

    /// <summary>
    /// Sorun başarıyla çözüldü
    /// Müşteri/çalışan memnun ayrıldı
    /// </summary>
    Tamamlandi = 3,

    /// <summary>
    /// Sorun çözülemedi veya alternatif çözüm bulundu
    /// Sebep belirtilmeli (donanım arızası, stokta yok vb.)
    /// </summary>
    Cozulemedi = 4
}
