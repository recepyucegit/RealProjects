namespace Domain.Enums
{
    /// <summary>
    /// Teknik Servis Durumu
    /// Sorun kayıtlarının durumunu gösterir
    /// </summary>
    public enum TechnicalServiceStatus
    {
        /// <summary>
        /// Sorun açıldı, henüz işleme alınmadı
        /// </summary>
        Acik = 1,

        /// <summary>
        /// Sorun üzerinde çalışılıyor
        /// </summary>
        Islemde = 2,

        /// <summary>
        /// Sorun çözüldü
        /// </summary>
        Tamamlandi = 3,

        /// <summary>
        /// Sorun çözülemedi
        /// </summary>
        Cozulemedi = 4
    }
}
