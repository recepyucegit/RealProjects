using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProductDTO
{
    /// <summary>
    /// Product DTO - Ürün bilgilerini göstermek için
    /// UI'a gönderilecek data
    /// 
    /// NEDEN Entity yerine DTO?
    /// - Entity'de navigation property'ler var (Category, Supplier)
    /// - Bunları JSON'a çevirirken circular reference hatası olur
    /// - DTO sadece gerekli bilgileri içerir
    /// </summary>
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public int CriticalStockLevel { get; set; }
        public StockStatus StockStatus { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }

        // Foreign Key değerleri
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }

        // İlişkili verilerin sadece isimleri (Görüntüleme için)
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }

        // Hesaplanmış property'ler
        public string StockStatusText => UnitsInStock <= 0
            ? "Tükendi"
            : UnitsInStock <= CriticalStockLevel
                ? "Kritik"
                : "Yeterli";

        public bool IsAvailable => UnitsInStock > 0 && IsActive;

        // Timestamp'ler
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Create Product DTO - Yeni ürün eklemek için
    /// 
    /// FARK: ID yok (Database otomatik oluşturuyor)
    /// FARK: CreatedDate yok (BaseEntity'de otomatik)
    /// FARK: StockStatus yok (Service layer hesaplıyor)
    /// </summary>
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Barkod numarası zorunludur")]
        [StringLength(50, ErrorMessage = "Barkod en fazla 50 karakter olabilir")]
        public string Barcode { get; set; }

        [Required(ErrorMessage = "Birim fiyat zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int UnitsInStock { get; set; }

        [Range(1, 1000, ErrorMessage = "Kritik seviye 1-1000 arasında olmalıdır")]
        public int CriticalStockLevel { get; set; } = 10;

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tedarikçi seçimi zorunludur")]
        public int SupplierId { get; set; }
    }

    /// <summary>
    /// Update Product DTO - Ürün güncellemek için
    /// 
    /// FARK CreateDTO'dan: ID var (Hangi ürün güncellenecek?)
    /// </summary>
    public class UpdateProductDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir")]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Barkod numarası zorunludur")]
        [StringLength(50)]
        public string Barcode { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int UnitsInStock { get; set; }

        [Range(1, 1000)]
        public int CriticalStockLevel { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Product List DTO - Liste görünümü için (Hafif)
    /// 
    /// NEDEN?
    /// - Liste görünümünde tüm bilgiler gerekmez
    /// - Performance: Daha az data transfer
    /// - UI'da sadece önemli bilgiler gösterilir
    /// </summary>
    public class ProductListDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public StockStatus StockStatus { get; set; }
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }
        public bool IsActive { get; set; }
    }
}