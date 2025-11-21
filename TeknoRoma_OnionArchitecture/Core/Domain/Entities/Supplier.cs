namespace Domain.Entities
{
    /// <summary>
    /// Tedarikçi Entity
    /// TEKNOROMA'nın ürün aldığı tedarikçi firmalar
    /// Örn: Apple, Samsung, Logitech, HP, Dell
    /// </summary>
    public class Supplier : BaseEntity
    {
        /// <summary>
        /// Tedarikçi Firma Adı
        /// </summary>
        public string CompanyName { get; set; } = null!;

        /// <summary>
        /// Yetkili Kişi Adı
        /// Tedarikçi firmayla iletişim kuracak kişi
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
        /// NEDEN? Yurtdışı tedarikçiler olabilir
        /// </summary>
        public string Country { get; set; } = "Türkiye";

        /// <summary>
        /// Vergi Numarası
        /// Muhasebe için gerekli
        /// </summary>
        public string TaxNumber { get; set; } = null!;

        /// <summary>
        /// Tedarikçi aktif mi?
        /// Artık çalışılmayan tedarikçiler için false
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Bu tedarikçiden alınan ürünler
        /// One-to-Many ilişki (Bir tedarikçiden birden fazla ürün)
        /// </summary>
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        /// <summary>
        /// Bu tedarikçiyle yapılan alım hareketleri
        /// Haluk Bey'in istediği: "Hangi tedarikçiden ne kadar almışız"
        /// </summary>
        public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();
    }
}