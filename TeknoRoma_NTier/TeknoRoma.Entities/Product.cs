using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Entities;

/// <summary>
/// Ürün Entity
/// TeknoRoma'nın sattığı tüm teknoloji ürünlerini temsil eder
/// Telefon, Bilgisayar, Tablet, Aksesuar vb.
///
/// NEDEN ÖNEMLİ?
/// - Gül Satar: "Ürün fiyatlarını döviz kuru ile görmek istiyorum"
/// - Durna Sabit: "Stok azaldığında bana bildirim gelmeli"
/// - Haluk Bey: "En çok satan ürünleri görmek istiyorum"
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Ürün adı
    /// Örn: "iPhone 15 Pro", "MacBook Air M2", "Samsung Galaxy S24"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ürün kodu (SKU - Stock Keeping Unit)
    /// Benzersiz ürün takip kodu
    /// Örn: "AAPL-IP15P-256-BLK"
    /// </summary>
    public string? ProductCode { get; set; }

    /// <summary>
    /// Ürün açıklaması - Detaylı teknik özellikler
    /// Müşteriye gösterilen detaylı bilgi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Ürün fiyatı (TL cinsinden)
    /// NEDEN TL? Tüm fiyatlar önce TL'ye çevrilerek saklanır
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// İndirimli fiyat - Kampanyalı ürünler için
    /// Null ise indirim yok demektir
    /// Gül Satar: "İndirimli fiyatı müşteriye hemen söyleyebilmeliyim"
    /// </summary>
    public decimal? DiscountPrice { get; set; }

    /// <summary>
    /// Stok miktarı
    /// NEDEN? Durna Sabit stok azaldığında uyarı alacak
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Stok durumu
    /// Yeterli, Azaliyor, Tukendi, CokFazla
    /// NEDEN? Durna Sabit'in istediği kritik stok uyarı sistemi için
    /// Otomatik hesaplanabilir ama performans için burada tutuyoruz
    /// </summary>
    public StockStatus StockStatus { get; set; } = StockStatus.Yeterli;

    /// <summary>
    /// Kritik stok seviyesi
    /// Stok bu seviyenin altına düşünce "Azaliyor" uyarısı
    /// Durna Sabit: "Kritik seviye 10 olsun, altına düşünce sipariş verelim"
    /// </summary>
    public int? CriticalStockLevel { get; set; } = 10;

    /// <summary>
    /// Ürün resmi URL'si
    /// Web sitesi ve Windows uygulamasında gösterilir
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Marka
    /// Örn: "Apple", "Samsung", "Lenovo"
    /// NEDEN? Markaya göre filtreleme ve raporlama
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model
    /// Örn: "iPhone 15 Pro", "Galaxy S24"
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Garanti süresi (ay olarak)
    /// Örn: 24 (2 yıl garanti)
    /// </summary>
    public int? WarrantyMonths { get; set; }

    /// <summary>
    /// Ürünün aktif olup olmadığını belirtir
    /// Pasif ürünler satışa sunulmaz
    /// NEDEN? Üretimi durmuş ürünler için
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Ürünün öne çıkan ürün olup olmadığını belirtir
    /// Ana sayfada ve vitrinde öne çıkan ürünler gösterilir
    /// Haluk Bey: "Yeni ürünleri öne çıkaralım"
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    // Foreign Keys
    /// <summary>
    /// Bu ürünün ait olduğu kategori ID'si
    /// Örn: Cep Telefonu, Bilgisayar, Tablet
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Bu ürünü tedarik eden firma ID'si
    /// </summary>
    public int SupplierId { get; set; }


    // ====== CALCULATED PROPERTIES ======

    /// <summary>
    /// Geçerli fiyat (indirimli varsa indirimli, yoksa normal)
    /// UI'da gösterilecek fiyat
    /// </summary>
    public decimal EffectivePrice => DiscountPrice ?? Price;

    /// <summary>
    /// İndirim yüzdesi
    /// Null ise indirim yok
    /// </summary>
    public decimal? DiscountPercentage
    {
        get
        {
            if (DiscountPrice.HasValue && Price > 0)
                return Math.Round(((Price - DiscountPrice.Value) / Price) * 100, 2);
            return null;
        }
    }

    /// <summary>
    /// Stokta var mı?
    /// </summary>
    public bool IsInStock => Stock > 0;


    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Bu ürünün ait olduğu kategori
    /// Many-to-One ilişki
    /// </summary>
    public virtual Category Category { get; set; } = null!;

    /// <summary>
    /// Bu ürünü tedarik eden firma
    /// Many-to-One ilişki
    /// </summary>
    public virtual Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Bu ürüne ait satış detayları
    /// One-to-Many ilişki: Bir ürün birden fazla satışta yer alabilir
    /// Haluk Bey: "Hangi ürün ne kadar satmış"
    /// </summary>
    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    /// <summary>
    /// Bu ürüne ait teknik servis kayıtları
    /// One-to-Many ilişki: Bir ürünün birden fazla servis kaydı olabilir
    /// Özgün Kablocu: "Hangi ürünlerde daha çok arıza oluyor"
    /// </summary>
    public virtual ICollection<TechnicalService> TechnicalServices { get; set; } = new List<TechnicalService>();
}
