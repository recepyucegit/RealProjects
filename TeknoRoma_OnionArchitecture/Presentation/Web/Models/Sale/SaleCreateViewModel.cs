using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Web.Models.Sale
{
    /// <summary>
    /// Satış Oluşturma ViewModel
    ///
    /// KULLANIM:
    /// - Gül Satar (Kasa Satış): Kasada satış yapar
    /// - Fahri Cepçi (Mobil Satış): Mobil satış yapar
    ///
    /// İŞ AKIŞI:
    /// 1. Müşteri seç (TC Kimlik ile ara)
    /// 2. Ürünleri sepete ekle (Barkod ile ara)
    /// 3. İndirim uygula (varsa)
    /// 4. Ödeme türü seç
    /// 5. Satışı tamamla
    ///
    /// HESAPLAMALAR:
    /// - Ara Toplam = ∑ (Ürün Fiyatı × Miktar)
    /// - KDV = Ara Toplam × %20
    /// - İndirim = Ara Toplam × (İndirim Yüzdesi / 100)
    /// - Genel Toplam = Ara Toplam + KDV - İndirim
    /// </summary>
    public class SaleCreateViewModel
    {
        // ====== MÜŞTERİ BİLGİLERİ ======

        [Required(ErrorMessage = "Müşteri seçimi zorunludur")]
        [Display(Name = "Müşteri")]
        public int CustomerId { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerIdentityNumber { get; set; }


        // ====== ÜRÜNLER (SEPET) ======

        public List<SaleItemViewModel> Items { get; set; } = new();


        // ====== ÖDEME BİLGİLERİ ======

        [Required(ErrorMessage = "Ödeme türü seçimi zorunludur")]
        [Display(Name = "Ödeme Türü")]
        public PaymentType PaymentType { get; set; }

        [Display(Name = "İndirim Yüzdesi")]
        [Range(0, 100, ErrorMessage = "İndirim %0 ile %100 arasında olmalıdır")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "Kasa Numarası")]
        public string? CashRegisterNumber { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(1000)]
        public string? Notes { get; set; }


        // ====== HESAPLANMIŞ DEĞERLER (ReadOnly) ======

        [Display(Name = "Ara Toplam")]
        public decimal Subtotal => Items.Sum(i => i.TotalAmount);

        [Display(Name = "KDV (%20)")]
        public decimal TaxAmount => Subtotal * 0.20m;

        [Display(Name = "İndirim Tutarı")]
        public decimal DiscountAmount => Subtotal * (DiscountPercentage / 100);

        [Display(Name = "Genel Toplam")]
        public decimal TotalAmount => Subtotal + TaxAmount - DiscountAmount;
    }

    /// <summary>
    /// Satış Satırı (Sepetteki ürün)
    /// </summary>
    public class SaleItemViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Ürün Adı")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Barkod")]
        public string Barcode { get; set; } = string.Empty;

        [Display(Name = "Birim Fiyat")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Miktar giriniz")]
        [Range(1, 1000, ErrorMessage = "Miktar 1 ile 1000 arasında olmalıdır")]
        [Display(Name = "Miktar")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "İndirim %")]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "Ara Toplam")]
        public decimal Subtotal => UnitPrice * Quantity;

        [Display(Name = "İndirim Tutarı")]
        public decimal DiscountAmount => Subtotal * (DiscountPercentage / 100);

        [Display(Name = "Toplam")]
        public decimal TotalAmount => Subtotal - DiscountAmount;

        // Stok kontrolü için
        public int AvailableStock { get; set; }
    }
}
