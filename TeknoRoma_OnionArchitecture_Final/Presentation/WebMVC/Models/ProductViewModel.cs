// ===================================================================================
// TEKNOROMA MVC - PRODUCT VIEW MODEL
// ===================================================================================
//
// Urun Create/Edit formlari icin view model.
// Validation attribute'lari ile form dogrulamasi saglar.
//
// ===================================================================================

using System.ComponentModel.DataAnnotations;

namespace WebMVC.Models
{
    /// <summary>
    /// Urun View Model
    /// </summary>
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Urun adi zorunludur.")]
        [StringLength(500, ErrorMessage = "Urun adi en fazla 500 karakter olabilir.")]
        [Display(Name = "Urun Adi")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Aciklama")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Barkod zorunludur.")]
        [StringLength(50, ErrorMessage = "Barkod en fazla 50 karakter olabilir.")]
        [Display(Name = "Barkod")]
        public string Barcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, 9999999.99, ErrorMessage = "Fiyat 0.01 ile 9,999,999.99 arasinda olmalidir.")]
        [Display(Name = "Birim Fiyat (TL)")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stok miktari zorunludur.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktari negatif olamaz.")]
        [Display(Name = "Stok Miktari")]
        public int UnitsInStock { get; set; }

        [Required(ErrorMessage = "Kritik stok seviyesi zorunludur.")]
        [Range(1, 10000, ErrorMessage = "Kritik stok seviyesi 1 ile 10,000 arasinda olmalidir.")]
        [Display(Name = "Kritik Stok Seviyesi")]
        public int CriticalStockLevel { get; set; } = 10;

        [Required(ErrorMessage = "Kategori secimi zorunludur.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tedarikci secimi zorunludur.")]
        [Display(Name = "Tedarikci")]
        public int SupplierId { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Gorsel URL")]
        [Url(ErrorMessage = "Gecerli bir URL giriniz.")]
        public string? ImageUrl { get; set; }
    }
}
