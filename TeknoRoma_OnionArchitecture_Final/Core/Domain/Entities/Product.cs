using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Ürün Entity
    /// Elektronik ürünler: Bilgisayar donanımları, cep telefonları, kameralar vb.
    /// </summary>
    public class Product : BaseEntity
    {
        /// <summary>
        /// Ürün Adı
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Ürün Açıklaması
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Barkod Numarası (UNIQUE)
        /// </summary>
        public string Barcode { get; set; } = null!;

        /// <summary>
        /// Birim Fiyatı (TL)
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Stok Miktarı
        /// </summary>
        public int UnitsInStock { get; set; }

        /// <summary>
        /// Kritik Stok Seviyesi
        /// </summary>
        public int CriticalStockLevel { get; set; } = 10;

        /// <summary>
        /// Stok Durumu
        /// </summary>
        public StockStatus StockStatus { get; set; }

        /// <summary>
        /// Hangi kategoride? (Foreign Key)
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Hangi tedarikçiden? (Foreign Key)
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Ürün aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ürün görseli (URL veya path)
        /// </summary>
        public string? ImageUrl { get; set; }


        // ====== CALCULATED PROPERTIES ======

        public string StockStatusText => UnitsInStock <= 0
            ? "Tükendi"
            : UnitsInStock <= CriticalStockLevel
                ? "Kritik"
                : "Yeterli";

        public bool IsAvailable => UnitsInStock > 0 && IsActive;


        // ====== NAVIGATION PROPERTIES ======

        public virtual Category Category { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
        public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();
    }
}
