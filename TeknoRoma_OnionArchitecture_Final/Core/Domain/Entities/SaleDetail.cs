// ============================================================================
// SaleDetail.cs - Satış Detayı Entity (Satış Kalemi/Satırı)
// ============================================================================
// AÇIKLAMA:
// Bir satış işlemindeki her bir ürün kalemini temsil eder.
// Sale (Master) - SaleDetail (Detail) ilişkisinin Detail tarafıdır.
// Aynı zamanda Sale ve Product arasındaki Many-to-Many ilişkiyi çözer.
//
// VERİTABANI İLİŞKİSİ:
// - Sale (1) → SaleDetail (N): Bir satışta birden fazla ürün
// - Product (1) → SaleDetail (N): Bir ürün birden fazla satışta
// - SaleDetail: Junction/Bridge table görevi görür
//
// İŞ KURALLARI:
// - UnitPrice ve ProductName satış anında snapshot olarak saklanır
// - Ürün fiyatı sonradan değişse bile satış kaydı etkilenmez
// - Quantity negatif olamaz
// - İndirimler kalem bazlı uygulanabilir
// ============================================================================

namespace Domain.Entities
{
    /// <summary>
    /// Satış Detayı Entity Sınıfı
    ///
    /// MASTER-DETAIL PATTERN:
    /// - Sale: Master (Fatura Başlığı)
    /// - SaleDetail: Detail (Fatura Kalemi)
    ///
    /// JUNCTION TABLE:
    /// - Many-to-Many ilişkiyi One-to-Many'ye çevirir
    /// - Sale ←→ Product ilişkisini çözer
    /// - Ek alanlar taşıyabilir (Quantity, UnitPrice vs.)
    ///
    /// ÖRNEK SATIR:
    /// SaleId=1, ProductId=10, Quantity=2, UnitPrice=5000
    /// → Satış #1'de, Ürün #10'dan 2 adet, birim 5000 TL
    /// </summary>
    public class SaleDetail : BaseEntity
    {
        // ====================================================================
        // İLİŞKİSEL ALANLAR (FOREIGN KEYS)
        // ====================================================================

        /// <summary>
        /// Satış ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Bu kalem hangi satışa ait?
        /// - Sales tablosundaki Id ile eşleşir
        /// - CASCADE DELETE: Satış silinirse detaylar da silinir
        ///
        /// COMPOSITE KEY OPSİYONU:
        /// - (SaleId, ProductId) birlikte primary key olabilir
        /// - Aynı ürün aynı satışta iki kez olabilir mi?
        /// - Hayır ise composite key, evet ise ayrı Id
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// Ürün ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Satılan ürün
        /// - Products tablosundaki Id ile eşleşir
        /// - Ürün silinse bile bu kayıt kalır (snapshot var)
        /// </summary>
        public int ProductId { get; set; }

        // ====================================================================
        // SNAPSHOT ALANLARI - SATIŞ ANI BİLGİLERİ
        // ====================================================================
        // Snapshot (Anlık Görüntü) Nedir?
        // - Satış anındaki ürün bilgilerinin kopyalanması
        // - Ürün bilgileri sonradan değişse bile satış kaydı korunur
        // - Fatura doğruluğu ve yasal gereklilik için zorunlu
        //
        // ÖRNEK SENARYO:
        // - Bugün: iPhone 50.000 TL, satış yapıldı
        // - Yarın: iPhone fiyatı 55.000 TL'ye çıktı
        // - Eski satış kaydı hala 50.000 TL göstermeli
        // ====================================================================

        /// <summary>
        /// Satış Anındaki Ürün Adı (Snapshot)
        ///
        /// AÇIKLAMA:
        /// - Ürün adı satış anında kopyalanır
        /// - Ürün adı değişse bile bu kayıt değişmez
        ///
        /// NEDEN GEREKLİ?
        /// - Fatura üzerinde doğru ürün adı gösterilmeli
        /// - Ürün silinse bile satış geçmişi korunmalı
        /// - Yasal belge niteliği taşır
        ///
        /// DENORMALİZASYON:
        /// - Normalde Product.Name'den çekilir
        /// - Performans ve tutarlılık için kopyalanır
        /// - Kontrollü denormalizasyon örneği
        /// </summary>
        public string ProductName { get; set; } = null!;

        /// <summary>
        /// Satış Anındaki Birim Fiyatı (Snapshot)
        ///
        /// AÇIKLAMA:
        /// - Ürünün satış anındaki fiyatı
        /// - Product.UnitPrice'dan bağımsız
        ///
        /// NEDEN SNAPSHOT?
        /// - Fiyat güncellemeleri eski satışları etkilemez
        /// - Fatura tutarlılığı sağlanır
        /// - "O gün ne fiyata sattık?" sorusuna cevap
        ///
        /// İNDİRİMLİ FİYAT:
        /// - Bu alan liste fiyatı olabilir
        /// - İndirim ayrı hesaplanır
        /// </summary>
        public decimal UnitPrice { get; set; }

        // ====================================================================
        // MİKTAR VE TUTAR ALANLARI
        // ====================================================================

        /// <summary>
        /// Satılan Miktar (Adet)
        ///
        /// AÇIKLAMA:
        /// - Bu üründen kaç adet satıldı?
        /// - int tipi (tam sayı) - parça satış yok
        /// - Minimum 1 olmalı
        ///
        /// STOK ETKİSİ:
        /// - Satış onaylandığında stoktan düşülür
        /// - Product.UnitsInStock -= Quantity
        /// - İade durumunda stok artırılır
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// İndirim Oranı (Yüzde)
        ///
        /// AÇIKLAMA:
        /// - Kalem bazlı indirim yüzdesi
        /// - 0-100 arası değer alır
        /// - %10 indirim = 10, %25 indirim = 25
        ///
        /// VARSAYILAN DEĞER:
        /// - "= 0" ile indirim yok olarak başlar
        ///
        /// HESAPLAMA:
        /// DiscountAmount = Subtotal × (DiscountPercentage / 100)
        ///
        /// ALTERNATİF YAKLAŞIM:
        /// - DiscountType enum (yüzde/tutar)
        /// - DiscountValue alanı
        /// </summary>
        public decimal DiscountPercentage { get; set; } = 0;

        /// <summary>
        /// Ara Toplam (İndirim Öncesi)
        ///
        /// AÇIKLAMA:
        /// - UnitPrice × Quantity
        /// - İndirim uygulanmadan önceki tutar
        ///
        /// HESAPLAMA:
        /// Subtotal = UnitPrice × Quantity
        /// Örn: 5000 TL × 2 adet = 10.000 TL
        ///
        /// NOT:
        /// - Bu alan hesaplanabilir (calculated) olabilir
        /// - Performans için saklanır (denormalizasyon)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// İndirim Tutarı (TL)
        ///
        /// AÇIKLAMA:
        /// - Uygulanan indirimin TL karşılığı
        /// - Subtotal × (DiscountPercentage / 100)
        ///
        /// HESAPLAMA:
        /// DiscountAmount = 10.000 × (10 / 100) = 1.000 TL
        ///
        /// KULLANIM:
        /// - Faturada indirim tutarı gösterimi
        /// - Kampanya raporları
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Net Tutar (İndirim Sonrası)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin bu kalem için ödeyeceği tutar
        /// - Subtotal - DiscountAmount
        ///
        /// HESAPLAMA:
        /// TotalAmount = 10.000 - 1.000 = 9.000 TL
        ///
        /// TUTARLIK KONTROLÜ:
        /// if (TotalAmount != Subtotal - DiscountAmount)
        ///     throw new InvalidOperationException("Tutar hesaplaması hatalı!");
        /// </summary>
        public decimal TotalAmount { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Ait Olduğu Satış (Navigation Property)
        ///
        /// İLİŞKİ: SaleDetail (N) → Sale (1)
        ///
        /// KULLANIM:
        /// var saleDate = saleDetail.Sale.SaleDate;
        /// var customer = saleDetail.Sale.Customer;
        ///
        /// INVERSE NAVIGATION:
        /// - Sale.SaleDetails bu property'nin tersi
        /// - EF Core otomatik eşleştirir
        /// </summary>
        public virtual Sale Sale { get; set; } = null!;

        /// <summary>
        /// Satılan Ürün (Navigation Property)
        ///
        /// İLİŞKİ: SaleDetail (N) → Product (1)
        ///
        /// KULLANIM:
        /// var category = saleDetail.Product.Category.Name;
        /// var currentStock = saleDetail.Product.UnitsInStock;
        ///
        /// NOT:
        /// - Snapshot alanları olsa bile Product'a erişim faydalı
        /// - Güncel ürün bilgisi için kullanılabilir
        /// </summary>
        public virtual Product Product { get; set; } = null!;
    }
}
