namespace TeknoRoma.Entities.Enums;

/// <summary>
/// Satış Durumu
/// İş Akışı: Beklemede → Hazirlaniyor → Tamamlandi (veya Iptal)
///
/// NEDEN? Her departman kendi aşamasını görmeli:
/// - Kasa Satış: Satışı kaydeder (Beklemede)
/// - Depo: Ürünleri hazırlar (Hazirlaniyor)
/// - Kasa Satış: Müşteriye teslim eder (Tamamlandi)
/// </summary>
public enum SaleStatus
{
    /// <summary>
    /// Satış kaydedildi ama henüz ödeme yapılmadı
    /// Gül Satar (Kasa Satış) müşteri bilgilerini girdi
    /// Müşteri ürünleri seçiyor
    /// </summary>
    Beklemede = 1,

    /// <summary>
    /// Ödeme yapıldı, depo ürünleri hazırlıyor
    /// Durna Sabit (Depo) bu durumu görüp ürünleri kasaya getiriyor
    /// </summary>
    Hazirlaniyor = 2,

    /// <summary>
    /// Ürünler kasaya getirildi, müşteriye teslim edildi
    /// İş akışı tamamlandı
    /// </summary>
    Tamamlandi = 3,

    /// <summary>
    /// Satış iptal edildi
    /// Stok geri eklendi
    /// Ödeme iade edildi
    /// </summary>
    Iptal = 4
}
