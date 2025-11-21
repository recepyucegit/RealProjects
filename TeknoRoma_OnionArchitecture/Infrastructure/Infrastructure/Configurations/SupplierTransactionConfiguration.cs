using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// SupplierTransaction (Tedarikçi İşlem) Entity Configuration
    ///
    /// AMAÇ:
    /// - Tedarikçiden ürün alım hareketlerini yönetir
    /// - Stok girişi ve maliyet takibi
    /// - Fatura ve ödeme takibi
    ///
    /// İŞ AKIŞI:
    /// 1. Tedarikçiden ürün alınır
    /// 2. Fatura kaydedilir
    /// 3. Stok güncellenir (Product.UnitsInStock artırılır)
    /// 4. Ödeme yapılır (IsPaid = true)
    ///
    /// HALUK BEY'İN RAP ORLARI:
    /// - "Hangi tedarikçiden ne kadar almışız?"
    /// - "Aylık alım tutarları"
    /// - "Ödenmemiş faturalar"
    ///
    /// NEDEN AYRI TABLO?
    /// - Satın alma ve satış işlemleri ayrı tutulmalı
    /// - Maliyet analizi için gerekli
    /// - Tedarikçi performans raporları
    /// </summary>
    public class SupplierTransactionConfiguration : IEntityTypeConfiguration<SupplierTransaction>
    {
        public void Configure(EntityTypeBuilder<SupplierTransaction> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("SupplierTransactions");

            // ====== PRIMARY KEY ======
            builder.HasKey(st => st.ID);

            // ====== PROPERTIES ======

            // İşlem Numarası - UNIQUE
            builder.Property(st => st.TransactionNumber)
                .IsRequired()
                .HasMaxLength(50); // Format: TH-2024-00001

            builder.HasIndex(st => st.TransactionNumber)
                .IsUnique(); // UNIQUE constraint
                // NEDEN UNIQUE?
                // - Her alım işleminin benzersiz takip numarası
                // - Fatura numarası ile eşleştirme

            // Tarih
            builder.Property(st => st.TransactionDate)
                .IsRequired(); // İşlem tarihi zorunlu

            // Miktar
            builder.Property(st => st.Quantity)
                .IsRequired();
                // Kaç adet alındı

            // Birim Fiyat (Tedarikçi alış fiyatı)
            builder.Property(st => st.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                // NEDEN AYRI SAKLA?
                // - Tedarikçi fiyatları değişebilir
                // - Maliyet analizi için geçmiş fiyatlar gerekli

            // Toplam Tutar
            builder.Property(st => st.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                // TotalAmount = UnitPrice × Quantity

            // Fatura Bilgisi
            builder.Property(st => st.InvoiceNumber)
                .HasMaxLength(100); // Tedarikçi fatura numarası (nullable)

            // Ödeme Durumu
            builder.Property(st => st.IsPaid)
                .IsRequired()
                .HasDefaultValue(false); // Varsayılan: Ödenmemiş

            builder.Property(st => st.PaymentDate)
                .IsRequired(false); // Nullable - Ödeme yapıldığında dolar

            // Notlar
            builder.Property(st => st.Notes)
                .HasMaxLength(1000); // İşlem notları (nullable)


            // ====== RELATIONSHIPS ======

            // SupplierTransaction - Supplier: Many-to-One
            // Her işlem bir tedarikçiye aittir
            builder.HasOne(st => st.Supplier)
                .WithMany(s => s.SupplierTransactions)
                .HasForeignKey(st => st.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
                // NEDEN RESTRICT?
                // - Tedarikçi silinirse işlemler korunmalı
                // - Geçmiş alım kayıtları önemli

            // SupplierTransaction - Product: Many-to-One
            // Her işlem bir ürüne aittir
            builder.HasOne(st => st.Product)
                .WithMany(p => p.SupplierTransactions)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                // NEDEN RESTRICT?
                // - Ürün silinirse alım kayıtları korunmalı
                // - Maliyet geçmişi önemli


            // ====== INDEXES ======
            // NEDEN BU INDEX'LER?
            // - Haluk Bey tedarikçi bazlı alım raporları isteyecek
            // - Tarih bazlı alım analizi
            // - Ürün bazlı maliyet takibi
            // - Ödenmemiş fatura takibi

            builder.HasIndex(st => st.TransactionDate); // Tarih bazlı raporlar
            builder.HasIndex(st => st.SupplierId); // Tedarikçi işlemleri
            builder.HasIndex(st => st.ProductId); // Ürün alım geçmişi
            builder.HasIndex(st => st.IsPaid); // Ödeme durumu
            builder.HasIndex(st => new { st.SupplierId, st.TransactionDate }); // Composite: Tedarikçi + Tarih
            builder.HasIndex(st => new { st.ProductId, st.TransactionDate }); // Composite: Ürün + Tarih (Maliyet analizi)
        }
    }
}
