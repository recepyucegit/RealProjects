// ===================================================================================
// TEKNOROMA - CALISAN ENTITY KONFIGURASYONU (EmployeeConfiguration.cs)
// ===================================================================================
//
// Calisan (personel) bilgilerinin veritabani konfigurasyonu.
// ASP.NET Identity ile entegre calisir.
//
// ===================================================================================

using Domain.Entities;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Employee Entity Konfigurasyonu
    /// </summary>
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(e => e.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Identity User ID: Zorunlu (ASP.NET Identity ile baglanti)
            builder.Property(e => e.IdentityUserId)
                .IsRequired()
                .HasMaxLength(450);  // GUID string uzunlugu

            // TC Kimlik Numarasi: Zorunlu
            builder.Property(e => e.IdentityNumber)
                .IsRequired()
                .HasMaxLength(11);

            // Ad: Zorunlu
            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            // Soyad: Zorunlu
            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // Email: Zorunlu
            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Telefon: Zorunlu
            builder.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);

            // =================================================================
            // PARA ALANLARI
            // =================================================================

            // Maas
            builder.Property(e => e.Salary)
                .HasPrecision(18, 2);

            // Satis Kotasi (opsiyonel)
            builder.Property(e => e.SalesQuota)
                .HasPrecision(18, 2);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // TC Kimlik benzersiz
            builder.HasIndex(e => e.IdentityNumber)
                .IsUnique()
                .HasDatabaseName("IX_Employees_IdentityNumber");

            // Email benzersiz
            builder.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Employees_Email");

            // Identity User ID benzersiz
            builder.HasIndex(e => e.IdentityUserId)
                .IsUnique()
                .HasDatabaseName("IX_Employees_IdentityUserId");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // Employee (N) --> Store (1)
            builder.HasOne(e => e.Store)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee (N) --> Department (1)
            builder.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(e => !e.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
            builder.HasData(TeknoRomaSeedData.GetEmployees());
        }
    }
}
