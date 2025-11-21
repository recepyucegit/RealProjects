using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Web.Models.Product
{
    /// <summary>
    /// Ürün Arama ViewModel
    ///
    /// KULLANIM:
    /// - Fahri Cepçi: Barkod okutarak ürün bulur
    /// - Gül Satar: Ürün adı ile arar
    /// - Kerim Zulacı: Stok kontrolü yapar
    ///
    /// ARAMA KRİTERLERİ:
    /// - Barkod (Öncelikli)
    /// - Ürün Adı
    /// - Kategori
    /// - Stok Durumu
    /// </summary>
    public class ProductSearchViewModel
    {
        [Display(Name = "Barkod")]
        public string? Barcode { get; set; }

        [Display(Name = "Ürün Adı")]
        public string? ProductName { get; set; }

        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }

        [Display(Name = "Stok Durumu")]
        public StockStatus? StockStatus { get; set; }

        [Display(Name = "Sadece Aktif Ürünler")]
        public bool OnlyActive { get; set; } = true;

        // Arama sonuçları
        public List<ProductListItemViewModel> Results { get; set; } = new();
    }

    /// <summary>
    /// Ürün Liste Elemanı
    /// Arama sonuçlarında gösterilir
    /// </summary>
    public class ProductListItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Ürün Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Barkod")]
        public string Barcode { get; set; } = string.Empty;

        [Display(Name = "Kategori")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Tedarikçi")]
        public string SupplierName { get; set; } = string.Empty;

        [Display(Name = "Birim Fiyat")]
        [DisplayFormat(DataFormatString = "{0:N2} TL")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Stok")]
        public int UnitsInStock { get; set; }

        [Display(Name = "Kritik Seviye")]
        public int CriticalStockLevel { get; set; }

        [Display(Name = "Stok Durumu")]
        public string StockStatusText { get; set; } = string.Empty;

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        // Stok uyarısı için CSS class
        public string StockStatusClass
        {
            get
            {
                if (UnitsInStock == 0) return "badge bg-danger";
                if (UnitsInStock <= CriticalStockLevel) return "badge bg-warning";
                return "badge bg-success";
            }
        }
    }

    /// <summary>
    /// Ürün Detay ViewModel
    /// Fahri Cepçi barkod okuttuğunda gösterir
    /// </summary>
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:N2} TL")]
        public decimal UnitPrice { get; set; }

        public int UnitsInStock { get; set; }
        public int CriticalStockLevel { get; set; }
        public string StockStatusText { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }

        // Sepete eklenebilir mi?
        public bool CanAddToCart => IsAvailable && UnitsInStock > 0;
    }
}
