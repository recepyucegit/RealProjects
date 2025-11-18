using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Customer Repository Interface
    /// Müşteri işlemleri için özel metodlar
    /// </summary>
    public interface ICustomerRepository : IRepository<Customer>
    {
        /// <summary>
        /// TC Kimlik numarasına göre müşteri bulur
        /// NEDEN?
        /// - Gül Satar: "TC Kimlik numarasını girdiğim anda müşteri bilgileri otomatik gelmeli"
        /// - Unique field
        /// </summary>
        Task<Customer> GetByIdentityNumberAsync(string identityNumber);

        /// <summary>
        /// Email adresine göre müşteri bulur
        /// </summary>
        Task<Customer> GetByEmailAsync(string email);

        /// <summary>
        /// Telefon numarasına göre müşteri bulur
        /// </summary>
        Task<Customer> GetByPhoneAsync(string phone);

        /// <summary>
        /// Aktif müşterileri getirir
        /// </summary>
        Task<IReadOnlyList<Customer>> GetActiveCustomersAsync();

        /// <summary>
        /// Yaş aralığına göre müşterileri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Müşteri kitlesi yaş analizi"
        /// - Demografik raporlar için
        /// </summary>
        Task<IReadOnlyList<Customer>> GetByAgeRangeAsync(int minAge, int maxAge);

        /// <summary>
        /// Cinsiyete göre müşterileri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Müşteri kitlesi cinsiyet analizi"
        /// </summary>
        Task<IReadOnlyList<Customer>> GetByGenderAsync(Domain.Enums.Gender gender);

        /// <summary>
        /// Müşterinin toplam alışveriş tutarını hesaplar
        /// Sadakat programı için kullanılabilir
        /// </summary>
        Task<decimal> GetCustomerTotalPurchaseAsync(int customerId);

        /// <summary>
        /// En çok alışveriş yapan müşterileri getirir
        /// VIP müşteri analizi için
        /// </summary>
        Task<IReadOnlyList<Customer>> GetTopCustomersAsync(int count);
    }
}