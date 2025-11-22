using System.ComponentModel.DataAnnotations;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Expense DTO - Gider bilgilerini API'de taşımak için
/// Feyza Paragöz'ün (Muhasebe Temsilcisi) kullandığı DTO'lar
///
/// ÖNEMLİ ÖZELLİKLER:
/// - Çoklu para birimi desteği (TRY, USD, EUR)
/// - Döviz kuru takibi (geçmiş kur bilgisi)
/// - Otomatik TL karşılığı hesaplama
/// - Ödeme durumu takibi (IsPaid)
/// - Vade takibi (PaymentDate)
///
/// NEDEN ÖNEMLİ?
/// - Feyza Paragöz: "O tarihteki döviz kurunu da görmek istiyorum"
/// - Haluk Bey: "Hangi mağazanın giderleri fazla?"
/// - Nakit akışı yönetimi için kritik
/// </summary>

/// <summary>
/// Expense Read DTO - GET /api/expenses/{id} endpoint'inden döner
/// </summary>
public class ExpenseDto
{
    public int Id { get; set; }

    public string ExpenseNumber { get; set; } = string.Empty;

    public DateTime ExpenseDate { get; set; }

    public ExpenseType ExpenseType { get; set; }

    public decimal Amount { get; set; }

    public Currency Currency { get; set; }

    /// <summary>
    /// Döviz kuru (o tarihteki)
    /// Frontend'de gösterilir: "1 USD = 32.50 TL"
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// TL karşılığı (hesaplanmış)
    /// Tüm raporlarda bu alan kullanılır
    /// </summary>
    public decimal AmountInTRY { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? DocumentNumber { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    public PaymentType? PaymentMethod { get; set; }

    // Foreign Keys
    public int StoreId { get; set; }
    public int? EmployeeId { get; set; }

    // Ek bilgiler (JOIN ile getirilir)
    public string StoreName { get; set; } = string.Empty;
    public string? EmployeeFullName { get; set; }

    /// <summary>
    /// Calculated property - Ödeme durumu metni
    /// Frontend'de badge olarak gösterilir
    /// </summary>
    public string PaymentStatusText => IsPaid ? "Ödendi" : "Bekliyor";

    /// <summary>
    /// Calculated property - Kaç gün gecikme var?
    /// PaymentDate geçtiyse ve henüz ödenmemişse hesaplanır
    /// </summary>
    public int? DaysOverdue
    {
        get
        {
            if (!IsPaid && PaymentDate.HasValue && PaymentDate.Value < DateTime.Now)
                return (DateTime.Now - PaymentDate.Value).Days;
            return null;
        }
    }

    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Expense Create DTO - POST /api/expenses endpoint'ine gönderilir
/// Feyza Paragöz yeni gider kaydı açarken kullanır
///
/// İŞ AKIŞI:
/// 1. Feyza Paragöz fatura/evrak alır
/// 2. Windows uygulamasından gider kaydı oluşturur
/// 3. ExpenseType seçer (Fatura, Maaş, Altyapı, Diğer)
/// 4. Currency seçer (TRY, USD, EUR)
/// 5. Eğer USD/EUR ise ExchangeRate otomatik çekilir (döviz kuru API'sinden)
/// 6. AmountInTRY otomatik hesaplanır (Amount * ExchangeRate)
/// </summary>
public class CreateExpenseDto
{
    // ExpenseNumber otomatik oluşturulur (G-2024-00001)

    [Required(ErrorMessage = "Gider tarihi zorunludur")]
    public DateTime ExpenseDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Gider türü zorunludur")]
    public ExpenseType ExpenseType { get; set; }

    [Required(ErrorMessage = "Tutar zorunludur")]
    [Range(0.01, 10000000, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Para birimi zorunludur")]
    public Currency Currency { get; set; } = Currency.TRY;

    /// <summary>
    /// Döviz kuru (opsiyonel)
    /// Frontend'de Currency = USD/EUR seçilirse otomatik API'den çekilir
    /// Kullanıcı manuel de girebilir
    /// TRY seçiliyse null kalır
    /// </summary>
    [Range(0.01, 1000, ErrorMessage = "Döviz kuru geçerli bir değer olmalıdır")]
    public decimal? ExchangeRate { get; set; }

    // AmountInTRY Service layer'da hesaplanır

    [Required(ErrorMessage = "Açıklama zorunludur")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DocumentNumber { get; set; }

    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Ödeme tarihi / Vade tarihi
    /// IsPaid = false ise vade tarihi (ne zaman ödenmeli)
    /// IsPaid = true ise ödeme tarihi (ne zaman ödendi)
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    public PaymentType? PaymentMethod { get; set; }

    [Required(ErrorMessage = "Mağaza seçimi zorunludur")]
    public int StoreId { get; set; }

    /// <summary>
    /// Çalışan seçimi (sadece ExpenseType = CalisanOdemesi için zorunlu)
    /// Maaş ödemelerinde hangi çalışana ödeme yapıldığı
    /// Frontend'de ExpenseType = CalisanOdemesi seçilirse zorunlu hale gelir
    /// </summary>
    public int? EmployeeId { get; set; }
}

/// <summary>
/// Expense Update DTO - PUT /api/expenses/{id} endpoint'ine gönderilir
///
/// NEDEN ExpenseNumber Değiştirilemez?
/// - Referans numarası değişmez
/// - Fatura/evrakta bu numara yazılı
///
/// NEDEN ExpenseDate Değiştirilebilir?
/// - Yanlış tarih girilmiş olabilir
/// - Ama dikkatli olunmalı (muhasebe raporları etkilenir)
/// </summary>
public class UpdateExpenseDto
{
    [Required]
    public int Id { get; set; }

    // ExpenseNumber değiştirilemez - referans

    [Required(ErrorMessage = "Gider tarihi zorunludur")]
    public DateTime ExpenseDate { get; set; }

    [Required(ErrorMessage = "Gider türü zorunludur")]
    public ExpenseType ExpenseType { get; set; }

    [Required(ErrorMessage = "Tutar zorunludur")]
    [Range(0.01, 10000000, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Para birimi zorunludur")]
    public Currency Currency { get; set; }

    [Range(0.01, 1000, ErrorMessage = "Döviz kuru geçerli bir değer olmalıdır")]
    public decimal? ExchangeRate { get; set; }

    // AmountInTRY Service layer'da yeniden hesaplanır

    [Required(ErrorMessage = "Açıklama zorunludur")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DocumentNumber { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    public PaymentType? PaymentMethod { get; set; }

    // StoreId değiştirilemez - gider hangi mağazaya aitse ona ait
    // EmployeeId değiştirilemez - maaş hangi çalışana aitse ona ait
}

/// <summary>
/// Expense Summary DTO - Gider listesi ve raporlar için hafif DTO
/// GET /api/expenses endpoint'inden döner
///
/// KULLANIM:
/// - Feyza Paragöz: Bekleyen ödemeler listesi
/// - Haluk Bey: Aylık gider raporu
/// - Dashboard: Bugünkü ödenecek giderler
/// </summary>
public class ExpenseSummaryDto
{
    public int Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public decimal AmountInTRY { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? EmployeeFullName { get; set; }
}
