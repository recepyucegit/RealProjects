using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Employee Repository Implementation
    ///
    /// AMAÇ:
    /// - Çalışan bilgilerini yönetir
    /// - ASP.NET Identity entegrasyonu
    /// - Satış performansı ve prim hesaplaması
    /// - Role-based filtreleme
    ///
    /// ÖNEMLİ İŞ KURALLARI:
    /// - TC Kimlik UNIQUE
    /// - IdentityUserId ile ASP.NET Identity bağlantısı
    /// - Satış Kotası: 10.000 TL
    /// - Prim Oranı: %10 (kotayı aşan kısım)
    ///
    /// PRİM HESAPLAMA ÖRNEĞİ:
    /// - Aylık Satış: 15.000 TL
    /// - Kota: 10.000 TL
    /// - Başarılı Satış: 15.000 - 10.000 = 5.000 TL
    /// - Prim: 5.000 × 0.10 = 500 TL
    /// </summary>
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// TC Kimlik numarasına göre çalışan bulur
        /// UNIQUE field
        /// </summary>
        public async Task<Employee> GetByIdentityNumberAsync(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                throw new ArgumentException("TC Kimlik numarası boş olamaz", nameof(identityNumber));

            if (identityNumber.Length != 11)
                throw new ArgumentException("TC Kimlik numarası 11 haneli olmalı", nameof(identityNumber));

            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.IdentityNumber == identityNumber);
        }

        /// <summary>
        /// Email adresine göre çalışan bulur
        /// Identity User ile eşleştirme için
        /// </summary>
        public async Task<Employee> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email boş olamaz", nameof(email));

            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        /// <summary>
        /// Identity User ID'ye göre çalışan bulur
        /// ASP.NET Identity entegrasyonu
        ///
        /// SENARIO:
        /// 1. Kullanıcı login olur (ASP.NET Identity)
        /// 2. IdentityUserId alınır
        /// 3. Employee kaydı bulunur ← BU METOD
        /// 4. Employee bilgileri ile işlem yapılır
        /// </summary>
        public async Task<Employee> GetByIdentityUserIdAsync(string identityUserId)
        {
            if (string.IsNullOrWhiteSpace(identityUserId))
                throw new ArgumentException("Identity User ID boş olamaz", nameof(identityUserId));

            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.IdentityUserId == identityUserId);
        }


        // ====== FİLTRELEME METODLARI ======

        /// <summary>
        /// Role göre çalışanları getirir
        /// Haluk Bey: "Sadece şube müdürleri görebilsin"
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetByRoleAsync(UserRole role)
        {
            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Department)
                .Where(e => e.Role == role)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Mağazaya göre çalışanları getirir
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Include(e => e.Department)
                .Where(e => e.StoreId == storeId)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Departmana göre çalışanları getirir
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId)
        {
            return await _dbSet
                .Include(e => e.Store)
                .Where(e => e.DepartmentId == departmentId)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Aktif çalışanları getirir
        /// İşten ayrılanları hariç tutar
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync()
        {
            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Department)
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Satış kotası olan çalışanları getirir
        /// Sadece satış ekibinin kotası var
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetEmployeesWithSalesQuotaAsync()
        {
            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Department)
                .Where(e => e.SalesQuota != null && e.SalesQuota > 0)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }


        // ====== PERFORMANS VE PRİM HESAPLAMALARI ======

        /// <summary>
        /// Çalışanın aylık satış performansını hesaplar
        /// Gül Satar: "Prime ne kadar yaklaştım?"
        ///
        /// HESAPLAMA:
        /// - Sales tablosundan EmployeeId'ye göre filtrele
        /// - Tarih aralığı: Belirtilen ay
        /// - Durum: Sadece Tamamlanan satışlar
        /// - Toplam: SUM(TotalAmount)
        /// </summary>
        public async Task<decimal> GetEmployeeSalesPerformanceAsync(int employeeId, int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var totalSales = await _context.Sales
                .Where(s => s.EmployeeId == employeeId
                         && s.SaleDate >= firstDayOfMonth
                         && s.SaleDate < firstDayOfNextMonth
                         && s.Status == SaleStatus.Tamamlandi)
                .SumAsync(s => s.TotalAmount);

            return totalSales;
        }

        /// <summary>
        /// Çalışanın prim tutarını hesaplar
        /// Haluk Bey: "Satış kotasını geçmiş mi, ne kadar prim haketmiş?"
        ///
        /// İŞ KURALI:
        /// - Satış Kotası: 10.000 TL (Employee.SalesQuota)
        /// - Prim Oranı: %10
        /// - Sadece kotayı aşan kısım primlendirilir
        ///
        /// ÖRNEK:
        /// - Aylık Satış: 15.000 TL
        /// - Kota: 10.000 TL
        /// - Başarılı Satış: 15.000 - 10.000 = 5.000 TL
        /// - Prim: 5.000 × 0.10 = 500 TL
        ///
        /// ÖZEL DURUMLAR:
        /// - Kotası yoksa (SalesQuota = null): Prim = 0
        /// - Kotayı geçememişse: Prim = 0
        /// - Sadece tamamlanan satışlar sayılır
        /// </summary>
        public async Task<decimal> CalculateEmployeeCommissionAsync(int employeeId, int year, int month)
        {
            // Çalışanı bul
            var employee = await _dbSet
                .FirstOrDefaultAsync(e => e.ID == employeeId);

            if (employee == null)
                throw new ArgumentException($"Çalışan bulunamadı: {employeeId}");

            // Kotası yoksa prim yok
            if (!employee.SalesQuota.HasValue || employee.SalesQuota.Value <= 0)
                return 0;

            // Aylık satış performansını hesapla
            var totalSales = await GetEmployeeSalesPerformanceAsync(employeeId, year, month);

            // Kotayı geçememişse prim yok
            if (totalSales <= employee.SalesQuota.Value)
                return 0;

            // Başarılı satış = Toplam - Kota
            var successfulSales = totalSales - employee.SalesQuota.Value;

            // Prim = Başarılı satış × %10
            var commission = successfulSales * 0.10m;

            return commission;
        }
    }
}
