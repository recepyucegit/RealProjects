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
        /// Açık sorunları öncelik sırasına göre getirir
        /// Özgün Kablocu: "Önce kritik sorunlar çözülmeli"
        ///
        /// FİLTRELEME:
        /// - Status = Acik veya Islemde
        /// - Önce yüksek Priority
        /// - Sonra eski tarihli
        ///
        /// NEDEN BU SIRALAMA?
        /// - Kritik sorunlar önce (Priority 4, 3, 2, 1)
        /// - Aynı öncelikteyse eski tarihli önce (FIFO)
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetOpenIssuesByPriorityAsync()
        {
            return await _dbSet
                .Include(ts => ts.Store)
                .Include(ts => ts.ReportedByEmployee)
                .Include(ts => ts.AssignedToEmployee)
                .Include(ts => ts.Customer)
                .Where(ts => ts.Status == TechnicalServiceStatus.Acik
                          || ts.Status == TechnicalServiceStatus.Islemde)
                .OrderByDescending(ts => ts.Priority) // 4 (Kritik) önce
                .ThenBy(ts => ts.ReportedDate) // Eski tarihli önce
                .ToListAsync();
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
    }
}
