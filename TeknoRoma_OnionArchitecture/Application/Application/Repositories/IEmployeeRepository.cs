using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Employee Repository Interface
    /// Çalışan işlemleri için özel metodlar
    /// </summary>
    public interface IEmployeeRepository : IRepository<Employee>
    {
        /// <summary>
        /// TC Kimlik numarasına göre çalışan bulur
        /// NEDEN?
        /// - Unique field
        /// - Çalışan araması için
        /// </summary>
        Task<Employee> GetByIdentityNumberAsync(string identityNumber);

        /// <summary>
        /// Email adresine göre çalışan bulur
        /// NEDEN?
        /// - Identity User ile eşleştirme için
        /// - Login işlemlerinde
        /// </summary>
        Task<Employee> GetByEmailAsync(string email);

        /// <summary>
        /// Identity User ID'ye göre çalışan bulur
        /// NEDEN?
        /// - ASP.NET Identity entegrasyonu için
        /// - Login sonrası kullanıcının Employee kaydını bulmak için
        /// </summary>
        Task<Employee> GetByIdentityUserIdAsync(string identityUserId);

        /// <summary>
        /// Role göre çalışanları getirir
        /// NEDEN?
        /// - Haluk Bey: "Sadece şube müdürleri görebilsin"
        /// - Role-based filtering için
        /// </summary>
        Task<IReadOnlyList<Employee>> GetByRoleAsync(UserRole role);

        /// <summary>
        /// Mağazaya göre çalışanları getirir
        /// </summary>
        Task<IReadOnlyList<Employee>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Departmana göre çalışanları getirir
        /// </summary>
        Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId);

        /// <summary>
        /// Aktif çalışanları getirir
        /// İşten ayrılanları hariç tutar
        /// </summary>
        Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync();

        /// <summary>
        /// Satış kotası olan çalışanları getirir
        /// NEDEN?
        /// - Sadece satış ekibinin kotası var
        /// - Prim hesaplaması için
        /// </summary>
        Task<IReadOnlyList<Employee>> GetEmployeesWithSalesQuotaAsync();

        /// <summary>
        /// Çalışanın aylık satış performansını hesaplar
        /// NEDEN?
        /// - Gül Satar: "Prime ne kadar yaklaştık görmek istiyoruz"
        /// - Haluk Bey: "Satış kotasını geçmiş mi, ne kadar prim haketmiş"
        /// </summary>
        /// <returns>Toplam satış tutarı</returns>
        Task<decimal> GetEmployeeSalesPerformanceAsync(int employeeId, int year, int month);

        /// <summary>
        /// Çalışanın prim tutarını hesaplar
        /// İŞ KURALI: Satış kotası 10.000 TL, üzerindeki satıştan %10 prim
        /// </summary>
        Task<decimal> CalculateEmployeeCommissionAsync(int employeeId, int year, int month);
    }
}