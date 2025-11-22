// ===================================================================================
// TEKNOROMA - RAPOR DTO DOSYASI (ReportDTOs.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, raporlama ile ilgili tum DTO'lari icerir.
// TeknoRoma'nin is zekasi (BI) ve karar destek sistemi icin kullanilir.
//
// RAPORLAMA NEDEN ONEMLI?
// -----------------------
// Raporlar, isletmenin:
// - Gecmis performansini analiz etmesini
// - Trendleri tespit etmesini
// - Kararlari veriye dayali almasini
// - Sorunlari erken fark etmesini saglar
//
// RAPOR KATEGORILERI
// ------------------
// 1. SATIS RAPORLARI:
//    - Donemsel satis ozeti
//    - Odeme tipi dagilimi
//    - Gunluk/haftalik/aylik trendler
//
// 2. URUN RAPORLARI:
//    - En cok satan urunler
//    - Kategori bazli satislar
//    - Stok durumu raporu
//    - Dusuk stoklu urunler
//
// 3. PERFORMANS RAPORLARI:
//    - Calisan performansi
//    - Magaza karsilastirmasi
//    - Teknisyen verimliligi
//
// 4. FINANSAL RAPORLAR:
//    - Kar/zarar raporu
//    - Gider analizi
//    - Brut/net kar marjlari
//
// 5. MUSTERI RAPORLARI:
//    - Musteri dagilimi
//    - Sadakat programi ozeti
//    - En iyi musteriler
//
// NOT:
// ----
// Bu dosyadaki DTO'lar SADECE veri tasimak icindir.
// Rapor hesaplama mantigi Service katmaninda bulunur.
//
// ILISKILI SERVIS
// ---------------
// Application/Interfaces/IReportService.cs
//
// ===================================================================================

namespace TeknoRoma.Application.DTOs;

#region SATIS RAPORLARI

/// <summary>
/// Belirli bir donem icin satis raporu.
///
/// ACIKLAMA:
/// ---------
/// Secilen tarih araligindaki tum satislarin detayli ozeti.
/// Yonetim raporlari ve analiz icin ana rapor.
///
/// UI KULLANIMI:
/// -------------
/// - Satis raporu sayfasi
/// - Excel/PDF export
/// - Yonetim sunumlari
///
/// ICERIK:
/// -------
/// - Tarih araligi
/// - Toplam ciro
/// - KDV ve indirim toplamları
/// - Islem sayisi
/// - Gunluk breakdown
/// - Odeme tipi dagilimi
///
/// ORNEK:
/// ------
/// var report = await _reportService.GetSalesReportAsync(startDate, endDate);
/// Console.WriteLine($"Donem: {report.StartDate:d} - {report.EndDate:d}");
/// Console.WriteLine($"Toplam Ciro: {report.TotalRevenue:C}");
/// Console.WriteLine($"Net Gelir: {report.NetRevenue:C}");
/// </summary>
public class SalesReportDto
{
    /// <summary>
    /// Rapor baslangic tarihi.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Rapor bitis tarihi.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Toplam brut ciro.
    /// Tum satislarin toplami (KDV dahil, indirim oncesi).
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Toplam KDV tutari.
    /// Vergi raporlari icin.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Toplam indirim tutari.
    /// Ne kadar indirim yapilmis.
    /// </summary>
    public decimal TotalDiscount { get; set; }

    /// <summary>
    /// Net gelir.
    /// Formul: TotalRevenue - TotalDiscount
    /// Gercek kasa girisi.
    /// </summary>
    public decimal NetRevenue { get; set; }

    /// <summary>
    /// Toplam islem sayisi.
    /// Kac fis/fatura kesilmis.
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Ortalama siparis tutari.
    /// Formul: TotalRevenue / TotalTransactions
    /// Musteri sepet buyuklugu.
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Gunluk satis breakdown'i.
    /// Her gun icin ayri ozet.
    /// Grafik ve trend analizi icin.
    /// </summary>
    public List<DailySalesSummaryDto> DailyBreakdown { get; set; } = new();

    /// <summary>
    /// Odeme tipi bazinda dagilim.
    /// Nakit, kart vb. oranları.
    /// </summary>
    public List<PaymentTypeSummaryDto> ByPaymentType { get; set; } = new();
}

/// <summary>
/// Odeme tipi bazinda satis ozeti.
///
/// ACIKLAMA:
/// ---------
/// Her odeme turu icin satis tutari ve orani.
/// Kasa raporu ve finansal analiz icin.
///
/// UI KULLANIMI:
/// -------------
/// - Pasta grafigi (odeme dagilimi)
/// - Kasa kapanisi raporu
/// </summary>
public class PaymentTypeSummaryDto
{
    /// <summary>
    /// Odeme tipi adi.
    /// Ornek: "Nakit", "Kredi Karti", "Banka Karti"
    /// </summary>
    public string PaymentType { get; set; } = null!;

    /// <summary>
    /// Bu odeme tipiyle yapilan toplam tutar.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Bu odeme tipiyle yapilan islem sayisi.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Toplam satis icindeki yuzde.
    /// Pasta grafiginde dilim buyuklugu.
    /// </summary>
    public decimal Percentage { get; set; }
}

#endregion

#region URUN RAPORLARI

/// <summary>
/// En cok satan urun raporu.
///
/// ACIKLAMA:
/// ---------
/// Satilan miktar veya gelire gore siralı urun listesi.
/// Stok planlama ve pazarlama kararlari icin.
///
/// UI KULLANIMI:
/// -------------
/// - Dashboard "Top 10 Urun" listesi
/// - Urun performans raporu
/// - Satinalma onerileri
/// </summary>
public class TopSellingProductReportDto
{
    /// <summary>
    /// Urun ID'si.
    /// Detaya gitmek icin.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Urun adi.
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Kategori adi.
    /// Hangi kategoriden.
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Satilan toplam adet.
    /// Populerlik gostergesi.
    /// </summary>
    public int QuantitySold { get; set; }

    /// <summary>
    /// Toplam satis geliri.
    /// Bu urunun getirdigi ciro.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Toplam kar.
    /// Formul: Satis Tutari - Alis Maliyeti
    /// </summary>
    public decimal Profit { get; set; }
}

/// <summary>
/// Calisan satis performansi raporu.
///
/// ACIKLAMA:
/// ---------
/// Her calisan icin satis metrikleri.
/// Performans degerlendirme ve prim hesaplama icin.
///
/// UI KULLANIMI:
/// -------------
/// - Calisan performans tablosu
/// - Sirali liste (en cok satandan en aza)
/// - Prim hesaplama
/// </summary>
public class EmployeeSalesReportDto
{
    /// <summary>
    /// Calisan ID'si.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Calisan adi.
    /// </summary>
    public string EmployeeName { get; set; } = null!;

    /// <summary>
    /// Calistigi magaza adi.
    /// </summary>
    public string? StoreName { get; set; }

    /// <summary>
    /// Toplam satis sayisi.
    /// Kac islem yapmis.
    /// </summary>
    public int TotalSales { get; set; }

    /// <summary>
    /// Toplam satis tutari.
    /// Ne kadar ciro yaratmis.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Ortalama siparis tutari.
    /// Satis becerisi gostergesi.
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Satilan urun adedi.
    /// Kac adet urun satmis.
    /// </summary>
    public int ItemsSold { get; set; }
}

/// <summary>
/// Kategori bazinda satis raporu.
///
/// ACIKLAMA:
/// ---------
/// Her kategori icin satis metrikleri.
/// Kategori performansi ve stok planlama icin.
/// </summary>
public class CategorySalesReportDto
{
    /// <summary>
    /// Kategori ID'si.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Kategori adi.
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Kategorideki urun sayisi.
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Satilan toplam adet.
    /// </summary>
    public int QuantitySold { get; set; }

    /// <summary>
    /// Toplam satis tutari.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Toplam satis icindeki yuzde.
    /// </summary>
    public decimal Percentage { get; set; }
}

#endregion

#region STOK RAPORLARI

/// <summary>
/// Stok durum raporu.
///
/// ACIKLAMA:
/// ---------
/// Tum urunlerin stok durumu ozeti.
/// Stok yonetimi ve siparis kararlari icin.
///
/// UI KULLANIMI:
/// -------------
/// - Stok ozet karti
/// - Kritik stok uyarilari
/// - Siparis onerileri
/// </summary>
public class StockReportDto
{
    /// <summary>
    /// Toplam urun cesidi.
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Stokta olan urun sayisi.
    /// Stock > MinStock
    /// </summary>
    public int InStockProducts { get; set; }

    /// <summary>
    /// Dusuk stoklu urun sayisi.
    /// 0 &lt; Stock &lt;= MinStock
    /// </summary>
    public int LowStockProducts { get; set; }

    /// <summary>
    /// Tukenmis urun sayisi.
    /// Stock = 0
    /// </summary>
    public int OutOfStockProducts { get; set; }

    /// <summary>
    /// Toplam stok degeri (TL).
    /// Tum urunlerin stok * alis fiyati toplami.
    /// </summary>
    public decimal TotalStockValue { get; set; }

    /// <summary>
    /// Dusuk stoklu urunler listesi.
    /// Acil siparis gereken urunler.
    /// </summary>
    public List<LowStockProductDto> LowStockItems { get; set; } = new();
}

/// <summary>
/// Dusuk stoklu urun bilgisi.
///
/// ACIKLAMA:
/// ---------
/// Stok seviyesi kritik olan urun detayi.
/// Siparis verilmesi gereken urunleri gosterir.
/// </summary>
public class LowStockProductDto
{
    /// <summary>
    /// Urun ID'si.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Urun adi.
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Kategori adi.
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Tedarikci adi.
    /// Siparis verilecek firma.
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// Mevcut stok.
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// Minimum stok seviyesi.
    /// </summary>
    public int MinStock { get; set; }

    /// <summary>
    /// Eksik miktar.
    /// Formul: MinStock - CurrentStock
    /// En az bu kadar siparis verilmeli.
    /// </summary>
    public int Deficit { get; set; }
}

#endregion

#region FINANSAL RAPORLAR

/// <summary>
/// Kar/Zarar raporu.
///
/// ACIKLAMA:
/// ---------
/// Belirli bir donem icin finansal performans ozeti.
/// Isletmenin karliligini gosterir.
///
/// HESAPLAMA:
/// ----------
/// Brut Kar = Satis Geliri - Satis Maliyeti (COGS)
/// Net Kar = Brut Kar - Giderler
/// Kar Marji = (Net Kar / Satis Geliri) * 100
///
/// UI KULLANIMI:
/// -------------
/// - Finansal ozet sayfasi
/// - Yonetim raporlari
/// - Karlilik analizi
/// </summary>
public class ProfitLossReportDto
{
    /// <summary>
    /// Rapor baslangic tarihi.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Rapor bitis tarihi.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Brut gelir (toplam satis).
    /// Tum satislar toplami.
    /// </summary>
    public decimal GrossRevenue { get; set; }

    /// <summary>
    /// Satilan mallarin maliyeti (COGS).
    /// Satilan urunlerin alis fiyati toplami.
    /// </summary>
    public decimal CostOfGoodsSold { get; set; }

    /// <summary>
    /// Brut kar.
    /// Formul: GrossRevenue - CostOfGoodsSold
    /// </summary>
    public decimal GrossProfit { get; set; }

    /// <summary>
    /// Toplam giderler.
    /// Kira, elektrik, maas vb.
    /// </summary>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// Net kar.
    /// Formul: GrossProfit - TotalExpenses
    /// Pozitif = Kar, Negatif = Zarar
    /// </summary>
    public decimal NetProfit { get; set; }

    /// <summary>
    /// Kar marji (yuzde).
    /// Formul: (NetProfit / GrossRevenue) * 100
    /// Isletme karlilik orani.
    /// </summary>
    public decimal ProfitMargin { get; set; }

    /// <summary>
    /// Gider turlerine gore dagilim.
    /// Her gider kategorisinin detayi.
    /// </summary>
    public List<ExpenseSummaryDto> ExpenseBreakdown { get; set; } = new();
}

#endregion

#region MUSTERI RAPORLARI

/// <summary>
/// Musteri raporu.
///
/// ACIKLAMA:
/// ---------
/// Musteri tabaninin ozet bilgileri.
/// CRM ve pazarlama kararlari icin.
/// </summary>
public class CustomerReportDto
{
    /// <summary>
    /// Toplam musteri sayisi.
    /// Kayitli tum musteriler.
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// Bu ay kayit olan musteri sayisi.
    /// Buyume gostergesi.
    /// </summary>
    public int NewCustomersThisMonth { get; set; }

    /// <summary>
    /// Aktif musteri sayisi.
    /// Son X gun icinde alisveris yapanlar.
    /// </summary>
    public int ActiveCustomers { get; set; }

    /// <summary>
    /// Dagitilan toplam sadakat puani.
    /// </summary>
    public decimal TotalLoyaltyPointsIssued { get; set; }

    /// <summary>
    /// Kullanilan toplam sadakat puani.
    /// </summary>
    public decimal TotalLoyaltyPointsRedeemed { get; set; }

    /// <summary>
    /// En iyi musteriler listesi.
    /// En cok alisveris yapanlar.
    /// </summary>
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

/// <summary>
/// En iyi musteri bilgisi.
///
/// ACIKLAMA:
/// ---------
/// VIP musteri detaylari.
/// Ozel kampanya ve ilgi icin.
/// </summary>
public class TopCustomerDto
{
    /// <summary>
    /// Musteri ID'si.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Musteri adi.
    /// </summary>
    public string CustomerName { get; set; } = null!;

    /// <summary>
    /// Toplam alisveris sayisi.
    /// </summary>
    public int TotalPurchases { get; set; }

    /// <summary>
    /// Toplam harcama tutari.
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// Mevcut sadakat puani.
    /// </summary>
    public decimal LoyaltyPoints { get; set; }

    /// <summary>
    /// Son alisveris tarihi.
    /// </summary>
    public DateTime LastPurchaseDate { get; set; }
}

#endregion

#region TEKNIK SERVIS RAPORLARI

/// <summary>
/// Teknik servis raporu.
///
/// ACIKLAMA:
/// ---------
/// Teknik servis operasyonlarinin ozeti.
/// Servis kalitesi ve verimlilik analizi icin.
/// </summary>
public class TechnicalServiceReportDto
{
    /// <summary>
    /// Rapor baslangic tarihi.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Rapor bitis tarihi.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Toplam servis kaydi.
    /// </summary>
    public int TotalTickets { get; set; }

    /// <summary>
    /// Tamamlanan servis sayisi.
    /// </summary>
    public int CompletedTickets { get; set; }

    /// <summary>
    /// Bekleyen servis sayisi.
    /// </summary>
    public int PendingTickets { get; set; }

    /// <summary>
    /// Toplam servis geliri.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Ortalama tamir suresi (saat).
    /// </summary>
    public decimal AverageRepairTimeHours { get; set; }

    /// <summary>
    /// Teknisyen bazinda performans.
    /// </summary>
    public List<TechnicianPerformanceDto> TechnicianPerformance { get; set; } = new();

    /// <summary>
    /// Cihaz turune gore dagilim.
    /// </summary>
    public List<DeviceTypeStatDto> ByDeviceType { get; set; } = new();
}

/// <summary>
/// Teknisyen performans bilgisi.
/// </summary>
public class TechnicianPerformanceDto
{
    /// <summary>
    /// Teknisyen ID'si.
    /// </summary>
    public int TechnicianId { get; set; }

    /// <summary>
    /// Teknisyen adi.
    /// </summary>
    public string TechnicianName { get; set; } = null!;

    /// <summary>
    /// Tamamlanan is sayisi.
    /// </summary>
    public int TicketsCompleted { get; set; }

    /// <summary>
    /// Toplam gelir.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Ortalama tamir suresi (saat).
    /// </summary>
    public decimal AverageRepairTimeHours { get; set; }
}

/// <summary>
/// Cihaz turu istatistikleri.
/// </summary>
public class DeviceTypeStatDto
{
    /// <summary>
    /// Cihaz turu adi.
    /// Ornek: "Telefon", "Laptop"
    /// </summary>
    public string DeviceType { get; set; } = null!;

    /// <summary>
    /// Bu turde kac servis kaydi var.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Toplam icindeki yuzde.
    /// </summary>
    public decimal Percentage { get; set; }
}

#endregion
