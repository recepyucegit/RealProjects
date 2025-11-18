namespace Domain.Enums
{
    /// <summary>
    /// Stok Durumu
    /// Gül Satar'ın istediği: "Kritik seviyenin altına düşen ürünler uyarı vermeli"
    /// Kerim Zulacı'nın istediği: "Kritik seviyenin altına düşen ürünleri görebilmeliyim"
    /// </summary>
    public enum StockStatus
    {
        /// <summary>
        /// Stok yeterli seviyede
        /// Ürün adedi kritik seviyenin üzerinde
        /// </summary>
        Yeterli = 1,

        /// <summary>
        /// Stok kritik seviyeye yaklaştı
        /// Uyarı verilmeli, yeni sipariş verilmeli
        /// </summary>
        Kritik = 2,

        /// <summary>
        /// Stok tükendi
        /// Satış yapılamaz
        /// </summary>
        Tukendi = 3
    }
}