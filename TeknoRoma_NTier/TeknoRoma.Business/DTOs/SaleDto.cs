using System.ComponentModel.DataAnnotations;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Sale DTO - Satış işlemlerini API'de taşımak için
/// Gül Satar'ın (Kasa Satış Temsilcisi) kullandığı DTO'lar
///
/// MASTER-DETAIL İLİŞKİSİ:
/// - Sale: Satış başlık bilgileri (Sipariş no, müşteri, toplam tutar)
/// - SaleDetail: Satış detay bilgileri (hangi ürünlerden kaç adet)
/// - Bir Sale birden fazla SaleDetail içerir
///
/// İŞ AKIŞI:
/// 1. Gül Satar müşteri bilgilerini girer
/// 2. Ürünleri seçer, miktarları belirler (SaleDetail'lar)
/// 3. İndirim uygular (varsa)
/// 4. Toplam tutarı hesaplar
/// 5. Ödeme tipini seçer (Nakit, Kredi Kartı, vb.)
/// 6. Kaydeder → Status = Beklemede
/// 7. Ödeme alınca → Status = Hazirlaniyor
/// 8. Durna Sabit (Depo) ürünleri kasaya getirir
/// 9. Müşteriye teslim → Status = Tamamlandi
///
/// NEDEN KOMPLEKs?
/// - Stok güncellemesi gerekir (Product.Stock -= Quantity)
/// - Transaction yönetimi gerekir (Sale + SaleDetails + Stock update)
/// - Fiyat hesaplamaları dinamik (indirimli fiyat, kargo, toplam)
/// </summary>

/// <summary>
/// SaleDetail DTO - Satış satır detayı
/// Bir satışta hangi ürünlerden kaç adet satıldığını gösterir
/// </summary>
public class SaleDetailDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Satış anındaki birim fiyat
    /// NEDEN? Ürün fiyatı sonradan değişebilir
    /// Fatura/rapor için satış anındaki fiyat önemli
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Satış miktarı
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Satır bazında indirim tutarı
    /// Örnek: 100 TL ürün, 10 TL indirim = 90 TL
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Calculated property - Satır toplamı
    /// (UnitPrice * Quantity) - Discount
    /// Frontend'de gösterilir
    /// </summary>
    public decimal LineTotal => (UnitPrice * Quantity) - Discount;
}

/// <summary>
/// Sale Read DTO - GET /api/sales/{id} endpoint'inden döner
/// Satış başlık + detaylar birlikte gelir
/// </summary>
public class SaleDto
{
    public int Id { get; set; }

    public string SaleNumber { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public SaleStatus Status { get; set; }

    public PaymentType PaymentMethod { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? ShippingAddress { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public string? Notes { get; set; }

    // Foreign Keys
    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public int EmployeeId { get; set; }

    // Ek bilgiler (JOIN ile getirilir)
    public string CustomerName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Satış detayları (satılan ürünler)
    /// Master-Detail ilişkisi
    /// </summary>
    public List<SaleDetailDto> SaleDetails { get; set; } = new List<SaleDetailDto>();

    /// <summary>
    /// Calculated property - Durum metni
    /// Frontend'de renkli badge olarak gösterilir
    /// </summary>
    public string StatusText => Status switch
    {
        SaleStatus.Beklemede => "Beklemede",
        SaleStatus.Hazirlaniyor => "Hazırlanıyor",
        SaleStatus.Tamamlandi => "Tamamlandı",
        SaleStatus.Iptal => "İptal Edildi",
        _ => "Belirsiz"
    };

    /// <summary>
    /// Calculated property - Ödeme durumu metni
    /// </summary>
    public string PaymentStatusText => IsPaid ? "Ödendi" : "Ödenmedi";

    /// <summary>
    /// Calculated property - Ürün sayısı (toplam adet)
    /// </summary>
    public int TotalItemCount => SaleDetails.Sum(d => d.Quantity);

    /// <summary>
    /// Calculated property - Ürün çeşit sayısı
    /// </summary>
    public int ProductTypeCount => SaleDetails.Count;

    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// SaleDetail Create DTO - Satış detayı oluşturmak için
/// CreateSaleDto içinde kullanılır
/// </summary>
public class CreateSaleDetailDto
{
    [Required(ErrorMessage = "Ürün seçimi zorunludur")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Birim fiyat zorunludur")]
    [Range(0.01, 10000000, ErrorMessage = "Birim fiyat geçerli bir değer olmalıdır")]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, 10000, ErrorMessage = "Miktar 1 ile 10000 arasında olmalıdır")]
    public int Quantity { get; set; }

    [Range(0, 10000000, ErrorMessage = "İndirim tutarı geçerli bir değer olmalıdır")]
    public decimal Discount { get; set; } = 0;
}

/// <summary>
/// Sale Create DTO - POST /api/sales endpoint'ine gönderilir
/// Gül Satar yeni satış kaydederken kullanır
///
/// İŞ AKIŞI (Service Layer):
/// 1. CreateSaleDto gelir
/// 2. Transaction başlatılır (BeginTransaction)
/// 3. Sale kaydı oluşturulur
/// 4. Her SaleDetail için:
///    a. SaleDetail kaydı oluşturulur
///    b. Product.Stock -= Quantity (stok azaltılır)
///    c. Stok kontrolü yapılır (negatife düşmemeli)
/// 5. TotalAmount hesaplanır
/// 6. Transaction commit edilir
/// 7. Hata olursa rollback edilir
///
/// NEDEN Transaction?
/// - Atomicity: Ya hepsi başarılı, ya hiçbiri
/// - Örnek: Sale eklendi ama stok güncellenemedi → Sale'i de geri al
/// </summary>
public class CreateSaleDto
{
    // SaleNumber otomatik oluşturulur (S-2024-00001)

    public DateTime SaleDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Müşteri seçimi zorunludur")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Mağaza seçimi zorunludur")]
    public int StoreId { get; set; }

    [Required(ErrorMessage = "Çalışan seçimi zorunludur")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Ödeme yöntemi zorunludur")]
    public PaymentType PaymentMethod { get; set; }

    /// <summary>
    /// Ödeme alındı mı?
    /// true: Ödeme alındı (PaymentDate = şimdi)
    /// false: Bekliyor (PaymentDate = null)
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Teslimat adresi (opsiyonel)
    /// Boş ise müşteri mağazadan alacak
    /// Dolu ise kargo ile gönderilecek (ShippingCost > 0)
    /// </summary>
    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Kargo ücreti
    /// ShippingAddress dolu ise > 0 olmalı
    /// Boş ise 0 kalmalı
    /// </summary>
    [Range(0, 10000, ErrorMessage = "Kargo ücreti geçerli bir değer olmalıdır")]
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>
    /// Genel indirim tutarı (tüm satış için)
    /// Satır bazlı indirim SaleDetail.Discount'ta
    /// Kampanya indirimi burada
    /// </summary>
    [Range(0, 10000000, ErrorMessage = "İndirim tutarı geçerli bir değer olmalıdır")]
    public decimal DiscountAmount { get; set; } = 0;

    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Satış detayları (en az 1 ürün olmalı)
    /// Frontend'de dinamik olarak ürün eklenir
    /// </summary>
    [Required(ErrorMessage = "En az 1 ürün eklemelisiniz")]
    [MinLength(1, ErrorMessage = "En az 1 ürün eklemelisiniz")]
    public List<CreateSaleDetailDto> SaleDetails { get; set; } = new List<CreateSaleDetailDto>();
}

/// <summary>
/// Sale Update DTO - PUT /api/sales/{id} endpoint'ine gönderilir
///
/// SINIRLI GÜNCELLEME!
/// - Sadece Status ve IsPaid güncellenebilir
/// - Ürünler değiştirilemez (zaten stok azalmış)
/// - Müşteri değiştirilemez
/// - Fiyatlar değiştirilemez
///
/// NEDEN?
/// - Muhasebe tutarlılığı
/// - Stok tutarlılığı
/// - Fatura kesilmiş olabilir
///
/// KULLANIM SENARYOLARI:
/// 1. Ödeme alındı: IsPaid = true, PaymentDate = şimdi
/// 2. Hazırlamaya başlandı: Status = Hazirlaniyor
/// 3. Teslim edildi: Status = Tamamlandi
/// 4. İptal edildi: Status = Iptal (stoklar geri eklenmeli)
/// </summary>
public class UpdateSaleDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Durum zorunludur")]
    public SaleStatus Status { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    // Diğer alanlar değiştirilemez
}

/// <summary>
/// Sale Summary DTO - Liste ve raporlar için hafif DTO
/// GET /api/sales endpoint'inden döner
///
/// KULLANIM:
/// - Gül Satar: Bugünkü satışlar
/// - Haluk Bey: Aylık satış raporu
/// - Dashboard: Bekleyen siparişler
/// </summary>
public class SaleSummaryDto
{
    public int Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public SaleStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalItemCount { get; set; }
}
