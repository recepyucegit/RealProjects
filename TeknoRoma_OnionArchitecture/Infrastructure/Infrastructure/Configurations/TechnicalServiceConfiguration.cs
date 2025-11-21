using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// TechnicalService (Teknik Servis) Entity Configuration
    ///
    /// AMAÇ:
    /// - Özgün Kablocu (Teknik Servis Temsilcisi) için sorun takibi
    /// - 2 tip sorun: Müşteri sorunları ve Sistem sorunları
    /// - Öncelik ve durum bazlı yönetim
    ///
    /// İŞ AKIŞI:
    /// Açık → İşlemde → Tamamlandı / Çözülemedi
    ///
    /// SORUN TÜRLERİ:
    /// 1. Müşteri Sorunları (IsCustomerIssue = true)
    ///    - Ürün arızası, şikayet, iade
    ///    - CustomerId dolu
    ///
    /// 2. Sistem Sorunları (IsCustomerIssue = false)
    ///    - Yazılım hatası, donanım arızası, network problemi
    ///    - CustomerId null
    ///
    /// ÖNCELİK SEVİYELERİ:
    /// - 1: Düşük
    /// - 2: Orta
    /// - 3: Yüksek
    /// - 4: Kritik (Sistem çökmesi, acil müdahale)
    ///
    /// ROLLER:
    /// - ReportedByEmployee: Sorunu bildiren çalışan
    /// - AssignedToEmployee: Sorunu çözecek teknik servis elemanı
    /// </summary>
    public class TechnicalServiceConfiguration : IEntityTypeConfiguration<TechnicalService>
    {
        public void Configure(EntityTypeBuilder<TechnicalService> builder)
        {
            // ====== TABLE NAME ======
            builder.ToTable("TechnicalServices");

            // ====== PRIMARY KEY ======
            builder.HasKey(ts => ts.ID);

            // ====== PROPERTIES ======

            // Servis Numarası - UNIQUE
            builder.Property(ts => ts.ServiceNumber)
                .IsRequired()
                .HasMaxLength(50); // Format: TS-2024-00001

            builder.HasIndex(ts => ts.ServiceNumber)
                .IsUnique(); // UNIQUE constraint
                // NEDEN UNIQUE?
                // - Her teknik servis kaydının benzersiz takip numarası
                // - Özgün Kablocu servis numarası ile arama yapacak

            // Sorun Bilgileri
            builder.Property(ts => ts.Title)
                .IsRequired()
                .HasMaxLength(200); // Sorun başlığı

            builder.Property(ts => ts.Description)
                .IsRequired()
                .HasMaxLength(2000); // Detaylı açıklama (uzun olabilir)

            // Durum - Enum
            builder.Property(ts => ts.Status)
                .IsRequired()
                .HasConversion<int>(); // Enum to Int
                // 1: Açık, 2: İşlemde, 3: Tamamlandı, 4: Çözülemedi

            // Öncelik
            builder.Property(ts => ts.Priority)
                .IsRequired()
                .HasDefaultValue(2); // Varsayılan: Orta (2)
                // 1: Düşük, 2: Orta, 3: Yüksek, 4: Kritik

            // Sorun Türü
            builder.Property(ts => ts.IsCustomerIssue)
                .IsRequired()
                .HasDefaultValue(false); // Varsayılan: Sistem sorunu
                // true: Müşteri sorunu, false: Sistem sorunu

            // Tarihler
            builder.Property(ts => ts.ReportedDate)
                .IsRequired(); // Bildirim tarihi zorunlu

            builder.Property(ts => ts.ResolvedDate)
                .IsRequired(false); // Nullable - Çözüldüğünde dolar

            // Çözüm Açıklaması
            builder.Property(ts => ts.Resolution)
                .HasMaxLength(2000); // Nullable - Çözüldüğünde yazılır


            // ====== RELATIONSHIPS ======

            // TechnicalService - Store: Many-to-One
            // Her sorun bir mağazaya aittir
            builder.HasOne(ts => ts.Store)
                .WithMany(s => s.TechnicalServices)
                .HasForeignKey(ts => ts.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // TechnicalService - Employee (Bildiren): Many-to-One
            // Her sorunu bir çalışan bildirir
            builder.HasOne(ts => ts.ReportedByEmployee)
                .WithMany(e => e.ReportedTechnicalServices)
                .HasForeignKey(ts => ts.ReportedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
                // NEDEN RESTRICT?
                // - Çalışan silinirse bile sorun kaydı korunmalı

            // TechnicalService - Employee (Atanan): Many-to-One
            // Soruna bir teknik servis elemanı atanır (nullable)
            builder.HasOne(ts => ts.AssignedToEmployee)
                .WithMany(e => e.AssignedTechnicalServices)
                .HasForeignKey(ts => ts.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.SetNull); // Çalışan silinirse atama kaldırılır
                // NEDEN SetNull?
                // - Teknik servis elemanı işten ayrılırsa atama kaldırılır
                // - Ancak sorun kaydı korunur

            // TechnicalService - Customer: Many-to-One
            // Müşteri sorunlarında CustomerId dolu
            builder.HasOne(ts => ts.Customer)
                .WithMany(c => c.TechnicalServices)
                .HasForeignKey(ts => ts.CustomerId)
                .OnDelete(DeleteBehavior.SetNull); // Müşteri silinirse CustomerId = NULL
                // NEDEN SetNull?
                // - Müşteri kaydı silinse bile sorun kaydı korunmalı


            // ====== INDEXES ======
            // NEDEN BU INDEX'LER?
            // - Özgün Kablocu durum bazlı sorun listesi görecek
            // - Öncelik bazlı sıralama (kritik sorunlar önce)
            // - Mağaza bazlı sorun analizi
            // - Çalışan performans takibi

            builder.HasIndex(ts => ts.Status); // Durum filtreleme
            builder.HasIndex(ts => ts.Priority); // Öncelik sıralaması
            builder.HasIndex(ts => ts.StoreId); // Mağaza sorunları
            builder.HasIndex(ts => ts.ReportedByEmployeeId); // Bildiren çalışan
            builder.HasIndex(ts => ts.AssignedToEmployeeId); // Atanan çalışan
            builder.HasIndex(ts => ts.CustomerId); // Müşteri sorunları
            builder.HasIndex(ts => ts.IsCustomerIssue); // Sorun türü
            builder.HasIndex(ts => ts.ReportedDate); // Tarih bazlı sıralama
            builder.HasIndex(ts => new { ts.Status, ts.Priority }); // Composite: Durum + Öncelik (En önemli sorgular)
            builder.HasIndex(ts => new { ts.StoreId, ts.Status }); // Composite: Mağaza + Durum
        }
    }
}
