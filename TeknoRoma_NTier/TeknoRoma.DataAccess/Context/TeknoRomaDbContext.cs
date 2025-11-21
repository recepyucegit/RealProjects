using Microsoft.EntityFrameworkCore;
using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Context;

/// <summary>
/// Entity Framework Core DbContext sınıfı
/// Veritabanı ile uygulama arasındaki köprü görevi görür
/// DbSet'ler üzerinden tablolara erişim sağlar
/// </summary>
public class TeknoRomaDbContext : DbContext
{
    /// <summary>
    /// Constructor - DbContextOptions ile veritabanı bağlantı ayarlarını alır
    /// </summary>
    public TeknoRomaDbContext(DbContextOptions<TeknoRomaDbContext> options) : base(options)
    {
    }

    // DbSet'ler - Her biri veritabanında bir tabloya karşılık gelir
    /// <summary>
    /// Kategoriler tablosu
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Ürünler tablosu
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Tedarikçiler tablosu
    /// </summary>
    public DbSet<Supplier> Suppliers { get; set; }

    /// <summary>
    /// Müşteriler tablosu
    /// </summary>
    public DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Siparişler tablosu
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Sipariş detayları tablosu
    /// </summary>
    public DbSet<OrderDetail> OrderDetails { get; set; }

    /// <summary>
    /// Model oluşturma ve ilişkileri yapılandırma
    /// Fluent API kullanarak entity konfigürasyonları yapılır
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CATEGORY CONFIGURATION
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(250);

            // İlişki: Category -> Products (One-to-Many)
            entity.HasMany(c => c.Products)
                  .WithOne(p => p.Category)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict); // Kategori silinirken ürünler silinmesin
        });

        // SUPPLIER CONFIGURATION
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

            // İlişki: Supplier -> Products (One-to-Many)
            entity.HasMany(s => s.Products)
                  .WithOne(p => p.Supplier)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PRODUCT CONFIGURATION
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(250);

            // Index'ler - Sorgulama performansı için
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
        });

        // CUSTOMER CONFIGURATION
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

            // İlişki: Customer -> Orders (One-to-Many)
            entity.HasMany(c => c.Orders)
                  .WithOne(o => o.Customer)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ORDER CONFIGURATION
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(250);
            entity.Property(e => e.ShippingCity).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShippingDistrict).HasMaxLength(50);
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(10);
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Unique constraint - Sipariş numarası benzersiz olmalı
            entity.HasIndex(e => e.OrderNumber).IsUnique();

            // Index'ler
            entity.HasIndex(e => e.OrderDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CustomerId);

            // İlişki: Order -> OrderDetails (One-to-Many)
            entity.HasMany(o => o.OrderDetails)
                  .WithOne(od => od.Order)
                  .HasForeignKey(od => od.OrderId)
                  .OnDelete(DeleteBehavior.Cascade); // Sipariş silinirse detaylar da silinsin
        });

        // ORDERDETAIL CONFIGURATION
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.DiscountRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)").IsRequired();

            // İlişki: OrderDetail -> Product (Many-to-One)
            entity.HasOne(od => od.Product)
                  .WithMany(p => p.OrderDetails)
                  .HasForeignKey(od => od.ProductId)
                  .OnDelete(DeleteBehavior.Restrict); // Ürün silinirse sipariş detayları silinmesin
        });

        // Global Query Filter - Soft Delete için
        // IsDeleted = true olan kayıtlar otomatik olarak sorguların dışında kalır
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrderDetail>().HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// SaveChanges override - Kaydetme işleminde otomatik işlemler
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
    /// Yeni eklenen kayıtlar için CreatedDate, güncellenen kayıtlar için UpdatedDate otomatik atanır
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
