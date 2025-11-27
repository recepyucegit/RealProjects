// ===================================================================================
// TEKNOROMA - KATEGORI ENTITY KONFIGURASYONU (CategoryConfiguration.cs)
// ===================================================================================
//
// Urun kategorilerinin veritabani konfigurasyonu.
// Ornek kategoriler: Telefon, Bilgisayar, TV, Beyaz Esya vb.
//
// ===================================================================================

using Domain.Entities;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Category Entity Konfigurasyonu
    /// </summary>
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Tablo adi
            builder.ToTable("Categories");

            // Primary Key
            builder.HasKey(c => c.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Kategori Adi: Zorunlu, maksimum 100 karakter
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Aciklama: Opsiyonel
            builder.Property(c => c.Description)
                .HasMaxLength(500);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Kategori adi benzersiz olmali
            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name");

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(c => !c.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
            builder.HasData(TeknoRomaSeedData.GetCategories());
        }
    }
}
