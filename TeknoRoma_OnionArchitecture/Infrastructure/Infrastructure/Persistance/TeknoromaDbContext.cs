using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// TEKNOROMA Database Context
    /// 
    /// NEDEN IdentityDbContext?
    /// - ASP.NET Identity kullanıyoruz (Authentication/Authorization)
    /// - User, Role, UserRole tabloları otomatik oluşturulur
    /// - IdentityDbContext<IdentityUser> yerine IdentityDbContext kullanıyoruz
    /// 
    /// SORUMLULUKLAR:
    /// 1. Database bağlantısını yönet
    /// 2. DbSet'leri tanımla (Her entity için bir tablo)
    /// 3. Fluent API konfigürasyonlarını uygula
    /// 4. SaveChanges'de özel işlemler yap (Audit, Soft Delete)
    /// </summary>
    public class TeknoromaDbContext : IdentityDbContext
    {
        /// <summary>
        /// Constructor - DbContextOptions ile bağlantı bilgisi alır
        /// NEDEN Options Pattern?
        /// - appsettings.json'dan connection string okur
        /// - Farklı ortamlarda farklı database (Development, Production)
        /// - Dependency Injection ile inject edilir
        /// </summary>
        public TeknoromaDbContext(DbContextOptions<TeknoromaDbContext> options) : base(options)
        {
        }

        // ====== DbSets (Database Tabloları) ======
        // Her DbSet bir tabloya karşılık gelir

        /// <summary>
        /// Mağazalar tablosu
        /// </summary>
        public DbSet<Store> Stores { get; set; }

        /// <summary>
        /// Departmanlar tablosu
        /// </summary>
        public DbSet<Department> Departments { get; set; }

        /// <summary>
        /// Çalışanlar tablosu
        /// </summary>
        public DbSet<Employee> Employees { get; set; }

        /// <summary>
        /// Müşteriler tablosu
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Tedarikçiler tablosu
        /// </summary>
        public DbSet<Supplier> Suppliers { get; set; }

        /// <summary>
        /// Kategoriler tablosu
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Ürünler tablosu
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Satışlar tablosu (Başlık)
        /// </summary>
        public DbSet<Sale> Sales { get; set; }

        /// <summary>
        /// Satış Detayları tablosu (Satırlar)
        /// </summary>
        public DbSet<SaleDetail> SaleDetails { get; set; }

        /// <summary>
        /// Tedarikçi Hareketleri tablosu
        /// </summary>
        public DbSet<SupplierTransaction> SupplierTransactions { get; set; }

        /// <summary>
        /// Giderler tablosu
        /// </summary>
        public DbSet<Expense> Expenses { get; set; }

        /// <summary>
        /// Teknik Servis Kayıtları tablosu
        /// </summary>
        public DbSet<TechnicalService> TechnicalServices { get; set; }


        /// <summary>
        /// Model oluşturma - Fluent API konfigürasyonları
        /// NEDEN OnModelCreating?
        /// - Data Annotations yerine Fluent API tercih ediyoruz
        /// - Daha güçlü ve esnek
        /// - Entity'leri temiz tutar
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tablolarını oluştur (User, Role, UserRole...)
            base.OnModelCreating(modelBuilder);

            // ====== Fluent API Configurations ======
            // Her entity için ayrı configuration sınıfı oluşturacağız
            // NEDEN? Separation of Concerns - Her entity'nin konfigürasyonu ayrı dosyada

            // Configuration'ları uygula
            // ApplyConfigurationsFromAssembly: Assembly'deki tüm IEntityTypeConfiguration'ları bulur ve uygular
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeknoromaDbContext).Assembly);

            // ====== Global Query Filters ======
            // NEDEN? Soft Delete için
            // IsDeleted = true olan kayıtları otomatik filtrele

            modelBuilder.Entity<Store>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Sale>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SaleDetail>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SupplierTransaction>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TechnicalService>().HasQueryFilter(e => !e.IsDeleted);

            // NOT: IgnoreQueryFilters() kullanarak bu filtreyi devre dışı bırakabiliriz
            // Örn: Silinmiş kayıtları görmek istiyorsak
        }

        /// <summary>
        /// SaveChanges - Her kayıt işleminde çalışır
        /// NEDEN Override?
        /// - Audit (CreatedDate, ModifiedDate) otomatik güncellemek için
        /// - Soft Delete işlemleri için
        /// - Business rules uygulamak için
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // ChangeTracker: DbContext'te yapılan tüm değişiklikleri takip eder
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // Yeni kayıt ekleniyor
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        // Kayıt güncelleniyor
                        entry.Entity.ModifiedDate = DateTime.Now;
                        break;

                    case EntityState.Deleted:
                        // Fiziksel silme yerine Soft Delete
                        // NEDEN? Veri kaybını önlemek için
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.ModifiedDate = DateTime.Now;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Senkron SaveChanges de override ediyoruz
        /// </summary>
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = DateTime.Now;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.ModifiedDate = DateTime.Now;
                        break;
                }
            }

            return base.SaveChanges();
        }
    }
}