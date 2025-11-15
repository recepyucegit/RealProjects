using Domain.Enums;
using static System.Formats.Asn1.AsnWriter;

namespace Domain.Entities
{
    /// <summary>
    /// Teknik Servis Entity
    /// Özgün Kablocu'nun (Teknik Servis Temsilcisi) yönettiği sorun kayıtları
    /// 
    /// 2 Tip Sorun:
    /// 1. Müşteri sorunları (Satış sonrası ürün arızası, teknik destek)
    /// 2. Sistem sorunları (Yazılım hatası, network sorunu)
    /// </summary>
    public class TechnicalService : BaseEntity
    {
        /// <summary>
        /// Servis Numarası (Benzersiz)
        /// Format: "TS-2024-00001"
        /// </summary>
        public string ServiceNumber { get; set; }

        /// <summary>
        /// Sorun Başlığı
        /// Kısa özet
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Sorun Açıklaması
        /// Detaylı açıklama
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Hangi mağaza? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Sorunu bildiren çalışan (Foreign Key)
        /// Özgün Kablocu: "Windows uygulamasından sorun bildirimi yapabilmeliler"
        /// </summary>
        public int ReportedByEmployeeId { get; set; }

        /// <summary>
        /// Sorunu çözen teknik servis elemanı (Foreign Key)
        /// Null olabilir (henüz atanmadıysa)
        /// </summary>
        public int? AssignedToEmployeeId { get; set; }

        /// <summary>
        /// Müşteri sorunu mu?
        /// true: Müşteri ürün arızası
        /// false: Şube içi teknik sorun (yazılım, donanım, network)
        /// </summary>
        public bool IsCustomerIssue { get; set; }

        /// <summary>
        /// Hangi müşteri? (Foreign Key)
        /// Sadece IsCustomerIssue = true ise dolu
        /// Null olabilir (sistem sorunlarında)
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Sorun Durumu
        /// Acik → Islemde → Tamamlandi/Cozulemedi
        /// </summary>
        public TechnicalServiceStatus Status { get; set; } = TechnicalServiceStatus.Acik;

        /// <summary>
        /// Öncelik Seviyesi
        /// 1: Düşük, 2: Orta, 3: Yüksek, 4: Kritik
        /// </summary>
        public int Priority { get; set; } = 2;

        /// <summary>
        /// Sorun bildirme tarihi
        /// </summary>
        public DateTime ReportedDate { get; set; }

        /// <summary>
        /// Çözüm tarihi
        /// Null olabilir (henüz çözülmediyse)
        /// </summary>
        public DateTime? ResolvedDate { get; set; }

        /// <summary>
        /// Çözüm açıklaması
        /// Ne yapıldı? Nasıl çözüldü?
        /// Null olabilir (henüz çözülmediyse)
        /// </summary>
        public string Resolution { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Sorunun bildirildiği mağaza
        /// Many-to-One ilişki
        /// </summary>
        public virtual Store Store { get; set; }

        /// <summary>
        /// Sorunu bildiren çalışan
        /// Many-to-One ilişki
        /// </summary>
        public virtual Employee ReportedByEmployee { get; set; }

        /// <summary>
        /// Sorunu çözen teknik servis elemanı
        /// Many-to-One ilişki
        /// Null olabilir
        /// </summary>
        public virtual Employee AssignedToEmployee { get; set; }

        /// <summary>
        /// Sorunu olan müşteri (müşteri sorunlarında)
        /// Many-to-One ilişki
        /// Null olabilir
        /// </summary>
        public virtual Customer Customer { get; set; }
    }
}