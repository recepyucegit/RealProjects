// ===================================================================================
// TEKNOROMA - GIDER ENTITY KONFIGURASYONU (ExpenseConfiguration.cs)
// ===================================================================================
//
// Isletme giderlerinin veritabani konfigurasyonu.
// Kira, elektrik, maas, malzeme gibi tum giderleri kapsar.
//
// ===================================================================================

using Domain.Entities;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Expense Entity Konfigurasyonu
    /// </summary>
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses");
            builder.HasKey(e => e.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Gider Numarasi: Zorunlu
            builder.Property(e => e.ExpenseNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Aciklama: Zorunlu
            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            // Belge Numarasi: Opsiyonel
            builder.Property(e => e.DocumentNumber)
                .HasMaxLength(50);

            // =================================================================
            // PARA ALANLARI
            // =================================================================

            // Tutar (orijinal para birimi)
            builder.Property(e => e.Amount)
                .HasPrecision(18, 2);

            // Doviz Kuru
            builder.Property(e => e.ExchangeRate)
                .HasPrecision(18, 4);  // 4 ondalik hassasiyet

            // TL Karsiligi
            builder.Property(e => e.AmountInTRY)
                .HasPrecision(18, 2);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Gider numarasi benzersiz
            builder.HasIndex(e => e.ExpenseNumber)
                .IsUnique()
                .HasDatabaseName("IX_Expenses_ExpenseNumber");

            // Gider tarihi index (raporlama icin)
            builder.HasIndex(e => e.ExpenseDate)
                .HasDatabaseName("IX_Expenses_ExpenseDate");

            // Gider tipi index
            builder.HasIndex(e => e.ExpenseType)
                .HasDatabaseName("IX_Expenses_ExpenseType");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // Expense (N) --> Store (1)
            builder.HasOne(e => e.Store)
                .WithMany(s => s.Expenses)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense (N) --> Employee (1) (Opsiyonel: calisan odemeleri icin)
            builder.HasOne(e => e.Employee)
                .WithMany(emp => emp.Expenses)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(e => !e.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
            builder.HasData(TeknoRomaSeedData.GetExpenses());
        }
    }
}
