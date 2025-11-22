namespace Domain.Enums
{
    /// <summary>
    /// Satış Durumu
    /// Satış sürecinin hangi aşamada olduğunu gösterir
    /// </summary>
    public enum SaleStatus
    {
        /// <summary>
        /// Satış oluşturuldu, ödeme bekleniyor
        /// </summary>
        Beklemede = 1,

        /// <summary>
        /// Ödeme alındı, ürünler hazırlanıyor
        /// </summary>
        Hazirlaniyor = 2,

        /// <summary>
        /// Satış tamamlandı
        /// </summary>
        Tamamlandi = 3,

        /// <summary>
        /// Satış iptal edildi
        /// </summary>
        Iptal = 4
    }
}
