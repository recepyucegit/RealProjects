// ===================================================================================
// TEKNOROMA - MUSTERI DTO DOSYASI (CustomerDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, musteri (Customer) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin musteri yonetimi ve sadakat programi icin kullanilir.
//
// MUSTERI KAVRAMI
// ---------------
// Musteri, magazadan alisveris yapan kisilerdir. Musterilerin sisteme kaydedilmesi:
// 1. Sadakat puani kazanmalarini saglar
// 2. Alisveris gecmisinin takibini saglar
// 3. Kisisellestirilmis kampanyalar icin verI saglar
// 4. Garanti ve servis takibi icin iletisim bilgisi saglar
//
// NOT: Musteri kaydi opsiyoneldir, anonim satis da yapilabilir.
//
// DTO TURLERI
// -----------
// 1. CustomerDto (Okuma): Musteri bilgilerini goruntulemek icin
// 2. CreateCustomerDto (Olusturma): Yeni musteri kaydi icin
// 3. UpdateCustomerDto (Guncelleme): Musteri bilgisi guncelleme icin
// 4. CustomerLoyaltyDto (Sadakat): Musteri sadakat puani bilgileri
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Customer.cs
// - Veritabani tablosunu temsil eden entity sinifi
//
// ILISKILI SERVISLER
// ------------------
// Application/Interfaces/ICustomerService.cs
// - CRUD islemleri
// - Sadakat puani yonetimi
// - Alisveris gecmisi sorgulama
//
// ===================================================================================

using Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Musteri bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan musteri bilgilerini UI'a tasir.
/// Customer entity'sinin dis dunyaya acilan yuzu.
///
/// UI KULLANIMI:
/// -------------
/// - Musteri listesi tablosu (DataGrid)
/// - Musteri arama sonuclari
/// - Musteri detay kartI
/// - Satis ekraninda musteri secimi popup'i
///
/// ORNEK:
/// ------
/// var customer = await _customerService.GetByIdAsync(id);
/// Console.WriteLine($"Musteri: {customer.FullName}");
/// Console.WriteLine($"Telefon: {customer.Phone}");
/// Console.WriteLine($"Puan: {customer.LoyaltyPoints}");
/// Console.WriteLine($"Toplam Alisveris: {customer.TotalPurchases}");
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Musterinin benzersiz ID'si.
    /// Veritabani tarafindan otomatik olusturulur.
    /// Satis islemlerinde musteri eslestirmesi icin kullanilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Musterinin adi.
    /// Ornek: "Ahmet"
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Musterinin soyadi.
    /// Ornek: "Yilmaz"
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Ad ve soyadın birlesimi (hesaplanan alan).
    /// UI'da gosterim icin kullanilir.
    /// Ornek: "Ahmet Yilmaz"
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Telefon numarasi (opsiyonel).
    /// Musteri ile iletisim, SMS kampanya icin.
    /// Format: "05XX XXX XX XX"
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi (opsiyonel).
    /// E-posta kampanyalari, fatura gonderimi icin.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Adres bilgisi (opsiyonel).
    /// Teslimat, fatura adresi icin.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Cinsiyet (enum).
    /// Male = Erkek, Female = Kadin, Other = Diger
    /// Kisisellestirilmis kampanyalar icin.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Cinsiyetin metinsel gosterimi.
    /// UI'da dropdown veya badge olarak.
    /// </summary>
    public string GenderDisplay => Gender.ToString();

    /// <summary>
    /// Mevcut sadakat puani.
    /// Her alisveriste belirli oranda puan kazanilir.
    /// Puanlar sonraki alisverislerde indirim olarak kullanilabilir.
    /// Ornek: 150.50 puan = 150.50 TL indirim hakki
    /// </summary>
    public decimal LoyaltyPoints { get; set; }

    /// <summary>
    /// Kayit tarihi.
    /// Musteri ne zaman kayit olmus.
    /// Musteri yasam dongusu analizi icin.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Toplam alisveris sayisi.
    /// Musteri ne kadar sadik (sik alisveris yapan).
    /// VIP musteri tespiti icin kullanilir.
    /// </summary>
    public int TotalPurchases { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni musteri kaydi olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Yeni Musteri" formundan gelen verileri tasir.
/// ID ve hesaplanan alanlar (LoyaltyPoints, TotalPurchases) yoktur.
///
/// KULLANIM SENARYOLARI:
/// ---------------------
/// 1. Kasa ekraninda "Yeni Musteri" butonu
/// 2. Backoffice'te musteri yonetimi
/// 3. Musteri self-servis kayit (varsa)
///
/// UI FORMU:
/// ---------
/// - Ad: TextBox (zorunlu)
/// - Soyad: TextBox (zorunlu)
/// - Telefon: TextBox (opsiyonel, format kontrolu)
/// - E-posta: TextBox (opsiyonel, format kontrolu)
/// - Adres: TextArea (opsiyonel)
/// - Cinsiyet: RadioButton veya Dropdown
///
/// ORNEK:
/// ------
/// var newCustomer = new CreateCustomerDto {
///     FirstName = "Ayse",
///     LastName = "Kaya",
///     Phone = "05551234567",
///     Email = "ayse@email.com",
///     Gender = Gender.Female
/// };
/// var result = await _customerService.CreateAsync(newCustomer);
/// </summary>
public class CreateCustomerDto
{
    /// <summary>
    /// Musteri adi (zorunlu).
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Musteri soyadi (zorunlu).
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Telefon numarasi (opsiyonel).
    /// Validasyon: Turkiye telefon formati.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi (opsiyonel).
    /// Validasyon: Gecerli e-posta formati.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Adres bilgisi (opsiyonel).
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Cinsiyet (varsayilan: Male).
    /// </summary>
    public Gender Gender { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut musteri bilgilerini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Musteri Duzenle" formundan gelen verileri tasir.
/// CreateCustomerDto'dan farki: ID alani vardir.
///
/// UI AKISI:
/// ---------
/// 1. Musteri listesinden kayit secilir
/// 2. GetByIdAsync ile mevcut veriler getirilir
/// 3. Form doldurulur (mevcut verilerle)
/// 4. Kullanici degisiklikleri yapar
/// 5. UpdateAsync(dto) cagrilir
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateCustomerDto {
///     Id = 15,
///     FirstName = "Ayse",
///     LastName = "Yilmaz",  // Evlendi, soyadi degisti
///     Phone = "05559876543",  // Telefon degisti
///     // ... diger alanlar
/// };
/// await _customerService.UpdateAsync(updateDto);
/// </summary>
public class UpdateCustomerDto
{
    /// <summary>
    /// Guncellenecek musterinin ID'si (zorunlu).
    /// Hidden field olarak formda tutulur.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Musteri adi.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Musteri soyadi.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Telefon numarasi.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Adres bilgisi.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Cinsiyet.
    /// </summary>
    public Gender Gender { get; set; }
}

#endregion

#region SADAKAT PROGRAMI DTO'SU

/// <summary>
/// Musteri sadakat puani bilgileri icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Musterinin sadakat programi detaylarini gosterir.
/// Puan kazanma, harcama ve gecmis bilgilerini icerir.
///
/// SADAKAT PROGRAMI NASIL CALISIR?
/// -------------------------------
/// 1. Musteri alisveris yapar
/// 2. Toplam tutarin belirli yuzdesi kadar puan kazanir (ornek: %1)
/// 3. Biriken puanlar sonraki alisveriste indirim olarak kullanilabilir
/// 4. 1 puan = 1 TL degerindedir (veya farkli oran)
///
/// UI KULLANIMI:
/// -------------
/// - Musteri detay sayfasinda "Sadakat Bilgileri" karti
/// - Satis ekraninda puan kullanim popup'i
/// - Musteri ozet bilgisi
///
/// ORNEK:
/// ------
/// var loyalty = await _customerService.GetLoyaltyInfoAsync(customerId);
/// Console.WriteLine($"Mevcut Puan: {loyalty.CurrentPoints}");
/// Console.WriteLine($"Toplam Kazanilan: {loyalty.LifetimePoints}");
/// Console.WriteLine($"Kullanilan: {loyalty.PointsRedeemed}");
/// </summary>
public class CustomerLoyaltyDto
{
    /// <summary>
    /// Musteri ID'si.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Musteri tam adi.
    /// Gosterim amacli.
    /// </summary>
    public string CustomerName { get; set; } = null!;

    /// <summary>
    /// Mevcut kullanilabilir puan.
    /// Indirim olarak kullanilabilecek miktar.
    /// </summary>
    public decimal CurrentPoints { get; set; }

    /// <summary>
    /// Omur boyu kazanilan toplam puan.
    /// Musteri sadakat seviyesi belirlemede kullanilir.
    /// </summary>
    public decimal LifetimePoints { get; set; }

    /// <summary>
    /// Simdiye kadar kullanilan (harcanan) toplam puan.
    /// LifetimePoints - PointsRedeemed = CurrentPoints
    /// </summary>
    public decimal PointsRedeemed { get; set; }
}

#endregion
