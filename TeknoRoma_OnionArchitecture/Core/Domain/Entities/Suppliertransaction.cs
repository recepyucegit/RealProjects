namespace Domain.Entities
{
    /// <summary>
    /// Tedarikçi Hareketi Entity
    /// Tedarikçiden ürün alımlarını kaydeder
    /// 
    /// Haluk Bey'in istediği rapor:
    /// "Hangi tedarikçiden bu ay hangi ürünleri ne kadar almışız, toplamda ne kadar alım yapmışız"
    /// 
    /// Şube Müdürü tedarikçi hareketlerini tanımlar
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
        /// Ürünler ne zaman alındı?
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
        /// Kaç adet ürün alındı?
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Birim fiyatı
        /// Tedarikçiden alış fiyatı
        /// Satış fiyatından farklı olabilir
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Toplam Tutar
        /// Hesaplama: UnitPrice * Quantity
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Fatura Numarası
        /// Tedarikçinin fatura numarası
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Açıklama/Not
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Ödeme yapıldı mı?
        /// Muhasebe takibi için
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Ödeme Tarihi
        /// Null olabilir (henüz ödeme yapılmadıysa)
        /// </summary>
        public DateTime? PaymentDate { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// İşlemin yapıldığı tedarikçi
        /// Many-to-One ilişki
        /// </summary>
        public virtual Supplier Supplier { get; set; } = null!;

        /// <summary>
        /// Alınan ürün
        /// Many-to-One ilişki
        /// </summary>
        public virtual Product Product { get; set; } = null!;
    }
}