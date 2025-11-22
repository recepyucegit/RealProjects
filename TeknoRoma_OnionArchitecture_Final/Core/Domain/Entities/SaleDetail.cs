namespace Domain.Entities
{
    /// <summary>
    /// Satış Detayı Entity (Satış Satırları)
    /// Sale ile Product arasındaki ilişkiyi çözer
    /// </summary>
    public class SaleDetail : BaseEntity
    {
        /// <summary>
        /// Hangi satışa ait? (Foreign Key)
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// Hangi ürün? (Foreign Key)
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Satış anındaki ürün adı (Snapshot)
        /// </summary>
        public string ProductName { get; set; } = null!;

        /// <summary>
        /// Satış anındaki birim fiyatı
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Satılan miktar
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// İndirim oranı (%)
        /// </summary>
        public decimal DiscountPercentage { get; set; } = 0;

        /// <summary>
        /// Ara Toplam (İndirim öncesi)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// İndirim Tutarı
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Net Tutar (İndirim sonrası)
        /// </summary>
        public decimal TotalAmount { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        public virtual Sale Sale { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
