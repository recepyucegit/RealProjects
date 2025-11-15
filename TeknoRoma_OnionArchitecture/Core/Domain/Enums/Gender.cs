namespace Domain.Enums
{
    /// <summary>
    /// Cinsiyet
    /// Müşteri demografik analizi için
    /// Haluk Bey'in istediği raporda: "En çok satılan ürünleri alan müşteri kitlesi yaş, cinsiyet"
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Erkek
        /// </summary>
        Erkek = 1,

        /// <summary>
        /// Kadın
        /// </summary>
        Kadin = 2,

        /// <summary>
        /// Belirtilmemiş (opsiyonel)
        /// </summary>
        Belirtilmemis = 3
    }
}