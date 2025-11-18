using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SaleDTO
{
    /// <summary>
    /// Sale DTO - Satış bilgilerini göstermek için
    /// </summary>
    public class SaleDTO
    {
        public int Id { get; set; }
        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public SaleStatus Status { get; set; }
        public PaymentType PaymentType { get; set; }
        public string CashRegisterNumber { get; set; }
        public string Notes { get; set; }

        // Fiyat bilgileri
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // İlişkili veriler
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerIdentityNumber { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        public int StoreId { get; set; }
        public string StoreName { get; set; }

        // Satış detayları (Satılan ürünler)
        // NEDEN List?
        /// - Bir satışta birden fazla ürün olabilir
        public List<SaleDetailDTO> SaleDetails { get; set; }

        // Timestamp'ler
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Sale Detail DTO - Satış satırları
    /// Sepetteki her ürün bir SaleDetail
    /// </summary>
    public class SaleDetailDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductBarcode { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Create Sale DTO - Yeni satış oluşturmak için
    /// 
    /// İŞ AKIŞI:
    /// 1. Kullanıcı müşteri seçer (TC girer)
    /// 2. Ürünleri sepete ekler (Barkod okutarak)
    /// 3. Ödeme türünü seçer
    /// 4. Satış oluşturulur
    /// </summary>
    public class CreateSaleDTO
    {
        [Required(ErrorMessage = "Müşteri seçimi zorunludur")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Çalışan bilgisi zorunludur")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Mağaza bilgisi zorunludur")]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "Ödeme türü seçimi zorunludur")]
        public PaymentType PaymentType { get; set; }

        [StringLength(50)]
        public string CashRegisterNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Satış detayları (Sepetteki ürünler)
        /// VALIDATION: En az 1 ürün olmalı
        /// </summary>
        [Required(ErrorMessage = "En az 1 ürün eklemelisiniz")]
        [MinLength(1, ErrorMessage = "En az 1 ürün eklemelisiniz")]
        public List<CreateSaleDetailDTO> SaleDetails { get; set; }
    }

    /// <summary>
    /// Create Sale Detail DTO - Sepete ürün eklemek için
    /// </summary>
    public class CreateSaleDetailDTO
    {
        [Required(ErrorMessage = "Ürün seçimi zorunludur")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır")]
        public int Quantity { get; set; }

        [Range(0, 100, ErrorMessage = "İndirim oranı 0-100 arasında olmalıdır")]
        public decimal DiscountPercentage { get; set; } = 0;

        // NOT: UnitPrice service layer'da Product'tan alınacak
        // Kullanıcı fiyat girmez, sistem otomatik çeker
    }

    /// <summary>
    /// Update Sale DTO
    /// DİKKAT: Sadece belirli durumda güncellenebilir
    /// </summary>
    public class UpdateSaleDTO
    {
        [Required]
        public int Id { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Sadece notlar ve bazı bilgiler güncellenebilir
        // Ürünler ve fiyatlar değiştirilemez (İş kuralı)
    }

    /// <summary>
    /// Sale List DTO - Liste görünümü için (Hafif)
    /// </summary>
    public class SaleListDTO
    {
        public int Id { get; set; }
        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public SaleStatus Status { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; } // Kaç ürün satıldı?
    }
}