using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Product Entity Configuration
    /// 
    /// NEDEN Fluent API?
    /// - Data Annotations yerine daha güçlü
    /// - Entity sınıflarını temiz tutar
    /// - Daha karmaşık konfigürasyonlar yapılabilir
    /// - Migration'lar daha kontrollü
    /// 
    /// NEDEN IEntityTypeConfiguration?
    /// - DbContext'te tek tek yazmak yerine ayrı sınıflar
    /// - Separation of Concerns
    /// - Her entity'nin konfigürasyonu ayrı dosyada
    /// </summary>
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("Products");

            // ====== PRIMARY KEY ======
            builder.HasKey(p => p.ID);

            // ====== PROPERTIES ======

            builder.Property(p => p.Name)
                .IsRequired() // NOT NULL
                .HasMaxLength(200); // VARCHAR(200)

            builder.Property(p => p.Description)
                .HasMaxLength(1000); // Nullable VARCHAR(1000)

            builder.Property(p => p.Barcode)
                .IsRequired()
                .HasMaxLength(50);

            // UNIQUE Index: Barkod unique olmalı
            builder.HasIndex(p => p.Barcode)
                .IsUnique();

            builder.Property(p => p.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // DECIMAL(18,2) - Para için hassas

            builder.Property(p => p.UnitsInStock)
                .IsRequired()
                .HasDefaultValue(0); // Varsayılan değer

            builder.Property(p => p.CriticalStockLevel)
                .IsRequired()
                .HasDefaultValue(10);

            // Enum: Database'de int olarak saklanır
            builder.Property(p => p.StockStatus)
                .IsRequired()
                .HasConversion<int>(); // Enum to Int

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);


            // ====== RELATIONSHIPS (İlişkiler) ======

            // Product - Category: Many-to-One
            builder.HasOne(p => p.Category) // Product'ın bir Category'si var
                .WithMany(c => c.Products) // Category'nin birden fazla Product'ı var
                .HasForeignKey(p => p.CategoryId) // Foreign Key
                .OnDelete(DeleteBehavior.Restrict); // Category silinirse Product silinmesin (RESTRICT)

            // Product - Supplier: Many-to-One
            builder.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product - SaleDetail: One-to-Many
            builder.HasMany(p => p.SaleDetails)
                .WithOne(sd => sd.Product)
                .HasForeignKey(sd => sd.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Product silinirse SaleDetail silinmesin

            // Product - SupplierTransaction: One-to-Many
            builder.HasMany(p => p.SupplierTransactions)
                .WithOne(st => st.Product)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            // ====== INDEXES ======
            // Performance için indexler

            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.SupplierId);
            builder.HasIndex(p => p.StockStatus);
            builder.HasIndex(p => p.IsActive);
        }
    }

    /// <summary>
    /// Category Configuration
    /// </summary>
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.ID);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(c => c.Name);
        }
    }

    /// <summary>
    /// Supplier Configuration
    /// </summary>
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");
            builder.HasKey(s => s.ID);

            builder.Property(s => s.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.ContactName)
                .HasMaxLength(100);

            builder.Property(s => s.Phone)
                .HasMaxLength(20);

            builder.Property(s => s.Email)
                .HasMaxLength(200);

            builder.Property(s => s.Address)
                .HasMaxLength(500);

            builder.Property(s => s.City)
                .HasMaxLength(100);

            builder.Property(s => s.Country)
                .HasMaxLength(100);

            builder.Property(s => s.TaxNumber)
                .HasMaxLength(50);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(s => s.CompanyName);
            builder.HasIndex(s => s.TaxNumber);
        }
    }
}