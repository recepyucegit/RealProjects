// ===================================================================================
// TEKNOROMA - MUSTERI ENTITY KONFIGURASYONU (CustomerConfiguration.cs)
// ===================================================================================
//
// Musteri bilgilerinin veritabani konfigurasyonu.
// KVKK uyumlu veri saklama gereksinimleri dikkate alinmistir.
//
// ===================================================================================

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Customer Entity Konfigurasyonu
    /// </summary>
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(c => c.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // TC Kimlik Numarasi: Zorunlu, 11 karakter
            builder.Property(c => c.IdentityNumber)
                .IsRequired()
                .HasMaxLength(11);

            // Ad: Zorunlu
            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            // Soyad: Zorunlu
            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // Telefon: Zorunlu
            builder.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(20);

            // Email: Opsiyonel
            builder.Property(c => c.Email)
                .HasMaxLength(100);

            // Adres: Opsiyonel
            builder.Property(c => c.Address)
                .HasMaxLength(500);

            // Sehir: Opsiyonel
            builder.Property(c => c.City)
                .HasMaxLength(50);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // TC Kimlik numarasi benzersiz olmali
            builder.HasIndex(c => c.IdentityNumber)
                .IsUnique()
                .HasDatabaseName("IX_Customers_IdentityNumber");

            // Telefon ile arama icin index
            builder.HasIndex(c => c.Phone)
                .HasDatabaseName("IX_Customers_Phone");

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
