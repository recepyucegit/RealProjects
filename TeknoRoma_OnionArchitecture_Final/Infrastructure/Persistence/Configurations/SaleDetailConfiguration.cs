// ===================================================================================
// TEKNOROMA - SATIS DETAY ENTITY KONFIGURASYONU (SaleDetailConfiguration.cs)
// ===================================================================================
//
// Satis kalemlerinin (fatura satirlari) veritabani konfigurasyonu.
// Detail table: Her satir bir urun kalemini temsil eder.
//
// MASTER-DETAIL PATTERN:
// Sale (1) --> SaleDetail (N)
// Bir satista birden fazla urun olabilir.
//
// ===================================================================================

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// SaleDetail Entity Konfigurasyonu (Detail Table)
    /// </summary>
    public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
    {
        public void Configure(EntityTypeBuilder<SaleDetail> builder)
        {
            builder.ToTable("SaleDetails");
            builder.HasKey(sd => sd.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Urun Adi (Snapshot): Satis anindaki urun adi
            builder.Property(sd => sd.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            // =================================================================
            // PARA ALANLARI
            // =================================================================

            // Birim Fiyat (Snapshot)
            builder.Property(sd => sd.UnitPrice)
                .HasPrecision(18, 2);

            // Ara Toplam
            builder.Property(sd => sd.Subtotal)
                .HasPrecision(18, 2);

            // Indirim Yuzdesi
            builder.Property(sd => sd.DiscountPercentage)
                .HasPrecision(5, 2);  // 100.00'a kadar

            // Indirim Tutari
            builder.Property(sd => sd.DiscountAmount)
                .HasPrecision(18, 2);

            // Net Tutar
            builder.Property(sd => sd.TotalAmount)
                .HasPrecision(18, 2);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Sale ID index
            builder.HasIndex(sd => sd.SaleId)
                .HasDatabaseName("IX_SaleDetails_SaleId");

            // Product ID index
            builder.HasIndex(sd => sd.ProductId)
                .HasDatabaseName("IX_SaleDetails_ProductId");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // SaleDetail (N) --> Sale (1)
            // CASCADE DELETE: Satis silinirse detaylar da silinir
            builder.HasOne(sd => sd.Sale)
                .WithMany(s => s.SaleDetails)
                .HasForeignKey(sd => sd.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // SaleDetail (N) --> Product (1)
            builder.HasOne(sd => sd.Product)
                .WithMany(p => p.SaleDetails)
                .HasForeignKey(sd => sd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(sd => !sd.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
        }
    }
}
