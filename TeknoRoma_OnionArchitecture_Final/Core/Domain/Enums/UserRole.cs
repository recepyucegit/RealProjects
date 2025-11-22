namespace Domain.Enums
{
    /// <summary>
    /// Kullanıcı Rolleri
    /// Haluk Bey'in istediği: "Kullanıcı sadece yetkisi dahilindeki bölümlere erişebilmeli"
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Genel Müdür - Tüm sisteme erişim
        /// </summary>
        GenelMudur = 1,

        /// <summary>
        /// Şube Müdürü - Kendi şubesinin tüm işlemleri
        /// </summary>
        SubeMuduru = 2,

        /// <summary>
        /// Kasa/Satış Personeli - Satış işlemleri
        /// </summary>
        KasaSatis = 3,

        /// <summary>
        /// Depo Personeli - Stok ve ürün işlemleri
        /// </summary>
        Depo = 4,

        /// <summary>
        /// Muhasebe Personeli - Finansal işlemler
        /// </summary>
        Muhasebe = 5,

        /// <summary>
        /// Teknik Servis Personeli - Servis kayıtları
        /// </summary>
        TeknikServis = 6
    }
}
