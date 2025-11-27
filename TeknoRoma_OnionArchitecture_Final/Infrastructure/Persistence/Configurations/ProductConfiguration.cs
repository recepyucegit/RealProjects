// ===================================================================================
// TEKNOROMA - URUN ENTITY KONFIGURASYONU (ProductConfiguration.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// ===================================================================================
// Bu dosya, Product entity'sinin veritabani ile nasil eslestirileceğini tanimlar.
// Entity Framework Core Fluent API kullanilarak tablo, sutun ve iliski ayarlari yapilir.
//
// NEDEN AYRI KONFIGURASYON DOSYASI?
// ===================================================================================
// 1. SEPARATION OF CONCERNS: Her entity'nin konfigurasyonu ayri dosyada
// 2. OKUNABILIRLIK: DbContext dosyasi sismez, temiz kalir
// 3. BAKIM KOLAYLIGI: Degisiklik yapilacak yer kolayca bulunur
// 4. TAKIMBACALISI: Farkli gelistiriciler farkli entity'ler uzerinde calisabilir
//
// IEntityTypeConfiguration<T> INTERFACE'I
// ===================================================================================
// Entity Framework Core'un sagladigi bir interface'tir.
// Configure metodu ile entity'nin tum ayarlari yapilir.
// DbContext'te ApplyConfigurationsFromAssembly ile otomatik yuklenir.
//
// ===================================================================================

using Domain.Entities;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Product Entity Konfigurasyonu
    ///
    /// Bu sinif IEntityTypeConfiguration interface'ini implement eder.
    /// Configure metodu, Product entity'si icin tum veritabani ayarlarini icerir.
    /// </summary>
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        /// <summary>
        /// Product entity'sinin veritabani konfigurasyonu
        ///
        /// EntityTypeBuilder: Fluent API metodlarini saglar
        /// - ToTable(): Tablo adi
        /// - HasKey(): Primary key
        /// - Property(): Sutun ayarlari
        /// - HasIndex(): Index tanimlari
        /// - HasOne/HasMany(): Iliski tanimlari
        /// </summary>
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // =================================================================
            // TABLO AYARLARI
            // =================================================================

            // Tablo adi (varsayilan: DbSet adi, yani "Products")
            builder.ToTable("Products");

            // Primary Key (Convention ile otomatik algilanir ama acikca belirtmek iyi pratik)
            builder.HasKey(p => p.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Urun Adi: Zorunlu, maksimum 200 karakter
            builder.Property(p => p.Name)
                .IsRequired()                    // NOT NULL
                .HasMaxLength(200);              // nvarchar(200)

            // Barkod: Zorunlu, maksimum 50 karakter
            builder.Property(p => p.Barcode)
                .IsRequired()
                .HasMaxLength(50);

            // Aciklama: Opsiyonel, uzun metin
            builder.Property(p => p.Description)
                .HasMaxLength(2000);             // nvarchar(2000)

            // Birim Fiyat: Decimal hassasiyeti
            // HasPrecision(18, 2): 18 toplam hane, 2 ondalik
            // Ornek: 9999999999999999.99 TL'ye kadar
            builder.Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            // Gorsel URL: Opsiyonel
            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Barkod icin UNIQUE INDEX
            // Ayni barkodla iki urun olamaz
            builder.HasIndex(p => p.Barcode)
                .IsUnique()
                .HasDatabaseName("IX_Products_Barcode");

            // Kategori ID icin index (sik sorgulanan alan)
            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            // Tedarikci ID icin index
            builder.HasIndex(p => p.SupplierId)
                .HasDatabaseName("IX_Products_SupplierId");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // Product (N) --> Category (1)
            // Bir kategoride birden fazla urun olabilir
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);  // Kategori silinirse urunler silinmesin

            // Product (N) --> Supplier (1)
            // Bir tedarikciden birden fazla urun alinabilir
            builder.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER (Soft Delete)
            // =================================================================
            // Silinmis urunler otomatik filtrelenir
            builder.HasQueryFilter(p => !p.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
            builder.HasData(TeknoRomaSeedData.GetProducts());
        }
    }
}
