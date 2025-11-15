namespace Domain.Enums
{
    /// <summary>
    /// Para Birimi
    /// Gül Satar ve Fahri Cepçi'nin istediği döviz kuru desteği için
    /// "Müşterilerime güncel döviz kuru üzerinden ürünün dolar ve euro fiyatını söyleyebilmeliyim"
    /// </summary>
    public enum Currency
    {
        /// <summary>
        /// Türk Lirası (varsayılan)
        /// </summary>
        TRY = 1,

        /// <summary>
        /// Amerikan Doları
        /// </summary>
        USD = 2,

        /// <summary>
        /// Euro
        /// </summary>
        EUR = 3
    }
}