// ===================================================================================
// TEKNOROMA - TEDARIKCI ISLEM ENTITY KONFIGURASYONU (SupplierTransactionConfiguration.cs)
// ===================================================================================
//
// Tedarikcilerden yapilan alimlarin veritabani konfigurasyonu.
// Stok girisi ve borc/alacak takibi icin kullanilir.
//
// ===================================================================================

using Domain.Entities;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// SupplierTransaction Entity Konfigurasyonu
    /// </summary>
    public class SupplierTransactionConfiguration : IEntityTypeConfiguration<SupplierTransaction>
    {
        public void Configure(EntityTypeBuilder<SupplierTransaction> builder)
        {
            builder.ToTable("SupplierTransactions");
            builder.HasKey(st => st.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Islem Numarasi: Zorunlu
            builder.Property(st => st.TransactionNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Fatura Numarasi: Opsiyonel
            builder.Property(st => st.InvoiceNumber)
                .HasMaxLength(50);

            // Notlar: Opsiyonel
            builder.Property(st => st.Notes)
                .HasMaxLength(1000);

            // =================================================================
            // PARA ALANLARI
            // =================================================================

            // Birim Fiyat
            builder.Property(st => st.UnitPrice)
                .HasPrecision(18, 2);

            // Toplam Tutar
            builder.Property(st => st.TotalAmount)
                .HasPrecision(18, 2);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Islem numarasi benzersiz
            builder.HasIndex(st => st.TransactionNumber)
                .IsUnique()
                .HasDatabaseName("IX_SupplierTransactions_TransactionNumber");

            // Islem tarihi index
            builder.HasIndex(st => st.TransactionDate)
                .HasDatabaseName("IX_SupplierTransactions_TransactionDate");

            // Tedarikci ID index
            builder.HasIndex(st => st.SupplierId)
                .HasDatabaseName("IX_SupplierTransactions_SupplierId");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // SupplierTransaction (N) --> Supplier (1)
            builder.HasOne(st => st.Supplier)
                .WithMany(s => s.SupplierTransactions)
                .HasForeignKey(st => st.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplierTransaction (N) --> Product (1)
            builder.HasOne(st => st.Product)
                .WithMany(p => p.SupplierTransactions)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(st => !st.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
            builder.HasData(TeknoRomaSeedData.GetSupplierTransactions());
        }
    }
}
