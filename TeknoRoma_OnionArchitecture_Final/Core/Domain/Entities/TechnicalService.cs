// ============================================================================
// TechnicalService.cs - Teknik Servis Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın teknik servis/destek taleplerini yöneten entity.
// Hem iç (mağaza) hem de müşteri kaynaklı sorunları takip eder.
//
// SERVİS TÜRLERİ:
// 1. İç Sorunlar (IsCustomerIssue = false):
//    - Mağaza ekipman arızaları (POS, bilgisayar, yazıcı)
//    - Altyapı sorunları (internet, elektrik)
//    - Yazılım sorunları
//
// 2. Müşteri Sorunları (IsCustomerIssue = true):
//    - Satılan ürünlerin garanti/tamir işlemi
//    - Müşteri şikayetleri
//    - Ürün iade/değişim talepleri
//
// İŞ KURALLARI:
// - Her talep benzersiz numara alır
// - Öncelik seviyeleri: 1 (Kritik) - 4 (Düşük)
// - Durum takibi: Açık → Atandı → İşlemde → Çözüldü → Kapatıldı
// ============================================================================

using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Teknik Servis Entity Sınıfı
    ///
    /// TİCKET SİSTEMİ:
    /// - Her sorun bir "ticket" olarak açılır
    /// - Atama, önceliklendirme, takip
    /// - SLA (Service Level Agreement) takibi yapılabilir
    ///
    /// WORKFLOW:
    /// 1. Sorun bildirimi (ReportedByEmployee)
    /// 2. Teknisyene atama (AssignedToEmployee)
    /// 3. Çözüm ve kapatma (Resolution, ResolvedDate)
    /// </summary>
    public class TechnicalService : BaseEntity
    {
        // ====================================================================
        // SERVİS TANIMLAYICI
        // ====================================================================

        /// <summary>
        /// Servis Numarası (Benzersiz)
        ///
        /// AÇIKLAMA:
        /// - Her servis talebine verilen benzersiz referans
        /// - Müşteri iletişiminde ve takipte kullanılır
        ///
        /// FORMAT: "TS-YYYY-NNNNN"
        /// - TS: Technical Service prefix'i
        /// - YYYY: Yıl (2024)
        /// - NNNNN: Sıra numarası
        /// - Örn: "TS-2024-00001", "TS-2024-00789"
        ///
        /// MÜŞTERİ İLETİŞİMİ:
        /// "Servis numaranız TS-2024-00789. Bu numara ile
        ///  durumu sorgulayabilirsiniz."
        /// </summary>
        public string ServiceNumber { get; set; } = null!;

        // ====================================================================
        // SORUN BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Sorun Başlığı
        ///
        /// AÇIKLAMA:
        /// - Sorunun kısa özeti
        /// - Listelemelerde görünür
        /// - Maksimum 100-200 karakter önerilir
        ///
        /// ÖRNEKLER:
        /// - "POS Cihazı Çalışmıyor"
        /// - "iPhone Ekran Kırığı - Garanti"
        /// - "Mağaza İnterneti Kesik"
        /// - "Müşteri Ürün İade Talebi"
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Sorun Açıklaması
        ///
        /// AÇIKLAMA:
        /// - Sorunun detaylı tanımı
        /// - Ne zaman başladı, nasıl oluştu
        /// - Yapılan ilk müdahaleler
        ///
        /// ÖRNEK:
        /// "Kasa 2 numaralı POS cihazı sabahtan beri açılmıyor.
        ///  Elektrik bağlantısı kontrol edildi, sorun devam ediyor.
        ///  Cihaz 6 aylık, garanti kapsamında."
        /// </summary>
        public string Description { get; set; } = null!;

        // ====================================================================
        // İLİŞKİSEL ALANLAR (FOREIGN KEYS)
        // ====================================================================

        /// <summary>
        /// Mağaza ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Sorunun bildirildiği mağaza
        /// - Stores tablosundaki Id ile eşleşir
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Bildiren Çalışan ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Sorunu sisteme giren çalışan
        /// - Employees tablosundaki Id ile eşleşir
        /// - Zorunlu alan (her kayıt birileri tarafından açılır)
        ///
        /// İZLENEBİLİRLİK:
        /// - Kim ne zaman bildirdi?
        /// - Geri bildirim için iletişim
        /// </summary>
        public int ReportedByEmployeeId { get; set; }

        /// <summary>
        /// Atanan Teknisyen ID (Foreign Key - Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Sorunu çözmekle görevli teknisyen
        /// - Employees tablosundaki Id ile eşleşir
        /// - Başlangıçta null (henüz atanmamış)
        ///
        /// NULLABLE (int?):
        /// - Yeni açılan taleplerde null
        /// - Atama yapıldığında set edilir
        ///
        /// ATAMA SÜRECİ:
        /// 1. Talep açılır (AssignedToEmployeeId = null)
        /// 2. Yönetici teknisyen atar
        /// 3. Status: Acik → Atandi
        /// </summary>
        public int? AssignedToEmployeeId { get; set; }

        // ====================================================================
        // MÜŞTERİ İLİŞKİSİ
        // ====================================================================

        /// <summary>
        /// Müşteri Sorunu mu?
        ///
        /// AÇIKLAMA:
        /// - true: Müşteriden gelen talep (garanti, tamir, şikayet)
        /// - false: İç sorun (mağaza ekipman, altyapı)
        ///
        /// AYRIMI NEDEN ÖNEMLİ?
        /// - Raporlama: İç sorunlar vs Müşteri talepleri
        /// - SLA: Müşteri taleplerinde daha sıkı süre takibi
        /// - İletişim: Müşteriye bilgilendirme gerekli
        /// </summary>
        public bool IsCustomerIssue { get; set; }

        /// <summary>
        /// Müşteri ID (Foreign Key - Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Müşteri sorunuysa, hangi müşteri?
        /// - Customers tablosundaki Id ile eşleşir
        ///
        /// NULLABLE (int?):
        /// - IsCustomerIssue = false ise null
        /// - IsCustomerIssue = true ise dolu olmalı
        ///
        /// DOĞRULAMA:
        /// if (IsCustomerIssue && !CustomerId.HasValue)
        ///     throw new ValidationException("Müşteri sorunu için müşteri seçilmeli!");
        /// </summary>
        public int? CustomerId { get; set; }

        // ====================================================================
        // DURUM VE ÖNCELİK
        // ====================================================================

        /// <summary>
        /// Servis Durumu (Enum)
        ///
        /// AÇIKLAMA:
        /// - Talebin mevcut durumu
        /// - TechnicalServiceStatus enum'undan değer alır
        ///
        /// DURUM DEĞERLERİ:
        /// - Acik (0): Yeni açılmış, henüz atanmamış
        /// - Atandi (1): Teknisyene atandı
        /// - Islemde (2): Teknisyen üzerinde çalışıyor
        /// - Beklemede (3): Parça/bilgi bekleniyor
        /// - Cozuldu (4): Sorun giderildi
        /// - Kapatildi (5): Talep kapatıldı
        ///
        /// VARSAYILAN:
        /// - "= TechnicalServiceStatus.Acik" ile başlar
        ///
        /// DURUM GEÇİŞLERİ:
        /// Acik → Atandi → Islemde → Cozuldu → Kapatildi
        ///                    ↓
        ///               Beklemede
        /// </summary>
        public TechnicalServiceStatus Status { get; set; } = TechnicalServiceStatus.Acik;

        /// <summary>
        /// Öncelik Seviyesi (1-4)
        ///
        /// AÇIKLAMA:
        /// - Talebin aciliyet derecesi
        /// - 1: Kritik (Hemen müdahale)
        /// - 2: Yüksek (Aynı gün)
        /// - 3: Normal (2-3 gün)
        /// - 4: Düşük (1 hafta)
        ///
        /// VARSAYILAN:
        /// - "= 2" ile Yüksek öncelik başlar
        /// - Çoğu talep yüksek öncelikli kabul edilir
        ///
        /// SLA (Service Level Agreement):
        /// - Öncelik 1: 2 saat içinde müdahale
        /// - Öncelik 2: 4 saat içinde müdahale
        /// - Öncelik 3: 24 saat içinde müdahale
        /// - Öncelik 4: 72 saat içinde müdahale
        /// </summary>
        public int Priority { get; set; } = 2;

        // ====================================================================
        // TARİH BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Bildirim Tarihi
        ///
        /// AÇIKLAMA:
        /// - Sorunun sisteme girildiği tarih/saat
        /// - SLA hesaplaması bu tarihten başlar
        ///
        /// OTOMATİK ATAMA:
        /// - Genellikle DateTime.Now ile set edilir
        /// - CreatedDate ile aynı olabilir
        /// </summary>
        public DateTime ReportedDate { get; set; }

        /// <summary>
        /// Çözüm Tarihi (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Sorunun çözüldüğü tarih/saat
        /// - Status = Cozuldu olduğunda set edilir
        ///
        /// NULLABLE (DateTime?):
        /// - Henüz çözülmediyse null
        ///
        /// PERFORMANS METRİĞİ:
        /// - Çözüm Süresi = ResolvedDate - ReportedDate
        /// - Ortalama çözüm süreleri raporlanır
        /// </summary>
        public DateTime? ResolvedDate { get; set; }

        // ====================================================================
        // ÇÖZÜM BİLGİSİ
        // ====================================================================

        /// <summary>
        /// Çözüm Açıklaması (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Sorunun nasıl çözüldüğü
        /// - Yapılan işlemler
        /// - Bilgi bankası için değerli
        ///
        /// ÖRNEK:
        /// "POS cihazının güç adaptörü arızalıydı.
        ///  Yeni adaptör takıldı, cihaz çalışıyor.
        ///  Garanti kapsamında değişim yapıldı."
        ///
        /// BİLGİ BANKASI:
        /// - Benzer sorunlarda referans olur
        /// - Yeni teknisyenler için eğitim materyali
        /// </summary>
        public string? Resolution { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Mağaza (Navigation Property)
        ///
        /// İLİŞKİ: TechnicalService (N) → Store (1)
        /// </summary>
        public virtual Store Store { get; set; } = null!;

        /// <summary>
        /// Bildiren Çalışan (Navigation Property)
        ///
        /// İLİŞKİ: TechnicalService (N) → Employee (1)
        ///
        /// İSİMLENDİRME AÇIKLAMASI:
        /// - "ReportedByEmployee" = Bildiren çalışan
        /// - Rol belirtir: Kim bildirdi?
        /// </summary>
        public virtual Employee ReportedByEmployee { get; set; } = null!;

        /// <summary>
        /// Atanan Teknisyen (Navigation Property - Opsiyonel)
        ///
        /// İLİŞKİ: TechnicalService (N) → Employee (1)
        ///
        /// NULLABLE (Employee?):
        /// - Henüz atanmadıysa null
        ///
        /// İSİMLENDİRME:
        /// - "AssignedToEmployee" = Atanan çalışan
        /// - Kime atandı?
        /// </summary>
        public virtual Employee? AssignedToEmployee { get; set; }

        /// <summary>
        /// İlişkili Müşteri (Navigation Property - Opsiyonel)
        ///
        /// İLİŞKİ: TechnicalService (N) → Customer (1)
        ///
        /// NULLABLE (Customer?):
        /// - Müşteri sorunu değilse null
        /// </summary>
        public virtual Customer? Customer { get; set; }
    }
}
