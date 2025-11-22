// ===================================================================================
// TEKNOROMA - KATEGORI DTO DOSYASI (CategoryDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, kategori (Category) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin urun kategorilendirmesi icin kullanilir.
//
// KATEGORI KAVRAMI
// ----------------
// Kategoriler, urunleri mantiksal gruplara ayirmak icin kullanilir.
// Ornek kategoriler:
// - Telefonlar
// - Bilgisayarlar
// - Tabletler
// - Aksesuarlar
// - Ses Sistemleri
//
// NEDEN KATEGORILENDIRME?
// -----------------------
// 1. Urun arama/filtreleme kolaylasiyor
// 2. Raporlama kategori bazli yapilabiliyor
// 3. Kampanyalar kategori bazli uygulanabiliyor
// 4. UI'da menuler ve filtrelemeler icin kullaniliyor
//
// DTO TURLERI
// -----------
// 1. CategoryDto (Okuma): Kategori bilgilerini goruntulemek icin
// 2. CreateCategoryDto (Olusturma): Yeni kategori eklemek icin
// 3. UpdateCategoryDto (Guncelleme): Kategori bilgisi guncellemek icin
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Category.cs
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/ICategoryService.cs
// - CRUD islemleri
// - Dropdown list icin GetSelectListAsync()
//
// ===================================================================================

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Kategori bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan kategori bilgilerini UI'a tasir.
/// Kategoriye bagli urun sayisini da icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Kategori listesi tablosu (DataGrid)
/// - Urun ekleme/duzenleme formunda dropdown
/// - Sol menu'de kategori filtresi
/// - Raporlarda kategori bazli gruplama
///
/// ORNEK:
/// ------
/// var categories = await _categoryService.GetAllAsync();
/// foreach (var cat in categories)
/// {
///     Console.WriteLine($"{cat.CategoryName}: {cat.ProductCount} urun");
/// }
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Kategorinin benzersiz ID'si.
    /// Veritabani tarafindan otomatik olusturulur (Primary Key).
    /// Urun tablosunda foreign key olarak kullanilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kategori adi.
    /// Benzersiz olmali (ayni isimde iki kategori olamaz).
    /// UI'da menu, dropdown ve filtre olarak gosterilir.
    /// Ornek: "Akilli Telefonlar", "Dizustu Bilgisayarlar"
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Kategori aciklamasi (opsiyonel).
    /// Kategorinin ne tur urunler icerdigi hakkinda bilgi.
    /// UI'da tooltip veya kategori detay sayfasinda gosterilir.
    /// Ornek: "Son teknoloji akilli telefonlar ve phabletler"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bu kategorideki urun sayisi (hesaplanan alan).
    /// Veritabanindan Count sorggusuyla getirilir.
    /// UI'da kategori yaninda badge olarak gosterilir: "Telefonlar (45)"
    /// Sifir ise "bu kategoride urun yok" uyarisi verilebilir.
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Kategorinin olusturulma tarihi.
    /// Yonetim amacli bilgi.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni kategori olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Yeni Kategori Ekle" formundan gelen verileri tasir.
/// ID ve ProductCount yoktur (sistem tarafindan yonetilir).
///
/// UI FORMU:
/// ---------
/// - Kategori Adi: TextBox (zorunlu, benzersiz)
/// - Aciklama: TextArea (opsiyonel)
///
/// VALIDASYON:
/// -----------
/// - CategoryName bos olamaz
/// - CategoryName benzersiz olmali (zaten var mi kontrolu)
///
/// ORNEK:
/// ------
/// var newCategory = new CreateCategoryDto {
///     CategoryName = "Giyilebilir Teknoloji",
///     Description = "Akilli saatler, fitness trackerlar"
/// };
/// await _categoryService.CreateAsync(newCategory);
/// </summary>
public class CreateCategoryDto
{
    /// <summary>
    /// Kategori adi (zorunlu).
    /// Benzersiz olmali - ayni isimde kategori varsa hata doner.
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Kategori aciklamasi (opsiyonel).
    /// Kategorinin icerigini tanimlayan metin.
    /// </summary>
    public string? Description { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut kategori bilgilerini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Kategori Duzenle" formundan gelen verileri tasir.
/// CreateCategoryDto'dan farki: ID alani vardir.
///
/// DIKKAT:
/// -------
/// Kategori silindiginde veya degistirildiginde, o kategoriye
/// bagli urunlerin durumu dusunulmeli!
/// - Silme: Bagli urun varsa silinememeli veya urunler baska kategoriye tasinmali
/// - Guncelleme: Urunleri etkilemez (sadece ID ile iliski var)
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateCategoryDto {
///     Id = 5,
///     CategoryName = "Akilli Telefonlar ve Phabletler",  // Isim guncellendi
///     Description = "Tum akilli telefon ve buyuk ekranli telefon cesitleri"
/// };
/// await _categoryService.UpdateAsync(updateDto);
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>
    /// Guncellenecek kategorinin ID'si (zorunlu).
    /// Hidden field olarak formda tutulur.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kategori adi.
    /// Degistirilirse benzersizlik kontrolu yapilir.
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Kategori aciklamasi.
    /// </summary>
    public string? Description { get; set; }
}

#endregion
