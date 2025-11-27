// ===================================================================================
// TEKNOROMA - SATIS ENTITY KONFIGURASYONU (SaleConfiguration.cs)
// ===================================================================================
//
// Satis islemlerinin (fatura basligi) veritabani konfigurasyonu.
// Master table: Sale --> Detail table: SaleDetail
//
// ===================================================================================

using Domain.Entities;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Sale Entity Konfigurasyonu (Master Table)
    /// </summary>
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.ToTable("Sales");
            builder.HasKey(s => s.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Satis Numarasi: Zorunlu, benzersiz
            builder.Property(s => s.SaleNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Kasa Numarasi: Opsiyonel
            builder.Property(s => s.CashRegisterNumber)
                .HasMaxLength(20);

            // Notlar: Opsiyonel
            builder.Property(s => s.Notes)
                .HasMaxLength(500);

            // =================================================================
            // PARA ALANLARI (Decimal Hassasiyeti)
            // =================================================================

            builder.Property(s => s.Subtotal)
                .HasPrecision(18, 2);

            builder.Property(s => s.TaxAmount)
                .HasPrecision(18, 2);

            builder.Property(s => s.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(s => s.TotalAmount)
                .HasPrecision(18, 2);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Satis numarasi benzersiz
            builder.HasIndex(s => s.SaleNumber)
                .IsUnique()
                .HasDatabaseName("IX_Sales_SaleNumber");

            // Satis tarihi index (raporlama icin sik sorgulanir)
            builder.HasIndex(s => s.SaleDate)
                .HasDatabaseName("IX_Sales_SaleDate");

            // Musteri ID index
            builder.HasIndex(s => s.CustomerId)
                .HasDatabaseName("IX_Sales_CustomerId");

            // Calisan ID index
            builder.HasIndex(s => s.EmployeeId)
                .HasDatabaseName("IX_Sales_EmployeeId");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // Sale (N) --> Customer (1)
            builder.HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sale (N) --> Employee (1)
            builder.HasOne(s => s.Employee)
                .WithMany(e => e.Sales)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sale (N) --> Store (1)
            builder.HasOne(s => s.Store)
                .WithMany(st => st.Sales)
                .HasForeignKey(s => s.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(s => !s.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
            builder.HasData(TeknoRomaSeedData.GetSales());
        }
    }
}
