using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Sale (Satış Başlık) Entity Configuration
    ///
    /// AMAÇ:
    /// - Satış işlemlerinin master kaydını yönetir
    /// - Satış numarası, tarih, müşteri, çalışan, mağaza bilgileri
    /// - Satış toplamları ve vergi hesaplamaları
    ///
    /// İŞ AKIŞI:
    /// Beklemede → Hazırlanıyor → Tamamlandı / İptal
    ///
    /// ÖNEMLİ İŞ KURALLARI:
    /// - Satış numarası UNIQUE (Format: S-2024-00001)
    /// - KDV %20 olarak hesaplanır
    /// - İndirim uygulanabilir
    /// - Ödeme türü: Nakit, Kredi Kartı, Havale, Çek
    ///
    /// NEDEN MASTER-DETAIL YAPISI?
    /// - Sale: Genel bilgiler (müşteri, tarih, toplam)
    /// - SaleDetail: Satılan ürünler ve miktarlar
    /// - Bu yapı SQL'de standart satış modeli
    /// </summary>
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("Sales");

            // ====== PRIMARY KEY ======
            builder.HasKey(s => s.ID);

            // ====== PROPERTIES ======

            // Satış Numarası - UNIQUE
            builder.Property(s => s.SaleNumber)
                .IsRequired()
                .HasMaxLength(50); // Format: S-2024-00001

            builder.HasIndex(s => s.SaleNumber)
                .IsUnique(); // UNIQUE constraint
                // NEDEN UNIQUE?
                // - Her satışın benzersiz numarası olmalı
                // - Fahri Cepçi satış numarası ile arama yapacak

            // Tarih Bilgisi
            builder.Property(s => s.SaleDate)
                .IsRequired(); // Satış tarihi zorunlu

            // Satış Durumu - Enum
            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<int>(); // Enum to Int
                // 1: Beklemede, 2: Hazırlanıyor, 3: Tamamlandı, 4: İptal

            // Ödeme Türü - Enum
            builder.Property(s => s.PaymentType)
                .IsRequired()
                .HasConversion<int>(); // Enum to Int
                // 1: Nakit, 2: Kredi Kartı, 3: Havale, 4: Çek

            // Finansal Bilgiler
            builder.Property(s => s.Subtotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // Ara toplam (ürün toplamları)

            builder.Property(s => s.TaxAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // KDV tutarı (%20)
                // NEDEN AYRI ALAN?
                // - Vergi hesaplamaları için ayrı takip
                // - Muhasebe raporlarında vergi toplamı

            builder.Property(s => s.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)") // İndirim tutarı
                .HasDefaultValue(0); // Varsayılan 0

            builder.Property(s => s.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // Genel toplam
                // TotalAmount = Subtotal + TaxAmount - DiscountAmount

            // Kasa Bilgisi
            builder.Property(s => s.CashRegisterNumber)
                .HasMaxLength(50); // Örnek: "Kasa 1", "Kasa 2", "Mobil"

            // Notlar
            builder.Property(s => s.Notes)
                .HasMaxLength(1000); // Satış ile ilgili notlar (nullable)


            // ====== RELATIONSHIPS ======

            // Sale - Customer: Many-to-One
            // Her satış bir müşteriye aittir
            builder.HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sale - Employee: Many-to-One
            // Her satışı bir çalışan yapar (Kasa Satış temsilcisi)
            builder.HasOne(s => s.Employee)
                .WithMany(e => e.Sales)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sale - Store: Many-to-One
            // Her satış bir mağazada yapılır
            builder.HasOne(s => s.Store)
                .WithMany(st => st.Sales)
                .HasForeignKey(s => s.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sale - SaleDetail: One-to-Many
            // Bir satışın birden fazla satır detayı (ürünler)
            builder.HasMany(s => s.SaleDetails)
                .WithOne(sd => sd.Sale)
                .HasForeignKey(sd => sd.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
                // NEDEN CASCADE?
                // - Satış silinirse satır detayları da silinmeli
                // - Master-Detail ilişkisinde standart yaklaşım


            // ====== INDEXES ======
            // NEDEN BU INDEX'LER?
            // - Haluk Bey tarih bazlı satış raporları isteyecek
            // - Müşteri bazlı satış geçmişi
            // - Çalışan bazlı satış performansı
            // - Mağaza bazlı satış analizi
            // - Durum bazlı filtreleme (bekleyen siparişler vb.)

            builder.HasIndex(s => s.SaleDate); // Tarih bazlı raporlar
            builder.HasIndex(s => s.CustomerId); // Müşteri satış geçmişi
            builder.HasIndex(s => s.EmployeeId); // Çalışan satış performansı
            builder.HasIndex(s => s.StoreId); // Mağaza satış analizi
            builder.HasIndex(s => s.Status); // Durum filtreleme
            builder.HasIndex(s => s.PaymentType); // Ödeme türü analizi
            builder.HasIndex(s => new { s.SaleDate, s.StoreId }); // Composite: Tarih + Mağaza
            builder.HasIndex(s => new { s.EmployeeId, s.SaleDate }); // Composite: Çalışan + Tarih (Prim hesabı için)
        }
    }
}
