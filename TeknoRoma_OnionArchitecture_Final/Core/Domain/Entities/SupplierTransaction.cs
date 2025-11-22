namespace Domain.Entities
{
    /// <summary>
    /// Tedarikçi Hareketi Entity
    /// Tedarikçiden ürün alımlarını kaydeder
    /// </summary>
    public class SupplierTransaction : BaseEntity
    {
        /// <summary>
        /// İşlem Numarası (Benzersiz)
        /// Format: "TH-2024-00001"
        /// </summary>
        public string TransactionNumber { get; set; } = null!;

        /// <summary>
        /// İşlem Tarihi
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Hangi tedarikçiden? (Foreign Key)
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Hangi ürün? (Foreign Key)
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Alınan miktar
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Birim fiyatı
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Toplam Tutar
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Fatura Numarası
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Açıklama/Not
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Ödeme yapıldı mı?
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Ödeme Tarihi
        /// </summary>
        public DateTime? PaymentDate { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
