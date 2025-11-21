namespace TeknoRoma.Entities.Enums;

/// <summary>
/// Stok Durumu
/// Durna Sabit'in (Depo Temsilcisi) istediği stok uyarı sistemi
///
/// NEDEN? Stok seviyeleri kritik:
/// - Yeterli: Normal stok seviyesi
/// - Azaliyor: Yeni sipariş verilmeli (kritik stok)
/// - Tukendi: Acil tedarik gerekli
/// - CokFazla: Fazla stok, kampanya yapılabilir
/// </summary>
public enum StockStatus
{
    /// <summary>
    /// Stok yeterli seviyede
    /// Normal operasyon
    /// </summary>
    Yeterli = 1,

    /// <summary>
    /// Stok azalıyor - Kritik seviye
    /// Durna Sabit'e bildirim gitmeli
    /// Yeni sipariş verilmeli
    /// </summary>
    Azaliyor = 2,

    /// <summary>
    /// Stok tükendi
    /// Ürün satılamaz
    /// Acil tedarik gerekli
    /// </summary>
    Tukendi = 3,

    /// <summary>
    /// Çok fazla stok var
    /// Kampanya yapılabilir
    /// Depo maliyeti artıyor
    /// </summary>
    CokFazla = 4
}
