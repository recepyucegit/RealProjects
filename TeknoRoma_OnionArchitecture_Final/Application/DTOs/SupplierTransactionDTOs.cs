// ===================================================================================
// TEKNOROMA - TEDARIKCI ISLEMI DTO DOSYASI (SupplierTransactionDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, tedarikci islemleri (SupplierTransaction) ile ilgili DTO'lari icerir.
// TeknoRoma'nin tedarikcilerle yaptigi alis ve odeme islemlerini takip eder.
//
// TEDARIKCI ISLEMI KAVRAMI
// ------------------------
// Tedarikci islemi, magazanin tedarikcilerden yaptigi mal alimlarini
// ve bu alimlara karsilik yapilan odemeleri kapsar.
//
// ISLEM TURLERI:
// --------------
// - Mal Alimi: Tedarikciden urun alimi (borc olusur)
// - Odeme: Tedarikciye yapilan odeme (borc kapanir)
//
// DOVIZ DESTEGI
// -------------
// Yabanci tedarikciler icin dovizli islem destegi vardir:
// - Currency: Islemin para birimi (TRY, USD, EUR)
// - ExchangeRate: O gunun doviz kuru
// - AmountInTRY: TL karsiligi (raporlama icin)
//
// BORC TAKIBI
// -----------
// Her tedarikci icin:
// - Toplam Borc: Alinan mallarin toplam degeri
// - Toplam Odeme: Yapilan odemelerin toplami
// - Bakiye: Toplam Borc - Toplam Odeme (kalan borc)
//
// DTO TURLERI
// -----------
// 1. SupplierTransactionDto (Okuma): Islem kayitlarini goruntulemek icin
// 2. CreateSupplierTransactionDto (Olusturma): Yeni islem kaydi icin
// 3. UpdateSupplierTransactionDto (Guncelleme): Islem guncellemek icin
// 4. SupplierTransactionPaymentDto (Odeme): Odeme islemi icin
// 5. SupplierBalanceDto (Bakiye): Tedarikci bakiye ozeti icin
//
// ILISKILI ENTITY
// ---------------
// Domain/Entities/SupplierTransaction.cs
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/ISupplierTransactionService.cs
//
// ===================================================================================

using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region OKUMA DTO'SU

/// <summary>
/// Tedarikci islemi bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Veritabanindan okunan tedarikci islem kayitlarini UI'a tasir.
/// Tedarikci, urun, doviz ve odeme bilgilerini icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Tedarikci islem listesi (DataGrid)
/// - Tedarikci hesap ekstresi
/// - Borc/alacak raporu
/// - Odeme takip ekrani
///
/// ORNEK:
/// ------
/// var transactions = await _supplierTransactionService.GetBySupplierAsync(supplierId);
/// foreach (var tx in transactions)
/// {
///     Console.WriteLine($"{tx.TransactionDate:d} - {tx.Description}");
///     Console.WriteLine($"  Tutar: {tx.Amount} {tx.CurrencyDisplay}");
///     Console.WriteLine($"  TL: {tx.AmountInTRY:C}");
///     Console.WriteLine($"  Odendi: {tx.IsPaid}");
/// }
/// </summary>
public class SupplierTransactionDto
{
    /// <summary>
    /// Islem kaydinin benzersiz ID'si.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tedarikci ID'si.
    /// Islemin ait oldugu tedarikci.
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// Tedarikci adi (navigation property'den).
    /// UI'da gosterim icin.
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// Islem aciklamasi (opsiyonel).
    /// Fatura numarasi, detay bilgisi vb.
    /// Ornek: "Fatura #2024-001 - Samsung telefonlar"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Islem tutari (orijinal para biriminde).
    /// Currency alanina bagli olarak TRY, USD veya EUR olabilir.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para birimi (enum).
    /// TRY = Turk Lirasi, USD = Amerikan Dolari, EUR = Euro
    /// </summary>
    public Currency Currency { get; set; }

    /// <summary>
    /// Para biriminin metinsel gosterimi.
    /// UI'da sembol veya kisaltma olarak.
    /// </summary>
    public string CurrencyDisplay => Currency.ToString();

    /// <summary>
    /// Doviz kuru (opsiyonel).
    /// TRY disindaki para birimleri icin o gunun kuru.
    /// null = TRY isleminde
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// TL karsiligi tutar.
    /// Raporlama ve bakiye hesaplamasi icin.
    /// Formul: Amount * ExchangeRate (TRY icin Amount)
    /// </summary>
    public decimal AmountInTRY { get; set; }

    /// <summary>
    /// Islem tarihi.
    /// Fatura tarihi veya odeme tarihi.
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Odendi mi?
    /// true = Tedarikciye odeme yapildi
    /// false = Hala borc var
    /// </summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// Odeme tarihi (opsiyonel).
    /// IsPaid = true ise doldurulur.
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Ilgili urun ID'si (opsiyonel).
    /// Belirli bir urune ait alim ise.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Urun adi (navigation property'den).
    /// Hangi urun icin alim yapildi.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Alinan miktar (opsiyonel).
    /// Kac adet urun alindi.
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Kayit tarihi.
    /// Sistemde olusturulma zamani.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}

#endregion

#region OLUSTURMA DTO'SU

/// <summary>
/// Yeni tedarikci islemi olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Tedarikciden mal alimi veya odeme kaydı icin.
/// Mal girisi yapildiginda veya fatura geldiginde kullanilir.
///
/// UI FORMU:
/// ---------
/// - Tedarikci: Dropdown (zorunlu)
/// - Tutar: NumberBox (zorunlu)
/// - Para Birimi: Dropdown (varsayilan: TRY)
/// - Doviz Kuru: NumberBox (TRY haricinde zorunlu)
/// - Tarih: DatePicker (varsayilan: bugun)
/// - Aciklama: TextArea (opsiyonel)
/// - Urun: Dropdown (opsiyonel)
/// - Miktar: NumberBox (opsiyonel)
///
/// ORNEK:
/// ------
/// var transaction = new CreateSupplierTransactionDto {
///     SupplierId = 5,
///     Description = "Fatura #2024-001 - iPhone 15 siparisi",
///     Amount = 10000,
///     Currency = Currency.USD,
///     ExchangeRate = 32.50m,
///     ProductId = 15,
///     Quantity = 20
/// };
/// await _supplierTransactionService.CreateAsync(transaction);
/// </summary>
public class CreateSupplierTransactionDto
{
    /// <summary>
    /// Tedarikci ID'si (zorunlu).
    /// Dropdown'dan secilir.
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// Islem aciklamasi (opsiyonel).
    /// Fatura no, siparis detayi vb.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Islem tutari (zorunlu).
    /// Fatura veya odeme tutari.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para birimi (varsayilan: TRY).
    /// Yabanci tedarikciler icin USD/EUR secilebilir.
    /// </summary>
    public Currency Currency { get; set; } = Currency.TRY;

    /// <summary>
    /// Doviz kuru (opsiyonel).
    /// TRY haricinde zorunlu.
    /// O gunun TCMB kurundan alinabilir.
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Islem tarihi (varsayilan: bugun).
    /// Fatura tarihi.
    /// </summary>
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Ilgili urun ID'si (opsiyonel).
    /// Belirli bir urun icin alim yapiliyorsa.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Alinan miktar (opsiyonel).
    /// Kac adet alindi.
    /// </summary>
    public int? Quantity { get; set; }
}

#endregion

#region GUNCELLEME DTO'SU

/// <summary>
/// Mevcut tedarikci islemini guncellemek icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Yanlis girilen islem bilgilerini duzeltmek icin.
/// CreateSupplierTransactionDto'dan farki: ID alani vardir.
///
/// NOT:
/// ----
/// IsPaid alani bu DTO'da yok - odeme icin ozel SupplierTransactionPaymentDto kullanilir.
///
/// ORNEK:
/// ------
/// var updateDto = new UpdateSupplierTransactionDto {
///     Id = 100,
///     Amount = 10500,  // Tutar duzeltildi
///     Description = "Fatura #2024-001 - Duzeltme"
/// };
/// await _supplierTransactionService.UpdateAsync(updateDto);
/// </summary>
public class UpdateSupplierTransactionDto
{
    /// <summary>
    /// Guncellenecek islemin ID'si (zorunlu).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Islem aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Islem tutari.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para birimi.
    /// </summary>
    public Currency Currency { get; set; }

    /// <summary>
    /// Doviz kuru.
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Islem tarihi.
    /// </summary>
    public DateTime TransactionDate { get; set; }
}

#endregion

#region ODEME DTO'SU

/// <summary>
/// Tedarikci odemesi kaydi icin ozel DTO.
///
/// ACIKLAMA:
/// ---------
/// Bir islemi "odendi" olarak isaretlemek icin.
/// Tedarikciye odeme yapildiginda kullanilir.
///
/// UI KULLANIMI:
/// -------------
/// - "Odeme Yap" butonu tiklandiginda
/// - Odeme tarih popup'i gosterilir
/// - Onaylandiginda islem odendi olarak isaretlenir
///
/// ORNEK:
/// ------
/// var payment = new SupplierTransactionPaymentDto {
///     Id = 100,
///     PaymentDate = DateTime.Now
/// };
/// await _supplierTransactionService.MarkAsPaidAsync(payment);
/// </summary>
public class SupplierTransactionPaymentDto
{
    /// <summary>
    /// Odeme yapilacak islemin ID'si.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Odeme tarihi (varsayilan: bugun).
    /// Odemenin yapildigi tarih.
    /// </summary>
    public DateTime PaymentDate { get; set; } = DateTime.Now;
}

#endregion

#region BAKIYE DTO'SU

/// <summary>
/// Tedarikci bakiye ozeti icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Bir tedarikcinin borc/alacak durumunu ozetler.
/// Tedarikciye ne kadar borcumuz var, ne kadar odedik.
///
/// UI KULLANIMI:
/// -------------
/// - Tedarikci detay sayfasinda bakiye karti
/// - Borc/alacak listesi
/// - Odeme planlama
/// - Finansal raporlar
///
/// HESAPLAMA:
/// ----------
/// Balance = TotalDebt - TotalPaid
/// Pozitif Balance = Tedarikciye borcumuz var
/// Negatif Balance = Tedarikciden alacak var (nadir)
///
/// ORNEK:
/// ------
/// var balance = await _supplierTransactionService.GetBalanceAsync(supplierId);
/// Console.WriteLine($"Tedarikci: {balance.SupplierName}");
/// Console.WriteLine($"Toplam Borc: {balance.TotalDebt:C}");
/// Console.WriteLine($"Odenen: {balance.TotalPaid:C}");
/// Console.WriteLine($"Bakiye: {balance.Balance:C}");
/// Console.WriteLine($"Odenmemis Islem: {balance.UnpaidTransactionCount}");
/// </summary>
public class SupplierBalanceDto
{
    /// <summary>
    /// Tedarikci ID'si.
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// Tedarikci adi.
    /// Gosterim amacli.
    /// </summary>
    public string SupplierName { get; set; } = null!;

    /// <summary>
    /// Toplam borc (TL).
    /// Tum alimlarin TL karsiligi toplami.
    /// </summary>
    public decimal TotalDebt { get; set; }

    /// <summary>
    /// Toplam odeme (TL).
    /// Yapilan odemelerin toplami.
    /// </summary>
    public decimal TotalPaid { get; set; }

    /// <summary>
    /// Bakiye (kalan borc).
    /// Formul: TotalDebt - TotalPaid
    /// Pozitif = Borc, Negatif = Alacak
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Odenmemis islem sayisi.
    /// Kac tane fatura/islem bekliyor.
    /// </summary>
    public int UnpaidTransactionCount { get; set; }
}

#endregion
