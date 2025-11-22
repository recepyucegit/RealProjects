// ============================================================================
// IRepository.cs - Generic Repository Interface
// ============================================================================
// AÇIKLAMA:
// Repository Pattern'in temel interface'i. Tüm entity'ler için ortak CRUD
// (Create, Read, Update, Delete) operasyonlarını tanımlar.
//
// REPOSITORY PATTERN NEDİR?
// - Veri erişim mantığını iş mantığından ayırır
// - Veritabanı soyutlaması sağlar
// - Test edilebilirliği artırır (Mock kullanımı)
// - Kod tekrarını önler (DRY - Don't Repeat Yourself)
//
// GENERIC REPOSITORY AVANTAJLARI:
// - T tipi sayesinde her entity için kullanılabilir
// - Ortak metodlar tek yerde tanımlanır
// - Tip güvenliği (Type Safety) sağlar
//
// ONION ARCHITECTURE'DA YERİ:
// - Application katmanında INTERFACE tanımı (burası)
// - Infrastructure katmanında IMPLEMENTATION (EfRepository<T>)
// - Dependency Injection ile bağlantı
//
// CQRS (Command Query Responsibility Segregation):
// - QUERY: Okuma işlemleri (GetById, GetAll)
// - COMMAND: Yazma işlemleri (Add, Update, Delete)
// ============================================================================

using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Repositories
{
    /// <summary>
    /// Generic Repository Interface
    ///
    /// TİP PARAMETRESİ: T
    /// - T, BaseEntity'den türeyen herhangi bir entity olabilir
    /// - "where T : BaseEntity" kısıtı bunu garanti eder
    ///
    /// NEDEN GENERIC?
    /// Her entity için ayrı interface yerine tek interface yeterli:
    /// IRepository&lt;Product&gt;, IRepository&lt;Customer&gt;, IRepository&lt;Sale&gt;
    ///
    /// DEPENDENCY INJECTION KULLANIMI:
    /// <code>
    /// // Startup.cs
    /// services.AddScoped(typeof(IRepository&lt;&gt;), typeof(EfRepository&lt;&gt;));
    ///
    /// // Service sınıfında
    /// public ProductService(IRepository&lt;Product&gt; productRepository)
    /// </code>
    /// </summary>
    /// <typeparam name="T">BaseEntity'den türeyen entity tipi</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        // ====================================================================
        // QUERY (READ) OPERASYONLARI
        // ====================================================================
        // Bu metotlar veriyi okur, değiştirmez.
        // CQRS pattern'de Query tarafını oluşturur.
        // ====================================================================

        /// <summary>
        /// ID'ye Göre Tekil Kayıt Getir
        ///
        /// PARAMETRE: id - Entity'nin primary key'i
        /// DÖNÜŞ: T? - Kayıt veya null
        ///
        /// NULLABLE (T?): Kayıt bulunamazsa null döner
        ///
        /// ÖRNEK:
        /// <code>
        /// var product = await _repository.GetByIdAsync(5);
        /// if (product == null)
        ///     throw new NotFoundException("Ürün bulunamadı");
        /// </code>
        ///
        /// SQL: SELECT * FROM Products WHERE Id = 5
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm Kayıtları Getir
        ///
        /// DÖNÜŞ: IReadOnlyList&lt;T&gt; - Tüm aktif kayıtlar
        ///
        /// NEDEN IReadOnlyList?
        /// - Güvenlik: Liste değiştirilemez
        /// - Niyet belirtme: "Sadece okuma için"
        ///
        /// DİKKAT: Büyük tablolarda sayfalama tercih edilmeli
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Koşula Göre Filtrelenmiş Kayıtları Getir
        ///
        /// PARAMETRE: predicate - Lambda ifadesi (LINQ Where)
        ///
        /// Expression&lt;Func&lt;T, bool&gt;&gt; NEDİR?
        /// - Lambda'nın expression tree'si
        /// - EF Core bunu SQL'e çevirebilir
        ///
        /// ÖRNEK:
        /// <code>
        /// // Aktif ürünler
        /// var active = await _repository.GetAllAsync(p => p.IsActive);
        ///
        /// // Kategoriye göre
        /// var products = await _repository.GetAllAsync(p => p.CategoryId == 5);
        /// </code>
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Koşula Uyan İlk Kaydı Getir
        ///
        /// ÖRNEK:
        /// <code>
        /// var customer = await _repository.GetFirstOrDefaultAsync(
        ///     c => c.Email == "ahmet@example.com");
        /// </code>
        ///
        /// SQL: SELECT TOP 1 * FROM Customers WHERE Email = '...'
        /// </summary>
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Koşula Uyan Kayıt Var mı Kontrolü
        ///
        /// PERFORMANS: COUNT yerine EXISTS kullanır (daha hızlı)
        ///
        /// ÖRNEK:
        /// <code>
        /// var exists = await _repository.ExistsAsync(
        ///     c => c.Email == "ahmet@example.com");
        /// if (exists)
        ///     throw new BusinessException("Email zaten kayıtlı!");
        /// </code>
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Toplam Kayıt Sayısı
        ///
        /// KULLANIM: Sayfalama için gerekli
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// Koşula Göre Kayıt Sayısı
        ///
        /// ÖRNEK:
        /// <code>
        /// var activeCount = await _repository.CountAsync(p => p.IsActive);
        /// </code>
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // ====================================================================
        // COMMAND (WRITE) OPERASYONLARI
        // ====================================================================
        // Bu metotlar veriyi değiştirir.
        // Transaction yönetimi UnitOfWork ile yapılır.
        // ====================================================================

        /// <summary>
        /// Yeni Kayıt Ekle
        ///
        /// DÖNÜŞ: Eklenen entity (Id atanmış halde)
        ///
        /// ÖRNEK:
        /// <code>
        /// var product = new Product { Name = "iPhone 15", UnitPrice = 50000 };
        /// var added = await _repository.AddAsync(product);
        /// // added.Id artık dolu
        /// </code>
        ///
        /// NOT: SaveChanges UnitOfWork'te çağrılmalı!
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Toplu Kayıt Ekleme
        ///
        /// PERFORMANS: Bulk insert kullanır, tek tek eklemekten hızlı
        ///
        /// KULLANIM: Excel import, seed data, batch işlemler
        /// </summary>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Mevcut Kaydı Güncelle
        ///
        /// DİKKAT: Entity'nin Id'si veritabanında var olmalı
        ///
        /// ÖRNEK:
        /// <code>
        /// var product = await _repository.GetByIdAsync(5);
        /// product.UnitPrice = 55000;
        /// await _repository.UpdateAsync(product);
        /// </code>
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Kalıcı Silme (Hard Delete)
        ///
        /// DİKKAT: Kayıt veritabanından TAMAMEN silinir!
        /// ÖNERİ: Genellikle SoftDeleteAsync tercih edilmeli
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Yumuşak Silme (Soft Delete)
        ///
        /// AÇIKLAMA:
        /// - Kayıt silinmiş gibi işaretlenir (IsDeleted = true)
        /// - Veritabanında kalır
        /// - Geri alınabilir
        ///
        /// AVANTAJLAR:
        /// - Veri kaybı önlenir
        /// - Audit trail korunur
        /// - İlişkili kayıtlar bozulmaz
        /// </summary>
        Task SoftDeleteAsync(T entity);
    }

    /// <summary>
    /// Simple Repository Interface
    ///
    /// Basit entity'ler için marker interface.
    /// Özel metod gerektirmeyen entity'ler:
    /// Category, Supplier, Store, Department
    ///
    /// IRepository'deki tüm metodları miras alır,
    /// ekstra metod tanımlamaz.
    /// </summary>
    /// <typeparam name="T">Basit entity tipi</typeparam>
    public interface ISimpleRepository<T> : IRepository<T> where T : BaseEntity
    {
        // IRepository'deki tüm metodları miras alır
        // Marker interface - ek metod yok
    }
}

// ============================================================================
// EK BİLGİLER
// ============================================================================
//
// IMPLEMENTATION (Infrastructure):
// public class EfRepository<T> : IRepository<T> where T : BaseEntity
// {
//     protected readonly AppDbContext _context;
//     protected readonly DbSet<T> _dbSet;
//
//     public async Task<T?> GetByIdAsync(int id)
//         => await _dbSet.FindAsync(id);
//
//     public async Task<IReadOnlyList<T>> GetAllAsync()
//         => await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
// }
//
// TEST (Mock):
// var mockRepo = new Mock<IRepository<Product>>();
// mockRepo.Setup(r => r.GetByIdAsync(1))
//     .ReturnsAsync(new Product { Id = 1, Name = "Test" });
// ============================================================================
