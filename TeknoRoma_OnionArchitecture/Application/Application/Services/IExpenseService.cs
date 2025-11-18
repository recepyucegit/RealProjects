using Application.DTOs.ExpenseDTO;

using Application.DTOs.TechnicalServiceDTOs;
using Domain.Enums;

namespace Application.Services
{
    /// <summary>
    /// Expense Service Interface
    /// Gider iş mantığı
    /// 
    /// ÖZEL İŞ MANTIĞI:
    /// - Döviz kuru dönüşümleri
    /// - Gider türüne göre işlemler
    /// - Aylık raporlar
    /// </summary>
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync();
        Task<ExpenseDTO> GetExpenseByIdAsync(int id);
        Task<ExpenseDTO> GetExpenseByNumberAsync(string expenseNumber);

        Task<IEnumerable<ExpenseDTO>> GetExpensesByStoreAsync(int storeId);
        Task<IEnumerable<ExpenseDTO>> GetExpensesByTypeAsync(ExpenseType expenseType);
        Task<IEnumerable<ExpenseDTO>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ExpenseDTO>> GetUnpaidExpensesAsync();

        /// <summary>
        /// Yeni gider ekler
        /// 
        /// İŞ MANTIĞI:
        /// 1. Gider numarası otomatik oluştur (G-2024-00001)
        /// 2. Döviz kuru varsa TL'ye çevir
        /// 3. Gider kaydı oluştur
        /// </summary>
        Task<ExpenseDTO> CreateExpenseAsync(CreateExpenseDTO dto);

        Task<ExpenseDTO> UpdateExpenseAsync(UpdateExpenseDTO dto);
        Task<bool> DeleteExpenseAsync(int id);

        /// <summary>
        /// Ödeme işaretler
        /// IsPaid = true, PaymentDate = DateTime.Now
        /// </summary>
        Task<bool> MarkAsPaidAsync(int expenseId);

        /// <summary>
        /// Aylık toplam gideri hesaplar
        /// NEDEN?
        /// - Feyza Paragöz: "Aylık giriş çıkışları görebilmeliyim"
        /// </summary>
        Task<decimal> GetMonthlyTotalExpenseAsync(int year, int month, int? storeId = null);

        /// <summary>
        /// Gider türüne göre aylık toplam
        /// Detaylı analiz için
        /// </summary>
        Task<decimal> GetMonthlyExpenseByTypeAsync(int year, int month, ExpenseType expenseType, int? storeId = null);

        /// <summary>
        /// Döviz kuruyla TL'ye çevirir
        /// NEDEN?
        /// - Feyza Paragöz: "O tarihteki döviz kurunu görmek istiyorum"
        /// </summary>
        Task<decimal> ConvertToTRYAsync(decimal amount, Currency currency, decimal? exchangeRate = null);
    }

    /// <summary>
    /// TechnicalService Service Interface
    /// Teknik servis iş mantığı
    /// 
    /// ÖZEL İŞ MANTIĞI:
    /// - Durum yönetimi (Acik → Islemde → Tamamlandi)
    /// - Öncelik yönetimi
    /// - Bildirim sistemi
    /// </summary>
    public interface ITechnicalServiceService
    {
        Task<IEnumerable<TechnicalServiceDTO>> GetAllServicesAsync();
        Task<TechnicalServiceDTO> GetServiceByIdAsync(int id);
        Task<TechnicalServiceDTO> GetServiceByNumberAsync(string serviceNumber);

        Task<IEnumerable<TechnicalServiceDTO>> GetServicesByStatusAsync(TechnicalServiceStatus status);
        Task<IEnumerable<TechnicalServiceDTO>> GetServicesByStoreAsync(int storeId);
        Task<IEnumerable<TechnicalServiceDTO>> GetServicesByAssignedEmployeeAsync(int employeeId);

        /// <summary>
        /// Açık sorunları öncelik sırasına göre getirir
        /// NEDEN?
        /// - Özgün Kablocu: "Kritik sorunlar önce çözülmeli"
        /// </summary>
        Task<IEnumerable<TechnicalServiceDTO>> GetOpenIssuesByPriorityAsync();

        /// <summary>
        /// Yeni servis kaydı oluşturur
        /// 
        /// İŞ MANTIĞI:
        /// 1. Servis numarası otomatik oluştur (TS-2024-00001)
        /// 2. Status = Acik olarak ayarla
        /// 3. ReportedDate = DateTime.Now
        /// 4. Bildirim gönder (teknik servise)
        /// </summary>
        Task<TechnicalServiceDTO> CreateServiceAsync(CreateTechnicalServiceDTO dto);

        Task<TechnicalServiceDTO> UpdateServiceAsync(UpdateTechnicalServiceDTO dto);
        Task<bool> DeleteServiceAsync(int id);

        /// <summary>
        /// Servisi teknik servis elemanına atar
        /// Status = Islemde yapar
        /// </summary>
        Task<bool> AssignToEmployeeAsync(int serviceId, int employeeId);

        /// <summary>
        /// Servisi tamamlar
        /// 
        /// İŞ MANTIĞI:
        /// 1. Çözüm açıklaması zorunlu
        /// 2. Status = Tamamlandi
        /// 3. ResolvedDate = DateTime.Now
        /// 4. Bildirim gönder (bildirene)
        /// </summary>
        Task<bool> CompleteServiceAsync(int serviceId, string resolution);

        /// <summary>
        /// Servisi çözülemedi olarak işaretle
        /// </summary>
        Task<bool> MarkAsUnresolvedAsync(int serviceId, string reason);
    }
}