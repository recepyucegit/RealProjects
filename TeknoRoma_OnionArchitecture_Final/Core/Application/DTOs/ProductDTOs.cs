// ===================================================================================
// TEKNOROMA - URUN DTO DOSYASI (ProductDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, urun (Product) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin urun yonetimi icin kullanilir.
//
// DTO TURLERI
// -----------
// Bu dosyada 4 farkli DTO bulunur:
//
// 1. ProductDto (Okuma/Listeleme):
//    - Veritabanindan okunan urun bilgilerini UI'a tasir
//    - Kullanim: Urun listesi, urun detay sayfasi
//
// 2. CreateProductDto (Olusturma):
//    - Yeni urun eklerken formdan gelen verileri tasir
//    - ID yoktur (veritabani olusturacak)
//    - Kullanim: "Yeni Urun Ekle" formu
//
// 3. UpdateProductDto (Guncelleme):
//    - Mevcut urunu duzenlerken formdan gelen verileri tasir
//    - ID vardir (hangi urunun guncelleneceği bilgisi)
//    - Kullanim: "Urun Duzenle" formu
//
// 4. ProductStockUpdateDto (Stok Guncelleme):
//    - Sadece stok miktarini guncellemek icin ozel DTO
//    - Kullanim: Stok sayimi, stok girisi/cikisi
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Product.cs
// - Veritabani tablosunu temsil eden entity sinifi
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/IProductService.cs
// - CreateAsync(CreateProductDto dto)
// - UpdateAsync(UpdateProductDto dto)
// - GetByIdAsync(int id) -> ProductDto
// - GetAllAsync() -> List<ProductDto>
//
// ILISKILI REPOSITORY
// -------------------
// Application/Interfaces/IProductRepository.cs
// - Entity bazli CRUD islemleri
//
// ===================================================================================

using Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region OKUMA/LISTELEME DTO'SU

/// <summary>
/// Urun bilgilerini okuma ve listeleme icin DTO.
///
/// AÇIKLAMA:
/// ---------
/// Bu DTO, veritabanindan okunan urun bilgilerini ust katmanlara (API, UI) tasir.
/// Product entity'sinin "dis dunyaya acilan yuzu"dur.
///
/// ENTITY'DEN FARKLARI:
/// --------------------
/// 1. Navigation property'ler yerine sadece isim/aciklama alanları var (CategoryName, SupplierName)
/// 2. Hesaplanmis alanlar var (StockStatusDisplay)
/// 3. Hassas bilgiler filtrelenmis olabilir
///
/// UI KULLANIMI:
/// -------------
/// - Urun listesi tablosu (DataGrid)
/// - Urun karti (Card component)
/// - Urun detay sayfasi
/// - Satis ekraninda urun secimi
///
/// ORNEK KULLANIM:
/// ---------------
/// // Service'den urun listesi alma
/// var products = await _productService.GetAllAsync();
///
/// // Tek urun detayi alma
/// var product = await _productService.GetByIdAsync(id);
///
/// // UI'da gosterim
/// Console.WriteLine($"{product.ProductName} - {product.SalePrice:C} - Stok: {product.Stock}");
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Urunun benzersiz ID'si.
    /// Veritabani tarafindan otomatik olusturulur (Primary Key).
    /// UI'da gizli alan (hidden field) olarak tutulur.
    /// Guncelleme ve silme islemlerinde bu ID kullanilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Urunun barkod numarasi.
    /// Genellikle 13 haneli EAN-13 veya 12 haneli UPC formati.
    /// Barkod okuyucu ile urun arama yapilir.
    /// UI'da: Satis ekraninda barkod arama, etiket yazdirma.
    /// Ornek: "8690000000000"
    /// </summary>
    public string Barcode { get; set; } = null!;

    /// <summary>
    /// Urun adi.
    /// Kullanicilara gosterilen ana tanimlayici.
    /// UI'da: Tablo sutunu, kart basligi, fatura satiri.
    /// Ornek: "Samsung Galaxy S24 Ultra"
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Urun aciklamasi (opsiyonel).
    /// Detayli urun bilgisi, teknik ozellikler vb.
    /// UI'da: Urun detay sayfasinda, tooltip'te.
    /// Ornek: "256GB, 12GB RAM, Titanium Gray"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Alis fiyati (maliyet).
    /// Tedarikçiden alinirken odenen fiyat.
    /// Kar hesaplamalarinda kullanilir: Kar = SalePrice - PurchasePrice
    /// UI'da: Yonetici panelinde gorunur, satis ekraninda gizli.
    /// Para birimi: TRY (Turk Lirasi).
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Satis fiyati.
    /// Musteriye satilan fiyat.
    /// UI'da: Satis ekrani, fiyat etiketi, fatura.
    /// Para birimi: TRY (Turk Lirasi).
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Mevcut stok adedi.
    /// Depodaki/rafta ki toplam urun sayisi.
    /// Satis yapildiginda otomatik azalir.
    /// UI'da: Stok durumu gosterimi, satis ekraninda kontrol.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Minimum stok seviyesi (uyari esigi).
    /// Stok bu seviyeye dustugunde uyari verilir.
    /// UI'da: Dusuk stok uyarisi (kirmizi ikon).
    /// Varsayilan: 10 adet.
    /// </summary>
    public int MinStock { get; set; }

    /// <summary>
    /// Stok durumu (enum).
    /// Stok miktarina gore otomatik hesaplanir:
    /// - InStock (Stokta): Stock > MinStock
    /// - LowStock (Dusuk Stok): 0 &lt; Stock &lt;= MinStock
    /// - OutOfStock (Tukendi): Stock == 0
    /// UI'da: Renkli badge (yesil/sari/kirmizi).
    /// </summary>
    public StockStatus StockStatus { get; set; }

    /// <summary>
    /// Stok durumunun metinsel gosterimi.
    /// Enum degerini string'e cevirir.
    /// UI'da: Kullaniciya okunabilir stok durumu.
    /// Ornek: "InStock" -> "Stokta", "LowStock" -> "Az Kaldi"
    /// </summary>
    public string StockStatusDisplay => StockStatus.ToString();

    /// <summary>
    /// Bagli oldugu kategori ID'si.
    /// Kategori secimi icin foreign key.
    /// Ornek: 1 = "Telefonlar", 2 = "Bilgisayarlar"
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Kategori adi (navigation property'den gelen).
    /// Join islemi sonucu doldurulur.
    /// UI'da: Urun tablosunda kategori sutunu.
    /// Ornek: "Akilli Telefonlar"
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Tedarikci ID'si (opsiyonel).
    /// Urunun alis yapildigi tedarikci.
    /// null olabilir (tedarikci tanimlanmamis).
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Tedarikci adi (navigation property'den gelen).
    /// UI'da: Tedarikci bilgisi, siparis olusturma.
    /// Ornek: "Samsung Turkiye A.S."
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// Urunun sisteme eklendigi tarih.
    /// Otomatik olarak olusturulur.
    /// UI'da: "Eklenme Tarihi" bilgisi, raporlama.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni urun olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Yeni Urun Ekle" formundan gelen verileri tasir.
/// ID alani yoktur cunku veritabani otomatik olusturacak.
///
/// NEDEN AYRI DTO?
/// ---------------
/// 1. ID girilmesini engeller (guvenlik)
/// 2. CreatedDate girilmesini engeller (sistem otomatik atar)
/// 3. Sadece gerekli alanlar alinir (validasyon kolayligi)
/// 4. StockStatus hesaplanir, kullanici girmez
///
/// UI FORMU:
/// ---------
/// - Barkod: TextBox (zorunlu)
/// - Urun Adi: TextBox (zorunlu)
/// - Aciklama: TextArea (opsiyonel)
/// - Alis Fiyati: NumberBox (zorunlu)
/// - Satis Fiyati: NumberBox (zorunlu)
/// - Stok: NumberBox (zorunlu)
/// - Min Stok: NumberBox (varsayilan: 10)
/// - Kategori: Dropdown (zorunlu)
/// - Tedarikci: Dropdown (opsiyonel)
///
/// ORNEK:
/// ------
/// var newProduct = new CreateProductDto {
///     Barcode = "8690000000001",
///     ProductName = "iPhone 15 Pro Max",
///     PurchasePrice = 55000,
///     SalePrice = 65000,
///     Stock = 50,
///     CategoryId = 1
/// };
/// await _productService.CreateAsync(newProduct);
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// Urun barkodu (zorunlu).
    /// Benzersiz olmali, baska urunle ayni olamaz.
    /// Barkod okuyucu ile okutulabilir.
    /// </summary>
    public string Barcode { get; set; } = null!;

    /// <summary>
    /// Urun adi (zorunlu).
    /// Urunun tanimlanmasinda kullanilir.
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Urun aciklamasi (opsiyonel).
    /// Detayli bilgi, teknik ozellikler.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Alis fiyati (zorunlu).
    /// Tedarikçiden alinan fiyat.
    /// Sifirdan buyuk olmali.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Satis fiyati (zorunlu).
    /// Musteriye satis fiyati.
    /// Genellikle PurchasePrice'tan buyuk olmali (kar).
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Baslangic stok miktari.
    /// Urun ilk eklendiginde stok adedi.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Minimum stok seviyesi (varsayilan: 10).
    /// Stok bu seviyeye dustugunde uyari verilir.
    /// </summary>
    public int MinStock { get; set; } = 10;

    /// <summary>
    /// Kategori ID'si (zorunlu).
    /// Dropdown'dan secilir.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Tedarikci ID'si (opsiyonel).
    /// Dropdown'dan secilir, bos birakilabilir.
    /// </summary>
    public int? SupplierId { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut urunu guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Urun Duzenle" formundan gelen verileri tasir.
/// CreateProductDto'dan farki: ID alani vardir.
///
/// NEDEN ID GEREKLI?
/// -----------------
/// Veritabaninda hangi kaydın guncelleneceğini belirtir.
/// UPDATE Product SET ... WHERE Id = @Id
///
/// UI KULLANIMI:
/// -------------
/// 1. Kullanici listeden urun secer
/// 2. Mevcut veriler forma yuklenir (GetByIdAsync)
/// 3. Kullanici degisiklik yapar
/// 4. Form submit edildiginde UpdateProductDto olusur
/// 5. UpdateAsync(dto) cagrilir
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateProductDto {
///     Id = 5,  // Guncellenecek urun
///     Barcode = "8690000000001",
///     ProductName = "iPhone 15 Pro Max 256GB",  // Isim guncellendi
///     SalePrice = 70000,  // Fiyat guncellendi
///     // ... diger alanlar
/// };
/// await _productService.UpdateAsync(updateDto);
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Guncellenecek urunun ID'si (zorunlu).
    /// Form'da hidden field olarak tutulur.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Urun barkodu.
    /// Genellikle degismez ama degistirilebilir.
    /// </summary>
    public string Barcode { get; set; } = null!;

    /// <summary>
    /// Urun adi.
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Urun aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Alis fiyati.
    /// Maliyet degisirse guncellenir.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Satis fiyati.
    /// Kampanya, zamlar icin guncellenir.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Stok miktari.
    /// Manuel stok duzeltmesi icin kullanilir.
    /// NOT: Normal stok hareketleri icin ProductStockUpdateDto kullanin.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Minimum stok seviyesi.
    /// </summary>
    public int MinStock { get; set; }

    /// <summary>
    /// Kategori ID'si.
    /// Kategori degisikligi icin.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Tedarikci ID'si.
    /// Tedarikci degisikligi icin.
    /// </summary>
    public int? SupplierId { get; set; }
}

#endregion

#region STOK GUNCELLEME DTO'SU

/// <summary>
/// Urun stogunu guncellemek icin ozel DTO.
///
/// ACIKLAMA:
/// ---------
/// Sadece stok miktarini artirmak veya azaltmak icin kullanilir.
/// UpdateProductDto'dan farkli olarak sadece stok islemine odaklanir.
///
/// NEDEN AYRI DTO?
/// ---------------
/// 1. Tek sorumluluk: Sadece stok islemi
/// 2. Guvenlik: Diger alanlar (fiyat vb.) degistirilemez
/// 3. Izlenebilirlik: Stok hareketleri ayri loglanabilir
/// 4. Is mantigi: Satis sirasinda stok otomatik duser
///
/// KULLANIM SENARYOLARI:
/// ---------------------
/// 1. Stok sayimi sonrasi duzeltme
/// 2. Tedarikci'den mal girisi (IsAddition = true)
/// 3. Fire/zayi kaydi (IsAddition = false)
/// 4. Baska subeden transfer
///
/// ORNEK:
/// ------
/// // Mal girisi (50 adet ekleme)
/// var stockUpdate = new ProductStockUpdateDto {
///     ProductId = 5,
///     Quantity = 50,
///     IsAddition = true
/// };
///
/// // Fire kaydi (3 adet dusme)
/// var stockUpdate = new ProductStockUpdateDto {
///     ProductId = 5,
///     Quantity = 3,
///     IsAddition = false
/// };
///
/// await _productService.UpdateStockAsync(stockUpdate);
/// </summary>
public class ProductStockUpdateDto
{
    /// <summary>
    /// Stok guncellenecek urunun ID'si.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Eklenecek veya cikarilacak miktar.
    /// Pozitif sayi olmali.
    /// IsAddition'a gore ekleme veya cikarma yapilir.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Ekleme mi cikarma mi?
    /// true: Stok arttirilir (mal girisi)
    /// false: Stok azaltilir (fire, iade vb.)
    /// Varsayilan: true (ekleme)
    /// </summary>
    public bool IsAddition { get; set; } = true;
}

#endregion
