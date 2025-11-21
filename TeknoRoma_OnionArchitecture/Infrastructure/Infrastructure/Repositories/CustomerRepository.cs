using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Customer Repository Implementation
    ///
    /// AMAÇ:
    /// - Müşteri bilgilerini yönetir
    /// - TC Kimlik ile hızlı arama (Gül Satar için)
    /// - Demografik analizler (Haluk Bey raporları)
    /// - VIP müşteri analizi
    ///
    /// ÖNEMLİ İŞ KURALLARI:
    /// - TC Kimlik UNIQUE olmalı
    /// - Yaş hesaplaması: DateTime.Now - BirthDate
    /// - Toplam alışveriş: Tamamlanan satışların toplamı
    /// </summary>
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// TC Kimlik numarasına göre müşteri bulur
        /// Gül Satar: "TC Kimlik girdiğimde müşteri bilgileri otomatik gelmeli"
        ///
        /// NEDEN UNIQUE?
        /// - Her kişinin TC Kimliği benzersizdir
        /// - Çift kayıt önlenir
        /// - Hızlı erişim (index sayesinde)
        /// </summary>
        public async Task<Customer> GetByIdentityNumberAsync(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                throw new ArgumentException("TC Kimlik numarası boş olamaz", nameof(identityNumber));

            // TC Kimlik 11 haneli olmalı
            if (identityNumber.Length != 11)
                throw new ArgumentException("TC Kimlik numarası 11 haneli olmalı", nameof(identityNumber));

            return await _dbSet
                .FirstOrDefaultAsync(c => c.IdentityNumber == identityNumber);
        }

        /// <summary>
        /// Email adresine göre müşteri bulur
        /// </summary>
        public async Task<Customer> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email boş olamaz", nameof(email));

            return await _dbSet
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        /// <summary>
        /// Telefon numarasına göre müşteri bulur
        /// </summary>
        public async Task<Customer> GetByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Telefon boş olamaz", nameof(phone));

            return await _dbSet
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        /// <summary>
        /// Aktif müşterileri getirir
        /// IsActive = true olanlar
        /// </summary>
        public async Task<IReadOnlyList<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }


        // ====== DEMOGRAFİK ANALİZLER ======

        /// <summary>
        /// Yaş aralığına göre müşterileri getirir
        /// Haluk Bey: "18-25 yaş arası müşterilerimiz kimler?"
        ///
        /// NASIL HESAPLANIYOR?
        /// - Yaş = Şu an - Doğum tarihi
        /// - DateTime.Now.Year - BirthDate.Year
        ///
        /// NEDEN NULLABLE KONTROLÜ?
        /// - BirthDate nullable (müşteri vermeyebilir)
        /// - Null olanlar filtrelenmeli
        /// </summary>
        public async Task<IReadOnlyList<Customer>> GetByAgeRangeAsync(int minAge, int maxAge)
        {
            if (minAge < 0 || maxAge < 0)
                throw new ArgumentException("Yaş negatif olamaz");

            if (minAge > maxAge)
                throw new ArgumentException("Minimum yaş maksimum yaştan büyük olamaz");

            var today = DateTime.Today;

            // Yaş hesaplaması için tarih aralığı
            // Örnek: 18 yaşındakiler = Bugünden 18 yıl önce doğanlar
            var maxBirthDate = today.AddYears(-minAge); // 18 yaş için: 2006
            var minBirthDate = today.AddYears(-maxAge - 1); // 25 yaş için: 1999

            return await _dbSet
                .Where(c => c.BirthDate != null // Doğum tarihi olan müşteriler
                         && c.BirthDate <= maxBirthDate
                         && c.BirthDate >= minBirthDate)
                .OrderBy(c => c.BirthDate)
                .ToListAsync();
        }

        /// <summary>
        /// Cinsiyete göre müşterileri getirir
        /// Haluk Bey: "Erkek müşterilerin oranı nedir?"
        /// </summary>
        public async Task<IReadOnlyList<Customer>> GetByGenderAsync(Gender gender)
        {
            return await _dbSet
                .Where(c => c.Gender == gender)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }


        // ====== FİNANSAL ANALİZLER ======

        /// <summary>
        /// Müşterinin toplam alışveriş tutarını hesaplar
        ///
        /// NASIL HESAPLANIYOR?
        /// - Sales tablosundan CustomerID'ye göre filtrele
        /// - Sadece Tamamlanan satışlar (Status = Tamamlandi)
        /// - TotalAmount toplamı
        ///
        /// NEDEN SADECE TAMAMLANAN?
        /// - Beklemede veya İptal edilen satışlar dahil değil
        /// - Sadece gerçekleşen satışlar
        ///
        /// KULLANIM ALANLARI:
        /// - VIP müşteri belirleme
        /// - Sadakat programı puanları
        /// - İndirim oranı belirleme
        /// </summary>
        public async Task<decimal> GetCustomerTotalPurchaseAsync(int customerId)
        {
            var total = await _context.Sales
                .Where(s => s.CustomerId == customerId
                         && s.Status == SaleStatus.Tamamlandi)
                .SumAsync(s => s.TotalAmount);

            return total;
        }

        /// <summary>
        /// En çok alışveriş yapan müşterileri getirir
        /// VIP müşteri analizi
        ///
        /// NASIL HESAPLANIYOR?
        /// - Her müşteri için toplam satış hesapla
        /// - Büyükten küçüğe sırala
        /// - İlk N müşteriyi getir
        ///
        /// SQL Eşdeğeri:
        /// SELECT c.*, SUM(s.TotalAmount) as TotalPurchase
        /// FROM Customers c
        /// LEFT JOIN Sales s ON c.ID = s.CustomerId
        /// WHERE s.Status = Tamamlandi
        /// GROUP BY c.ID
        /// ORDER BY TotalPurchase DESC
        /// LIMIT count
        ///
        /// HALUK BEY'İN KULLANIMI:
        /// - "En çok alışveriş yapan 10 müşteri kimler?"
        /// - "VIP müşterilere özel kampanya yapalım"
        /// </summary>
        public async Task<IReadOnlyList<Customer>> GetTopCustomersAsync(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Miktar pozitif olmalı", nameof(count));

            // Müşterileri toplam satış tutarına göre sırala
            var topCustomers = await _dbSet
                .Include(c => c.Sales) // Satışları dahil et
                .Where(c => c.Sales.Any(s => s.Status == SaleStatus.Tamamlandi)) // En az bir tamamlanmış satışı olan
                .Select(c => new
                {
                    Customer = c,
                    TotalPurchase = c.Sales
                        .Where(s => s.Status == SaleStatus.Tamamlandi)
                        .Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.TotalPurchase)
                .Take(count)
                .Select(x => x.Customer)
                .ToListAsync();

            return topCustomers;
        }
    }
}
