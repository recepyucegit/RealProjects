// ===================================================================================
// TEKNOROMA - GIDER DTO DOSYASI (ExpenseDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, gider (Expense) entity'si ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin gider takibi ve maliyet yonetimi icin kullanilir.
//
// GIDER KAVRAMI
// -------------
// Gider, isletmenin faaliyetleri icin yaptigi tum harcamalari kapsar.
// Satisindan elde edilen gelirden giderler cikarilarak kar/zarar hesaplanir.
//
// GIDER TURLERI (ExpenseType enum):
// ---------------------------------
// - Kira: Magaza kira odemesi
// - Elektrik: Elektrik faturasi
// - Su: Su faturasi
// - Dogalgaz: Isitma/dogalgaz faturasi
// - Internet: Internet ve iletisim
// - Maas: Personel maaslari
// - Vergi: Devlete odenen vergiler
// - Sigorta: Isyeri ve calisan sigortalari
// - Bakim: Tadilat, tamirat
// - Diger: Kategorize edilemeyen giderler
//
// KAR HESAPLAMA
// -------------
// Net Kar = Toplam Satis - Toplam Gider
// Bu hesaplama icin giderlerin dogru ve duzgun kaydedilmesi kritik!
//
// DTO TURLERI
// -----------
// 1. ExpenseDto (Okuma): Gider kayitlarini goruntulemek icin
// 2. CreateExpenseDto (Olusturma): Yeni gider kaydi icin
// 3. UpdateExpenseDto (Guncelleme): Gider bilgisi guncellemek icin
// 4. ExpenseSummaryDto (Ozet): Gider turune gore ozet
// 5. MonthlyExpenseDto (Rapor): Aylik gider raporu
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/Expense.cs
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/IExpenseService.cs
//
// ===================================================================================

using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Gider bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan gider kayitlarini UI'a tasir.
/// Iliskili magaza ve calisan bilgilerini de icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Gider listesi tablosu (DataGrid)
/// - Gider detay karti
/// - Kar/zarar raporu icin veri
/// - Gider grafikleri
///
/// ORNEK:
/// ------
/// var expenses = await _expenseService.GetByDateRangeAsync(startDate, endDate);
/// foreach (var exp in expenses)
/// {
///     Console.WriteLine($"{exp.ExpenseTypeDisplay}: {exp.Amount:C}");
///     Console.WriteLine($"  Tarih: {exp.ExpenseDate:d}");
///     Console.WriteLine($"  Magaza: {exp.StoreName}");
/// }
/// </summary>
public class ExpenseDto
{
    /// <summary>
    /// Gider kaydinin benzersiz ID'si.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gider turu (enum).
    /// Kira, Elektrik, Maas, Vergi vb.
    /// Raporlamada gruplama icin kullanilir.
    /// </summary>
    public ExpenseType ExpenseType { get; set; }

    /// <summary>
    /// Gider turunun metinsel gosterimi.
    /// UI'da dropdown, badge veya tablo sutunu olarak.
    /// Ornek: "Elektrik", "Kira", "Maas"
    /// </summary>
    public string ExpenseTypeDisplay => ExpenseType.ToString();

    /// <summary>
    /// Gider aciklamasi (opsiyonel).
    /// Detayli bilgi, fatura numarasi vb.
    /// Ornek: "Ocak 2024 elektrik faturasi - Fatura No: 123456"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gider tutari (TL).
    /// Odenen veya odenecek miktar.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Giderin ait oldugu tarih.
    /// Fatura donemi veya odeme tarihi.
    /// Raporlamada filtreleme icin kullanilir.
    /// </summary>
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    /// Ilgili magaza ID'si (opsiyonel).
    /// null = Merkez/genel gider (tum magzalara dagitilir).
    /// Belirli magaza = Sadece o subenin gideri.
    /// </summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// Magaza adi (navigation property'den).
    /// UI'da hangi subenin gideri oldugunu gosterir.
    /// </summary>
    public string? StoreName { get; set; }

    /// <summary>
    /// Gideri kaydeden calisan ID'si (opsiyonel).
    /// Denetim ve izlenebilirlik icin.
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// Calisan adi (navigation property'den).
    /// Kimin kaydettigini gosterir.
    /// </summary>
    public string? EmployeeName { get; set; }

    /// <summary>
    /// Kaydin sisteme eklendigi tarih.
    /// Denetim amacli.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni gider kaydi olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// "Yeni Gider Ekle" formundan gelen verileri tasir.
/// Fatura geldiginde veya odeme yapildiginda kullanilir.
///
/// UI FORMU:
/// ---------
/// - Gider Turu: Dropdown (zorunlu)
/// - Tutar: NumberBox (zorunlu)
/// - Tarih: DatePicker (varsayilan: bugun)
/// - Aciklama: TextArea (opsiyonel)
/// - Magaza: Dropdown (opsiyonel)
///
/// ORNEK:
/// ------
/// var expense = new CreateExpenseDto {
///     ExpenseType = ExpenseType.Elektrik,
///     Amount = 3500,
///     ExpenseDate = new DateTime(2024, 1, 15),
///     Description = "Ocak 2024 elektrik faturasi",
///     StoreId = 1
/// };
/// await _expenseService.CreateAsync(expense);
/// </summary>
public class CreateExpenseDto
{
    /// <summary>
    /// Gider turu (zorunlu).
    /// Dropdown'dan secilir.
    /// </summary>
    public ExpenseType ExpenseType { get; set; }

    /// <summary>
    /// Gider aciklamasi (opsiyonel).
    /// Fatura numarasi, detayli bilgi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gider tutari (zorunlu).
    /// Sifirdan buyuk olmali.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gider tarihi (varsayilan: bugun).
    /// Fatura donemi veya odeme tarihi.
    /// </summary>
    public DateTime ExpenseDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Magaza ID'si (opsiyonel).
    /// null = Genel gider, deger = Sube gideri.
    /// </summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// Kaydeden calisan ID'si (opsiyonel).
    /// Genellikle oturum acmis kullanicidan alinir.
    /// </summary>
    public int? EmployeeId { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut gider kaydini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Yanlis girilen gider bilgilerini duzeltmek icin.
/// CreateExpenseDto'dan farki: ID alani vardir.
///
/// NOT:
/// ----
/// Gider kayitlari muhasebe acisindan hassastir.
/// Guncelleme yetkisi sadece yetkilendirilmis kisilerde olmali.
/// Buyuk degisiklikler loglanmali.
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateExpenseDto {
///     Id = 25,
///     ExpenseType = ExpenseType.Elektrik,
///     Amount = 3750,  // Tutar duzeltildi
///     Description = "Ocak 2024 elektrik - Duzeltme",
///     ExpenseDate = new DateTime(2024, 1, 15),
///     StoreId = 1
/// };
/// await _expenseService.UpdateAsync(updateDto);
/// </summary>
public class UpdateExpenseDto
{
    /// <summary>
    /// Guncellenecek gider kaydinin ID'si (zorunlu).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gider turu.
    /// </summary>
    public ExpenseType ExpenseType { get; set; }

    /// <summary>
    /// Gider aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gider tutari.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gider tarihi.
    /// </summary>
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    /// Magaza ID'si.
    /// </summary>
    public int? StoreId { get; set; }
}

#endregion

#region RAPOR DTO'LARI

/// <summary>
/// Gider turune gore ozet bilgisi icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Giderleri kategorilere gore gruplayarak ozetler.
/// Pasta grafigi ve karsilastirma tablolari icin kullanilir.
///
/// UI KULLANIMI:
/// -------------
/// - Pasta grafigi (gider dagilimi)
/// - Gider karsilastirma tablosu
/// - Butce analizi
///
/// ORNEK VERI:
/// -----------
/// ExpenseType: Kira
/// TotalAmount: 50000
/// Count: 5 (5 aylik kira)
/// Percentage: 35% (toplam giderin %35'i)
/// </summary>
public class ExpenseSummaryDto
{
    /// <summary>
    /// Gider turu.
    /// Gruplama kriteri.
    /// </summary>
    public ExpenseType ExpenseType { get; set; }

    /// <summary>
    /// Gider turunun metinsel gosterimi.
    /// Grafik ve tablo etiketlerinde.
    /// </summary>
    public string ExpenseTypeDisplay => ExpenseType.ToString();

    /// <summary>
    /// Bu turdeki toplam gider tutari.
    /// Belirlenen donemde.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Bu turde kac adet gider kaydi var.
    /// Ortalama hesaplamak icin.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Toplam gider icindeki yuzdesi.
    /// Pasta grafiginde dilim buyuklugu.
    /// Ornek: 25 = %25
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Aylik gider raporu icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Bir ayin gider ozetini ve tur bazli dagılımını gosterir.
/// Aylik trend analizi ve butce karsilastirmasi icin.
///
/// UI KULLANIMI:
/// -------------
/// - Aylik gider karti
/// - Yillik trend grafigi (12 aylik veri)
/// - Butce vs gerceklesen karsilastirmasi
///
/// ORNEK:
/// ------
/// var monthlyExpense = await _expenseService.GetMonthlyAsync(2024, 1);
/// Console.WriteLine($"{monthlyExpense.MonthName} {monthlyExpense.Year}");
/// Console.WriteLine($"Toplam: {monthlyExpense.TotalExpense:C}");
/// foreach (var item in monthlyExpense.ByType)
///     Console.WriteLine($"  {item.ExpenseTypeDisplay}: {item.TotalAmount:C}");
/// </summary>
public class MonthlyExpenseDto
{
    /// <summary>
    /// Yil.
    /// Ornek: 2024
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Ay numarasi (1-12).
    /// Ornek: 1 = Ocak, 12 = Aralik
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Ay adi.
    /// UI gosterimi icin.
    /// Ornek: "Ocak", "Subat"
    /// </summary>
    public string MonthName { get; set; } = null!;

    /// <summary>
    /// Ay icindeki toplam gider.
    /// Tum gider turlerinin toplami.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Gider turune gore dagilim listesi.
    /// Her gider turunu ayri ayri gosterir.
    /// </summary>
    public List<ExpenseSummaryDto> ByType { get; set; } = new();
}

#endregion
