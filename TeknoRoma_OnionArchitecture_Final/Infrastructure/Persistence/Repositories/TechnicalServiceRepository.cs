// ===================================================================================
// TEKNOROMA - TEKNIK SERVIS REPOSITORY IMPLEMENTASYONU (TechnicalServiceRepository.cs)
// ===================================================================================
//
// Bu dosya teknik servis taleplerini yonetmek icin veritabani erisim katmanini saglar.
// Ticket sistemi, SLA takibi ve is dagilimi icin kullanilir.
//
// IS SENARYOLARI:
// - Acik talep listesi (dashboard)
// - Teknisyene atanmamis talepler
// - Durum bazli filtreleme
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Teknik Servis Repository Implementasyonu
    ///
    /// TICKET SISTEMI: Sorun takibi ve cozum yonetimi
    /// </summary>
    public class TechnicalServiceRepository : EfRepository<TechnicalService>, ITechnicalServiceRepository
    {
        public TechnicalServiceRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Servis numarasi ile getir
        /// "TS-2024-00001" formatinda
        /// Musteri sorgulamasi icin
        /// </summary>
        public async Task<TechnicalService?> GetByServiceNumberAsync(string serviceNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ts => ts.ServiceNumber == serviceNumber);
        }

        /// <summary>
        /// Duruma gore talepler
        /// Acik, Islemde, Tamamlandi, Cozulemedi
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status)
        {
            return await _dbSet
                .Where(ts => ts.Status == status)
                .OrderByDescending(ts => ts.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Magaza talepleri
        /// Sube bazli servis takibi
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Where(ts => ts.StoreId == storeId)
                .OrderByDescending(ts => ts.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Teknisyene atanan talepler
        /// Calisan is yuku goruntuleme
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Where(ts => ts.AssignedToEmployeeId == employeeId)
                .OrderByDescending(ts => ts.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Musteri talepleri
        /// Musteri sikayet gecmisi
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(ts => ts.CustomerId == customerId)
                .OrderByDescending(ts => ts.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Acik talepler
        ///
        /// Status != Tamamlandi && Status != Cozulemedi
        /// Dashboard uyarisi icin
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetOpenIssuesAsync()
        {
            return await _dbSet
                .Where(ts => ts.Status != TechnicalServiceStatus.Tamamlandi
                          && ts.Status != TechnicalServiceStatus.Cozulemedi)
                .OrderByDescending(ts => ts.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Atanmamis talepler
        ///
        /// AssignedToEmployeeId = null
        /// Is dagilimi icin kritik
        /// </summary>
        public async Task<IReadOnlyList<TechnicalService>> GetUnassignedAsync()
        {
            return await _dbSet
                .Where(ts => ts.AssignedToEmployeeId == null)
                .OrderByDescending(ts => ts.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Acik talep sayisi
        ///
        /// Dashboard badge/counter icin
        /// Hizli sorgu - sadece COUNT
        /// </summary>
        public async Task<int> GetOpenIssuesCountAsync()
        {
            return await _dbSet
                .CountAsync(ts => ts.Status != TechnicalServiceStatus.Tamamlandi
                               && ts.Status != TechnicalServiceStatus.Cozulemedi);
        }

        /// <summary>
        /// Yeni servis numarasi olustur
        ///
        /// FORMAT: "TS-YYYY-NNNNN"
        /// Ornek: "TS-2024-00001"
        /// </summary>
        public async Task<string> GenerateServiceNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"TS-{year}-";

            // Bu yilin son servis numarasini bul
            var lastService = await _dbSet
                .Where(ts => ts.ServiceNumber.StartsWith(prefix))
                .OrderByDescending(ts => ts.ServiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastService != null)
            {
                var lastNumberStr = lastService.ServiceNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }
    }
}
