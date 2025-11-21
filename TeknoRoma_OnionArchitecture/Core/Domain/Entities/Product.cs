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
        /// Örn: "iPhone 15 Pro 256GB", "Samsung Galaxy S24", "Logitech MX Master 3"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Ürün Açıklaması
        /// Detaylı teknik özellikler
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Barkod Numarası
        /// UNIQUE constraint olacak
        /// Fahri Cepçi'nin istediği: "Ürün barkodunu okutup hızlıca bilgi alabilmeliyim"
        /// </summary>
        public string Barcode { get; set; } = null!;

        /// <summary>
        /// Birim Fiyatı (TL)
        /// Decimal: Para hesaplamaları için hassas tip
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Stok Miktarı
        /// Gül Satar: "Stokta olmayan ürün satılmamalı"
        /// </summary>
        public int UnitsInStock { get; set; }

        /// <summary>
        /// Kritik Stok Seviyesi
        /// Bu değerin altına düşerse uyarı verilir
        /// Gül Satar: "Kritik seviyenin altına düştüğünde uyarı vermeli"
        /// </summary>
        public int CriticalStockLevel { get; set; } = 10;

        /// <summary>
        /// Stok Durumu
        /// NEDEN hesaplanmış property değil?
        /// - Performance: Her sorguda hesaplama yapmak yerine hazır tutuyoruz
        /// - Filtreleme kolaylığı: SQL'de WHERE StockStatus = Kritik yazabiliyoruz
        /// Update/Insert trigger'da veya service layer'da otomatik set edilecek
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
        /// Haluk Bey: "Şuanda satmadığımız eskiden sattığımız ürünler"
        /// Artık satılmayan ürünler false olur ama kayıt silinmez
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ürün görseli (URL veya path)
        /// Opsiyonel
        /// </summary>
        public string? ImageUrl { get; set; }


        // ====== CALCULATED PROPERTIES ======
        // Runtime'da hesaplanır, database'e kaydedilmez

        /// <summary>
        /// Stok durumu metni
        /// UI'da gösterim için
        /// </summary>
        public string StockStatusText => UnitsInStock <= 0
            ? "Tükendi"
            : UnitsInStock <= CriticalStockLevel
                ? "Kritik"
                : "Yeterli";

        /// <summary>
        /// Stok yeterli mi?
        /// Satış yapılabilir mi kontrolü için
        /// </summary>
        public bool IsAvailable => UnitsInStock > 0 && IsActive;


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Ürünün kategorisi
        /// Many-to-One ilişki
        /// </summary>
        public virtual Category Category { get; set; } = null!;

        /// <summary>
        /// Ürünün tedarikçisi
        /// Many-to-One ilişki
        /// </summary>
        public virtual Supplier Supplier { get; set; } = null!;

        /// <summary>
        /// Bu ürünün satış detayları
        /// One-to-Many ilişki
        /// Haluk Bey'in raporu: "En çok satılan 10 ürün"
        /// </summary>
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

        /// <summary>
        /// Bu ürünün tedarikçi hareketleri
        /// Hangi tarihte ne kadar alındı?
        /// </summary>
        public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();
    }
}