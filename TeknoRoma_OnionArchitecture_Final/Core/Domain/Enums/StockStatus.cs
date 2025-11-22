namespace Domain.Enums
{
    /// <summary>
    /// Stok Durumu
    /// Ürün stok seviyesi göstergesi
    /// </summary>
    public enum StockStatus
    {
        /// <summary>
        /// Stok yeterli seviyede
        /// </summary>
        Yeterli = 1,

        /// <summary>
        /// Stok kritik seviyenin altında
        /// </summary>
        Kritik = 2,

        /// <summary>
        /// Stokta ürün yok
        /// </summary>
        Tukendi = 3
    }
}
