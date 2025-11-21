using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Department (Departman) Entity Configuration
    ///
    /// AMAÇ:
    /// - 30 departmanın veritabanı şemasını tanımlar
    /// - Satış, Depo, Muhasebe, Teknik Servis departmanları
    /// - Mağaza-Departman ilişkisini yönetir
    ///
    /// İŞ KURALI:
    /// - Her departman bir mağazaya bağlıdır
    /// - Bir departmanda birden fazla çalışan olabilir
    /// - Departman silinirse çalışanlar silinmez (Restrict)
    /// </summary>
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("Departments");

            // ====== PRIMARY KEY ======
            builder.HasKey(d => d.ID);

            // ====== PROPERTIES ======

            builder.Property(d => d.Name)
                .IsRequired() // Departman adı zorunlu
                .HasMaxLength(200);
                // Örnek: "Satış Departmanı", "Depo", "Muhasebe"

            builder.Property(d => d.Description)
                .HasMaxLength(1000); // Departman açıklaması (nullable)

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);


            // ====== RELATIONSHIPS ======

            // Department - Store: Many-to-One
            // Her departman bir mağazaya aittir
            builder.HasOne(d => d.Store)
                .WithMany(s => s.Departments)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
                // NEDEN RESTRICT?
                // - Mağaza silindiğinde departmanlar korunmalı
                // - Veri kaybını önler

            // Department - Employee: One-to-Many
            // Bir departmanda birden fazla çalışan
            builder.HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);


            // ====== INDEXES ======
            // NEDEN INDEX?
            // - Departman bazlı çalışan listesi için hızlı sorgular
            // - Mağaza-Departman kombinasyonu sık kullanılır

            builder.HasIndex(d => d.StoreId); // Mağaza bazlı departman listesi
            builder.HasIndex(d => d.Name); // Departman adına göre arama
            builder.HasIndex(d => d.IsActive); // Aktif departmanlar
            builder.HasIndex(d => new { d.StoreId, d.Name }); // Composite: Aynı mağazada aynı isimli departman kontrolü
        }
    }
}
