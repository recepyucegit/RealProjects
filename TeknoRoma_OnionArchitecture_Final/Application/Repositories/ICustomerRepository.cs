// ============================================================================
// ICustomerRepository.cs - Müşteri Repository Interface
// ============================================================================
// AÇIKLAMA:
// Müşteri entity'sine özgü veri erişim metodlarını tanımlar.
// KVKK uyumlu müşteri arama ve sadakat programı için metodlar içerir.
//
// İŞ SENARYOLARI:
// - TC Kimlik ile müşteri bulma (tekil eşleşme)
// - En değerli müşteriler (VIP program)
// - Müşteri arama (ad/soyad ile)
// ============================================================================

using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Müşteri Repository Interface
    ///
    /// KVKK UYUMU: TC Kimlik araması sadece yetkili personele açık olmalı
    /// </summary>
    public interface ICustomerRepository : IRepository<Customer>
    {
        /// <summary>
        /// TC Kimlik ile Müşteri Getir
        ///
        /// Benzersiz tanımlayıcı - tekil sonuç döner
        /// Kayıt sırasında mükerrer kontrolü için de kullanılır
        /// </summary>
        Task<Customer?> GetByIdentityNumberAsync(string identityNumber);

        /// <summary>
        /// Aktif Müşteriler
        ///
        /// Son X ay içinde alışveriş yapmış müşteriler
        /// Pazarlama kampanyaları için
        /// </summary>
        Task<IReadOnlyList<Customer>> GetActiveCustomersAsync();

        /// <summary>
        /// İsme Göre Arama
        ///
        /// Ad veya soyad ile LIKE araması
        /// Kasa işlemlerinde müşteri bulma
        /// </summary>
        Task<IReadOnlyList<Customer>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// En Çok Alışveriş Yapan Müşteriler
        ///
        /// VIP müşteri listesi, sadakat programı
        ///
        /// PARAMETRELER:
        /// - count: Kaç müşteri dönsün (örn: TOP 10)
        /// - startDate/endDate: Belirli dönem (opsiyonel)
        /// </summary>
        Task<IReadOnlyList<Customer>> GetTopCustomersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    }
}
