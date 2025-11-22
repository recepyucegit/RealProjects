namespace Domain.Entities
{
    /// <summary>
    /// Kategori Entity
    /// Ürün kategorileri
    /// Örn: "Bilgisayar Donanımları", "Cep Telefonları", "Kameralar"
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Kategori Adı
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Kategori Açıklaması
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Kategori aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== NAVIGATION PROPERTIES ======

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
