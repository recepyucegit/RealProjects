// ============================================================================
// IEmployeeRepository.cs - Çalışan Repository Interface
// ============================================================================
// AÇIKLAMA:
// Çalışan entity'sine özgü veri erişim metodlarını tanımlar.
// ASP.NET Identity entegrasyonu ve performans değerlendirme için metodlar.
//
// İŞ SENARYOLARI:
// - Login sonrası Employee bilgisi çekme (IdentityUserId ile)
// - Mağaza/departman bazlı personel listesi
// - En çok satan personel (prim hesaplama)
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Çalışan Repository Interface
    ///
    /// IDENTITY ENTEGRASYONU:
    /// IdentityUser.Id ile Employee eşleştirmesi yapılır
    /// </summary>
    public interface IEmployeeRepository : IRepository<Employee>
    {
        /// <summary>
        /// TC Kimlik ile Çalışan Getir
        /// Personel kaydı kontrolü, bordro işlemleri
        /// </summary>
        Task<Employee?> GetByIdentityNumberAsync(string identityNumber);

        /// <summary>
        /// Identity UserId ile Çalışan Getir
        ///
        /// KRİTİK: Login sonrası kullanıcı bilgisi çekmek için
        /// User.FindFirstValue(ClaimTypes.NameIdentifier) -> IdentityUserId
        /// </summary>
        Task<Employee?> GetByIdentityUserIdAsync(string identityUserId);

        /// <summary>
        /// Mağaza Çalışanları
        /// Şube müdürü kendi personelini görür
        /// </summary>
        Task<IReadOnlyList<Employee>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Departman Çalışanları
        /// Departman bazlı organizasyon şeması
        /// </summary>
        Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId);

        /// <summary>
        /// Role Göre Çalışanlar
        /// Yetki bazlı listeleme (tüm kasiyerler, tüm teknisyenler)
        /// </summary>
        Task<IReadOnlyList<Employee>> GetByRoleAsync(UserRole role);

        /// <summary>
        /// Aktif Çalışanlar
        /// İşten çıkmamış personeller
        /// </summary>
        Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync();

        /// <summary>
        /// En Çok Satan Personel
        ///
        /// Prim hesaplama, performans değerlendirme
        /// Satış tutarı toplamına göre sıralı
        /// </summary>
        Task<IReadOnlyList<Employee>> GetTopSellersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    }
}
