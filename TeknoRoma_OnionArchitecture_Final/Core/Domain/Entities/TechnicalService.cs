using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Teknik Servis Entity
    /// </summary>
    public class TechnicalService : BaseEntity
    {
        /// <summary>
        /// Servis Numarası (Benzersiz)
        /// Format: "TS-2024-00001"
        /// </summary>
        public string ServiceNumber { get; set; } = null!;

        /// <summary>
        /// Sorun Başlığı
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Sorun Açıklaması
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Hangi mağaza? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Sorunu bildiren çalışan (Foreign Key)
        /// </summary>
        public int ReportedByEmployeeId { get; set; }

        /// <summary>
        /// Sorunu çözen teknik servis elemanı (Foreign Key)
        /// </summary>
        public int? AssignedToEmployeeId { get; set; }

        /// <summary>
        /// Müşteri sorunu mu?
        /// </summary>
        public bool IsCustomerIssue { get; set; }

        /// <summary>
        /// Hangi müşteri? (Foreign Key)
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Sorun Durumu
        /// </summary>
        public TechnicalServiceStatus Status { get; set; } = TechnicalServiceStatus.Acik;

        /// <summary>
        /// Öncelik Seviyesi (1-4)
        /// </summary>
        public int Priority { get; set; } = 2;

        /// <summary>
        /// Sorun bildirme tarihi
        /// </summary>
        public DateTime ReportedDate { get; set; }

        /// <summary>
        /// Çözüm tarihi
        /// </summary>
        public DateTime? ResolvedDate { get; set; }

        /// <summary>
        /// Çözüm açıklaması
        /// </summary>
        public string? Resolution { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        public virtual Store Store { get; set; } = null!;
        public virtual Employee ReportedByEmployee { get; set; } = null!;
        public virtual Employee? AssignedToEmployee { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
