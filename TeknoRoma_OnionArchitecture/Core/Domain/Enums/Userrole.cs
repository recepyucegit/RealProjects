namespace Domain.Enums
{
    /// <summary>
    /// Kullanıcı Rolleri
    /// NEDEN ENUM? 
    /// - Roller sabit ve sınırlı
    /// - Database'de int olarak saklanır (1, 2, 3, 4, 5)
    /// - String yerine enum kullanmak performans ve güvenlik sağlar
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Şube Müdürü - Tüm yetkilere sahip
        /// - Tedarikçi, kategori, ürün, çalışan tanımlamaları
        /// - Tüm raporlara erişim
        /// </summary>
        SubeYoneticisi = 1,

        /// <summary>
        /// Kasa Satış Temsilcisi
        /// - Satış işlemleri
        /// - Müşteri tanımlama
        /// - Satış raporları görme
        /// </summary>
        KasaSatis = 2,

        /// <summary>
        /// Depo Temsilcisi
        /// - Satış sonrası ürün hazırlama
        /// - Stok kontrol
        /// - Kritik stok raporları
        /// </summary>
        Depo = 3,

        /// <summary>
        /// Muhasebe Temsilcisi
        /// - Ödeme işlemleri
        /// - Gider kayıtları
        /// - Finansal raporlar
        /// </summary>
        Muhasebe = 4,

        /// <summary>
        /// Teknik Servis Temsilcisi
        /// - Sorun kayıtları
        /// - Müşteri teknik destek
        /// - Teknik raporlar
        /// </summary>
        TeknikServis = 5
    }
}