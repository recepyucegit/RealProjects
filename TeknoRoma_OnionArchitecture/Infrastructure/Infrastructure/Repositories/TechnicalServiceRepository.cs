using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// TechnicalService Repository Implementation
    ///
    /// AMAÇ:
    /// - Özgün Kablocu (Teknik Servis Temsilcisi) için sorun takibi
    /// - Müşteri sorunları ve sistem sorunları yönetimi
    /// - Öncelik bazlı sıralama
    /// - Durum takibi (Açık, İşlemde, Tamamlandı, Çözülemedi)
    ///
    /// SORUN TÜRLERİ:
    /// 1. Müşteri Sorunları (IsCustomerIssue = true)
    /// 2. Sistem Sorunları (IsCustomerIssue = false)
    ///
    /// ÖNCELİK SEVİYELERİ:
    /// 1: Düşük, 2: Orta, 3: Yüksek, 4: Kritik
    /// </summary>
    public class TechnicalServiceRepository : Repository<TechnicalService>, ITechnicalServiceRepository
    {
        public TechnicalServiceRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// Servis numarasına göre servis kaydı bulur
        /// Format: TS-2024-00001
        /// </summary>
        public async Task<TechnicalService> GetByServiceNumberAsync(string serviceNumber)
        {
            if (string.IsNullOrWhiteSpace(serviceNumber))
                throw new ArgumentException("Servis numarası boş olamaz", nameof(serviceNumber));

            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .FirstOrDefaultAsync(ts => ts.ServiceNumber == serviceNumber);
        }

        /// <summary>
        /// Duruma göre servis kayıtlarını getirir
        /// Özgün Kablocu: "Açık sorunları görüp öncelik sırasına göre çözmeliyim"
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status)
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.Status == status)
                .OrderByDescending(ts => ts.Priority) // Önce yüksek öncelikli
                .ThenBy(ts => ts.ReportedDate) // Sonra eski tarihli
                .ToListAsync();
        }

        /// <summary>
        /// Mağazaya göre servis kayıtlarını getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.StoreId == storeId)
                .OrderByDescending(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Atanan teknik servis elemanına göre getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.AssignedToEmployeeId == employeeId)
                .OrderByDescending(ts => ts.Priority)
                .ThenBy(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Sorunu bildiren çalışana göre getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByReportedEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.ReportedByEmployeeId == employeeId)
                .OrderByDescending(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Müşteriye göre servis kayıtlarını getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Where(ts => ts.CustomerId == customerId)
                .OrderByDescending(ts => ts.ReportedDate)
                .ToListAsync();
        }


        // ====== SORUN TÜRÜ FİLTRELEME ======

        /// <summary>
        /// Müşteri sorunlarını getirir
        /// IsCustomerIssue = true
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetCustomerIssuesAsync()
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.IsCustomerIssue == true)
                .OrderByDescending(ts => ts.Priority)
                .ThenBy(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Sistem sorunlarını getirir
        /// IsCustomerIssue = false
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetSystemIssuesAsync()
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Where(ts => ts.IsCustomerIssue == false)
                .OrderByDescending(ts => ts.Priority)
                .ThenBy(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Önceliğe göre servis kayıtlarını getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByPriorityAsync(int priority)
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.Priority == priority)
                .OrderBy(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Tarih aralığındaki servis kayıtlarını getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.ReportedDate >= startDate && ts.ReportedDate <= endDate)
                .OrderByDescending(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Açık ve atanmamış sorunları getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetUnassignedAsync()
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.AssignedToEmployeeId == null
                          && ts.Status == TechnicalServiceStatus.Acik)
                .OrderByDescending(ts => ts.Priority)
                .ThenBy(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Kritik öncelikli ve açık durumda olan sorunları getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetCriticalOpenIssuesAsync()
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.Priority == 4
                          && (ts.Status == TechnicalServiceStatus.Acik
                              || ts.Status == TechnicalServiceStatus.Islemde))
                .OrderBy(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli süreyi aşmış çözülmemiş sorunları getirir
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetOverdueIssuesAsync(int hours)
        {
            var cutoffDate = DateTime.Now.AddHours(-hours);

            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.ReportedDate <= cutoffDate
                          && (ts.Status == TechnicalServiceStatus.Acik
                              || ts.Status == TechnicalServiceStatus.Islemde))
                .OrderByDescending(ts => ts.Priority)
                .ThenBy(ts => ts.ReportedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Servis kaydını ilişkili verilerle getirir (Eager Loading)
        /// </summary>
        public async Task<TechnicalService> GetWithDetailsAsync(int serviceId)
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .FirstOrDefaultAsync(ts => ts.ID == serviceId);
        }

        /// <summary>
        /// Ortalama çözüm süresini hesaplar (saat cinsinden)
        /// </summary>
        public async Task<double> GetAverageResolutionTimeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Where(ts => ts.Status == TechnicalServiceStatus.Tamamlandi
                          && ts.ResolvedDate.HasValue);

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(ts => ts.ReportedDate >= startDate.Value
                                       && ts.ReportedDate <= endDate.Value);
            }

            var resolvedServices = await query.ToListAsync();

            if (!resolvedServices.Any())
                return 0;

            var totalHours = resolvedServices
                .Select(ts => (ts.ResolvedDate!.Value - ts.ReportedDate).TotalHours)
                .Average();

            return Math.Round(totalHours, 2);
        }


        // ====== NUMARA OLUŞTURMA ======

        /// <summary>
        /// Yeni servis numarası oluşturur
        /// Format: TS-2024-00001, TS-2024-00002, ...
        /// </summary>
        public async Task<string> GenerateServiceNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            var prefix = $"TS-{currentYear}-";

            var lastService = await _dbSet
                .Where(ts => ts.ServiceNumber.StartsWith(prefix))
                .OrderByDescending(ts => ts.ServiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastService != null)
            {
                var lastNumberPart = lastService.ServiceNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}"; // Format: TS-2024-00001
        }

        /// <summary>
        /// Durum bazlı istatistikleri getirir
        /// </summary>
        public async Task<Dictionary<TechnicalServiceStatus, int>> GetStatusStatisticsAsync()
        {
            var statistics = await _dbSet
                .GroupBy(ts => ts.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return statistics.ToDictionary(x => x.Status, x => x.Count);
        }
    }
}
