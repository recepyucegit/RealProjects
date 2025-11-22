using Microsoft.EntityFrameworkCore;
using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Context;

/// <summary>
/// Entity Framework Core DbContext sınıfı
/// Veritabanı ile uygulama arasındaki köprü görevi görür
///
/// NEDEN Bu Kadar Önemli?
/// - Her DbSet bir tabloya karşılık gelir
/// - Fluent API ile ilişkiler (relationships) yapılandırılır
/// - Global Query Filter'lar ile Soft Delete implementasyonu
/// - SaveChanges override ile otomatik timestamp yönetimi
/// </summary>
public class TeknoRomaDbContext : DbContext
{
    public TeknoRomaDbContext(DbContextOptions<TeknoRomaDbContext> options) : base(options)
    {
    }

    // ====== DBSET'LER (TABLOLAR) ======
    // Her DbSet, veritabanında bir tabloya karşılık gelir

    /// <summary>
    /// Mağazalar/Şubeler - 55 mağaza
    /// </summary>
    public DbSet<Store> Stores { get; set; }

    /// <summary>
    /// Departmanlar - 30 departman
    /// </summary>
    public DbSet<Department> Departments { get; set; }

    /// <summary>
    /// Çalışanlar - 258 çalışan
    /// </summary>
    public DbSet<Employee> Employees { get; set; }

    /// <summary>
    /// Kategoriler
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Ürünler
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Tedarikçiler
    /// </summary>
    public DbSet<Supplier> Suppliers { get; set; }

    /// <summary>
    /// Müşteriler
    /// </summary>
    public DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Satışlar (Order yerine Sale)
    /// </summary>
    public DbSet<Sale> Sales { get; set; }

    /// <summary>
    /// Satış Detayları
    /// </summary>
    public DbSet<SaleDetail> SaleDetails { get; set; }

    /// <summary>
    /// Giderler - Muhasebe için
    /// </summary>
    public DbSet<Expense> Expenses { get; set; }

    /// <summary>
    /// Teknik Servis Kayıtları
    /// </summary>
    public DbSet<TechnicalService> TechnicalServices { get; set; }


    /// <summary>
    /// Model oluşturma ve ilişkileri yapılandırma
    /// Fluent API kullanarak entity konfigürasyonları yapılır
    ///
    /// NEDEN Fluent API?
    /// - Data Annotations'dan daha güçlü
    /// - Karmaşık ilişkileri tanımlayabilir
    /// - Entity sınıflarını kirletmez (attribute'lerle)
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====== STORE (ŞUBE/MAĞAZA) CONFIGURATION ======
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StoreCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.City).IsRequired().HasMaxLength(50);
            entity.Property(e => e.District).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SquareMeters).HasColumnType("decimal(10,2)");

            // Unique Constraint - Mağaza kodu benzersiz olmalı
            entity.HasIndex(e => e.StoreCode).IsUnique();

            // Index'ler - Sorgulama performansı için
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.IsActive);
        });

        // ====== DEPARTMENT (DEPARTMAN) CONFIGURATION ======
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DepartmentCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MonthlySalesTarget).HasColumnType("decimal(18,2)");

            // Unique Constraint - Aynı mağazada aynı departman kodu olamaz
            entity.HasIndex(e => new { e.StoreId, e.DepartmentCode }).IsUnique();

            // İlişki: Department -> Store (Many-to-One)
            entity.HasOne(d => d.Store)
                  .WithMany(s => s.Departments)
                  .HasForeignKey(d => d.StoreId)
                  .OnDelete(DeleteBehavior.Restrict); // Mağaza silinirse departman silinmesin
        });

        // ====== EMPLOYEE (ÇALIŞAN) CONFIGURATION ======
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IdentityNumber).IsRequired().HasMaxLength(11);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.SalesQuota).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.IdentityUserId).HasMaxLength(450); // ASP.NET Identity User ID

            // Unique Constraints
            entity.HasIndex(e => e.IdentityNumber).IsUnique(); // TC Kimlik No benzersiz
            entity.HasIndex(e => e.Email).IsUnique(); // Email benzersiz
            entity.HasIndex(e => e.IdentityUserId).IsUnique(); // Identity User ID benzersiz

            // Index'ler
            entity.HasIndex(e => e.Role); // Role bazlı sorgular için
            entity.HasIndex(e => e.IsActive);

            // İlişki: Employee -> Store (Many-to-One)
            entity.HasOne(e => e.Store)
                  .WithMany(s => s.Employees)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Employee -> Department (Many-to-One)
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Employee -> Sales (One-to-Many)
            // Çalışanın yaptığı satışlar
            entity.HasMany(e => e.Sales)
                  .WithOne(s => s.Employee)
                  .HasForeignKey(s => s.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Employee -> Expenses (One-to-Many)
            // Çalışana yapılan ödemeler
            entity.HasMany(e => e.Expenses)
                  .WithOne(ex => ex.Employee)
                  .HasForeignKey(ex => ex.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Employee -> TechnicalServices (One-to-Many)
            // Çalışanın bildirdiği sorunlar
            entity.HasMany(e => e.ReportedTechnicalServices)
                  .WithOne(ts => ts.ReportedByEmployee)
                  .HasForeignKey(ts => ts.ReportedByEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Employee -> TechnicalServices (One-to-Many)
            // Çalışanın çözdüğü sorunlar
            entity.HasMany(e => e.AssignedTechnicalServices)
                  .WithOne(ts => ts.AssignedToEmployee)
                  .HasForeignKey(ts => ts.AssignedToEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== CATEGORY CONFIGURATION ======
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(250);

            entity.HasIndex(e => e.Name);

            // İlişki: Category -> Products (One-to-Many)
            entity.HasMany(c => c.Products)
                  .WithOne(p => p.Category)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== SUPPLIER CONFIGURATION ======
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContactName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContactTitle).HasMaxLength(50);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(10);

            entity.HasIndex(e => e.CompanyName);

            // İlişki: Supplier -> Products (One-to-Many)
            entity.HasMany(s => s.Products)
                  .WithOne(p => p.Supplier)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== PRODUCT CONFIGURATION ======
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(250);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);

            // Unique Constraint - Ürün kodu benzersiz
            entity.HasIndex(e => e.ProductCode).IsUnique();

            // Index'ler - Performans için
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Brand);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsFeatured);
            entity.HasIndex(e => e.StockStatus);

            // Calculated properties'ler veritabanında hesaplanmaz, sadece kod tarafında
            // Bu yüzden Ignore etmiyoruz, EF Core otomatik anlıyor
        });

        // ====== CUSTOMER CONFIGURATION ======
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(10);

            // Unique constraint - Email benzersiz olmalı
            entity.HasIndex(e => e.Email).IsUnique();

            // İlişki: Customer -> Sales (One-to-Many)
            entity.HasMany(c => c.Orders) // Orders property'si var ama Sale entity'sine bağlı
                  .WithOne(s => s.Customer)
                  .HasForeignKey(s => s.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Customer -> TechnicalServices (One-to-Many)
            entity.HasMany(c => c.TechnicalServices)
                  .WithOne(ts => ts.Customer)
                  .HasForeignKey(ts => ts.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== SALE (SATIŞ) CONFIGURATION ======
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SaleNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShippingAddress).HasMaxLength(250);
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Unique constraint - Satış numarası benzersiz
            entity.HasIndex(e => e.SaleNumber).IsUnique();

            // Index'ler - Raporlama için
            entity.HasIndex(e => e.SaleDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.StoreId);
            entity.HasIndex(e => e.EmployeeId);

            // İlişki: Sale -> Store (Many-to-One)
            entity.HasOne(s => s.Store)
                  .WithMany(st => st.Sales)
                  .HasForeignKey(s => s.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Sale -> SaleDetails (One-to-Many)
            entity.HasMany(s => s.SaleDetails)
                  .WithOne(sd => sd.Sale)
                  .HasForeignKey(sd => sd.SaleId)
                  .OnDelete(DeleteBehavior.Cascade); // Satış silinirse detaylar da silinsin
        });

        // ====== SALEDETAIL (SATIŞ DETAYI) CONFIGURATION ======
        modelBuilder.Entity<SaleDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.DiscountRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)").IsRequired();

            // Index'ler
            entity.HasIndex(e => e.SaleId);
            entity.HasIndex(e => e.ProductId);

            // İlişki: SaleDetail -> Product (Many-to-One)
            entity.HasOne(sd => sd.Product)
                  .WithMany(p => p.SaleDetails)
                  .HasForeignKey(sd => sd.ProductId)
                  .OnDelete(DeleteBehavior.Restrict); // Ürün silinirse satış detayları silinmesin
        });

        // ====== EXPENSE (GİDER) CONFIGURATION ======
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExpenseNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(10,4)");
            entity.Property(e => e.AmountInTRY).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DocumentNumber).HasMaxLength(50);

            // Unique constraint - Gider numarası benzersiz
            entity.HasIndex(e => e.ExpenseNumber).IsUnique();

            // Index'ler - Raporlama için
            entity.HasIndex(e => e.ExpenseDate);
            entity.HasIndex(e => e.ExpenseType);
            entity.HasIndex(e => e.StoreId);
            entity.HasIndex(e => e.IsPaid);

            // İlişki: Expense -> Store (Many-to-One)
            entity.HasOne(e => e.Store)
                  .WithMany(s => s.Expenses)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== TECHNICALSERVICE CONFIGURATION ======
        modelBuilder.Entity<TechnicalService>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Resolution).HasMaxLength(2000);
            entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");

            // Unique constraint - Servis numarası benzersiz
            entity.HasIndex(e => e.ServiceNumber).IsUnique();

            // Index'ler - Takip ve raporlama için
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.IsCustomerIssue);
            entity.HasIndex(e => e.ReportedDate);
            entity.HasIndex(e => e.StoreId);

            // İlişki: TechnicalService -> Store (Many-to-One)
            entity.HasOne(ts => ts.Store)
                  .WithMany(s => s.TechnicalServices)
                  .HasForeignKey(ts => ts.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            // İlişki: TechnicalService -> Product (Many-to-One)
            entity.HasOne(ts => ts.Product)
                  .WithMany(p => p.TechnicalServices)
                  .HasForeignKey(ts => ts.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });


        // ====== GLOBAL QUERY FILTERS - SOFT DELETE ======
        // NEDEN? IsDeleted = true olan kayıtlar otomatik olarak sorguların dışında kalır
        // Fiziksel silme yapmadan verileri "gizleriz"
        // Gerektiğinde IgnoreQueryFilters() ile gerçek kayıtları görebiliriz

        modelBuilder.Entity<Store>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Sale>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaleDetail>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TechnicalService>().HasQueryFilter(e => !e.IsDeleted);
    }


    /// <summary>
    /// SaveChanges override - Kaydetme işleminde otomatik işlemler
    /// NEDEN Override? Her kayıt işleminde otomatik timestamp güncellemesi yapmak için
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// SaveChangesAsync override - Asenkron kaydetme işleminde otomatik işlemler
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Timestamp'leri otomatik güncelleme
    /// NEDEN?
    /// - Yeni kayıt (Added): CreatedDate otomatik atanır
    /// - Güncelleme (Modified): UpdatedDate otomatik atanır
    /// - Manuel olarak yapmaya gerek kalmaz, hata riski azalır
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedDate = DateTime.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedDate = DateTime.Now;
            }
        }
    }
}
