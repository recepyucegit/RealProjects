namespace Domain.Enums
{
    /// <summary>
    /// Teknik Servis Durumu
    /// Özgün Kablocu'nun (Teknik Servis Temsilcisi) istediği takip sistemi
    /// İş Akışı: Acik → Islemde → Tamamlandi/Cozulemedi
    /// </summary>
    public enum TechnicalServiceStatus
    {
        /// <summary>
        /// Sorun bildirildi, henüz üzerine düşülmedi
        /// </summary>
        Acik = 1,

        /// <summary>
        /// Teknik servis sorunu çözmeye çalışıyor
        /// </summary>
        Islemde = 2,

        /// <summary>
        /// Sorun başarıyla çözüldü
        /// </summary>
        Tamamlandi = 3,

        /// <summary>
        /// Sorun çözülemedi veya başka bir çözüm bulundu
        /// </summary>
        Cozulemedi = 4
    }
}