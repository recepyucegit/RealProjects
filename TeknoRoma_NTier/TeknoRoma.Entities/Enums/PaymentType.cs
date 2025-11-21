namespace TeknoRoma.Entities.Enums;

/// <summary>
/// Ödeme Türü
/// Müşteri satış yaparken hangi yöntemle ödeme yaptı
///
/// NEDEN?
/// - Muhasebe departmanı için ödeme türüne göre raporlama
/// - Nakit ödemeleri kasa sayımında kullanılır
/// - Kredi kartı ödemeleri banka dekontlarıyla eşleştirilir
/// - Havale ve çek takibi için ayrı süreçler var
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// Nakit ödeme
    /// Kasa sayımında kullanılır
    /// </summary>
    Nakit = 1,

    /// <summary>
    /// Kredi kartı ile ödeme
    /// POS cihazından çekilir
    /// </summary>
    KrediKarti = 2,

    /// <summary>
    /// Banka havalesi / EFT
    /// Banka dekontlarıyla eşleştirilir
    /// </summary>
    Havale = 3,

    /// <summary>
    /// Çek ile ödeme
    /// Çek takip sistemiyle yönetilir
    /// </summary>
    Cek = 4
}
