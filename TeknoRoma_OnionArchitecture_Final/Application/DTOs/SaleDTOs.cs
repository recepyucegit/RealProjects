// ===================================================================================
// TEKNOROMA - SATIS DTO DOSYASI (SaleDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, satis (Sale) islemi ile ilgili tum DTO'lari icerir.
// TeknoRoma elektronik magazasinin satis yonetimi icin kullanilir.
//
// SATIS ISLEMI NEDIR?
// -------------------
// Satis, musteriye urun satma islemidir. Her satista:
// 1. Bir veya birden fazla urun satilir (SaleDetail)
// 2. Toplam tutar hesaplanir (KDV dahil)
// 3. Odeme alinir (Nakit, Kart vb.)
// 4. Stoklar otomatik duser
// 5. Musteri sadakat puani kazanir
//
// DTO TURLERI
// -----------
// 1. SaleDto (Okuma): Satis kayitlarini goruntulemek icin
// 2. SaleDetailDto (Okuma): Satis kalemlerini goruntulemek icin
// 3. CreateSaleDto (Olusturma): Yeni satis olusturmak icin
// 4. CreateSaleDetailDto (Olusturma): Satis kalemi eklemek icin
// 5. SaleSummaryDto (Rapor): Satis ozet bilgileri
// 6. DailySalesSummaryDto (Rapor): Gunluk satis ozeti
//
// MASTER-DETAIL YAPISI
// --------------------
// Satis, master-detail (ust-alt) yapidadir:
// - Sale (Master): Satis basligi (tarih, musteri, odeme tipi, toplam)
// - SaleDetail (Detail): Satis satirlari (urun, adet, fiyat)
//
// ILISKILI ENTITY'LER
// -------------------
// - Domain/Entities/Sale.cs (Satis basligi)
// - Domain/Entities/SaleDetail.cs (Satis satirlari)
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/ISaleService.cs
// - CreateAsync(CreateSaleDto dto) -> Yeni satis
// - GetByIdAsync(int id) -> SaleDto (detaylarla birlikte)
// - GetDailySummaryAsync(DateTime date) -> DailySalesSummaryDto
//
// ===================================================================================

using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

#region SATIS OKUMA DTO'LARI

/// <summary>
/// Satis bilgilerini okuma ve listeleme icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Tamamlanmis veya devam eden satis islemlerini gosterir.
/// Satis gecmisi, fatura goruntuleme, iade islemleri icin kullanilir.
///
/// MASTER-DETAIL YAPISI:
/// ---------------------
/// Bu DTO, satis basligini (master) ve satis detaylarini (detail)
/// birlikte tasir. Details listesi, satilan tum urunleri icerir.
///
/// UI KULLANIMI:
/// -------------
/// - Satis gecmisi listesi (DataGrid)
/// - Fatura/Fis goruntuleme
/// - Satis detay popup'i
/// - Iade islemi icin satis bulma
///
/// ORNEK:
/// ------
/// var sale = await _saleService.GetByIdAsync(saleId);
/// Console.WriteLine($"Satis No: {sale.Id}");
/// Console.WriteLine($"Tarih: {sale.SaleDate}");
/// Console.WriteLine($"Musteri: {sale.CustomerName ?? "Anonim"}");
/// Console.WriteLine($"Toplam: {sale.TotalAmount:C}");
/// foreach (var detail in sale.Details)
///     Console.WriteLine($"  - {detail.ProductName} x{detail.Quantity} = {detail.TotalPrice:C}");
/// </summary>
public class SaleDto
{
    /// <summary>
    /// Satis ID'si (Fis/Fatura numarasi olarak kullanilabilir).
    /// UI'da: "Satis No: 12345" seklinde gosterilir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Satis tarihi ve saati.
    /// Satisin yapildigi an.
    /// UI'da: Tarih sutunu, fis uzerinde tarih.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Ara toplam (KDV oncesi, indirim oncesi).
    /// Tum kalemlerin birim fiyat x adet toplami.
    /// Formul: Sum(UnitPrice * Quantity)
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// KDV tutari.
    /// Turkiye'de genel KDV orani %20'dir.
    /// Formul: SubTotal * 0.20 (veya urun bazli farkli oranlar)
    /// UI'da: Fis/faturada "KDV: 150 TL" seklinde.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Indirim tutari.
    /// Toplam satistan yapilan indirim.
    /// Kampanya, sadakat puani kullanimi vb.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Genel toplam (odenen tutar).
    /// Formul: SubTotal + TaxAmount - DiscountAmount
    /// UI'da: Fis altinda buyuk puntolarla "TOPLAM: 1.500 TL"
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Odeme tipi (enum).
    /// Cash = Nakit, CreditCard = Kredi Karti, DebitCard = Banka Karti,
    /// MixedPayment = Karisik Odeme
    /// Kasa islemleri ve raporlar icin onemli.
    /// </summary>
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// Odeme tipinin metinsel gosterimi.
    /// UI'da: "Nakit", "Kredi Karti" seklinde.
    /// </summary>
    public string PaymentTypeDisplay => PaymentType.ToString();

    /// <summary>
    /// Satis durumu (enum).
    /// Completed = Tamamlandi, Cancelled = Iptal, Refunded = Iade Edildi
    /// Iade ve iptal islemleri icin kullanilir.
    /// </summary>
    public SaleStatus Status { get; set; }

    /// <summary>
    /// Satis durumunun metinsel gosterimi.
    /// UI'da: Durum badge'i (yesil/kirmizi/sari).
    /// </summary>
    public string StatusDisplay => Status.ToString();

    /// <summary>
    /// Musteri ID'si (opsiyonel).
    /// null = Anonim musteri (fis kesmeden satis).
    /// Sadakat sistemi icin musteri bilgisi gerekli.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Musteri adi (navigation property'den).
    /// UI'da: "Musteri: Ahmet Yilmaz" veya "Anonim".
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Satisi yapan calisan ID'si.
    /// Performans takibi ve sorumluluk icin.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Calisan adi (navigation property'den).
    /// UI'da: "Kasiyer: Mehmet Demir".
    /// </summary>
    public string? EmployeeName { get; set; }

    /// <summary>
    /// Satisin yapildigi magaza ID'si.
    /// Coklu magaza yapisinda hangi subede yapildigini belirtir.
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Magaza adi (navigation property'den).
    /// UI'da: "Sube: Kadikoy".
    /// </summary>
    public string? StoreName { get; set; }

    /// <summary>
    /// Satis kalemleri (detail satirlari).
    /// Bu satista satilan tum urunlerin listesi.
    /// Her kalem: urun, adet, birim fiyat, indirim, toplam.
    /// </summary>
    public List<SaleDetailDto> Details { get; set; } = new();
}

/// <summary>
/// Satis kalemi (detail) bilgilerini okuma icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Bir satistaki tek bir urunu temsil eder.
/// Her satis birden fazla SaleDetail icerebb+ilir.
///
/// UI KULLANIMI:
/// -------------
/// - Satis detay tablosu (fis/fatura uzerinde urun listesi)
/// - Fis yazdirma
/// - Iade islemi icin urun secimi
///
/// HESAPLAMA:
/// ----------
/// TotalPrice = UnitPrice * Quantity * (1 - DiscountPercent/100)
/// Ornek: 100 TL * 2 adet * (1 - 0.10) = 180 TL
/// </summary>
public class SaleDetailDto
{
    /// <summary>
    /// Satis kalemi ID'si.
    /// Iade islemlerinde hangi kalemin iade edileceğini belirtir.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Satilan urunun ID'si.
    /// Stok guncelleme, raporlama icin kullanilir.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Urun adi (navigation property'den).
    /// Fis/faturada urun satiri olarak gosterilir.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Urun barkodu.
    /// Fis uzerinde barkod bilgisi, iade kontrolu.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Satilan adet.
    /// Ornek: 2 adet telefon kilifi.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Birim satis fiyati (o anki).
    /// Satis anindaki fiyat kaydedilir (fiyat degisse bile).
    /// Tarihsel veri butunlugu icin onemli.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Kalem bazinda indirim yuzdesi.
    /// Ornek: 10 = %10 indirim.
    /// Kampanyali urunler icin kullanilir.
    /// </summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>
    /// Kalem toplam tutari.
    /// Formul: UnitPrice * Quantity * (1 - DiscountPercent/100)
    /// Fis uzerinde her satirin toplami.
    /// </summary>
    public decimal TotalPrice { get; set; }
}

#endregion

#region SATIS OLUSTURMA DTO'LARI

/// <summary>
/// Yeni satis olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Satis ekranindan gelen satis bilgilerini tasir.
/// Kasa/POS uygulamasindan API'ye gonderilir.
///
/// SATIS AKISI:
/// ------------
/// 1. Kasiyer urunleri barkod ile okutarak Details'e ekler
/// 2. Musteri varsa musteri secilir (opsiyonel)
/// 3. Odeme tipi secilir
/// 4. Indirim varsa girilir
/// 5. Sadakat puani kullanilacaksa girilir
/// 6. Satis tamamlanir (CreateAsync)
///
/// SERVICE TARAFINDA:
/// ------------------
/// - Stoklar kontrol edilir ve dusurulur
/// - Fiyatlar urun tablosundan alinir
/// - KDV hesaplanir
/// - Sadakat puani hesaplanir (varsa)
/// - Sale ve SaleDetail kayitlari olusturulur
///
/// ORNEK:
/// ------
/// var sale = new CreateSaleDto {
///     CustomerId = 5,  // veya null (anonim)
///     EmployeeId = 10,
///     StoreId = 1,
///     PaymentType = PaymentType.CreditCard,
///     Details = new List&lt;CreateSaleDetailDto&gt; {
///         new() { ProductId = 1, Quantity = 1 },
///         new() { ProductId = 5, Quantity = 2, DiscountPercent = 10 }
///     }
/// };
/// var result = await _saleService.CreateAsync(sale);
/// </summary>
public class CreateSaleDto
{
    /// <summary>
    /// Musteri ID'si (opsiyonel).
    /// null = Anonim satis (sadakat puani kazanilmaz).
    /// Dropdown'dan musteri secilir veya bos birakilir.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Satisi yapan calisan ID'si (zorunlu).
    /// Oturum acmis kullanicinin ID'si otomatik atanir.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Magaza ID'si (zorunlu).
    /// Calisanin bagli oldugu sube otomatik atanir.
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Odeme tipi (zorunlu).
    /// Radio button veya dropdown ile secilir.
    /// Kasa kapanisinda odeme tipine gore rapor alinir.
    /// </summary>
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// Genel indirim tutari.
    /// Toplam satisa uygulanan indirim (kupon, yonetici indirimi).
    /// Kalem bazinda indirimler Details icinde.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Kullanilacak sadakat puani.
    /// Musterinin biriktirdigi puanlardan harcama.
    /// 1 puan = 1 TL indirim (veya farkli oran).
    /// </summary>
    public decimal? UseLoyaltyPoints { get; set; }

    /// <summary>
    /// Satis kalemleri (satilan urunler).
    /// En az 1 kalem olmali (bos satis olamaz).
    /// </summary>
    public List<CreateSaleDetailDto> Details { get; set; } = new();
}

/// <summary>
/// Satis kalemi olusturmak icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Satisa eklenecek tek bir urunu temsil eder.
/// Barkod okutuldugunda veya manuel urun secildiginde olusur.
///
/// NOT:
/// ----
/// UnitPrice burada yok cunku Service katmaninda
/// urun tablosundan guncel fiyat alinir.
///
/// ORNEK:
/// ------
/// // Barkod okutuldu, 2 adet eklendi
/// var detail = new CreateSaleDetailDto {
///     ProductId = 15,
///     Quantity = 2,
///     DiscountPercent = null  // Indirim yok
/// };
/// </summary>
public class CreateSaleDetailDto
{
    /// <summary>
    /// Urun ID'si (zorunlu).
    /// Barkod ile arama yapilip ID bulunur.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Adet (zorunlu).
    /// Varsayilan 1, +/- butonlari ile degistirilebilir.
    /// Stok kontrolu yapilir (stoktan fazla satilamaz).
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Kalem indirimi yuzdesi (opsiyonel).
    /// null = Indirim yok.
    /// Ornek: 15 = %15 indirim.
    /// </summary>
    public decimal? DiscountPercent { get; set; }
}

#endregion

#region SATIS RAPOR DTO'LARI

/// <summary>
/// Satis ozet bilgileri icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Belirli bir donem icin satis istatistiklerini tasir.
/// Dashboard ve raporlama icin kullanilir.
///
/// UI KULLANIMI:
/// -------------
/// - Dashboard KPI kartlari
/// - Performans raporlari
/// - Yonetici ozet ekrani
///
/// ORNEK:
/// ------
/// var summary = await _saleService.GetSummaryAsync(startDate, endDate);
/// Console.WriteLine($"Toplam Satis: {summary.TotalSales} adet");
/// Console.WriteLine($"Toplam Ciro: {summary.TotalRevenue:C}");
/// Console.WriteLine($"Ortalama Sepet: {summary.AverageOrderValue:C}");
/// </summary>
public class SaleSummaryDto
{
    /// <summary>
    /// Toplam satis adedi.
    /// Donem icinde tamamlanan satis sayisi.
    /// </summary>
    public int TotalSales { get; set; }

    /// <summary>
    /// Toplam ciro (gelir).
    /// Tum satislarin TotalAmount toplami.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Toplam KDV tutari.
    /// Vergi raporlari icin.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Toplam indirim tutari.
    /// Indirim analizi icin.
    /// </summary>
    public decimal TotalDiscount { get; set; }

    /// <summary>
    /// Ortalama siparis tutari (sepet buyuklugu).
    /// Formul: TotalRevenue / TotalSales
    /// Musteri davranisi analizi icin onemli.
    /// </summary>
    public decimal AverageOrderValue { get; set; }
}

/// <summary>
/// Gunluk satis ozeti icin DTO.
///
/// ACIKLAMA:
/// ---------
/// Tek bir gune ait satis istatistiklerini tasir.
/// Haftalik/aylik grafiklerin verisi olarak kullanilir.
///
/// UI KULLANIMI:
/// -------------
/// - Dashboard'da son 7 gun grafigi
/// - Gunluk kasa raporu
/// - Trend analizi grafikleri
///
/// ORNEK VERI:
/// -----------
/// Date: 2024-01-15
/// SaleCount: 45
/// TotalRevenue: 125000
/// CashSales: 50000
/// CardSales: 75000
/// </summary>
public class DailySalesSummaryDto
{
    /// <summary>
    /// Ozet tarihi.
    /// Grafiklerde X ekseninde kullanilir.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// O gunku satis adedi.
    /// Grafiklerde bar yuksekligi veya cizgi noktasi.
    /// </summary>
    public int SaleCount { get; set; }

    /// <summary>
    /// O gunku toplam ciro.
    /// Ana metrik olarak kullanilir.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Nakit satis tutari.
    /// Kasa icindeki nakit miktarini belirler.
    /// </summary>
    public decimal CashSales { get; set; }

    /// <summary>
    /// Kartli satis tutari.
    /// Pos cihazi islemleri toplami.
    /// </summary>
    public decimal CardSales { get; set; }
}

#endregion
