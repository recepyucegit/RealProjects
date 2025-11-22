// ===================================================================================
// TEKNOROMA - TEKNIK SERVIS DTO DOSYASI (TechnicalServiceDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, teknik servis (TechnicalService) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin cihaz tamir ve bakim hizmetleri icin kullanilir.
//
// TEKNIK SERVIS KAVRAMI
// ---------------------
// Teknik servis, musterilerin bozuk veya arizali cihazlarinin tamir edilmesi
// hizmetini kapsar. Bir teknik servis kaydi (ticket/fis) su asamalardan gecer:
//
// SERVIS DURUMU (TechnicalServiceStatus enum):
// --------------------------------------------
// 1. Received (Teslim Alindi): Cihaz magazaya getirildi
// 2. Diagnosing (Teshis): Ariza tespiti yapiliyor
// 3. WaitingParts (Parca Bekleniyor): Yedek parca siparisi verildi
// 4. Repairing (Tamir Ediliyor): Aktif tamir islemi
// 5. Completed (Tamamlandi): Tamir bitti, musteri bilgilendirildi
// 6. Delivered (Teslim Edildi): Cihaz musteriye verildi
// 7. Cancelled (Iptal): Tamir iptal edildi
//
// SERVIS AKISI
// ------------
// 1. Musteri cihazini getirir
// 2. Kayit olusturulur, fis verilir
// 3. Teknisyen atanir
// 4. Ariza teshisi yapilir
// 5. Maliyet belirlenir, musteri onaylar
// 6. Tamir yapilir
// 7. Musteri aranir, cihaz teslim edilir
//
// DTO TURLERI
// -----------
// 1. TechnicalServiceDto (Okuma): Servis kaydini goruntulemek icin
// 2. CreateTechnicalServiceDto (Olusturma): Yeni servis kaydi icin
// 3. UpdateTechnicalServiceDto (Guncelleme): Kayit guncellemek icin
// 4. TechnicalServiceStatusUpdateDto (Durum): Sadece durum degistirmek icin
// 5. TechnicalServiceSummaryDto (Ozet): Dashboard istatistikleri icin
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/TechnicalService.cs
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/ITechnicalServiceService.cs
//
// ===================================================================================

using Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Teknik servis kaydini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan servis kayitlarini UI'a tasir.
/// Cihaz bilgileri, musteri bilgileri ve durum bilgilerini icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Servis listesi tablosu (tum kayitlar)
/// - Servis detay sayfasi
/// - Musteri servis gecmisi
/// - Teknisyen is listesi
///
/// ORNEK:
/// ------
/// var ticket = await _technicalServiceService.GetByIdAsync(id);
/// Console.WriteLine($"Fis No: {ticket.Id}");
/// Console.WriteLine($"Cihaz: {ticket.Brand} {ticket.Model}");
/// Console.WriteLine($"Problem: {ticket.ProblemDescription}");
/// Console.WriteLine($"Durum: {ticket.StatusDisplay}");
/// </summary>
public class TechnicalServiceDto
{
    /// <summary>
    /// Servis fis numarasi.
    /// Musteriye verilen ve cihazi takip icin kullanilan numara.
    /// UI'da buyuk puntolarla "Fis No: 12345" seklinde gosterilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Cihaz turu.
    /// Ne tur bir cihaz getirildigi.
    /// Ornek: "Telefon", "Laptop", "Tablet", "Televizyon"
    /// </summary>
    public string DeviceType { get; set; } = null!;

    /// <summary>
    /// Marka (opsiyonel).
    /// Cihazin markasi.
    /// Ornek: "Samsung", "Apple", "Xiaomi"
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model (opsiyonel).
    /// Cihazin modeli.
    /// Ornek: "Galaxy S24", "iPhone 15 Pro"
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Seri numarasi (opsiyonel).
    /// Cihazin IMEI veya seri numarasi.
    /// Garanti kontrolu ve cihaz dogrulama icin.
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Problem tanimi.
    /// Musterinin bildirdigi ariza/sikayet.
    /// Ornek: "Ekran kirikligi", "Sarj olmuyor", "Acilmiyor"
    /// </summary>
    public string ProblemDescription { get; set; } = null!;

    /// <summary>
    /// Teshis notlari (opsiyonel).
    /// Teknisyenin ariza tespiti sonucu yazdigi notlar.
    /// Teknik detaylar, bulunan sorunlar.
    /// </summary>
    public string? DiagnosticNotes { get; set; }

    /// <summary>
    /// Tamir notlari (opsiyonel).
    /// Yapilan islemlerin kaydi.
    /// Ornek: "Ekran degistirildi, test edildi"
    /// </summary>
    public string? RepairNotes { get; set; }

    /// <summary>
    /// Tahmini maliyet (opsiyonel).
    /// Teshis sonrasi musteriye soylenen tahmini ucret.
    /// Musteri onayindan once belirlenir.
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Son maliyet (opsiyonel).
    /// Tamir tamamlandiginda kesinlesen ucret.
    /// Fatura tutari olarak kullanilir.
    /// </summary>
    public decimal? FinalCost { get; set; }

    /// <summary>
    /// Servis durumu (enum).
    /// Cihazin hangi asamada oldugu.
    /// </summary>
    public TechnicalServiceStatus Status { get; set; }

    /// <summary>
    /// Durumun metinsel gosterimi.
    /// UI'da renkli badge olarak gosterilir.
    /// </summary>
    public string StatusDisplay => Status.ToString();

    /// <summary>
    /// Cihazin teslim alindigi tarih.
    /// Servis baslangic tarihi.
    /// </summary>
    public DateTime ReceivedDate { get; set; }

    /// <summary>
    /// Tamir tamamlanma tarihi (opsiyonel).
    /// null = Henuz tamamlanmadi.
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Musteriye teslim tarihi (opsiyonel).
    /// null = Henuz teslim edilmedi.
    /// </summary>
    public DateTime? DeliveredDate { get; set; }

    /// <summary>
    /// Musteri ID'si (opsiyonel).
    /// Kayitli musteri varsa ID'si.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Musteri adi (navigation property'den).
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Musteri telefonu.
    /// Tamamlandiginda arama yapmak icin.
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Atanan teknisyen ID'si (opsiyonel).
    /// null = Henuz teknisyen atanmadi.
    /// </summary>
    public int? TechnicianId { get; set; }

    /// <summary>
    /// Teknisyen adi (navigation property'den).
    /// </summary>
    public string? TechnicianName { get; set; }

    /// <summary>
    /// Magaza ID'si.
    /// Cihazin teslim alindigi sube.
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Magaza adi (navigation property'den).
    /// </summary>
    public string? StoreName { get; set; }

    /// <summary>
    /// Kayit tarihi.
    /// Sistemde olusturulma zamani.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni teknik servis kaydi olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Musteri cihazini getirdiginde yeni kayit olusturmak icin.
/// Teslim alma formundan gelen verileri tasir.
///
/// UI FORMU:
/// ---------
/// - Cihaz Turu: Dropdown (zorunlu)
/// - Marka: TextBox (opsiyonel)
/// - Model: TextBox (opsiyonel)
/// - Seri No: TextBox (opsiyonel)
/// - Problem: TextArea (zorunlu)
/// - Tahmini Ucret: NumberBox (opsiyonel)
/// - Musteri: Dropdown veya Yeni Musteri alanlari
///
/// ORNEK:
/// ------
/// var newTicket = new CreateTechnicalServiceDto {
///     DeviceType = "Telefon",
///     Brand = "Samsung",
///     Model = "Galaxy S24",
///     ProblemDescription = "Ekran kirikligi",
///     EstimatedCost = 2500,
///     CustomerId = 15,
///     StoreId = 1
/// };
/// await _technicalServiceService.CreateAsync(newTicket);
/// </summary>
public class CreateTechnicalServiceDto
{
    /// <summary>
    /// Cihaz turu (zorunlu).
    /// </summary>
    public string DeviceType { get; set; } = null!;

    /// <summary>
    /// Marka (opsiyonel).
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model (opsiyonel).
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Seri numarasi (opsiyonel).
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Problem tanimi (zorunlu).
    /// Musterinin belirttigi ariza.
    /// </summary>
    public string ProblemDescription { get; set; } = null!;

    /// <summary>
    /// Tahmini maliyet (opsiyonel).
    /// On degerelendirme sonrasi belirlenebilir.
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Musteri ID'si (opsiyonel).
    /// Mevcut kayitli musteri secilirse.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Teknisyen ID'si (opsiyonel).
    /// Direkt atanacaksa.
    /// </summary>
    public int? TechnicianId { get; set; }

    /// <summary>
    /// Magaza ID'si (zorunlu).
    /// Teslim alinan sube.
    /// </summary>
    public int StoreId { get; set; }

    // Yeni musteri alanlari (CustomerId bos ise kullanilir)

    /// <summary>
    /// Yeni musteri adi.
    /// Kayitli musteri yoksa buradan yeni kayit olusturulur.
    /// </summary>
    public string? CustomerFirstName { get; set; }

    /// <summary>
    /// Yeni musteri soyadi.
    /// </summary>
    public string? CustomerLastName { get; set; }

    /// <summary>
    /// Yeni musteri telefonu.
    /// Tamamlandiginda aramak icin gerekli.
    /// </summary>
    public string? CustomerPhone { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut teknik servis kaydini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Servis sureci boyunca kayit guncellemeleri icin.
/// Teshis notlari, tamir notlari, maliyet vb.
///
/// NOT:
/// ----
/// Durum degisikligi icin TechnicalServiceStatusUpdateDto kullanin.
/// Bu DTO tum alanlari gunceller.
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateTechnicalServiceDto {
///     Id = 50,
///     DeviceType = "Telefon",
///     Brand = "Samsung",
///     DiagnosticNotes = "Ekran konnektoru hasar gormus",
///     EstimatedCost = 2000,
///     Status = TechnicalServiceStatus.WaitingParts
/// };
/// await _technicalServiceService.UpdateAsync(updateDto);
/// </summary>
public class UpdateTechnicalServiceDto
{
    /// <summary>
    /// Guncellenecek kaydin ID'si (zorunlu).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Cihaz turu.
    /// </summary>
    public string DeviceType { get; set; } = null!;

    /// <summary>
    /// Marka.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Seri numarasi.
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Problem tanimi.
    /// </summary>
    public string ProblemDescription { get; set; } = null!;

    /// <summary>
    /// Teshis notlari.
    /// Teknisyen tarafindan doldurulur.
    /// </summary>
    public string? DiagnosticNotes { get; set; }

    /// <summary>
    /// Tamir notlari.
    /// Yapilan islemler.
    /// </summary>
    public string? RepairNotes { get; set; }

    /// <summary>
    /// Tahmini maliyet.
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Son maliyet.
    /// Tamir tamamlandiginda belirlenir.
    /// </summary>
    public decimal? FinalCost { get; set; }

    /// <summary>
    /// Servis durumu.
    /// </summary>
    public TechnicalServiceStatus Status { get; set; }

    /// <summary>
    /// Teknisyen ID'si.
    /// Atama veya degistirme icin.
    /// </summary>
    public int? TechnicianId { get; set; }
}

#endregion

#region DURUM GUNCELLEME DTO'SU

/// <summary>
/// Sadece servis durumunu degistirmek icin ozel DTO.
///
/// ACIKLAMA:
/// ---------
/// Hizli durum degisikligi icin.
/// Diger alanlara dokunmadan sadece durumu ve ilgili notlari gunceller.
///
/// UI KULLANIMI:
/// -------------
/// - Durum dropdown'i degistirme
/// - Hizli aksiyonlar (Tamamla, Teslim Et)
///
/// OZEL DURUMLAR:
/// --------------
/// - Completed: FinalCost girilmeli
/// - Delivered: DeliveredDate otomatik atanir
/// - Cancelled: Notes'a iptal nedeni yazilmali
///
/// ORNEK:
/// ------
/// // Tamir tamamlandi
/// var statusUpdate = new TechnicalServiceStatusUpdateDto {
///     Id = 50,
///     NewStatus = TechnicalServiceStatus.Completed,
///     FinalCost = 1800,
///     Notes = "Ekran degistirildi, test edildi"
/// };
/// await _technicalServiceService.UpdateStatusAsync(statusUpdate);
/// </summary>
public class TechnicalServiceStatusUpdateDto
{
    /// <summary>
    /// Durumu guncellenecek kaydin ID'si.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Yeni durum.
    /// </summary>
    public TechnicalServiceStatus NewStatus { get; set; }

    /// <summary>
    /// Durum degisikligi notu (opsiyonel).
    /// Neden degistirildigi veya ek bilgi.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Son maliyet (opsiyonel).
    /// Completed durumunda girilmeli.
    /// </summary>
    public decimal? FinalCost { get; set; }
}

#endregion

#region OZET DTO'SU

/// <summary>
/// Teknik servis istatistikleri ozeti icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Dashboard ve raporlarda teknik servis durumunu ozetler.
/// Bekleyen isler, tamamlanan isler, gelir bilgisi.
///
/// UI KULLANIMI:
/// -------------
/// - Dashboard KPI kartlari
/// - Teknik servis yonetici paneli
/// - Performans raporlari
///
/// ORNEK:
/// ------
/// var summary = await _technicalServiceService.GetSummaryAsync();
/// Console.WriteLine($"Toplam: {summary.TotalTickets}");
/// Console.WriteLine($"Bekleyen: {summary.PendingTickets}");
/// Console.WriteLine($"Gelir: {summary.TotalRevenue:C}");
/// </summary>
public class TechnicalServiceSummaryDto
{
    /// <summary>
    /// Toplam servis kaydi sayisi.
    /// </summary>
    public int TotalTickets { get; set; }

    /// <summary>
    /// Bekleyen (henuz baslanmamis) kayit sayisi.
    /// Acil ilgi gerektiren is yuku.
    /// </summary>
    public int PendingTickets { get; set; }

    /// <summary>
    /// Devam eden (aktif tamir) kayit sayisi.
    /// </summary>
    public int InProgressTickets { get; set; }

    /// <summary>
    /// Tamamlanan (teslim bekleyen) kayit sayisi.
    /// </summary>
    public int CompletedTickets { get; set; }

    /// <summary>
    /// Teslim edilen kayit sayisi.
    /// </summary>
    public int DeliveredTickets { get; set; }

    /// <summary>
    /// Toplam teknik servis geliri.
    /// Tamamlanan ve teslim edilen islerden.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Ortalama tamir suresi (saat).
    /// Alim'dan tamamlanmaya kadar gecen sure.
    /// Verimlilik olcusu.
    /// </summary>
    public decimal AverageRepairTime { get; set; }
}

#endregion
