// ===================================================================================
// TEKNOROMA - ANA VERİTABANI BAĞLAM SINIFI (AppDbContext.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// -----------------
// Bu dosya, Entity Framework Core'un kalbi olan DbContext sınıfını içerir.
// Uygulama ile veritabanı arasındaki tüm iletişim bu sınıf üzerinden yapılır.
//
// DbContext NEDİR?
// ----------------
// DbContext, Entity Framework Core'un ana sınıfıdır ve şu görevleri üstlenir:
//
// 1. VERİTABANI BAĞLANTISI
//    - SQL Server'a bağlanır
//    - Connection string'i yönetir
//    - Connection pooling sağlar
//
// 2. ENTITY-TABLO EŞLEMESİ (ORM)
//    - C# sınıflarını SQL tablolarına eşler
//    - Property'leri sütunlara çevirir
//    - İlişkileri foreign key'lere dönüştürür
//
// 3. DEĞİŞİKLİK TAKİBİ (Change Tracking)
//    - Entity'lerdeki değişiklikleri izler
//    - Added, Modified, Deleted durumlarını tutar
//    - SaveChanges'da uygun SQL komutları oluşturur
//
// 4. SORGULAMA (Querying)
//    - LINQ sorgularını SQL'e çevirir
//    - Include ile ilişkili verileri getirir
//    - Lazy/Eager loading yönetir
//
// 5. İŞLEM YÖNETİMİ (Transaction)
//    - SaveChanges otomatik transaction kullanır
//    - Manuel transaction desteği sağlar
//
// ONION ARCHITECTURE'DA YERİ
// --------------------------
// Infrastructure/Persistence klasöründe bulunur.
// - Domain katmanındaki Entity'leri kullanır
// - Application katmanındaki Interface'leri implement eder
// - Dış katman olduğu için EF Core bağımlılığı burada
//
// KULLANIM ÖRNEĞİ
// ---------------
// var products = await _context.Products
//     .Where(p => p.IsActive && !p.IsDeleted)
//     .Include(p => p.Category)
//     .ToListAsync();
//
// await _context.SaveChangesAsync();
//
// ===================================================================================

using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// TeknoRoma Uygulama Veritabanı Bağlam Sınıfı
    ///
    /// MİRAS (INHERITANCE):
    /// DbContext'ten miras alarak EF Core özelliklerini kazanır.
    /// DbContext soyut değildir ama genellikle özelleştirilerek kullanılır.
    ///
    /// YAŞAM DÖNGÜSÜ (Lifetime):
    /// - Scoped olarak DI'a kayıt edilir
    /// - Her HTTP request için yeni instance oluşur
    /// - Request sonunda dispose edilir
    /// - Thread-safe DEĞİLDİR (her thread kendi instance'ını kullanmalı)
    ///
    /// REGISTRATION (Program.cs):
    /// builder.Services.AddDbContext&lt;AppDbContext&gt;(options =>
    ///     options.UseSqlServer(connectionString));
    /// </summary>
    public class AppDbContext : DbContext
    {
        // =====================================================================
        // CONSTRUCTOR - YAPILANDIRICI METOT
        // =====================================================================

        /// <summary>
        /// AppDbContext Constructor
        ///
        /// DEPENDENCY INJECTION:
        /// - DbContextOptions DI container tarafından sağlanır
        /// - Connection string ve diğer ayarları içerir
        ///
        /// BASE CONSTRUCTOR:
        /// - ": base(options)" ile üst sınıfa options geçirilir
        /// - DbContext'in düzgün çalışması için gerekli
        ///
        /// PARAMETRE AÇIKLAMASI:
        /// DbContextOptions&lt;AppDbContext&gt;: Generic tip, bu context'e özel ayarlar
        /// - ConnectionString
        /// - Retry policy
        /// - Logging ayarları
        /// - Timeout değerleri
        ///
        /// KULLANIM:
        /// DI ile otomatik inject edilir:
        /// public class ProductRepository
        /// {
        ///     private readonly AppDbContext _context;
        ///     public ProductRepository(AppDbContext context) => _context = context;
        /// }
        /// </summary>
        /// <param name="options">Veritabanı yapılandırma seçenekleri</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Constructor body boş - tüm initialization base class'ta yapılır
            // İhtiyaç durumunda burada ek initialization yapılabilir
        }

        // =====================================================================
        // DbSet TANIMLARI - VERİTABANI TABLOLARI
        // =====================================================================
        //
        // DbSet<T> NEDİR?
        // ---------------
        // - Veritabanındaki bir tabloyu temsil eder
        // - T: Tabloya karşılık gelen Entity sınıfı
        // - CRUD işlemleri için entry point
        // - LINQ sorgularının başlangıç noktası
        //
        // NAMING CONVENTION:
        // - Property adı çoğul (Products, Categories)
        // - Entity adı tekil (Product, Category)
        // - Tablo adı varsayılan olarak DbSet adını alır
        //
        // ÖRNEK SORGULAR:
        // - _context.Products.ToList()           → SELECT * FROM Products
        // - _context.Products.Find(1)            → SELECT * FROM Products WHERE Id = 1
        // - _context.Products.Add(product)       → INSERT INTO Products ...
        // - _context.Products.Remove(product)    → DELETE FROM Products WHERE ...
        //
        // =====================================================================

        #region ÜRÜN YÖNETİMİ TABLOLARI

        /// <summary>
        /// Ürünler Tablosu (Products)
        ///
        /// AÇIKLAMA:
        /// TeknoRoma'da satılan tüm elektronik ürünler.
        /// iPhone, Samsung, Laptop, TV vb.
        ///
        /// İLİŞKİLER:
        /// - Product (N) → Category (1): Her ürün bir kategoride
        /// - Product (N) → Supplier (1): Her ürün bir tedarikçiden
        /// - Product (1) → SaleDetail (N): Ürün satış detaylarında
        /// - Product (1) → SupplierTransaction (N): Tedarik işlemlerinde
        ///
        /// ÖRNEK SORGULAR:
        /// // Aktif ürünleri getir
        /// var activeProducts = await _context.Products
        ///     .Where(p => p.IsActive && !p.IsDeleted)
        ///     .ToListAsync();
        ///
        /// // Kategorisiyle birlikte getir
        /// var productsWithCategory = await _context.Products
        ///     .Include(p => p.Category)
        ///     .ToListAsync();
        ///
        /// // Stok durumuna göre filtrele
        /// var lowStock = await _context.Products
        ///     .Where(p => p.UnitsInStock <= p.CriticalStockLevel)
        ///     .ToListAsync();
        /// </summary>
        public DbSet<Product> Products { get; set; } = null!;

        /// <summary>
        /// Kategoriler Tablosu (Categories)
        ///
        /// AÇIKLAMA:
        /// Ürün kategorileri: Telefon, Bilgisayar, TV, Beyaz Eşya vb.
        /// Ürün organizasyonu ve filtreleme için kullanılır.
        ///
        /// İLİŞKİLER:
        /// - Category (1) → Product (N): Bir kategoride birçok ürün
        ///
        /// ÖRNEK:
        /// var phoneCategory = await _context.Categories
        ///     .Include(c => c.Products)
        ///     .FirstOrDefaultAsync(c => c.Name == "Telefonlar");
        /// </summary>
        public DbSet<Category> Categories { get; set; } = null!;

        /// <summary>
        /// Tedarikçiler Tablosu (Suppliers)
        ///
        /// AÇIKLAMA:
        /// TeknoRoma'ya ürün sağlayan firmalar.
        /// Distribütörler, üreticiler, toptancılar.
        ///
        /// İLİŞKİLER:
        /// - Supplier (1) → Product (N): Bir tedarikçiden birçok ürün
        /// - Supplier (1) → SupplierTransaction (N): Tedarik işlemleri
        /// </summary>
        public DbSet<Supplier> Suppliers { get; set; } = null!;

        #endregion

        #region SATIŞ YÖNETİMİ TABLOLARI

        /// <summary>
        /// Satışlar Tablosu (Sales) - MASTER
        ///
        /// AÇIKLAMA:
        /// Yapılan satışların başlık bilgileri.
        /// Her kayıt bir fatura/fiş'e karşılık gelir.
        ///
        /// MASTER-DETAIL PATTERN:
        /// - Sale (Master): Fatura başlığı
        /// - SaleDetail (Detail): Fatura kalemleri
        ///
        /// İLİŞKİLER:
        /// - Sale (N) → Customer (1): Hangi müşteriye satıldı
        /// - Sale (N) → Employee (1): Kim sattı
        /// - Sale (N) → Store (1): Hangi mağazada
        /// - Sale (1) → SaleDetail (N): Satış kalemleri
        ///
        /// ÖRNEK:
        /// var todaySales = await _context.Sales
        ///     .Include(s => s.SaleDetails)
        ///         .ThenInclude(d => d.Product)
        ///     .Where(s => s.SaleDate.Date == DateTime.Today)
        ///     .ToListAsync();
        /// </summary>
        public DbSet<Sale> Sales { get; set; } = null!;

        /// <summary>
        /// Satış Detayları Tablosu (SaleDetails) - DETAIL
        ///
        /// AÇIKLAMA:
        /// Satış kalemlerini içerir.
        /// Bir satışta hangi ürünlerden kaç adet satıldığı bilgisi.
        ///
        /// İLİŞKİLER:
        /// - SaleDetail (N) → Sale (1): Hangi satışa ait
        /// - SaleDetail (N) → Product (1): Hangi ürün satıldı
        ///
        /// ÖRNEK:
        /// var bestSellers = await _context.SaleDetails
        ///     .GroupBy(d => d.ProductId)
        ///     .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(d => d.Quantity) })
        ///     .OrderByDescending(x => x.TotalQty)
        ///     .Take(10)
        ///     .ToListAsync();
        /// </summary>
        public DbSet<SaleDetail> SaleDetails { get; set; } = null!;

        #endregion

        #region MÜŞTERİ YÖNETİMİ TABLOLARI

        /// <summary>
        /// Müşteriler Tablosu (Customers)
        ///
        /// AÇIKLAMA:
        /// Mağazadan alışveriş yapan müşteriler.
        /// Sadakat programı ve pazarlama için kullanılır.
        ///
        /// İLİŞKİLER:
        /// - Customer (1) → Sale (N): Müşterinin satışları
        ///
        /// KVKK NOTU:
        /// Kişisel veriler korunmalı. Silme talebi için soft delete.
        /// </summary>
        public DbSet<Customer> Customers { get; set; } = null!;

        #endregion

        #region PERSONEL YÖNETİMİ TABLOLARI

        /// <summary>
        /// Çalışanlar Tablosu (Employees)
        ///
        /// AÇIKLAMA:
        /// TeknoRoma personeli: Satışçılar, teknisyenler, yöneticiler.
        /// ASP.NET Identity ile entegre çalışır.
        ///
        /// İLİŞKİLER:
        /// - Employee (N) → Store (1): Hangi mağazada çalışıyor
        /// - Employee (N) → Department (1): Hangi departmanda
        /// - Employee (1) → Sale (N): Yaptığı satışlar
        /// - Employee (1) → Expense (N): Kaydettiği giderler
        /// - Employee (1) → TechnicalService (N): Atandığı servisler
        /// </summary>
        public DbSet<Employee> Employees { get; set; } = null!;

        /// <summary>
        /// Departmanlar Tablosu (Departments)
        ///
        /// AÇIKLAMA:
        /// Şirket departmanları: Satış, Depo, Muhasebe, Teknik Servis vb.
        /// Çalışan organizasyonu için kullanılır.
        /// </summary>
        public DbSet<Department> Departments { get; set; } = null!;

        #endregion

        #region MAĞAZA YÖNETİMİ TABLOLARI

        /// <summary>
        /// Mağazalar Tablosu (Stores)
        ///
        /// AÇIKLAMA:
        /// TeknoRoma'nın fiziksel mağazaları.
        /// Şubeler ve satış noktaları.
        ///
        /// İLİŞKİLER:
        /// - Store (1) → Employee (N): Mağaza çalışanları
        /// - Store (1) → Sale (N): Mağazadaki satışlar
        /// </summary>
        public DbSet<Store> Stores { get; set; } = null!;

        #endregion

        #region FİNANS YÖNETİMİ TABLOLARI

        /// <summary>
        /// Giderler Tablosu (Expenses)
        ///
        /// AÇIKLAMA:
        /// Şirket giderleri: Kira, fatura, maaş, malzeme vb.
        /// Finansal raporlama için kullanılır.
        ///
        /// İLİŞKİLER:
        /// - Expense (N) → Employee (1): Kim kaydetmiş
        /// </summary>
        public DbSet<Expense> Expenses { get; set; } = null!;

        /// <summary>
        /// Tedarikçi İşlemleri Tablosu (SupplierTransactions)
        ///
        /// AÇIKLAMA:
        /// Tedarikçilerden yapılan alımlar ve ödemeler.
        /// Stok girişi ve borç/alacak takibi.
        ///
        /// İLİŞKİLER:
        /// - SupplierTransaction (N) → Supplier (1)
        /// - SupplierTransaction (N) → Product (1)
        /// </summary>
        public DbSet<SupplierTransaction> SupplierTransactions { get; set; } = null!;

        #endregion

        #region TEKNİK SERVİS TABLOLARI

        /// <summary>
        /// Teknik Servis Kayıtları Tablosu (TechnicalServices)
        ///
        /// AÇIKLAMA:
        /// Cihaz tamir ve bakım kayıtları.
        /// Müşteri cihazlarının servis takibi.
        ///
        /// İLİŞKİLER:
        /// - TechnicalService (N) → Employee (1): Atanan teknisyen
        /// </summary>
        public DbSet<TechnicalService> TechnicalServices { get; set; } = null!;

        #endregion

        // =====================================================================
        // OnModelCreating - MODEL YAPILANDIRMASI
        // =====================================================================

        /// <summary>
        /// Model Oluşturma Yapılandırması
        ///
        /// BU METOT NE ZAMAN ÇAĞRILIR?
        /// - DbContext ilk oluşturulduğunda
        /// - Migration oluşturulduğunda
        /// - Veritabanı şeması güncellendiğinde
        ///
        /// NE İÇİN KULLANILIR?
        /// 1. FLUENT API ile entity konfigürasyonu
        /// 2. İlişki tanımları (relationships)
        /// 3. İndeks tanımları
        /// 4. Seed data (başlangıç verileri)
        /// 5. Global query filters
        ///
        /// DATA ANNOTATIONS vs FLUENT API:
        /// - Data Annotations: Entity sınıfında [Attribute] ile
        /// - Fluent API: Burada kod ile (daha güçlü, tercih edilen)
        ///
        /// OVERRIDE KEYWORD:
        /// - Base class'taki virtual metodu eziyoruz
        /// - DbContext.OnModelCreating'i özelleştiriyoruz
        ///
        /// PROTECTED KEYWORD:
        /// - Sadece bu sınıf ve türeyen sınıflar erişebilir
        /// - Dışarıdan çağrılamaz
        /// </summary>
        /// <param name="modelBuilder">Model oluşturucu nesne</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // =================================================================
            // BASE CLASS CAGIRMA
            // =================================================================
            // ASP.NET Identity tablolari icin onemli.
            // IdentityDbContext kullaniliyorsa kesinlikle cagrilmali.
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // KONFIGURASYON DOSYALARINI OTOMATIK YUKLE
            // =================================================================
            // ApplyConfigurationsFromAssembly metodu, bu assembly icindeki
            // tum IEntityTypeConfiguration<T> implementasyonlarini bulur
            // ve otomatik olarak uygular.
            //
            // AVANTAJLARI:
            // 1. Her entity icin ayri konfigurasyon dosyasi
            // 2. DbContext temiz ve okunabilir kalir
            // 3. Yeni entity eklediginde otomatik algılanır
            // 4. Takım calismasinda conflict azalir
            //
            // KONFIGURASYON DOSYALARI:
            // Infrastructure/Persistence/Configurations/ klasorunde
            // - ProductConfiguration.cs
            // - CategoryConfiguration.cs
            // - SupplierConfiguration.cs
            // - SaleConfiguration.cs
            // - SaleDetailConfiguration.cs
            // - CustomerConfiguration.cs
            // - EmployeeConfiguration.cs
            // - StoreConfiguration.cs
            // - DepartmentConfiguration.cs
            // - ExpenseConfiguration.cs
            // - SupplierTransactionConfiguration.cs
            // - TechnicalServiceConfiguration.cs
            // =================================================================
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // =================================================================
            // SEED DATA - BAŞLANGIÇ VERİLERİ
            // =================================================================
            // ADIM 1: Category (10 kayıt - bağımlılık yok)
            // Tek tek test ederek ilerleyeceğiz
            modelBuilder.Entity<Category>().HasData(TeknoRomaSeedData.GetCategories());

            // TODO: Diğer seed datalar sırasıyla eklenecek
            // Adım 2: Store, Supplier, Customer (bağımlılık yok)
            // Adım 3: Department, Employee (Store'a bağımlı)
            // Adım 4: Product (Category ve Supplier'a bağımlı)
            // Adım 5: İşlemsel veriler (Sale, SaleDetail, Expense, vb.)
            // =================================================================
        }

        // =====================================================================
        // SaveChanges OVERRIDE - OTOMATİK AUDIT ALANLARI
        // =====================================================================

        /// <summary>
        /// Değişiklikleri Kaydet (Senkron)
        ///
        /// OVERRIDE SEBEBİ:
        /// - CreatedDate ve ModifiedDate alanlarını otomatik set etmek
        /// - Audit trail (iz takibi) sağlamak
        /// - Validation yapmak (opsiyonel)
        ///
        /// NE ZAMAN ÇAĞRILIR?
        /// - _context.SaveChanges() her çağrıldığında
        /// - Unit of Work pattern'de Commit() metodunda
        ///
        /// ÇALIŞMA MANTIGI:
        /// 1. ChangeTracker'dan değişen entity'leri al
        /// 2. Added olanlar için CreatedDate set et
        /// 3. Modified olanlar için ModifiedDate set et
        /// 4. Base SaveChanges'ı çağır (veritabanına yaz)
        /// </summary>
        /// <returns>Etkilenen kayıt sayısı</returns>
        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Değişiklikleri Kaydet (Asenkron)
        ///
        /// ASYNC VERSIYON:
        /// - I/O bound işlemler için tercih edilir
        /// - Thread'i bloke etmez
        /// - await _context.SaveChangesAsync() şeklinde kullanılır
        ///
        /// CancellationToken:
        /// - İşlemin iptal edilebilmesini sağlar
        /// - Timeout veya kullanıcı iptali durumunda
        /// </summary>
        /// <param name="cancellationToken">İptal token'ı</param>
        /// <returns>Etkilenen kayıt sayısı</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Audit Alanlarını Otomatik Doldur
        ///
        /// PRIVATE METHOD:
        /// - Sadece bu sınıf içinden çağrılabilir
        /// - SaveChanges ve SaveChangesAsync tarafından kullanılır
        /// - DRY prensibi - kod tekrarını önler
        ///
        /// CHANGE TRACKER:
        /// - EF Core'un entity değişikliklerini izlediği mekanizma
        /// - EntityState: Added, Modified, Deleted, Unchanged, Detached
        /// - Entries(): Tüm izlenen entity'leri döner
        ///
        /// OfType&lt;BaseEntity&gt;():
        /// - Sadece BaseEntity'den türeyen entity'leri filtreler
        /// - CreatedDate/ModifiedDate bu base class'ta tanımlı
        /// </summary>
        private void SetAuditFields()
        {
            // Değişen tüm BaseEntity'leri al
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                // Yeni eklenen kayıtlar için CreatedDate set et
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = DateTime.Now;
                }

                // Güncellenen kayıtlar için ModifiedDate set et
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedDate = DateTime.Now;

                    // CreatedDate değiştirilememeli
                    // Original value'yu koruyoruz
                    entry.Property(nameof(BaseEntity.CreatedDate)).IsModified = false;
                }
            }
        }
    }
}
