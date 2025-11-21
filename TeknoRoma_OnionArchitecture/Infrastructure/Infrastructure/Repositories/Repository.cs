using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Generic Repository Implementation
    ///
    /// AMAÇ:
    /// - IRepository<T> interface'ini implement eder
    /// - Tüm entity'ler için ortak CRUD operasyonları
    /// - Entity Framework Core ile veritabanı işlemleri
    ///
    /// NEDEN GENERIC?
    /// - Kod tekrarını önler (DRY - Don't Repeat Yourself)
    /// - Her entity için aynı CRUD kodunu yazmaya gerek yok
    /// - Merkezi hata yönetimi
    /// - Kolay test edilebilir (mock'lanabilir)
    ///
    /// NEDEN ASYNC?
    /// - Web uygulamalarında performans kritik
    /// - Thread'leri bloklamaz
    /// - Ölçeklenebilirlik sağlar
    /// - Microsoft best practice
    ///
    /// where T : BaseEntity
    /// - T sadece BaseEntity'den türeyen sınıflar olabilir
    /// - ID, CreatedDate, ModifiedDate, IsDeleted otomatik yönetilir
    /// </summary>
    /// <typeparam name="T">Entity tipi (BaseEntity'den türemelidir)</typeparam>
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        // DbContext - Entity Framework bağlantısı
        protected readonly TeknoromaDbContext _context;
        // DbSet - İlgili entity'nin tablosu
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor - Dependency Injection ile DbContext alır
        /// </summary>
        public Repository(TeknoromaDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            // Set<T>() sayesinde generic olarak entity'nin DbSet'ini alırız
        }


        // ====== CREATE OPERATIONS ======

        /// <summary>
        /// Yeni entity ekler
        /// NEDEN ASYNC: Database I/O işlemi - thread'i bloklamaz
        /// </summary>
        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // CreatedDate otomatik set edilir (DbContext.SaveChanges'de)
            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// Birden fazla entity ekler
        /// NEDEN RANGE: Tek seferde çok kayıt eklemek için performans optimizasyonu
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentNullException(nameof(entities));

            await _dbSet.AddRangeAsync(entities);
        }


        // ====== READ OPERATIONS ======

        /// <summary>
        /// ID'ye göre entity getirir
        /// NEDEN FIRSTORDEFAULT: ID yoksa null döner (exception atmaz)
        /// </summary>
        public async Task<T?> GetByIdAsync(int id)
        {
            // Global Query Filter otomatik uygulanır (IsDeleted = false)
            return await _dbSet.FirstOrDefaultAsync(e => e.ID == id);
        }

        /// <summary>
        /// Tüm entity'leri getirir
        /// DİKKAT: IsDeleted = false olanlar (Soft Delete)
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Global Query Filter sayesinde silinmişler gelmez
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Şarta göre entity'leri getirir
        /// Expression<Func<T, bool>> = Lambda expression (e => e.Name == "Test")
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// ID'ye göre entity getirir + İlişkili tablolar (Include)
        /// NEDEN INCLUDE: Navigation property'leri yükler (Eager Loading)
        /// Örnek: GetByIdWithIncludesAsync(1, p => p.Category, p => p.Supplier)
        /// </summary>
        public async Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            // Her include için Eager Loading
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => e.ID == id);
        }

        /// <summary>
        /// Şarta göre ilk kaydı getirir
        /// NEDEN FIRSTORDEFAULT: Bulamazsa null döner
        /// </summary>
        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Şarta uyan kayıt var mı kontrol eder
        /// NEDEN ANYASYNC: Count'tan daha performanslı (ilk bulduğunda durur)
        /// </summary>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.AnyAsync(predicate);
        }

        /// <summary>
        /// Toplam kayıt sayısı
        /// DİKKAT: IsDeleted = false olanlar
        /// </summary>
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        /// <summary>
        /// Şarta göre kayıt sayısı
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.CountAsync(predicate);
        }


        // ====== UPDATE OPERATIONS ======

        /// <summary>
        /// Entity günceller
        /// DİKKAT: SaveChangesAsync çağrılmalı!
        /// ModifiedDate otomatik güncellenir (DbContext.SaveChanges'de)
        /// </summary>
        public Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }

        /// <summary>
        /// Birden fazla entity günceller
        /// </summary>
        public Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentNullException(nameof(entities));

            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }


        // ====== DELETE OPERATIONS ======

        /// <summary>
        /// Entity'yi fiziksel olarak siler (Hard Delete)
        /// DİKKAT: Genellikle kullanılmaz! SoftDeleteAsync kullanılmalı
        /// NEDEN HARD DELETE RİSKLİ?
        /// - Veri kaybı
        /// - Foreign Key hataları
        /// - Audit trail kaybolur
        /// </summary>
        public Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Mantıksal silme (Soft Delete)
        /// IsDeleted = true yapılır, fiziksel olarak silinmez
        ///
        /// NEDEN SOFT DELETE?
        /// - Veri kaybı önlenir
        /// - Audit trail korunur
        /// - Geri alma (undelete) mümkün
        /// - Foreign Key sorunları olmaz
        /// - Yasal gereksinimler (kayıtlar korunmalı)
        ///
        /// HALUK BEY'İN TALEBİ:
        /// - "Hiçbir veri silinmemeli, sadece pasif edilmeli"
        /// </summary>
        public Task SoftDeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.IsDeleted = true;
            entity.ModifiedDate = DateTime.Now;
            _dbSet.Update(entity);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Birden fazla entity'yi fiziksel siler
        /// </summary>
        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
    }
}
