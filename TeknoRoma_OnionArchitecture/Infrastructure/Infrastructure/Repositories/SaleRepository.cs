using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Sale Repository Implementation
    ///
    /// AMAÇ:
    /// - Satış işlemlerinin tüm iş mantığı
    /// - Haluk Bey'in raporları: Çalışan performansı, mağaza satışları, en çok satan ürünler
    /// - Gül Satar'ın prim hesaplaması
    /// - Kerim Zulacı'nın bekleyen siparişleri
    ///
    /// KARMAŞIKLIK SEVİYESİ: YÜKSEK
    /// - Birden fazla tabloyu join eder (Sale, SaleDetail, Product, Customer, Employee)
    /// - Aggregate fonksiyonlar (Sum, Count, GroupBy)
    /// - Tarih aralığı hesaplamaları
    /// - İş mantığı hesaplamaları (toplam, prim vb.)
    /// </summary>
    public class SaleRepository : Repository<Sale>, ISaleRepository
    {
        public SaleRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// Satış numarasına göre satış bulur
        /// Fahri Cepçi: "Müşteri satış numarası ile sorgulayabilmeli"
        /// </summary>
        public async Task<Sale> GetBySaleNumberAsync(string saleNumber)
        {
            if (string.IsNullOrWhiteSpace(saleNumber))
                throw new ArgumentException("Satış numarası boş olamaz", nameof(saleNumber));

            return await _dbSet
                .Include(s => s.Customer)       // Müşteri bilgisi
                .Include(s => s.Employee)       // Satış yapan çalışan
                .Include(s => s.Store)          // Mağaza bilgisi
                .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber);
        }

        /// <summary>
        /// Satışı tüm detayları ile getirir
        ///
        /// NEDEN BU KADAR INCLUDE?
        /// - Fatura görüntüleme için tüm bilgiler gerekli
        /// - N+1 Query problemini önler
        /// - Tek sorguda tüm veri gelir
        ///
        /// PERFORMANS:
        /// - Lazy Loading yerine Eager Loading
        /// - Database'e tek sorgu
        /// </summary>
        public async Task<Sale> GetWithDetailsAsync(int saleId)
        {
            return await _dbSet
                .Include(s => s.Customer)                // Müşteri
                .Include(s => s.Employee)                // Çalışan
                    .ThenInclude(e => e.Department)      // Çalışanın departmanı
                .Include(s => s.Store)                   // Mağaza
                .Include(s => s.SaleDetails)             // Satır detayları
                    .ThenInclude(sd => sd.Product)       // Her satırdaki ürün
                        .ThenInclude(p => p.Category)    // Ürünün kategorisi
                .FirstOrDefaultAsync(s => s.ID == saleId);
        }

        /// <summary>
        /// Müşterinin tüm satışlarını getirir
        /// Müşteri geçmişi ve sadakat analizi için
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(s => s.Store)
                .Include(s => s.Employee)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.SaleDate) // En yeni satışlar önce
                .ToListAsync();
        }

        /// <summary>
        /// Çalışanın yaptığı satışları getirir
        /// Haluk Bey: "Hangi çalışanım ne kadar satmış?"
        /// Gül Satar: "Prime ne kadar yaklaştım?"
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.Store)
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Mağazanın satışlarını getirir
        /// Mağaza performans analizi için
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Where(s => s.StoreId == storeId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Duruma göre satışları getirir
        /// Kerim Zulacı: "Hazırlanıyor durumundaki satışları görmem lazım"
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByStatusAsync(SaleStatus status)
        {
            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Include(s => s.Store)
                .Where(s => s.Status == status)
                .OrderBy(s => s.SaleDate) // Eski siparişler önce (FIFO)
                .ToListAsync();
        }


        // ====== TARİH BAZLI SORGULAR ======

        /// <summary>
        /// Tarih aralığındaki satışları getirir
        /// Aylık, haftalık, yıllık raporlar için
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden sonra olamaz");

            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Include(s => s.Store)
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Bugünkü satışları getirir
        /// Kısayol metod - Günlük rapor için
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetTodaysSalesAsync()
        {
            var today = DateTime.Today; // Bugünün başlangıcı (00:00:00)
            var tomorrow = today.AddDays(1); // Yarının başlangıcı

            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Include(s => s.Store)
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Bu ayın satışlarını getirir
        /// Kısayol metod - Aylık rapor için
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetThisMonthsSalesAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1); // Ayın ilk günü
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1); // Gelecek ayın ilk günü

            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Include(s => s.Store)
                .Where(s => s.SaleDate >= firstDayOfMonth && s.SaleDate < firstDayOfNextMonth)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }


        // ====== FİNANSAL HESAPLAMALAR ======

        /// <summary>
        /// Çalışanın aylık satış toplamını hesaplar
        /// Gül Satar: "Satış kotasını geçmiş mi, ne kadar prim haketmiş?"
        ///
        /// İŞ KURALI:
        /// - Sadece Tamamlandı durumundaki satışlar sayılır
        /// - İptal edilen satışlar dahil değil
        /// - Beklemede veya Hazırlanıyor satışlar dahil değil
        ///
        /// PRİM HESAPLAMA:
        /// - Satış Kotası: 10.000 TL
        /// - Prim Oranı: %10
        /// - Örnek: Aylık satış = 15.000 TL
        ///   → Başarılı = 15.000 - 10.000 = 5.000 TL
        ///   → Prim = 5.000 × 0.10 = 500 TL
        /// </summary>
        public async Task<decimal> GetEmployeeMonthlySalesTotalAsync(int employeeId, int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var total = await _dbSet
                .Where(s => s.EmployeeId == employeeId
                         && s.SaleDate >= firstDayOfMonth
                         && s.SaleDate < firstDayOfNextMonth
                         && s.Status == SaleStatus.Tamamlandi) // Sadece tamamlanan satışlar
                .SumAsync(s => s.TotalAmount);

            return total;
        }

        /// <summary>
        /// Mağazanın günlük satış toplamını hesaplar
        /// Günlük performans takibi için
        /// </summary>
        public async Task<decimal> GetStoreDailySalesTotalAsync(int storeId, DateTime date)
        {
            var startOfDay = date.Date; // 00:00:00
            var endOfDay = startOfDay.AddDays(1); // Ertesi gün 00:00:00

            var total = await _dbSet
                .Where(s => s.StoreId == storeId
                         && s.SaleDate >= startOfDay
                         && s.SaleDate < endOfDay
                         && s.Status == SaleStatus.Tamamlandi)
                .SumAsync(s => s.TotalAmount);

            return total;
        }

        /// <summary>
        /// Mağazanın aylık satış toplamını hesaplar
        /// Haluk Bey: "Hangi mağazam daha çok satış yapıyor?"
        /// </summary>
        public async Task<decimal> GetStoreMonthlySalesTotalAsync(int storeId, int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var total = await _dbSet
                .Where(s => s.StoreId == storeId
                         && s.SaleDate >= firstDayOfMonth
                         && s.SaleDate < firstDayOfNextMonth
                         && s.Status == SaleStatus.Tamamlandi)
                .SumAsync(s => s.TotalAmount);

            return total;
        }


        // ====== ANALİZ VE RAPORLAMA ======

        /// <summary>
        /// En çok satan ürünleri getirir
        /// Haluk Bey: "En çok satılan 10 ürünü görmek istiyorum"
        ///
        /// NASIL HESAPLANIYOR?
        /// - SaleDetail tablosundan GROUP BY ProductId
        /// - SUM(Quantity) ile toplam satış miktarı
        /// - ORDER BY toplam DESC
        /// - Tarih aralığı opsiyonel
        ///
        /// SQL Eşdeğeri:
        /// SELECT ProductId, SUM(Quantity) as Total
        /// FROM SaleDetails sd
        /// JOIN Sales s ON sd.SaleId = s.ID
        /// WHERE s.Status = Tamamlandi [AND s.SaleDate BETWEEN ...]
        /// GROUP BY ProductId
        /// ORDER BY Total DESC
        /// LIMIT count
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetTopSellingProductsAsync(
            int count,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (count <= 0)
                throw new ArgumentException("Miktar pozitif olmalı", nameof(count));

            var query = _context.SaleDetails
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Category)
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Supplier)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Status == SaleStatus.Tamamlandi); // Sadece tamamlanan satışlar

            // Tarih filtresi varsa
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value
                                       && sd.Sale.SaleDate <= endDate.Value);
            }

            // Ürün bazında gruplama ve toplam satış miktarı
            var topProducts = await query
                .GroupBy(sd => sd.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(sd => sd.Quantity),
                    Product = g.First().Product // Ürün bilgisi
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(count)
                .Select(x => x.Product)
                .ToListAsync();

            return topProducts;
        }

        /// <summary>
        /// Ürünün satış sayısını getirir
        /// Belirli bir tarih aralığında kaç adet satıldı?
        /// </summary>
        public async Task<int> GetProductSalesCountAsync(
            int productId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.SaleDetails
                .Include(sd => sd.Sale)
                .Where(sd => sd.ProductId == productId
                          && sd.Sale.Status == SaleStatus.Tamamlandi);

            // Tarih filtresi varsa
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value
                                       && sd.Sale.SaleDate <= endDate.Value);
            }

            var totalQuantity = await query.SumAsync(sd => sd.Quantity);
            return totalQuantity;
        }


        // ====== NUMARA OLUŞTURMA ======

        /// <summary>
        /// Yeni satış numarası oluşturur
        /// Format: S-2024-00001, S-2024-00002, ...
        ///
        /// NASIL ÇALIŞIR?
        /// 1. Bu yılın en son satış numarasını bul
        /// 2. Sequence kısmını parse et (örn: 00001 → 1)
        /// 3. 1 artır (1 → 2)
        /// 4. Yeni format oluştur (S-2024-00002)
        ///
        /// NEDEN YILLIK RESET?
        /// - Her yıl sıfırdan başlar
        /// - Muhasebe dönemleri için uygun
        /// - Daha kısa numaralar
        /// </summary>
        public async Task<string> GenerateSaleNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            var prefix = $"S-{currentYear}-";

            // Bu yılın son satış numarasını bul
            var lastSale = await _dbSet
                .Where(s => s.SaleNumber.StartsWith(prefix))
                .OrderByDescending(s => s.SaleNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastSale != null)
            {
                // Örnek: "S-2024-00005" → "00005" → 5 → 6
                var lastNumberPart = lastSale.SaleNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Format: S-2024-00001 (5 haneli, sıfır ile doldur)
            return $"{prefix}{nextNumber:D5}";
        }
    }
}
