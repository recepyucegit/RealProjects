using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Customer (Müşteri) Entity Configuration
    ///
    /// AMAÇ:
    /// - Müşteri veritabanı şemasını tanımlar
    /// - Haluk Bey'in istediği demografik analiz için yapılandırma
    /// - TC Kimlik No ile tekil müşteri takibi
    ///
    /// İŞ KURALLARI:
    /// - TC Kimlik No UNIQUE olmalı
    /// - Yaş ve cinsiyet bilgisi demografik raporlar için
    /// - Müşteri satış geçmişi takip edilir
    ///
    /// HALUK BEY'İN İSTEDİĞİ RAPORLAR:
    /// - Yaş aralığına göre müşteri analizi
    /// - Cinsiyet bazlı satış analizi
    /// - VIP müşteriler (en çok alışveriş yapanlar)
    /// </summary>
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("Customers");

            // ====== PRIMARY KEY ======
            builder.HasKey(c => c.ID);

            // ====== PROPERTIES ======

            // TC Kimlik No - UNIQUE
            builder.Property(c => c.IdentityNumber)
                .IsRequired()
                .HasMaxLength(11); // TC Kimlik: 11 haneli

            builder.HasIndex(c => c.IdentityNumber)
                .IsUnique(); // UNIQUE constraint
                // NEDEN UNIQUE?
                // - Aynı müşteri birden fazla kayıt edilmemeli
                // - Müşteri geçmişi tek bir kayıtta toplanır

            // Kişisel Bilgiler
            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            // Demografik Bilgiler (Haluk Bey'in raporu için)
            builder.Property(c => c.BirthDate)
                .IsRequired(false); // Nullable - Müşteri vermeyebilir
                // NEDEN NULLABLE?
                // - Müşteri doğum tarihini vermek istemeyebilir
                // - Ancak verilirse demografik analiz için kullanılır

            builder.Property(c => c.Gender)
                .HasConversion<int>() // Enum to Int
                .IsRequired(false); // Nullable
                // 1: Erkek, 2: Kadın, 3: Belirtilmemiş

            // İletişim Bilgileri
            builder.Property(c => c.Email)
                .HasMaxLength(200); // Nullable

            builder.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(20); // Telefon zorunlu (iletişim için)

            builder.Property(c => c.Address)
                .HasMaxLength(500); // Nullable

            builder.Property(c => c.City)
                .HasMaxLength(100); // Nullable

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);


            // ====== RELATIONSHIPS ======

            // Customer - Sale: One-to-Many
            // Bir müşterinin birden fazla satış kaydı
            builder.HasMany(c => c.Sales)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
                // NEDEN RESTRICT?
                // - Müşteri silinirse satış kayıtları korunmalı
                // - Finansal kayıtlar asla silinmemeli

            // Customer - TechnicalService: One-to-Many
            // Bir müşterinin birden fazla teknik servis talebi
            builder.HasMany(c => c.TechnicalServices)
                .WithOne(ts => ts.Customer)
                .HasForeignKey(ts => ts.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
                // NEDEN SetNull?
                // - Müşteri silinirse teknik servis kaydı korunur
                // - Ancak müşteri bilgisi NULL olur


            // ====== INDEXES ======
            // NEDEN BU INDEX'LER?
            // - Haluk Bey demografik raporlar isteyecek
            // - Yaş aralığına göre müşteri analizi
            // - Cinsiyet bazlı satış analizi
            // - Şehir bazlı müşteri dağılımı

            builder.HasIndex(c => c.Gender); // Cinsiyet bazlı analiz
            builder.HasIndex(c => c.BirthDate); // Yaş hesaplaması için
            builder.HasIndex(c => c.City); // Şehir bazlı müşteri dağılımı
            builder.HasIndex(c => c.IsActive); // Aktif müşteriler
            builder.HasIndex(c => new { c.City, c.Gender }); // Composite: Şehir + Cinsiyet analizi
        }
    }
}
