using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Expense (Gider) Entity Configuration
    ///
    /// AMAÇ:
    /// - Feyza Paragöz (Muhasebe) için gider yönetimi
    /// - Çalışan ödemeleri, faturalar, altyapı giderleri
    /// - Döviz kuru ile çoklu para birimi desteği
    ///
    /// GİDER TÜRLERİ:
    /// 1. CalisanOdemesi: Maaş ödemeleri (EmployeeId dolu)
    /// 2. TeknikaltyapiGideri: Sunucu, internet, elektrik vb.
    /// 3. Fatura: Tedarikçi faturaları
    /// 4. DigerGider: Diğer giderler
    ///
    /// DÖVİZ KURU MANTĞI:
    /// - TRY: ExchangeRate = 1 (varsayılan)
    /// - USD/EUR: O tarihteki kur kaydedilir
    /// - AmountInTRY = Amount × ExchangeRate
    /// - Haluk Bey TL bazlı raporlar görmek isteyecek
    ///
    /// NEDEN DÖVİZ DESTEĞI?
    /// - Tedarikçi ödemeleri dolar/euro olabilir
    /// - Muhasebe raporlarında TL'ye çevrilmeli
    /// </summary>
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("Expenses");

            // ====== PRIMARY KEY ======
            builder.HasKey(e => e.ID);

            // ====== PROPERTIES ======

            // Gider Numarası - UNIQUE
            builder.Property(e => e.ExpenseNumber)
                .IsRequired()
                .HasMaxLength(50); // Format: G-2024-00001

            builder.HasIndex(e => e.ExpenseNumber)
                .IsUnique(); // UNIQUE constraint
                // NEDEN UNIQUE?
                // - Her giderin benzersiz takip numarası
                // - Feyza Paragöz gider numarası ile arama yapacak

            // Tarih
            builder.Property(e => e.ExpenseDate)
                .IsRequired(); // Gider tarihi zorunlu

            // Gider Türü - Enum
            builder.Property(e => e.ExpenseType)
                .IsRequired()
                .HasConversion<int>(); // Enum to Int
                // 1: CalisanOdemesi, 2: TeknikaltyapiGideri, 3: Fatura, 4: DigerGider

            // Açıklama
            builder.Property(e => e.Description)
                .HasMaxLength(1000); // Gider açıklaması (nullable)

            // Finansal Bilgiler
            builder.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // Orijinal tutar

            // Döviz - Enum
            builder.Property(e => e.Currency)
                .IsRequired()
                .HasConversion<int>() // Enum to Int
                .HasDefaultValue(1); // Varsayılan: TRY
                // 1: TRY, 2: USD, 3: EUR

            // Döviz Kuru
            builder.Property(e => e.ExchangeRate)
                .HasColumnType("decimal(18,4)"); // Nullable - TRY için null
                // NEDEN DECIMAL(18,4)?
                // - Döviz kurları hassas olmalı (örn: 34.5678)
                // - 4 ondalık yeterli

            // TL Tutar (Hesaplanmış)
            builder.Property(e => e.AmountInTRY)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                // AmountInTRY = Amount × ExchangeRate
                // TRY ise Amount'un kendisi

            // Ödeme Durumu
            builder.Property(e => e.IsPaid)
                .IsRequired()
                .HasDefaultValue(false); // Varsayılan: Ödenmemiş

            builder.Property(e => e.PaymentDate)
                .IsRequired(false); // Nullable - Ödeme yapıldığında dolar


            // ====== RELATIONSHIPS ======

            // Expense - Store: Many-to-One
            // Her gider bir mağazaya aittir
            builder.HasOne(e => e.Store)
                .WithMany(s => s.Expenses)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense - Employee: Many-to-One (Sadece Çalışan Ödemeleri için)
            // Gider türü CalisanOdemesi ise EmployeeId dolu
            builder.HasOne(e => e.Employee)
                .WithMany(emp => emp.Expenses)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull); // Çalışan silinirse EmployeeId = NULL
                // NEDEN SetNull?
                // - Gider kaydı korunmalı (muhasebe için)
                // - Çalışan silinse bile ödeme kaydı kalmalı


            // ====== INDEXES ======
            // NEDEN BU INDEX'LER?
            // - Feyza Paragöz tarih bazlı gider raporları isteyecek
            // - Gider türü bazlı filtreleme (maaşlar, faturalar vb.)
            // - Mağaza bazlı gider analizi
            // - Ödeme durumu takibi (ödenmemiş giderler)

            builder.HasIndex(e => e.ExpenseDate); // Tarih bazlı raporlar
            builder.HasIndex(e => e.ExpenseType); // Gider türü filtreleme
            builder.HasIndex(e => e.StoreId); // Mağaza giderleri
            builder.HasIndex(e => e.EmployeeId); // Çalışan maaş ödemeleri
            builder.HasIndex(e => e.IsPaid); // Ödeme durumu
            builder.HasIndex(e => new { e.ExpenseDate, e.StoreId }); // Composite: Tarih + Mağaza
            builder.HasIndex(e => new { e.ExpenseType, e.IsPaid }); // Composite: Tür + Ödeme durumu
        }
    }
}
