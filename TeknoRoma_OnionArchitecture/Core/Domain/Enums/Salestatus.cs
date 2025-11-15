namespace Domain.Enums
{
    /// <summary>
    /// Satış Durumu
    /// İş Akışı: Beklemede → Hazirlaniyor → Tamamlandi (veya Iptal)
    /// </summary>
    public enum SaleStatus
    {
        /// <summary>
        /// Satış kaydedildi ama henüz ödeme yapılmadı
        /// Kasa satış temsilcisi müşteri bilgilerini girdi
        /// </summary>
        Beklemede = 1,

        /// <summary>
        /// Ödeme yapıldı, depo ürünleri hazırlıyor
        /// Depo temsilcisi bu durumu görüp ürünleri kasaya getiriyor
        /// </summary>
        Hazirlaniyor = 2,

        /// <summary>
        /// Ürünler kasaya getirildi, müşteriye teslim edildi
        /// </summary>
        Tamamlandi = 3,

        /// <summary>
        /// Satış iptal edildi
        /// Stok geri eklendi
        /// </summary>
        Iptal = 4
    }
}