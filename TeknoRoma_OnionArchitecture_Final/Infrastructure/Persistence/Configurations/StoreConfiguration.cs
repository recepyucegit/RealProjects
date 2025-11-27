// ===================================================================================
// TEKNOROMA - MAGAZA ENTITY KONFIGURASYONU (StoreConfiguration.cs)
// ===================================================================================
//
// Magaza/sube bilgilerinin veritabani konfigurasyonu.
// TEKNOROMA'nin 55 magazasi: Istanbul(20), Izmir(13), Ankara(13), Bursa(9)
//
// ===================================================================================

using Domain.Entities;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Store Entity Konfigurasyonu
    /// </summary>
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Stores");
            builder.HasKey(s => s.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Magaza Adi: Zorunlu
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Sehir: Zorunlu
            builder.Property(s => s.City)
                .IsRequired()
                .HasMaxLength(50);

            // Ilce: Zorunlu
            builder.Property(s => s.District)
                .IsRequired()
                .HasMaxLength(50);

            // Adres: Zorunlu
            builder.Property(s => s.Address)
                .IsRequired()
                .HasMaxLength(500);

            // Telefon: Zorunlu
            builder.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20);

            // Email: Zorunlu
            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Magaza adi benzersiz
            builder.HasIndex(s => s.Name)
                .IsUnique()
                .HasDatabaseName("IX_Stores_Name");

            // Sehir bazli sorgular icin
            builder.HasIndex(s => s.City)
                .HasDatabaseName("IX_Stores_City");

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(s => !s.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
            builder.HasData(TeknoRomaSeedData.GetStores());
        }
    }
}
