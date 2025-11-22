// ===================================================================================
// TEKNOROMA - DEPARTMAN DTO DOSYASI (DepartmentDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, departman (Department) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin organizasyon yapisi icin kullanilir.
//
// DEPARTMAN KAVRAMI
// -----------------
// Departman, calisanlarin fonksiyonel olarak gruplandirildigi birimlerdir.
// Her departman farkli bir is fonksiyonunu temsil eder.
//
// Ornek departmanlar:
// - Satis: Kasa ve musteri hizmetleri
// - Teknik Servis: Cihaz tamir ve bakim
// - Depo/Lojistik: Stok yonetimi
// - Muhasebe: Mali isler
// - Insan Kaynaklari: Personel yonetimi
// - Yonetim: Ust duzey karar vericiler
//
// NEDEN DEPARTMAN YAPISI?
// -----------------------
// 1. Calisanlari fonksiyonel gruplama
// 2. Yetki ve sorumluluk belirleme
// 3. Departman bazli raporlama
// 4. Organizasyon semasi olusturma
// 5. Iletisim hiyerarsisi
//
// DTO TURLERI
// -----------
// 1. DepartmentDto (Okuma): Departman bilgilerini goruntulemek icin
// 2. CreateDepartmentDto (Olusturma): Yeni departman eklemek icin
// 3. UpdateDepartmentDto (Guncelleme): Departman bilgisi guncellemek icin
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Department.cs
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/IDepartmentService.cs
//
// ===================================================================================

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Departman bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan departman bilgilerini UI'a tasir.
/// Departmana bagli calisan sayisini da icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Departman listesi tablosu (yonetici paneli)
/// - Calisan formunda departman dropdown'i
/// - Organizasyon semasi
/// - Departman bazli raporlar
///
/// ORNEK:
/// ------
/// var departments = await _departmentService.GetAllAsync();
/// foreach (var dept in departments)
/// {
///     Console.WriteLine($"{dept.DepartmentName}: {dept.EmployeeCount} calisan");
///     Console.WriteLine($"  Aciklama: {dept.Description}");
/// }
/// </summary>
public class DepartmentDto
{
    /// <summary>
    /// Departmanin benzersiz ID'si.
    /// Veritabani tarafindan otomatik olusturulur.
    /// Calisan tablosunda foreign key olarak kullanilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Departman adi.
    /// Benzersiz olmali.
    /// UI'da dropdown, liste ve raporlarda gosterilir.
    /// Ornek: "Satis", "Teknik Servis", "Muhasebe"
    /// </summary>
    public string DepartmentName { get; set; } = null!;

    /// <summary>
    /// Departman aciklamasi (opsiyonel).
    /// Departmanin sorumluluk alanini tanimlar.
    /// Ornek: "Kasa islemleri, musteri iliskileri ve satis operasyonlari"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bu departmandaki calisan sayisi (hesaplanan).
    /// Departman buyuklugunu gosterir.
    /// Sifir ise bos departman (silinebilir mi kontrolu icin).
    /// </summary>
    public int EmployeeCount { get; set; }

    /// <summary>
    /// Departmanin olusturulma tarihi.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni departman olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Yeni Departman Ekle" formundan gelen verileri tasir.
/// ID ve EmployeeCount yoktur (sistem tarafindan yonetilir).
///
/// UI FORMU:
/// ---------
/// - Departman Adi: TextBox (zorunlu, benzersiz)
/// - Aciklama: TextArea (opsiyonel)
///
/// ORNEK:
/// ------
/// var newDept = new CreateDepartmentDto {
///     DepartmentName = "Pazarlama",
///     Description = "Reklam, kampanya ve sosyal medya yonetimi"
/// };
/// await _departmentService.CreateAsync(newDept);
/// </summary>
public class CreateDepartmentDto
{
    /// <summary>
    /// Departman adi (zorunlu).
    /// Benzersiz olmali - ayni isimde departman varsa hata doner.
    /// </summary>
    public string DepartmentName { get; set; } = null!;

    /// <summary>
    /// Departman aciklamasi (opsiyonel).
    /// Sorumluluk alanini tanimlayan metin.
    /// </summary>
    public string? Description { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut departman bilgilerini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Departman Duzenle" formundan gelen verileri tasir.
/// CreateDepartmentDto'dan farki: ID alani vardir.
///
/// DIKKAT:
/// -------
/// Departman silindiginde veya degistirildiginde:
/// - Silme: Bagli calisan varsa silinememeli
/// - Guncelleme: Calisanlari etkilemez (sadece ID ile iliski var)
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateDepartmentDto {
///     Id = 3,
///     DepartmentName = "Teknik Servis ve Destek",  // Genisletildi
///     Description = "Cihaz tamiri, yazilim destegi, garanti islemleri"
/// };
/// await _departmentService.UpdateAsync(updateDto);
/// </summary>
public class UpdateDepartmentDto
{
    /// <summary>
    /// Guncellenecek departmanin ID'si (zorunlu).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Departman adi.
    /// Degistirilirse benzersizlik kontrolu yapilir.
    /// </summary>
    public string DepartmentName { get; set; } = null!;

    /// <summary>
    /// Departman aciklamasi.
    /// </summary>
    public string? Description { get; set; }
}

#endregion
