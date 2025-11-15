

namespace Domain.Entities
{
    /// <summary>
    /// Kategori Entity
    /// Ürün kategorileri
    /// Örn: "Bilgisayar Donanımları", "Cep Telefonları", "Kameralar", "Fotoğraf Makinaları"
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Kategori Adı
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Kategori Açıklaması
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Kategori aktif mi?
        /// Artık kullanılmayan kategoriler için false
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Bu kategorideki ürünler
        /// One-to-Many ilişki (Bir kategoride birden fazla ürün)
        /// Haluk Bey'in istediği rapor: "Kategorilere göre ürünlerin listesi"
        /// </summary>
        public virtual ICollection<Product> Products { get; set; }
    }
}