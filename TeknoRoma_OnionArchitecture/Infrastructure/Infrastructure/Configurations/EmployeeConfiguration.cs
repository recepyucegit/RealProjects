using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Employee (Çalışan) Entity Configuration
    ///
    /// AMAÇ:
    /// - 258 çalışanın veritabanı şemasını tanımlar
    /// - ASP.NET Identity entegrasyonu
    /// - Maaş, prim, satış kotası yönetimi
    ///
    /// ÖNEMLİ İŞ KURALLARI:
    /// - TC Kimlik No UNIQUE olmalı
    /// - IdentityUserId ile ASP.NET Identity kullanıcısına bağlanır
    /// - Satış kotası aşıldığında prim hesaplanır
    /// - Roller: Şube Müdürü, Kasa Satış, Depo, Muhasebe, Teknik Servis
    ///
    /// NEDEN KARMAŞIK?
    /// - Birden fazla tabloya ilişkili (Store, Department, Sale, Expense, TechnicalService)
    /// - Identity sistemi ile entegre
    /// - Finansal hesaplamalar (maaş, prim)
    /// </summary>
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("Employees");

            // ====== PRIMARY KEY ======
            builder.HasKey(e => e.ID);

            // ====== PROPERTIES ======

            // Identity Entegrasyonu
            builder.Property(e => e.IdentityUserId)
                .HasMaxLength(450); // ASP.NET Identity User ID (GUID formatında string)
                // NULLABLE: Identity kullanıcısı olmayan çalışan olabilir

            // TC Kimlik No - UNIQUE
            builder.Property(e => e.IdentityNumber)
                .IsRequired()
                .HasMaxLength(11); // TC Kimlik: 11 haneli

            builder.HasIndex(e => e.IdentityNumber)
                .IsUnique(); // UNIQUE constraint
                // NEDEN UNIQUE?
                // - Her çalışanın TC kimliği benzersiz olmalı
                // - Çift kayıt önlenir

            // Kişisel Bilgiler
            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.BirthDate)
                .IsRequired(); // Doğum tarihi zorunlu

            builder.Property(e => e.Address)
                .HasMaxLength(500); // Adres (nullable)

            // İstihdam Bilgileri
            builder.Property(e => e.HireDate)
                .IsRequired(); // İşe başlama tarihi

            builder.Property(e => e.Salary)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // DECIMAL(18,2) - Para için hassas
                // NEDEN DECIMAL(18,2)?
                // - Float kullanırsak yuvarlama hataları olur
                // - Maaş hesaplamalarında hassasiyet kritik

            // Rol - Enum
            builder.Property(e => e.Role)
                .IsRequired()
                .HasConversion<int>(); // Enum to Int
                // 1: SubeYoneticisi, 2: KasaSatis, 3: Depo, 4: Muhasebe, 5: TeknikServis

            // Satış Kotası (nullable - sadece satış çalışanları için)
            builder.Property(e => e.SalesQuota)
                .HasColumnType("decimal(18,2)"); // Nullable
                // NEDEN NULLABLE?
                // - Sadece satış çalışanlarının kotası var
                // - Depo, muhasebe çalışanlarının kotası yok

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true); // Varsayılan olarak aktif


            // ====== RELATIONSHIPS ======

            // Employee - Store: Many-to-One
            // Her çalışan bir mağazada çalışır
            builder.HasOne(e => e.Store)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee - Department: Many-to-One
            // Her çalışan bir departmana aittir
            builder.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee - Sale: One-to-Many
            // Bir çalışan birden fazla satış yapabilir
            builder.HasMany(e => e.Sales)
                .WithOne(s => s.Employee)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee - Expense: One-to-Many (Maaş ödemeleri)
            // Bir çalışanın birden fazla gider kaydı (maaş ödemeleri)
            builder.HasMany(e => e.Expenses)
                .WithOne(ex => ex.Employee)
                .HasForeignKey(ex => ex.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull); // Çalışan silinirse Expense.EmployeeId = NULL
                // NEDEN SetNull?
                // - Gider kayıtları korunmalı (muhasebe için)
                // - Çalışan bilgisi silinse bile ödeme kaydı kalmalı

            // Employee - TechnicalService (Bildiren): One-to-Many
            // Bir çalışan birden fazla teknik servis bildirimi yapabilir
            builder.HasMany(e => e.ReportedTechnicalServices)
                .WithOne(ts => ts.ReportedByEmployee)
                .HasForeignKey(ts => ts.ReportedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee - TechnicalService (Atanan): One-to-Many
            // Bir teknik servis çalışanına birden fazla iş atanabilir
            builder.HasMany(e => e.AssignedTechnicalServices)
                .WithOne(ts => ts.AssignedToEmployee)
                .HasForeignKey(ts => ts.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.SetNull); // Çalışan silinirse atama kaldırılır


            // ====== INDEXES ======
            // NEDEN BU INDEX'LER?
            // - Haluk Bey çalışan performansı raporu isteyecek
            // - Mağaza bazlı çalışan listesi sık sorgulanır
            // - Departman bazlı çalışan listesi
            // - Role göre filtreleme (satış elemanları, depo çalışanları vb.)

            builder.HasIndex(e => e.StoreId); // Mağaza bazlı çalışanlar
            builder.HasIndex(e => e.DepartmentId); // Departman bazlı çalışanlar
            builder.HasIndex(e => e.Role); // Role göre filtreleme
            builder.HasIndex(e => e.IsActive); // Aktif çalışanlar
            builder.HasIndex(e => e.IdentityUserId); // Identity entegrasyonu için hızlı erişim
            builder.HasIndex(e => new { e.StoreId, e.Role, e.IsActive }); // Composite: Mağaza + Rol + Aktiflik
        }
    }
}
