namespace Domain.Entities
{
    /// <summary>
    /// Tedarikçi Entity
    /// TEKNOROMA'nın ürün aldığı tedarikçi firmalar
    /// </summary>
    public class Supplier : BaseEntity
    {
        /// <summary>
        /// Tedarikçi Firma Adı
        /// </summary>
        public string CompanyName { get; set; } = null!;

        /// <summary>
        /// Yetkili Kişi Adı
        /// </summary>
        public string ContactName { get; set; } = null!;

        /// <summary>
        /// Telefon numarası
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Email adresi
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Adres bilgisi
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Şehir
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Ülke
        /// </summary>
        public string Country { get; set; } = "Türkiye";

        /// <summary>
        /// Vergi Numarası
        /// </summary>
        public string TaxNumber { get; set; } = null!;

        /// <summary>
        /// Tedarikçi aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== NAVIGATION PROPERTIES ======

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();
    }
}
