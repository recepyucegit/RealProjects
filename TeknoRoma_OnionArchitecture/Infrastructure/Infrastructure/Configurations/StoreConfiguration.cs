using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Store (Mağaza) Entity Configuration
    ///
    /// AMAÇ:
    /// - 55 mağaza için veritabanı şemasını tanımlar
    /// - İstanbul, İzmir, Ankara, Bursa şubelerini yönetir
    /// - Mağaza bazlı satış ve gider raporları için index'ler
    ///
    /// NEDEN AYRI DOSYA?
    /// - Her entity için tek sorumluluk prensibi (Single Responsibility)
    /// - Kolay bulunabilirlik ve bakım
    /// - Birden fazla geliştirici aynı anda çalışabilir
    /// </summary>
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("Stores");

            // ====== PRIMARY KEY ======
            builder.HasKey(s => s.ID);

            // ====== PROPERTIES ======

            builder.Property(s => s.Name)
                .IsRequired() // NOT NULL - Mağaza adı zorunlu
                .HasMaxLength(200);

            builder.Property(s => s.City)
                .IsRequired()
                .HasMaxLength(100); // İstanbul, İzmir, Ankara, Bursa

            builder.Property(s => s.Address)
                .IsRequired()
                .HasMaxLength(500); // Tam adres

            builder.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20); // Telefon formatı: +90 (212) 123 45 67

            builder.Property(s => s.Email)
                .HasMaxLength(200); // Mağaza email'i (nullable)

            builder.Property(s => s.ManagerName)
                .HasMaxLength(100); // Şube müdürü adı (nullable)

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true); // Varsayılan olarak aktif


            // ====== RELATIONSHIPS ======

            // Store - Employee: One-to-Many
            // Bir mağazada birden fazla çalışan
            builder.HasMany(s => s.Employees)
                .WithOne(e => e.Store)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict); // Mağaza silinirse çalışanlar silinmesin

            // Store - Department: One-to-Many
            // Bir mağazada birden fazla departman
            builder.HasMany(s => s.Departments)
                .WithOne(d => d.Store)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store - Sale: One-to-Many
            // Bir mağazada birden fazla satış
            builder.HasMany(s => s.Sales)
                .WithOne(sale => sale.Store)
                .HasForeignKey(sale => sale.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store - Expense: One-to-Many
            // Bir mağazanın birden fazla gideri
            builder.HasMany(s => s.Expenses)
                .WithOne(e => e.Store)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store - TechnicalService: One-to-Many
            // Bir mağazada birden fazla teknik servis kaydı
            builder.HasMany(s => s.TechnicalServices)
                .WithOne(ts => ts.Store)
                .HasForeignKey(ts => ts.StoreId)
                .OnDelete(DeleteBehavior.Restrict);


            // ====== INDEXES ======
            // NEDEN INDEX?
            // - Haluk Bey şehir bazlı raporlar isteyecek
            // - Mağaza bazlı satış performansı sorguları hızlanır
            // - IsActive ile aktif mağaza filtreleme hızlanır

            builder.HasIndex(s => s.City); // Şehre göre filtreleme için
            builder.HasIndex(s => s.IsActive); // Aktif mağazalar için
            builder.HasIndex(s => new { s.City, s.IsActive }); // Composite index: Aktif mağazalar + Şehir
        }
    }
}
