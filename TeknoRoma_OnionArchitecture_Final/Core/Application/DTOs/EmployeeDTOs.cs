// ===================================================================================
// TEKNOROMA - CALISAN DTO DOSYASI (EmployeeDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, calisan (Employee) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin personel yonetimi ve kimlik dogrulama icin kullanilir.
//
// CALISAN KAVRAMI
// ---------------
// Calisanlar, sistemde islem yapan kullanicilardir:
// - Kasiyerler: Satis islemi yapar
// - Teknisyenler: Teknik servis islemi yapar
// - Yoneticiler: Raporlari gorur, ayar yapar
// - Mudurler: Tam yetki
//
// YETKİLENDİRME SİSTEMİ
// ---------------------
// Her calisanin bir rolu (UserRole) vardir.
// Bu rol, sistemde hangi islemleri yapabilecegini belirler.
// Rol tabanli yetkilendirme (RBAC) kullanilir.
//
// DTO TURLERI
// -----------
// 1. EmployeeDto (Okuma): Calisan bilgilerini goruntulemek icin
// 2. CreateEmployeeDto (Olusturma): Yeni calisan kaydi icin
// 3. UpdateEmployeeDto (Guncelleme): Calisan bilgisi guncelleme icin
// 4. LoginDto (Giris): Kullanici girisi icin
// 5. LoginResultDto (Giris Sonucu): Giris islem sonucu
// 6. ChangePasswordDto (Sifre): Sifre degistirme icin
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Employee.cs
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/IEmployeeService.cs
// - CRUD islemleri
// - Login/Logout
// - Sifre yonetimi
//
// ===================================================================================

using Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Calisan bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan calisan bilgilerini UI'a tasir.
/// Hassas bilgiler (sifre hash'i) DAHIL EDILMEZ - guvenlik!
///
/// UI KULLANIMI:
/// -------------
/// - Calisan listesi tablosu (DataGrid)
/// - Calisan profil sayfasi
/// - Yonetici panelinde calisan yonetimi
/// - Satis fisi uzerinde kasiyer bilgisi
///
/// ORNEK:
/// ------
/// var employees = await _employeeService.GetAllAsync();
/// foreach (var emp in employees)
/// {
///     Console.WriteLine($"{emp.FullName} - {emp.RoleDisplay}");
///     Console.WriteLine($"  Magaza: {emp.StoreName}");
///     Console.WriteLine($"  Maas: {emp.Salary:C}");
/// }
/// </summary>
public class EmployeeDto
{
    /// <summary>
    /// Calisanin benzersiz ID'si.
    /// Veritabani tarafindan otomatik olusturulur.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Calisanin adi.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Calisanin soyadi.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Tam ad (hesaplanan alan).
    /// UI gosterimi icin.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Telefon numarasi (opsiyonel).
    /// Acil iletisim icin.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi.
    /// Giris (login) icin kullanici adi olarak kullanilir.
    /// Benzersiz olmali!
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Adres bilgisi (opsiyonel).
    /// Bordro, ozluk isleri icin.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Cinsiyet (enum).
    /// Ozluk bilgisi.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Cinsiyetin metinsel gosterimi.
    /// </summary>
    public string GenderDisplay => Gender.ToString();

    /// <summary>
    /// Kullanici rolu (enum).
    /// Sistemdeki yetki seviyesini belirler.
    /// Admin, Mudur, Kasiyer, Teknisyen vb.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Rolun metinsel gosterimi.
    /// UI'da badge veya etiket olarak.
    /// </summary>
    public string RoleDisplay => Role.ToString();

    /// <summary>
    /// Aylik maas tutari (TL).
    /// Sadece yetkililer gorebilir (gizlilik!).
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Ise baslama tarihi.
    /// Kidem hesaplama, izin hak edis icin.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Aktif mi?
    /// false = Isten ayrılmis veya askiya alinmis.
    /// Pasif calisanlar sisteme giris yapamaz.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Bagli oldugu magaza ID'si (opsiyonel).
    /// null = Merkez ofis veya tum subelere yetkili.
    /// </summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// Magaza adi (navigation property'den).
    /// </summary>
    public string? StoreName { get; set; }

    /// <summary>
    /// Bagli oldugu departman ID'si (opsiyonel).
    /// Ornek: Satis, Teknik Servis, Muhasebe
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Departman adi (navigation property'den).
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Kayit tarihi.
    /// Calisanin sisteme eklendigi tarih.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni calisan kaydi olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Yeni Calisan Ekle" formundan gelen verileri tasir.
/// Ilk sifre bu formda belirlenir.
///
/// ONEMLI ALANLAR:
/// ---------------
/// - Email: Benzersiz olmali (giris icin kullanilacak)
/// - Password: Hash'lenerek saklanir (asla plain text degil!)
/// - Role: Sistem yetkilerini belirler
///
/// UI FORMU:
/// ---------
/// - Ad, Soyad: TextBox (zorunlu)
/// - E-posta: TextBox (zorunlu, unique)
/// - Telefon, Adres: TextBox (opsiyonel)
/// - Cinsiyet: RadioButton
/// - Rol: Dropdown (Admin, Kasiyer vb.)
/// - Maas: NumberBox
/// - Ise Baslama: DatePicker
/// - Magaza: Dropdown
/// - Departman: Dropdown
/// - Sifre: PasswordBox (zorunlu)
///
/// ORNEK:
/// ------
/// var newEmployee = new CreateEmployeeDto {
///     FirstName = "Mehmet",
///     LastName = "Demir",
///     Email = "mehmet@teknoroma.com",
///     Password = "Guclu$ifre123",
///     Role = UserRole.KasaSatis,
///     Salary = 25000,
///     StoreId = 1
/// };
/// await _employeeService.CreateAsync(newEmployee);
/// </summary>
public class CreateEmployeeDto
{
    /// <summary>
    /// Calisan adi (zorunlu).
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Calisan soyadi (zorunlu).
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Telefon numarasi (opsiyonel).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi (zorunlu, benzersiz).
    /// Giris isleminde kullanici adi olarak kullanilir.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Adres bilgisi (opsiyonel).
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Cinsiyet.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Kullanici rolu (varsayilan: KasaSatis).
    /// Yeni calisanlar genellikle kasiyer olarak baslar.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.KasaSatis;

    /// <summary>
    /// Aylik maas tutari.
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Ise baslama tarihi (varsayilan: bugun).
    /// </summary>
    public DateTime HireDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Magaza ID'si (opsiyonel).
    /// Hangi subede calisacak.
    /// </summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// Departman ID'si (opsiyonel).
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Ilk sifre (zorunlu).
    /// Hash'lenerek veritabanina kaydedilir.
    /// Guclu sifre kurallari uygulanmali!
    /// </summary>
    public string Password { get; set; } = null!;
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut calisan bilgilerini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Calisan Duzenle" formundan gelen verileri tasir.
/// Sifre bu DTO'da YOKTUR - ayri bir islem (ChangePasswordDto).
///
/// NOT:
/// ----
/// IsActive alani burada var - calisani pasif yapma yetkisi.
/// Pasif yapilan calisan sisteme giris yapamaz.
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateEmployeeDto {
///     Id = 5,
///     FirstName = "Mehmet",
///     LastName = "Demir",
///     Role = UserRole.Mudur,  // Terfi!
///     Salary = 35000,         // Zam!
///     IsActive = true
/// };
/// await _employeeService.UpdateAsync(updateDto);
/// </summary>
public class UpdateEmployeeDto
{
    /// <summary>
    /// Guncellenecek calisanin ID'si (zorunlu).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Calisan adi.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Calisan soyadi.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Telefon numarasi.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi.
    /// Degistirilirse benzersizlik kontrolu yapilmali.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Adres bilgisi.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Cinsiyet.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Kullanici rolu.
    /// Terfi/tenzil durumunda degisir.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Maas tutari.
    /// Zam/dusurme durumunda degisir.
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Aktiflik durumu.
    /// Isten cikarma veya askiya alma icin false yapilir.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Magaza ID'si.
    /// Sube degisikligi icin.
    /// </summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// Departman ID'si.
    /// Departman degisikligi icin.
    /// </summary>
    public int? DepartmentId { get; set; }
}

#endregion

#region KIMLIK DOGRULAMA DTO'LARI

/// <summary>
/// Kullanici girisi (login) icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Login ekranindan gelen bilgileri tasir.
/// E-posta + Sifre kombinasyonu ile dogrulama yapilir.
///
/// GUVENLIK NOTLARI:
/// -----------------
/// - Sifre plain text gelir ama hash'lenerek kontrol edilir
/// - HTTPS kullanilmali (sifre sifreli iletilsin)
/// - Basarisiz giris denemeleri loglanmali
/// - Belirli sayida basarisiz denemede hesap kilitlenmeli
///
/// UI KULLANIMI:
/// -------------
/// Login sayfasindaki formdan gelir.
/// - E-posta: TextBox
/// - Sifre: PasswordBox
///
/// ORNEK:
/// ------
/// var loginDto = new LoginDto {
///     Email = "mehmet@teknoroma.com",
///     Password = "Guclu$ifre123"
/// };
/// var result = await _employeeService.LoginAsync(loginDto);
/// if (result.Success)
///     // Basarili giris, session olustur
/// else
///     // Hata mesaji goster
/// </summary>
public class LoginDto
{
    /// <summary>
    /// E-posta adresi (kullanici adi).
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Sifre (plain text - hash'lenecek).
    /// </summary>
    public string Password { get; set; } = null!;
}

/// <summary>
/// Login islem sonucu icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Login isleminin sonucunu doner.
/// Basariliysa calisan bilgileri ve (opsiyonel) JWT token icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Success = true: Ana sayfaya yonlendir
/// - Success = false: Hata mesaji goster
/// - Employee: Session'a kaydet
/// - Token: API isteklerinde kullan
///
/// ORNEK BASARILI YANIT:
/// --------------------
/// {
///   "success": true,
///   "message": "Giris basarili",
///   "employee": { "id": 5, "fullName": "Mehmet Demir", "role": "Kasiyer" },
///   "token": "eyJhbGciOiJIUzI1NiIs..."
/// }
///
/// ORNEK BASARISIZ YANIT:
/// ---------------------
/// {
///   "success": false,
///   "message": "E-posta veya sifre hatali"
/// }
/// </summary>
public class LoginResultDto
{
    /// <summary>
    /// Giris basarili mi?
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Sonuc mesaji.
    /// Basari: "Giris basarili"
    /// Hata: "E-posta veya sifre hatali", "Hesap pasif"
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Giris yapan calisanin bilgileri.
    /// Basariliysa dolu, basarisizsa null.
    /// Session'a kaydedilir.
    /// </summary>
    public EmployeeDto? Employee { get; set; }

    /// <summary>
    /// JWT token (API authentication icin).
    /// Web API kullaniliyorsa her istekte header'da gonderilir.
    /// Opsiyonel - session tabanli auth kullaniliyorsa null.
    /// </summary>
    public string? Token { get; set; }
}

/// <summary>
/// Sifre degistirme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Kullanicinin kendi sifresini degistirmesi icin kullanilir.
/// Mevcut sifre dogrulanmadan yeni sifre kabul edilmez.
///
/// GUVENLIK:
/// ---------
/// - Mevcut sifre zorunlu (hesabi ele gecirmeyi onler)
/// - Yeni sifre guclu olmali (min 8 karakter, buyuk/kucuk harf, rakam)
/// - Son X sifre tekrar kullanilamaz (opsiyonel)
///
/// UI FORMU:
/// ---------
/// - Mevcut Sifre: PasswordBox
/// - Yeni Sifre: PasswordBox
/// - Yeni Sifre (Tekrar): PasswordBox (frontend kontrolu)
///
/// ORNEK:
/// ------
/// var changeDto = new ChangePasswordDto {
///     EmployeeId = 5,
///     CurrentPassword = "EskiSifre123",
///     NewPassword = "YeniGuclu$ifre456"
/// };
/// var result = await _employeeService.ChangePasswordAsync(changeDto);
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Sifresi degistirilecek calisanin ID'si.
    /// Genellikle oturum acmis kullanicinin ID'si.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Mevcut sifre (dogrulama icin).
    /// Yanlis girilirse islem reddedilir.
    /// </summary>
    public string CurrentPassword { get; set; } = null!;

    /// <summary>
    /// Yeni sifre.
    /// Guclu sifre kurallarina uygun olmali.
    /// </summary>
    public string NewPassword { get; set; } = null!;
}

#endregion
