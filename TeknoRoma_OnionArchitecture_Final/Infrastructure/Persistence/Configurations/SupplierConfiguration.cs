// ===================================================================================
// TEKNOROMA - TEDARIKCI ENTITY KONFIGURASYONU (SupplierConfiguration.cs)
// ===================================================================================
//
// Tedarikci firmalarinin veritabani konfigurasyonu.
// Apple, Samsung, Xiaomi gibi distributorlerin bilgileri.
//
// ===================================================================================

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Supplier Entity Konfigurasyonu
    /// </summary>
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");
            builder.HasKey(s => s.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Firma Adi: Zorunlu
            builder.Property(s => s.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            // Yetkili Kisi: Zorunlu
            builder.Property(s => s.ContactName)
                .IsRequired()
                .HasMaxLength(100);

            // Telefon: Zorunlu
            builder.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20);

            // Email: Zorunlu
            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Adres: Opsiyonel
            builder.Property(s => s.Address)
                .HasMaxLength(500);

            // Sehir: Opsiyonel
            builder.Property(s => s.City)
                .HasMaxLength(50);

            // Ulke: Zorunlu, varsayilan "Turkiye"
            builder.Property(s => s.Country)
                .IsRequired()
                .HasMaxLength(50);

            // Vergi Numarasi: Zorunlu
            builder.Property(s => s.TaxNumber)
                .IsRequired()
                .HasMaxLength(20);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Vergi numarasi benzersiz olmali
            builder.HasIndex(s => s.TaxNumber)
                .IsUnique()
                .HasDatabaseName("IX_Suppliers_TaxNumber");

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(s => !s.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
        }
    }
}
