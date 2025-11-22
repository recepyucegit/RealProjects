// ============================================================================
// StockStatus.cs - Stok Durumu Enum
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'daki ürünlerin stok seviyesini kategorize eden enum.
// Stok yönetimi ve tedarik planlaması için kritik önem taşır.
//
// STOK SEVİYELERİ:
// ┌──────────────────────────────────────────────────────────────┐
// │ Stok Miktarı  │ Durum    │ Aksiyon                          │
// ├───────────────┼──────────┼──────────────────────────────────┤
// │ > Kritik      │ Yeterli  │ Normal satış                     │
// │ <= Kritik     │ Kritik   │ Tedarikçi siparişi gerekli       │
// │ = 0           │ Tukendi  │ Satış durdurulmalı               │
// └──────────────────────────────────────────────────────────────┘
//
// HESAPLAMA MANTIGI:
// Product entity'sindeki StockStatus (calculated property):
// - UnitsInStock > ReorderLevel → Yeterli
// - 0 < UnitsInStock <= ReorderLevel → Kritik
// - UnitsInStock = 0 → Tukendi
//
// İŞ KURALLARI:
// - Kritik stoklar için otomatik sipariş oluşturulabilir
// - Tükenen ürünler için satış engellenebilir
// - Stok durumu dashboard'da gösterilir
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Stok Durumu Enum'u
    ///
    /// STOK YÖNETİMİ:
    /// - Otomatik tedarik uyarıları
    /// - Satış kısıtlamaları
    /// - Raporlama ve analiz
    ///
    /// PRODUCT ENTITY İLE KULLANIM:
    /// <code>
    /// // Product.cs'deki calculated property
    /// public StockStatus StockStatus =>
    ///     UnitsInStock == 0 ? StockStatus.Tukendi :
    ///     UnitsInStock <= ReorderLevel ? StockStatus.Kritik :
    ///     StockStatus.Yeterli;
    /// </code>
    ///
    /// RAPORLAMA:
    /// <code>
    /// // Stok durumuna göre ürün sayıları
    /// var stockReport = products
    ///     .GroupBy(p => p.StockStatus)
    ///     .Select(g => new { Status = g.Key, Count = g.Count() });
    /// </code>
    /// </summary>
    public enum StockStatus
    {
        /// <summary>
        /// Yeterli - Stok Seviyesi Normal
        ///
        /// AÇIKLAMA:
        /// - Stok miktarı kritik seviyenin üzerinde
        /// - Normal satış operasyonu devam eder
        /// - Acil tedarik gerekmiyor
        ///
        /// KOŞUL:
        /// UnitsInStock > ReorderLevel
        ///
        /// ÖRNEK:
        /// - iPhone 15: Stok=50, Kritik Seviye=10 → Yeterli
        /// - Aksi takdirde satış kaybı yaşanabilir
        ///
        /// DASHBOARD GÖSTERIMI:
        /// <code>
        /// // Yeşil renk ile göster
        /// &lt;span class="badge bg-success"&gt;Stok Yeterli&lt;/span&gt;
        /// </code>
        ///
        /// İSTATİSTİK:
        /// <code>
        /// var adequateStockCount = products
        ///     .Count(p => p.StockStatus == StockStatus.Yeterli);
        /// </code>
        /// </summary>
        Yeterli = 1,

        /// <summary>
        /// Kritik - Stok Seviyesi Düşük
        ///
        /// AÇIKLAMA:
        /// - Stok miktarı kritik seviyeye ulaştı veya altında
        /// - Hala satış yapılabilir ama tedarik gerekli
        /// - Uyarı gösterilmeli ve sipariş verilmeli
        ///
        /// KOŞUL:
        /// 0 < UnitsInStock <= ReorderLevel
        ///
        /// ÖRNEK:
        /// - iPhone 15: Stok=8, Kritik Seviye=10 → Kritik
        /// - Samsung TV: Stok=2, Kritik Seviye=5 → Kritik
        ///
        /// OTOMATİK SİPARİŞ:
        /// <code>
        /// // Kritik stoklar için otomatik tedarikçi siparişi
        /// var criticalProducts = products
        ///     .Where(p => p.StockStatus == StockStatus.Kritik)
        ///     .ToList();
        ///
        /// foreach (var product in criticalProducts)
        /// {
        ///     await _supplierService.CreateAutoOrder(
        ///         productId: product.Id,
        ///         quantity: product.ReorderLevel * 2
        ///     );
        /// }
        /// </code>
        ///
        /// BİLDİRİM:
        /// <code>
        /// // Depo yöneticisine e-posta uyarısı
        /// if (product.StockStatus == StockStatus.Kritik)
        /// {
        ///     await _emailService.SendLowStockAlert(
        ///         product.Name,
        ///         product.UnitsInStock
        ///     );
        /// }
        /// </code>
        ///
        /// DASHBOARD GÖSTERIMI:
        /// <code>
        /// // Sarı/turuncu renk ile göster
        /// &lt;span class="badge bg-warning"&gt;Kritik Stok&lt;/span&gt;
        /// </code>
        /// </summary>
        Kritik = 2,

        /// <summary>
        /// Tükendi - Stokta Ürün Yok
        ///
        /// AÇIKLAMA:
        /// - Stok miktarı sıfır
        /// - Satış yapılamaz (veya ön sipariş)
        /// - Acil tedarik gerekli
        ///
        /// KOŞUL:
        /// UnitsInStock = 0
        ///
        /// SATIŞ ENGELİ:
        /// <code>
        /// // Satış işleminde kontrol
        /// public async Task&lt;bool&gt; CanSell(int productId, int quantity)
        /// {
        ///     var product = await _productRepository.GetByIdAsync(productId);
        ///
        ///     if (product.StockStatus == StockStatus.Tukendi)
        ///     {
        ///         throw new StockException($"{product.Name} stokta yok!");
        ///     }
        ///
        ///     if (product.UnitsInStock < quantity)
        ///     {
        ///         throw new StockException(
        ///             $"Yetersiz stok. Mevcut: {product.UnitsInStock}");
        ///     }
        ///
        ///     return true;
        /// }
        /// </code>
        ///
        /// ÖN SİPARİŞ ALTERNATİFİ:
        /// <code>
        /// // Stok yoksa ön sipariş alınabilir
        /// if (product.StockStatus == StockStatus.Tukendi
        ///     && product.AllowPreOrder)
        /// {
        ///     // Ön sipariş kaydı oluştur
        ///     var preOrder = new PreOrder
        ///     {
        ///         ProductId = product.Id,
        ///         CustomerId = customerId,
        ///         ExpectedDate = DateTime.Now.AddDays(7)
        ///     };
        /// }
        /// </code>
        ///
        /// WEB SİTESİ GÖSTERIMI:
        /// <code>
        /// // Kırmızı "Stokta Yok" etiketi
        /// @if (product.StockStatus == StockStatus.Tukendi)
        /// {
        ///     &lt;span class="badge bg-danger"&gt;Stokta Yok&lt;/span&gt;
        ///     &lt;button disabled&gt;Sepete Ekle&lt;/button&gt;
        /// }
        /// </code>
        ///
        /// ACİL TEDARİK:
        /// - Tükenen ürünler öncelikli sipariş listesine alınır
        /// - Yöneticiye anlık bildirim gönderilir
        /// - Alternatif ürün önerisi sunulabilir
        /// </summary>
        Tukendi = 3
    }
}

// ============================================================================
// EK BİLGİLER VE BEST PRACTICES
// ============================================================================
//
// STOK HESAPLAMA DETAYI (Product.cs):
// <code>
// public class Product : BaseEntity
// {
//     public int UnitsInStock { get; set; }    // Mevcut stok
//     public int ReorderLevel { get; set; }    // Kritik seviye (eşik)
//     public int UnitsOnOrder { get; set; }    // Siparişteki miktar
//
//     // Calculated property (veritabanına kaydedilmez)
//     public StockStatus StockStatus =>
//         UnitsInStock == 0 ? StockStatus.Tukendi :
//         UnitsInStock <= ReorderLevel ? StockStatus.Kritik :
//         StockStatus.Yeterli;
//
//     // Kullanılabilir stok (rezerv düşülmüş)
//     public int AvailableStock =>
//         UnitsInStock - ReservedStock;
// }
// </code>
//
// DASHBOARD WİDGET:
// <code>
// // Stok özet kartı
// var stockSummary = new
// {
//     TotalProducts = products.Count,
//     AdequateStock = products.Count(p => p.StockStatus == StockStatus.Yeterli),
//     CriticalStock = products.Count(p => p.StockStatus == StockStatus.Kritik),
//     OutOfStock = products.Count(p => p.StockStatus == StockStatus.Tukendi)
// };
// </code>
//
// KRİTİK SEVİYE BELİRLEME:
// ReorderLevel değeri şunlara göre belirlenir:
// - Ortalama günlük satış adedi
// - Tedarikçi teslimat süresi
// - Güvenlik stoğu ihtiyacı
// - Mevsimsel talep değişimleri
//
// ÖRNEK FORMÜL:
// ReorderLevel = (Günlük Ortalama Satış × Teslimat Süresi) + Güvenlik Stoğu
// Örn: (5 adet/gün × 7 gün) + 10 = 45 adet kritik seviye
// ============================================================================
