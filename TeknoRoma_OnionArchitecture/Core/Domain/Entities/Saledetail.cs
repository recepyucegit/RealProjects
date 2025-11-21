namespace Domain.Entities
{
    /// <summary>
    /// Satış Detayı Entity (Satış Satırları)
    /// Sale ile Product arasındaki Many-to-Many ilişkiyi çözen junction table
    /// 
    /// Bir satışta birden fazla ürün olabilir
    /// Bir ürün birden fazla satışta olabilir
    /// 
    /// Örnek:
    /// SaleID=1 için:
    ///   - SaleDetail 1: iPhone 15 Pro, Miktar=2, BirimFiyat=50000
    ///   - SaleDetail 2: AirPods Pro, Miktar=1, BirimFiyat=8000
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
        /// Satış anındaki ürün adı
        /// NEDEN? Ürün adı sonradan değişebilir ama satış kaydı değişmemeli
        /// Snapshot: O anki ürün bilgisi
        /// </summary>
        public string ProductName { get; set; } = null!;

        /// <summary>
        /// Satış anındaki birim fiyatı
        /// NEDEN? Ürün fiyatı sonradan değişebilir
        /// Bu satışta ürün ne kadara satıldı?
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Satılan miktar
        /// Kaç adet satıldı?
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// İndirim oranı (%)
        /// Özel kampanya veya toplu alım indirimi
        /// 0-100 arası değer (Örn: 10 = %10 indirim)
        /// </summary>
        public decimal DiscountPercentage { get; set; } = 0;

        /// <summary>
        /// Ara Toplam (İndirim öncesi)
        /// Hesaplama: UnitPrice * Quantity
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// İndirim Tutarı
        /// Hesaplama: Subtotal * (DiscountPercentage / 100)
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Net Tutar (İndirim sonrası)
        /// Hesaplama: Subtotal - DiscountAmount
        /// Bu satırın toplam tutarı
        /// </summary>
        public decimal TotalAmount { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Bağlı olduğu satış
        /// Many-to-One ilişki
        /// </summary>
        public virtual Sale Sale { get; set; } = null!;

        /// <summary>
        /// Satılan ürün
        /// Many-to-One ilişki
        /// </summary>
        public virtual Product Product { get; set; } = null!;
    }
}