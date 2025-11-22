// ============================================================================
// Product.cs - Ürün Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'da satılan elektronik ürünleri temsil eden entity.
// Bilgisayar, telefon, tablet, TV, beyaz eşya gibi tüm ürünler bu sınıfla yönetilir.
//
// İŞ KURALLARI:
// - Her ürünün benzersiz bir barkodu olmalı
// - Stok miktarı negatif olamaz
// - Kritik stok seviyesinin altına düşen ürünler için uyarı verilir
// - Fiyatlar TL cinsinden saklanır
//
// VERİTABANI İLİŞKİLERİ:
// - Product (N) → Category (1): Her ürün bir kategoriye ait
// - Product (N) → Supplier (1): Her ürün bir tedarikçiden alınır
// - Product (1) → SaleDetail (N): Bir ürün birden fazla satışta yer alabilir
// ============================================================================

// "using" direktifi - Başka namespace'lerdeki tipleri kullanmak için
// Domain.Enums namespace'indeki StockStatus enum'unu kullanacağız
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Ürün Entity Sınıfı
    ///
    /// Entity Framework Mapping:
    /// - Bu sınıf "Products" tablosuna map edilir
    /// - Her property bir sütuna karşılık gelir
    /// - Navigation property'ler foreign key ilişkilerini temsil eder
    /// </summary>
    public class Product : BaseEntity
    {
        // ====================================================================
        // TEMEL ÜRÜN BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Ürün Adı
        ///
        /// AÇIKLAMA:
        /// - Ürünün tam adı ve modeli
        /// - Örn: "Apple iPhone 15 Pro Max 256GB", "Samsung Galaxy S24 Ultra"
        /// - Arama ve filtreleme için kullanılır
        ///
        /// VERİTABANI:
        /// - nvarchar(max) veya nvarchar(500) olarak saklanır
        /// - Fluent API ile MaxLength belirlenebilir
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Ürün Açıklaması
        ///
        /// AÇIKLAMA:
        /// - Ürünün detaylı teknik özellikleri
        /// - Örn: "6.7 inç ekran, A17 Pro çip, 48MP kamera, 5G desteği"
        /// - E-ticaret sitesinde ürün detay sayfasında gösterilir
        ///
        /// HTML İÇERİK:
        /// - Zengin metin (rich text) formatında olabilir
        /// - XSS saldırılarına karşı sanitize edilmeli
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Barkod Numarası (UNIQUE)
        ///
        /// AÇIKLAMA:
        /// - Uluslararası standart EAN-13 veya UPC formatında
        /// - Her ürün için benzersiz (unique constraint)
        /// - Kasada barkod okuyucu ile hızlı satış için kullanılır
        ///
        /// FORMAT ÖRNEKLERİ:
        /// - EAN-13: 8690123456789 (13 haneli, Türkiye 869 ile başlar)
        /// - UPC-A: 012345678905 (12 haneli, ABD)
        ///
        /// VERİTABANI:
        /// - UNIQUE INDEX ile benzersizlik sağlanır
        /// - Fluent API: HasIndex(p => p.Barcode).IsUnique()
        /// </summary>
        public string Barcode { get; set; } = null!;

        // ====================================================================
        // FİYAT VE STOK BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Birim Satış Fiyatı (TL)
        ///
        /// AÇIKLAMA:
        /// - Ürünün KDV dahil perakende satış fiyatı
        /// - Türk Lirası cinsinden
        ///
        /// "decimal" TİPİ SEÇİMİ:
        /// - Para birimi için her zaman decimal kullanılır
        /// - float/double KULLANMAYIN - hassasiyet kaybı olur!
        /// - Örn: float ile 0.1 + 0.2 = 0.30000000000000004 olur
        /// - decimal ile 0.1 + 0.2 = 0.3 (doğru sonuç)
        ///
        /// VERİTABANI:
        /// - SQL Server'da decimal(18,2) olarak saklanır
        /// - 18 toplam hane, 2 ondalık hane
        /// - Fluent API: HasPrecision(18, 2)
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Stok Miktarı (Adet)
        ///
        /// AÇIKLAMA:
        /// - Depoda/mağazada mevcut ürün sayısı
        /// - Satış yapıldığında otomatik azalır
        /// - Stok girişinde otomatik artar
        ///
        /// İŞ KURALLARI:
        /// - Negatif değer alamaz (check constraint ile kontrol)
        /// - 0 olduğunda ürün "Tükendi" olarak işaretlenir
        /// - Stok yönetimi concurrency kontrolü gerektirir
        /// </summary>
        public int UnitsInStock { get; set; }

        /// <summary>
        /// Kritik Stok Seviyesi (Adet)
        ///
        /// AÇIKLAMA:
        /// - Bu seviyenin altına düşüldüğünde uyarı verilir
        /// - Varsayılan değer: 10 adet
        /// - Her ürün için farklı belirlenebilir (iPhone için 5, kablo için 100)
        ///
        /// KULLANIM:
        /// - Dashboard'da "Kritik stoktaki ürünler" listesi
        /// - Otomatik sipariş önerisi
        /// - E-posta/SMS uyarısı
        ///
        /// "= 10" VARSAYILAN DEĞER:
        /// - Property initializer ile varsayılan atanır
        /// - Yeni ürün eklenirken değer verilmezse 10 kullanılır
        /// </summary>
        public int CriticalStockLevel { get; set; } = 10;

        /// <summary>
        /// Stok Durumu (Enum)
        ///
        /// AÇIKLAMA:
        /// - Ürünün stok durumunu kategorize eder
        /// - StockStatus enum'undan değer alır
        ///
        /// ENUM DEĞERLERİ:
        /// - InStock: Stokta var
        /// - LowStock: Kritik seviyede
        /// - OutOfStock: Tükendi
        ///
        /// VERİTABANI:
        /// - int olarak saklanır (enum değeri)
        /// - EF Core otomatik olarak enum ↔ int dönüşümü yapar
        /// </summary>
        public StockStatus StockStatus { get; set; }

        // ====================================================================
        // FOREIGN KEY ALANLARI
        // ====================================================================

        /// <summary>
        /// Kategori ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Ürünün ait olduğu kategoriyi belirtir
        /// - Categories tablosundaki Id ile eşleşir
        ///
        /// FOREIGN KEY NEDİR?
        /// - Başka bir tabloya referans veren alan
        /// - Veritabanı ilişkilerini (relationships) tanımlar
        /// - Referential integrity sağlar (olmayan kategoriye ürün eklenemez)
        ///
        /// EF CORE CONVENTION:
        /// - [NavigationPropertyName]Id formatı otomatik FK olarak algılanır
        /// - CategoryId → Category navigation property ile eşleşir
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Tedarikçi ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Ürünün tedarik edildiği firmayı belirtir
        /// - Suppliers tablosundaki Id ile eşleşir
        ///
        /// KULLANIM:
        /// - Tedarikçi bazlı raporlama
        /// - Stok yenileme siparişleri
        /// - Tedarikçi borç/alacak takibi
        /// </summary>
        public int SupplierId { get; set; }

        // ====================================================================
        // DURUM BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Ürün Aktif mi?
        ///
        /// AÇIKLAMA:
        /// - true: Ürün satışa açık
        /// - false: Ürün satıştan kaldırılmış (discontinued)
        ///
        /// IsActive vs IsDeleted FARKI:
        /// - IsActive=false: Geçici olarak satıştan çekilmiş, tekrar aktif edilebilir
        /// - IsDeleted=true: Tamamen silinmiş (soft delete)
        ///
        /// KULLANIM SENARYOLARI:
        /// - Sezonluk ürünler (sadece yaz/kış satılan)
        /// - Geçici tedarik problemi olan ürünler
        /// - Fiyat güncellemesi bekleyen ürünler
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ürün Görseli URL'i
        ///
        /// AÇIKLAMA:
        /// - Ürün resminin dosya yolu veya URL'i
        /// - Örn: "/images/products/iphone15.jpg" veya "https://cdn.../image.jpg"
        ///
        /// NULLABLE (string?):
        /// - "?" işareti null olabileceğini belirtir
        /// - Görseli olmayan ürünler için null kalır
        /// - UI'da varsayılan "no-image" görseli gösterilir
        ///
        /// ALTERNATİF YAKLAŞIMLAR:
        /// - Azure Blob Storage URL'i
        /// - Base64 encoded image (küçük görseller için)
        /// - Ayrı ProductImage tablosu (birden fazla görsel için)
        /// </summary>
        public string? ImageUrl { get; set; }

        // ====================================================================
        // CALCULATED PROPERTIES - HESAPLANAN ÖZELLİKLER
        // ====================================================================
        // Calculated Property Nedir?
        // - Veritabanında saklanmaz, her erişimde hesaplanır
        // - "=>" (expression body) ile tanımlanır
        // - Get-only property (sadece okunabilir, yazılamaz)
        // - EF Core sorgularında kullanılamaz (client-side evaluation)
        // ====================================================================

        /// <summary>
        /// Stok Durumu Metni (Calculated)
        ///
        /// AÇIKLAMA:
        /// - Stok miktarına göre Türkçe durum metni döner
        /// - UI'da kullanıcıya gösterilir
        ///
        /// TERNARY OPERATOR (?:):
        /// koşul ? doğruysa_değer : yanlışsa_değer
        ///
        /// NESTED TERNARY:
        /// - İç içe ternary operatör kullanımı
        /// - Okunabilirlik için dikkatli kullanılmalı
        ///
        /// MANTIK:
        /// 1. UnitsInStock <= 0 → "Tükendi"
        /// 2. UnitsInStock <= CriticalStockLevel → "Kritik"
        /// 3. Diğer durumlar → "Yeterli"
        /// </summary>
        public string StockStatusText => UnitsInStock <= 0
            ? "Tükendi"                                    // Stok 0 veya negatif
            : UnitsInStock <= CriticalStockLevel
                ? "Kritik"                                 // Kritik seviyede veya altında
                : "Yeterli";                               // Normal stok seviyesi

        /// <summary>
        /// Ürün Satılabilir mi? (Calculated)
        ///
        /// AÇIKLAMA:
        /// - Ürünün satışa uygun olup olmadığını kontrol eder
        /// - İki koşulun ikisi de true olmalı:
        ///   1. Stokta ürün var (UnitsInStock > 0)
        ///   2. Ürün aktif (IsActive == true)
        ///
        /// && (AND) OPERATÖRÜ:
        /// - Her iki koşul da true ise sonuç true
        /// - Short-circuit evaluation: İlk koşul false ise ikinci kontrol edilmez
        ///
        /// KULLANIM:
        /// if (product.IsAvailable) { /* Sepete ekle */ }
        /// </summary>
        public bool IsAvailable => UnitsInStock > 0 && IsActive;

        // ====================================================================
        // NAVIGATION PROPERTIES - İLİŞKİSEL GEZİNME ÖZELLİKLERİ
        // ====================================================================

        /// <summary>
        /// Kategori (Navigation Property)
        ///
        /// İLİŞKİ: Product (N) → Category (1)
        /// - Birden fazla ürün aynı kategoride olabilir (Many-to-One)
        ///
        /// REFERENCE NAVIGATION:
        /// - Tek bir entity'ye referans (collection değil)
        /// - CategoryId foreign key ile eşleşir
        ///
        /// KULLANIM:
        /// var categoryName = product.Category.Name;
        ///
        /// EAGER LOADING:
        /// _context.Products.Include(p => p.Category).ToList();
        /// </summary>
        public virtual Category Category { get; set; } = null!;

        /// <summary>
        /// Tedarikçi (Navigation Property)
        ///
        /// İLİŞKİ: Product (N) → Supplier (1)
        /// - Birden fazla ürün aynı tedarikçiden alınabilir
        ///
        /// KULLANIM:
        /// var supplierPhone = product.Supplier.Phone;
        /// </summary>
        public virtual Supplier Supplier { get; set; } = null!;

        /// <summary>
        /// Satış Detayları (Collection Navigation Property)
        ///
        /// İLİŞKİ: Product (1) → SaleDetail (N)
        /// - Bir ürün birden fazla satışta yer alabilir
        ///
        /// KULLANIM:
        /// - "Bu ürün kaç kez satıldı?" → product.SaleDetails.Count
        /// - "Toplam satış miktarı?" → product.SaleDetails.Sum(d => d.Quantity)
        /// </summary>
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

        /// <summary>
        /// Tedarikçi İşlemleri (Collection Navigation Property)
        ///
        /// İLİŞKİ: Product (1) → SupplierTransaction (N)
        /// - Bir ürün için birden fazla tedarik işlemi olabilir
        ///
        /// KULLANIM:
        /// - Ürünün tedarik geçmişi
        /// - Alış fiyatı değişim analizi
        /// </summary>
        public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();
    }
}
