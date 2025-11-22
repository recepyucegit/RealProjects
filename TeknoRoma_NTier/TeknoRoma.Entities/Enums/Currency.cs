namespace TeknoRoma.Entities.Enums;

/// <summary>
/// Para Birimi
/// Gül Satar'ın istediği döviz kuru desteği için
/// "Müşterilerime güncel döviz kuru üzerinden ürünün dolar ve euro fiyatını söyleyebilmeliyim"
///
/// NEDEN? TeknoRoma uluslararası tedarikçilerden alım yapıyor
/// Ürün fiyatları USD veya EUR olabilir
/// </summary>
public enum Currency
{
    /// <summary>
    /// Türk Lirası (varsayılan)
    /// </summary>
    TRY = 1,

    /// <summary>
    /// Amerikan Doları
    /// İthal ürünlerde kullanılır
    /// </summary>
    USD = 2,

    /// <summary>
    /// Euro
    /// Avrupa menşeili ürünlerde kullanılır
    /// </summary>
    EUR = 3
}
