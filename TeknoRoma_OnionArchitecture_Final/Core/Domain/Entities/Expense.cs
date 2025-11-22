using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Gider Entity
    /// </summary>
    public class Expense : BaseEntity
    {
        /// <summary>
        /// Gider Numarası (Benzersiz)
        /// Format: "G-2024-00001"
        /// </summary>
        public string ExpenseNumber { get; set; } = null!;

        /// <summary>
        /// Gider Tarihi
        /// </summary>
        public DateTime ExpenseDate { get; set; }

        /// <summary>
        /// Gider Türü
        /// </summary>
        public ExpenseType ExpenseType { get; set; }

        /// <summary>
        /// Hangi mağaza? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Hangi çalışana ödeme? (Foreign Key)
        /// Sadece ExpenseType = CalisanOdemesi ise dolu
        /// </summary>
        public int? EmployeeId { get; set; }

        /// <summary>
        /// Gider Tutarı
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Para Birimi
        /// </summary>
        public Currency Currency { get; set; } = Currency.TRY;

        /// <summary>
        /// Döviz Kuru
        /// </summary>
        public decimal? ExchangeRate { get; set; }

        /// <summary>
        /// TL karşılığı
        /// </summary>
        public decimal AmountInTRY { get; set; }

        /// <summary>
        /// Gider Açıklaması
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Fatura/Evrak Numarası
        /// </summary>
        public string? DocumentNumber { get; set; }

        /// <summary>
        /// Ödeme yapıldı mı?
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Ödeme Tarihi
        /// </summary>
        public DateTime? PaymentDate { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        public virtual Store Store { get; set; } = null!;
        public virtual Employee? Employee { get; set; }
    }
}
