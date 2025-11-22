// ===================================================================================
// TEKNOROMA - MAGAZA/SUBE DTO DOSYASI (StoreDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, magaza/sube (Store) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin coklu sube yonetimi icin kullanilir.
//
// MAGAZA KAVRAMI
// --------------
// Magaza (Store), TeknoRoma'nin fiziksel satis noktalarini temsil eder.
// Her magaza:
// - Kendi calisanlarina sahiptir
// - Kendi satis kayitlarini tutar
// - Bagimsiz raporlanabilir
//
// Ornek magazalar:
// - TeknoRoma Kadikoy
// - TeknoRoma Besiktas
// - TeknoRoma Ankara Kizilay
// - TeknoRoma Online (varsa)
//
// COKLU MAGAZA AVANTAJLARI
// ------------------------
// 1. Her sube ayri raporlanabilir
// 2. Calisan performansi sube bazli izlenebilir
// 3. Stok transferleri takip edilebilir
// 4. Bolgesel kampanyalar yapilabilir
//
// DTO TURLERI
// -----------
// 1. StoreDto (Okuma): Magaza bilgilerini goruntulemek icin
// 2. CreateStoreDto (Olusturma): Yeni magaza eklemek icin
// 3. UpdateStoreDto (Guncelleme): Magaza bilgisi guncellemek icin
// 4. StorePerformanceDto (Rapor): Magaza performans ozeti icin
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Store.cs
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/IStoreService.cs
//
// ===================================================================================

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Magaza bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan magaza bilgilerini UI'a tasir.
/// Calisan sayisi ve toplam satis gibi hesaplanan alanlar icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Magaza listesi tablosu (yonetici paneli)
/// - Calisan formunda magaza dropdown'i
/// - Satis raporlarinda magaza filtresi
/// - Dashboard'da magaza bazli karsilastirma
///
/// ORNEK:
/// ------
/// var stores = await _storeService.GetAllAsync();
/// foreach (var store in stores)
/// {
///     Console.WriteLine($"{store.StoreName}");
///     Console.WriteLine($"  Adres: {store.Address}");
///     Console.WriteLine($"  Calisan: {store.EmployeeCount}");
///     Console.WriteLine($"  Satis: {store.TotalSales:C}");
/// }
/// </summary>
public class StoreDto
{
    /// <summary>
    /// Magazanin benzersiz ID'si.
    /// Veritabani tarafindan otomatik olusturulur.
    /// Calisan ve satis kayitlarinda foreign key olarak kullanilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Magaza adi.
    /// Sube tanimlayici isim.
    /// UI'da menu, dropdown ve raporlarda gosterilir.
    /// Ornek: "TeknoRoma Kadikoy", "TeknoRoma Forum Istanbul"
    /// </summary>
    public string StoreName { get; set; } = null!;

    /// <summary>
    /// Magaza adresi (opsiyonel).
    /// Fiziksel konum bilgisi.
    /// Fatura, teslimat ve navigasyon icin.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Magaza telefon numarasi (opsiyonel).
    /// Musteri iletisimi icin.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Magaza e-posta adresi (opsiyonel).
    /// Kurumsal iletisim icin.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Bu magazadaki calisan sayisi (hesaplanan).
    /// Personel yogunlugu gosterir.
    /// </summary>
    public int EmployeeCount { get; set; }

    /// <summary>
    /// Toplam satis tutari (hesaplanan).
    /// Magazanin bugune kadar yaptigi satis toplami.
    /// Performans karsilastirmasinda kullanilir.
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// Magazanin acilma/kayit tarihi.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni magaza olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Yeni sube acildiginda kullanilir.
/// ID ve hesaplanan alanlar yoktur.
///
/// UI FORMU:
/// ---------
/// - Magaza Adi: TextBox (zorunlu)
/// - Adres: TextArea (opsiyonel)
/// - Telefon: TextBox (opsiyonel)
/// - E-posta: TextBox (opsiyonel)
///
/// ORNEK:
/// ------
/// var newStore = new CreateStoreDto {
///     StoreName = "TeknoRoma Izmir Karşıyaka",
///     Address = "Karşıyaka Carsi, No: 45",
///     Phone = "0232 123 4567",
///     Email = "karsiyaka@teknoroma.com"
/// };
/// await _storeService.CreateAsync(newStore);
/// </summary>
public class CreateStoreDto
{
    /// <summary>
    /// Magaza adi (zorunlu).
    /// Benzersiz olmali.
    /// </summary>
    public string StoreName { get; set; } = null!;

    /// <summary>
    /// Magaza adresi (opsiyonel).
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Telefon numarasi (opsiyonel).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi (opsiyonel).
    /// </summary>
    public string? Email { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut magaza bilgilerini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Magaza bilgileri degistiginde kullanilir.
/// CreateStoreDto'dan farki: ID alani vardir.
///
/// KULLANIM SENARYOLARI:
/// ---------------------
/// - Magaza tasindi (adres degisikligi)
/// - Telefon degisti
/// - Isim degisikligi (yeniden markalama)
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateStoreDto {
///     Id = 3,
///     StoreName = "TeknoRoma Kadikoy Merkez",  // Isim guncellendi
///     Address = "Yeni adres...",
///     Phone = "0216 999 8888"
/// };
/// await _storeService.UpdateAsync(updateDto);
/// </summary>
public class UpdateStoreDto
{
    /// <summary>
    /// Guncellenecek magazanin ID'si (zorunlu).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Magaza adi.
    /// </summary>
    public string StoreName { get; set; } = null!;

    /// <summary>
    /// Magaza adresi.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Telefon numarasi.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi.
    /// </summary>
    public string? Email { get; set; }
}

#endregion

#region PERFORMANS RAPORU DTO'SU

/// <summary>
/// Magaza performans ozeti icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Magazalarin performansini karsilastirmak icin kullanilir.
/// Dashboard ve raporlarda magaza bazli analiz icin.
///
/// UI KULLANIMI:
/// -------------
/// - Magaza karsilastirma tablosu
/// - En iyi/kotu performansli magazalar listesi
/// - Yonetici dashboard'u
///
/// METRIKLER:
/// ----------
/// - TotalRevenue: Toplam ciro
/// - TotalSales: Satis adedi
/// - EmployeeCount: Calisan sayisi
/// - AverageOrderValue: Ortalama siparis tutari
///
/// ORNEK KULLANIM:
/// ---------------
/// var performances = await _storeService.GetPerformanceAsync(startDate, endDate);
/// var bestStore = performances.OrderByDescending(p => p.TotalRevenue).First();
/// Console.WriteLine($"En iyi magaza: {bestStore.StoreName} - {bestStore.TotalRevenue:C}");
/// </summary>
public class StorePerformanceDto
{
    /// <summary>
    /// Magaza ID'si.
    /// Detaya gitmek icin kullanilir.
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Magaza adi.
    /// Gosterim amacli.
    /// </summary>
    public string StoreName { get; set; } = null!;

    /// <summary>
    /// Toplam ciro (gelir).
    /// Belirlenen donemde yapilan satis tutari.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Toplam satis adedi.
    /// Kac tane fis/fatura kesilmis.
    /// </summary>
    public int TotalSales { get; set; }

    /// <summary>
    /// Calisan sayisi.
    /// Verimlilik hesaplamasi icin.
    /// Ciro/Calisan = Calisan basina verimlilik
    /// </summary>
    public int EmployeeCount { get; set; }

    /// <summary>
    /// Ortalama siparis tutari.
    /// Formul: TotalRevenue / TotalSales
    /// Musteri sepet buyuklugunu gosterir.
    /// </summary>
    public decimal AverageOrderValue { get; set; }
}

#endregion
