using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Satış Entity (Satış Başlığı)
    /// </summary>
    public class Sale : BaseEntity
    {
        /// <summary>
        /// Satış Numarası (Benzersiz)
        /// Format: "S-2024-00001"
        /// </summary>
        public string SaleNumber { get; set; } = null!;

        /// <summary>
        /// Satış Tarihi
        /// </summary>
        public DateTime SaleDate { get; set; }

        /// <summary>
        /// Hangi müşteriye yapıldı? (Foreign Key)
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Hangi çalışan yaptı? (Foreign Key)
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        /// Hangi mağazada yapıldı? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Satış Durumu
        /// </summary>
        public SaleStatus Status { get; set; } = SaleStatus.Beklemede;

        /// <summary>
        /// Ödeme Türü
        /// </summary>
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Ara Toplam
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// KDV Toplamı
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// İndirim Tutarı
        /// </summary>
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Genel Toplam
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Hangi kasadan yapıldı?
        /// </summary>
        public string? CashRegisterNumber { get; set; }

        /// <summary>
        /// Satış notu
        /// </summary>
        public string? Notes { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        public virtual Customer Customer { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;
        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
