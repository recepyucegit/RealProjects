namespace TeknoRoma.Entities.Enums;

/// <summary>
/// Kullanıcı Rolleri
/// NEDEN ENUM?
/// - Roller sabit ve sınırlı (5 rol var)
/// - Database'de int olarak saklanır (1, 2, 3, 4, 5) - Performans
/// - String yerine enum kullanmak tip güvenliği sağlar
/// - Magic string'lerden kaçınırız
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Şube Müdürü - Haluk Bey gibi
    /// Tüm yetkilere sahip:
    /// - Tedarikçi, kategori, ürün, çalışan tanımlamaları
    /// - Tüm raporlara erişim
    /// - Çalışan yönetimi
    /// </summary>
    SubeYoneticisi = 1,

    /// <summary>
    /// Kasa Satış Temsilcisi - Gül Satar gibi
    /// Yetkiler:
    /// - Satış işlemleri
    /// - Müşteri tanımlama
    /// - Satış raporları görme
    /// - Ürün arama ve fiyat sorgulama
    /// </summary>
    KasaSatis = 2,

    /// <summary>
    /// Depo Temsilcisi - Durna Sabit gibi
    /// Yetkiler:
    /// - Satış sonrası ürün hazırlama
    /// - Stok kontrol
    /// - Kritik stok raporları
    /// - Ürün girişi ve sevkiyatı
    /// </summary>
    Depo = 3,

    /// <summary>
    /// Muhasebe Temsilcisi - Feyza Paragöz gibi
    /// Yetkiler:
    /// - Ödeme işlemleri
    /// - Gider kayıtları
    /// - Finansal raporlar
    /// - Maaş ödemeleri
    /// </summary>
    Muhasebe = 4,

    /// <summary>
    /// Teknik Servis Temsilcisi - Özgün Kablocu gibi
    /// Yetkiler:
    /// - Sorun kayıtları
    /// - Müşteri teknik destek
    /// - Teknik raporlar
    /// - Arızalı ürün takibi
    /// </summary>
    TeknikServis = 5
}
