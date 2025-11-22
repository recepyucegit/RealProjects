// ===================================================================================
// TEKNOROMA - TEDARIKCI DTO DOSYASI (SupplierDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, tedarikci (Supplier) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin tedarikci yonetimi icin kullanilir.
//
// TEDARIKCI KAVRAMI
// -----------------
// Tedarikci, magazaya urun saglayan firma veya kisidir.
// Her tedarikci bir veya daha fazla urun cesidi temin edebilir.
// Ornek tedarikciler:
// - Samsung Turkiye A.S.
// - Apple Turkiye
// - Xiaomi Distributorluk
// - Yerel aksesuarci
//
// TEDARIKCI YONETIMI NEDEN ONEMLI?
// --------------------------------
// 1. Siparis verirken tedarikci bilgisi gerekli
// 2. Odeme takibi (borc/alacak)
// 3. Urun temin suresi ve kalite takibi
// 4. Fiyat karsilastirma ve pazarlik
// 5. Garanti ve iade islemleri
//
// DTO TURLERI
// -----------
// 1. SupplierDto (Okuma): Tedarikci bilgilerini goruntulemek icin
// 2. CreateSupplierDto (Olusturma): Yeni tedarikci kaydi icin
// 3. UpdateSupplierDto (Guncelleme): Tedarikci bilgisi guncellemek icin
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Supplier.cs
//
// ILISKILI SERVISLER
// ------------------
// Application/Interfaces/ISupplierService.cs
// - CRUD islemleri
// - Tedarikci bakiyesi sorgulama
// - Islem gecmisi
//
// ===================================================================================

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Tedarikci bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan tedarikci bilgilerini UI'a tasir.
/// Iliskili urun sayisi ve toplam islem tutarini da icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Tedarikci listesi tablosu (DataGrid)
/// - Urun ekleme formunda tedarikci dropdown'i
/// - Siparis olusturma ekrani
/// - Tedarikci detay karti
///
/// ORNEK:
/// ------
/// var suppliers = await _supplierService.GetAllAsync();
/// foreach (var sup in suppliers)
/// {
///     Console.WriteLine($"{sup.SupplierName}");
///     Console.WriteLine($"  Yetkili: {sup.ContactPerson}");
///     Console.WriteLine($"  Urun Sayisi: {sup.ProductCount}");
///     Console.WriteLine($"  Toplam Islem: {sup.TotalTransactionAmount:C}");
/// }
/// </summary>
public class SupplierDto
{
    /// <summary>
    /// Tedarikcinin benzersiz ID'si.
    /// Veritabani tarafindan otomatik olusturulur.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tedarikci firma/kisi adi.
    /// Resmi unvan veya ticari isim.
    /// Ornek: "Samsung Elektronik Sanayi ve Ticaret A.S."
    /// </summary>
    public string SupplierName { get; set; } = null!;

    /// <summary>
    /// Irtibat kisisi (opsiyonel).
    /// Tedarikci firmadaki muhatap kisi.
    /// Siparis ve iletisim icin.
    /// Ornek: "Ahmet Bey - Satis Muduru"
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Telefon numarasi (opsiyonel).
    /// Siparis vermek, sorgulamak icin.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi (opsiyonel).
    /// Siparis, fatura islemleri icin.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Adres bilgisi (opsiyonel).
    /// Teslimat ve fatura adresi.
    /// Ziyaret gerektiren durumlarda.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Bu tedarikciden alinan urun cesit sayisi (hesaplanan).
    /// Tedarikci onem derecesini gosterir.
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Toplam islem tutari (hesaplanan).
    /// Bu tedarikciye yapilan tum odemelerin toplami.
    /// Tedarikci iliskisinin buyuklugunu gosterir.
    /// </summary>
    public decimal TotalTransactionAmount { get; set; }

    /// <summary>
    /// Kayit tarihi.
    /// Tedarikci ne zamandan beri calisiyor.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni tedarikci kaydi olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Yeni Tedarikci Ekle" formundan gelen verileri tasir.
/// ID ve hesaplanan alanlar (ProductCount, TotalTransactionAmount) yoktur.
///
/// UI FORMU:
/// ---------
/// - Firma Adi: TextBox (zorunlu)
/// - Yetkili Kisi: TextBox (opsiyonel)
/// - Telefon: TextBox (opsiyonel)
/// - E-posta: TextBox (opsiyonel)
/// - Adres: TextArea (opsiyonel)
///
/// ORNEK:
/// ------
/// var newSupplier = new CreateSupplierDto {
///     SupplierName = "Xiaomi Turkiye Distributorluk",
///     ContactPerson = "Mehmet Yilmaz",
///     Phone = "0212 555 1234",
///     Email = "siparis@xiaomi-tr.com"
/// };
/// await _supplierService.CreateAsync(newSupplier);
/// </summary>
public class CreateSupplierDto
{
    /// <summary>
    /// Tedarikci firma adi (zorunlu).
    /// Resmi unvan veya ticari isim.
    /// </summary>
    public string SupplierName { get; set; } = null!;

    /// <summary>
    /// Irtibat kisisi (opsiyonel).
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Telefon numarasi (opsiyonel).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// E-posta adresi (opsiyonel).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Adres bilgisi (opsiyonel).
    /// </summary>
    public string? Address { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut tedarikci bilgilerini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Tedarikci Duzenle" formundan gelen verileri tasir.
/// CreateSupplierDto'dan farki: ID alani vardir.
///
/// KULLANIM SENARYOLARI:
/// ---------------------
/// - Yetkili kisi degisti
/// - Telefon/adres guncellendi
/// - Firma adi degisti (birlesme, devir vb.)
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateSupplierDto {
///     Id = 10,
///     SupplierName = "Xiaomi Turkiye A.S.",  // Sirketlesti
///     ContactPerson = "Ali Veli",  // Yeni yetkili
///     Phone = "0212 666 7890"      // Yeni telefon
/// };
/// await _supplierService.UpdateAsync(updateDto);
/// </summary>
public class UpdateSupplierDto
{
    /// <summary>
    /// Guncellenecek tedarikcinin ID'si (zorunlu).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tedarikci firma adi.
    /// </summary>
    public string SupplierName { get; set; } = null!;

    /// <summary>
    /// Irtibat kisisi.
    /// </summary>
    public string? ContactPerson { get; set; }

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
}

#endregion
