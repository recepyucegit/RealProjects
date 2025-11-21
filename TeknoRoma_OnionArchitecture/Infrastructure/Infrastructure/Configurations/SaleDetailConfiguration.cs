using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// SaleDetail (Satış Detay) Entity Configuration
    ///
    /// AMAÇ:
    /// - Satışın ürün bazlı detaylarını yönetir
    /// - Her satır bir ürünü temsil eder
    /// - Junction Table: Sale ↔ Product many-to-many ilişkisi
    ///
    /// İŞ KURALLARI:
    /// - Her satır bir Sale'e ve bir Product'a aittir
    /// - Ürün fiyatı ve adı snapshot olarak saklanır
    /// - İndirim satır bazında uygulanabilir
    ///
    /// NEDEN SNAPSHOT?
    /// - Ürün fiyatı gelecekte değişebilir
    /// - Ama satış yapıldığı anki fiyat saklanmalı
    /// - Örnek: Bugün 1000 TL'ye satıldı, yarın fiyat 1200 TL olsa bile
    ///   satış kaydında 1000 TL görünmeli
    ///
    /// HESAPLAMA:
    /// - Subtotal = UnitPrice × Quantity
    /// - DiscountAmount = Subtotal × (DiscountPercentage / 100)
    /// - TotalAmount = Subtotal - DiscountAmount
    /// </summary>
    public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
    {
        public void Configure(EntityTypeBuilder<SaleDetail> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("SaleDetails");

            // ====== PRIMARY KEY ======
            builder.HasKey(sd => sd.ID);

            // ====== PROPERTIES ======

            // Snapshot: Ürün adı (o anki ürün adı)
            builder.Property(sd => sd.ProductName)
                .IsRequired()
                .HasMaxLength(200);
                // NEDEN SNAPSHOT?
                // - Ürün adı gelecekte değişebilir
                // - Satış yapıldığı anki ad saklanmalı

            // Snapshot: Ürün fiyatı (o anki fiyat)
            builder.Property(sd => sd.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                // NEDEN SNAPSHOT?
                // - Ürün fiyatı sürekli değişir
                // - Satış yapıldığı anki fiyat kayıt altına alınmalı
                // - Muhasebe için gerekli

            // Miktar
            builder.Property(sd => sd.Quantity)
                .IsRequired();
                // Kaç adet satıldı

            // İndirim
            builder.Property(sd => sd.DiscountPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)") // Örnek: 15.50 = %15.50 indirim
                .HasDefaultValue(0);

            // Finansal Hesaplamalar
            builder.Property(sd => sd.Subtotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                // Subtotal = UnitPrice × Quantity

            builder.Property(sd => sd.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
                // DiscountAmount = Subtotal × (DiscountPercentage / 100)

            builder.Property(sd => sd.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                // TotalAmount = Subtotal - DiscountAmount


            // ====== RELATIONSHIPS ======

            // SaleDetail - Sale: Many-to-One
            // Her satır bir satışa aittir
            builder.HasOne(sd => sd.Sale)
                .WithMany(s => s.SaleDetails)
                .HasForeignKey(sd => sd.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
                // NEDEN CASCADE?
                // - Satış silinirse satır detayları da silinmeli
                // - Master-Detail ilişkisi

            // SaleDetail - Product: Many-to-One
            // Her satır bir ürüne aittir
            builder.HasOne(sd => sd.Product)
                .WithMany(p => p.SaleDetails)
                .HasForeignKey(sd => sd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                // NEDEN RESTRICT?
                // - Ürün silinirse satış detayları korunmalı
                // - Geçmiş satış kayıtları silinmemeli


            // ====== INDEXES ======
            // NEDEN BU INDEX'LER?
            // - Haluk Bey "En çok satılan 10 ürün" raporu isteyecek
            // - Satış bazlı detay listeleme (fatura görüntüleme)
            // - Ürün bazlı satış analizi

            builder.HasIndex(sd => sd.SaleId); // Satış detaylarını getirme
            builder.HasIndex(sd => sd.ProductId); // Ürün satış geçmişi
            builder.HasIndex(sd => new { sd.SaleId, sd.ProductId }); // Composite: Aynı satışta aynı ürün kontrolü
        }
    }
}
