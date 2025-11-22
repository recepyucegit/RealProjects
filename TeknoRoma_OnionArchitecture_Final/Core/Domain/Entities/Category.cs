// ============================================================================
// Category.cs - Kategori Entity
// ============================================================================
// AÇIKLAMA:
// Ürünlerin gruplandırıldığı kategorileri temsil eden entity.
// Her ürün bir kategoriye aittir (zorunlu ilişki).
//
// ÖRNEK KATEGORİLER:
// - Bilgisayar Donanımları (Laptop, Masaüstü, Monitör)
// - Cep Telefonları (iPhone, Samsung, Xiaomi)
// - Tablet ve iPad
// - Kameralar (DSLR, Aynasız, Kompakt)
// - Beyaz Eşya
// - Küçük Ev Aletleri
// - TV ve Görüntü Sistemleri
// - Oyun Konsolları
//
// İŞ KURALLARI:
// - Kategori adı benzersiz olmalı
// - Kategorisi olmayan ürün eklenemez
// - Kategori silinmeden önce ürünler başka kategoriye taşınmalı
// ============================================================================

namespace Domain.Entities
{
    /// <summary>
    /// Kategori Entity Sınıfı
    ///
    /// HİYERARŞİK KATEGORİ:
    /// - Mevcut tasarım tek seviyeli (flat)
    /// - Hiyerarşik yapı için ParentCategoryId eklenebilir
    /// - Örn: Elektronik → Telefon → Akıllı Telefon
    ///
    /// SELF-REFERENCING İLİŞKİ (Opsiyonel):
    /// public int? ParentCategoryId { get; set; }
    /// public virtual Category? ParentCategory { get; set; }
    /// public virtual ICollection<Category> SubCategories { get; set; }
    /// </summary>
    public class Category : BaseEntity
    {
        // ====================================================================
        // TEMEL BİLGİLER
        // ====================================================================

        /// <summary>
        /// Kategori Adı
        ///
        /// AÇIKLAMA:
        /// - Kategorinin görünen adı
        /// - Benzersiz olmalı (unique constraint)
        /// - Menülerde, filtrelerde ve raporlarda kullanılır
        ///
        /// ÖRNEKLER:
        /// - "Cep Telefonları"
        /// - "Laptop & Notebook"
        /// - "TV & Görüntü"
        ///
        /// SEO DOSTU İSİMLENDİRME:
        /// - Slug alanı eklenebilir: "cep-telefonlari"
        /// - URL'de kullanılır: /kategori/cep-telefonlari
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Kategori Açıklaması (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Kategorinin detaylı tanımı
        /// - SEO için meta description olarak kullanılabilir
        /// - Kullanıcıya kategori hakkında bilgi verir
        ///
        /// NULLABLE (string?):
        /// - Açıklama zorunlu değil
        /// - "?" işareti null olabileceğini belirtir
        ///
        /// ÖRNEK:
        /// "En son teknoloji akıllı telefonlar. iPhone, Samsung, Xiaomi
        ///  ve daha fazla marka burada."
        /// </summary>
        public string? Description { get; set; }

        // ====================================================================
        // DURUM
        // ====================================================================

        /// <summary>
        /// Kategori Aktif mi?
        ///
        /// AÇIKLAMA:
        /// - true: Kategori görünür ve kullanılabilir
        /// - false: Kategori gizli, yeni ürün eklenemez
        ///
        /// KULLANIM:
        /// - Sezonluk kategoriler (Yaz Ürünleri, Kış Ürünleri)
        /// - Yayından kaldırılan kategoriler
        /// - Test kategorileri
        ///
        /// IsActive vs IsDeleted:
        /// - IsActive=false: Geçici olarak gizli
        /// - IsDeleted=true: Kalıcı olarak silinmiş
        ///
        /// VARSAYILAN DEĞER:
        /// - "= true" ile aktif olarak başlar
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Kategorideki Ürünler (Collection Navigation Property)
        ///
        /// İLİŞKİ: Category (1) → Product (N)
        /// - Bir kategoride birden fazla ürün olabilir
        /// - One-to-Many ilişki
        ///
        /// KULLANIM:
        /// // Kategorideki ürün sayısı
        /// var productCount = category.Products.Count;
        ///
        /// // Kategorideki aktif ürünler
        /// var activeProducts = category.Products
        ///     .Where(p => p.IsActive && !p.IsDeleted)
        ///     .ToList();
        ///
        /// // Kategorinin toplam stok değeri
        /// var stockValue = category.Products
        ///     .Sum(p => p.UnitsInStock * p.UnitPrice);
        ///
        /// EAGER LOADING:
        /// _context.Categories
        ///     .Include(c => c.Products)
        ///     .ToList();
        ///
        /// PERFORMANS NOTU:
        /// - Çok ürünlü kategorilerde tüm ürünleri yüklemek pahalı
        /// - Sayfalama (pagination) kullanılmalı
        /// - AsNoTracking() ile readonly sorgular hızlandırılabilir
        /// </summary>
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
