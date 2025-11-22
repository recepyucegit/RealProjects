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
            // Base class'ın OnModelCreating'ini çağır (Identity için önemli)
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // GLOBAL QUERY FILTER - SOFT DELETE
            // =================================================================
            // Her sorguda otomatik olarak "WHERE IsDeleted = false" eklenir.
            // Silinen kayıtlar otomatik filtrelenir.
            //
            // KULLANIM:
            // _context.Products.ToList() → Sadece IsDeleted = false olanlar
            // _context.Products.IgnoreQueryFilters().ToList() → Tümü (silinmişler dahil)
            // =================================================================

            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Sale>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<SaleDetail>().HasQueryFilter(sd => !sd.IsDeleted);
            modelBuilder.Entity<Store>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SupplierTransaction>().HasQueryFilter(st => !st.IsDeleted);
            modelBuilder.Entity<TechnicalService>().HasQueryFilter(ts => !ts.IsDeleted);

            // =================================================================
            // PRODUCT ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Product>(entity =>
            {
                // TABLO ADI (Opsiyonel - Convention: DbSet adı kullanılır)
                entity.ToTable("Products");

                // PRIMARY KEY (Convention: Id otomatik algılanır)
                entity.HasKey(p => p.Id);

                // PROPERTY KONFİGÜRASYONLARI
                // --------------------------

                // Name: Zorunlu, maksimum 200 karakter
                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                // Barcode: Zorunlu, benzersiz, 50 karakter
                entity.Property(p => p.Barcode)
                    .IsRequired()
                    .HasMaxLength(50);

                // UNIQUE INDEX - Barkod benzersizliği
                entity.HasIndex(p => p.Barcode)
                    .IsUnique()
                    .HasDatabaseName("IX_Products_Barcode");

                // Description: Opsiyonel, uzun metin
                entity.Property(p => p.Description)
                    .HasMaxLength(2000);

                // UnitPrice: Decimal hassasiyeti (18 hane, 2 ondalık)
                entity.Property(p => p.UnitPrice)
                    .HasPrecision(18, 2);

                // İLİŞKİLER (RELATIONSHIPS)
                // -------------------------

                // Product (N) → Category (1)
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Kategorisi olan ürün silinemesin

                // Product (N) → Supplier (1)
                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =================================================================
            // CATEGORY ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Description)
                    .HasMaxLength(500);

                // Kategori adı benzersiz olmalı
                entity.HasIndex(c => c.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Categories_Name");
            });

            // =================================================================
            // SUPPLIER ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");

                entity.Property(s => s.CompanyName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(s => s.ContactName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(s => s.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Address)
                    .HasMaxLength(500);

                entity.Property(s => s.City)
                    .HasMaxLength(50);

                entity.Property(s => s.Country)
                    .IsRequired()
                    .HasMaxLength(50);

                // Vergi numarası benzersiz
                entity.Property(s => s.TaxNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(s => s.TaxNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Suppliers_TaxNumber");
            });

            // =================================================================
            // SALE ENTITY CONFIGURATION (MASTER)
            // =================================================================

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("Sales");

                // Satış numarası benzersiz
                entity.Property(s => s.SaleNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(s => s.SaleNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Sales_SaleNumber");

                // Para alanları hassasiyeti
                entity.Property(s => s.Subtotal).HasPrecision(18, 2);
                entity.Property(s => s.TaxAmount).HasPrecision(18, 2);
                entity.Property(s => s.DiscountAmount).HasPrecision(18, 2);
                entity.Property(s => s.TotalAmount).HasPrecision(18, 2);

                // İLİŞKİLER
                entity.HasOne(s => s.Customer)
                    .WithMany(c => c.Sales)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Employee)
                    .WithMany(e => e.Sales)
                    .HasForeignKey(s => s.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Store)
                    .WithMany(st => st.Sales)
                    .HasForeignKey(s => s.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Satış tarihi indeksi (sık sorgulanır)
                entity.HasIndex(s => s.SaleDate)
                    .HasDatabaseName("IX_Sales_SaleDate");
            });

            // =================================================================
            // SALEDETAIL ENTITY CONFIGURATION (DETAIL)
            // =================================================================

            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.ToTable("SaleDetails");

                entity.Property(sd => sd.UnitPrice).HasPrecision(18, 2);
                entity.Property(sd => sd.Subtotal).HasPrecision(18, 2);
                entity.Property(sd => sd.DiscountPercentage).HasPrecision(5, 2);
                entity.Property(sd => sd.DiscountAmount).HasPrecision(18, 2);
                entity.Property(sd => sd.TotalAmount).HasPrecision(18, 2);

                entity.Property(sd => sd.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                // MASTER-DETAIL ilişki
                entity.HasOne(sd => sd.Sale)
                    .WithMany(s => s.SaleDetails)
                    .HasForeignKey(sd => sd.SaleId)
                    .OnDelete(DeleteBehavior.Cascade); // Sale silinince detaylar da silinsin

                entity.HasOne(sd => sd.Product)
                    .WithMany(p => p.SaleDetails)
                    .HasForeignKey(sd => sd.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =================================================================
            // CUSTOMER ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.Property(c => c.IdentityNumber)
                    .IsRequired()
                    .HasMaxLength(11);

                // TC Kimlik benzersiz
                entity.HasIndex(c => c.IdentityNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Customers_IdentityNumber");

                entity.Property(c => c.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(c => c.Email)
                    .HasMaxLength(100);

                entity.Property(c => c.Address)
                    .HasMaxLength(500);

                entity.Property(c => c.City)
                    .HasMaxLength(50);
            });

            // =================================================================
            // EMPLOYEE ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");

                entity.Property(e => e.IdentityUserId)
                    .IsRequired()
                    .HasMaxLength(450); // GUID string length

                entity.Property(e => e.IdentityNumber)
                    .IsRequired()
                    .HasMaxLength(11);

                // TC Kimlik benzersiz
                entity.HasIndex(e => e.IdentityNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Employees_IdentityNumber");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Salary).HasPrecision(18, 2);
                entity.Property(e => e.SalesQuota).HasPrecision(18, 2);

                // İLİŞKİLER
                entity.HasOne(e => e.Store)
                    .WithMany(s => s.Employees)
                    .HasForeignKey(e => e.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =================================================================
            // STORE ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Stores");

                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                // Mağaza adı benzersiz
                entity.HasIndex(s => s.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Stores_Name");

                entity.Property(s => s.City)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(s => s.District)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(s => s.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(s => s.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(s => s.Email)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // =================================================================
            // DEPARTMENT ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");

                entity.Property(d => d.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.Description)
                    .HasMaxLength(500);

                // Mağaza ilişkisi
                entity.HasOne(d => d.Store)
                    .WithMany(s => s.Departments)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =================================================================
            // EXPENSE ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.ToTable("Expenses");

                // Gider numarası benzersiz
                entity.Property(e => e.ExpenseNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(e => e.ExpenseNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Expenses_ExpenseNumber");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.DocumentNumber)
                    .HasMaxLength(50);

                // Para alanları
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.ExchangeRate).HasPrecision(18, 4);
                entity.Property(e => e.AmountInTRY).HasPrecision(18, 2);

                // Mağaza ilişkisi
                entity.HasOne(e => e.Store)
                    .WithMany(s => s.Expenses)
                    .HasForeignKey(e => e.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Çalışan ilişkisi (opsiyonel - çalışan ödemeleri için)
                entity.HasOne(e => e.Employee)
                    .WithMany(emp => emp.Expenses)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Tarih indeksi
                entity.HasIndex(e => e.ExpenseDate)
                    .HasDatabaseName("IX_Expenses_ExpenseDate");
            });

            // =================================================================
            // SUPPLIER TRANSACTION ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<SupplierTransaction>(entity =>
            {
                entity.ToTable("SupplierTransactions");

                // İşlem numarası benzersiz
                entity.Property(st => st.TransactionNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(st => st.TransactionNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_SupplierTransactions_TransactionNumber");

                entity.Property(st => st.UnitPrice).HasPrecision(18, 2);
                entity.Property(st => st.TotalAmount).HasPrecision(18, 2);

                entity.Property(st => st.InvoiceNumber)
                    .HasMaxLength(50);

                entity.Property(st => st.Notes)
                    .HasMaxLength(1000);

                entity.HasOne(st => st.Supplier)
                    .WithMany(s => s.SupplierTransactions)
                    .HasForeignKey(st => st.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(st => st.Product)
                    .WithMany(p => p.SupplierTransactions)
                    .HasForeignKey(st => st.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // İşlem tarihi indeksi
                entity.HasIndex(st => st.TransactionDate)
                    .HasDatabaseName("IX_SupplierTransactions_TransactionDate");
            });

            // =================================================================
            // TECHNICAL SERVICE ENTITY CONFIGURATION
            // =================================================================

            modelBuilder.Entity<TechnicalService>(entity =>
            {
                entity.ToTable("TechnicalServices");

                // Servis numarası benzersiz
                entity.Property(ts => ts.ServiceNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(ts => ts.ServiceNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_TechnicalServices_ServiceNumber");

                // Sorun bilgileri
                entity.Property(ts => ts.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(ts => ts.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(ts => ts.Resolution)
                    .HasMaxLength(2000);

                // İLİŞKİLER

                // Mağaza ilişkisi
                entity.HasOne(ts => ts.Store)
                    .WithMany(s => s.TechnicalServices)
                    .HasForeignKey(ts => ts.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Bildiren çalışan ilişkisi
                entity.HasOne(ts => ts.ReportedByEmployee)
                    .WithMany()
                    .HasForeignKey(ts => ts.ReportedByEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Atanan teknisyen ilişkisi (opsiyonel)
                entity.HasOne(ts => ts.AssignedToEmployee)
                    .WithMany(e => e.AssignedTechnicalServices)
                    .HasForeignKey(ts => ts.AssignedToEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Müşteri ilişkisi (opsiyonel)
                entity.HasOne(ts => ts.Customer)
                    .WithMany()
                    .HasForeignKey(ts => ts.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Tarih indeksi
                entity.HasIndex(ts => ts.ReportedDate)
                    .HasDatabaseName("IX_TechnicalServices_ReportedDate");
            });
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
