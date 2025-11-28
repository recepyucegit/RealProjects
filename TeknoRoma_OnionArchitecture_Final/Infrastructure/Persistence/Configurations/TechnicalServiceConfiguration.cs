// ===================================================================================
// TEKNOROMA - TEKNIK SERVIS ENTITY KONFIGURASYONU (TechnicalServiceConfiguration.cs)
// ===================================================================================
//
// Teknik servis/destek taleplerinin veritabani konfigurasyonu.
// Hem ic sorunlar (magaza ekipman) hem de musteri cihaz tamiri icin kullanilir.
//
// ===================================================================================

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// TechnicalService Entity Konfigurasyonu
    /// </summary>
    public class TechnicalServiceConfiguration : IEntityTypeConfiguration<TechnicalService>
    {
        public void Configure(EntityTypeBuilder<TechnicalService> builder)
        {
            builder.ToTable("TechnicalServices");
            builder.HasKey(ts => ts.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Servis Numarasi: Zorunlu
            builder.Property(ts => ts.ServiceNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Baslik: Zorunlu
            builder.Property(ts => ts.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Aciklama: Zorunlu
            builder.Property(ts => ts.Description)
                .IsRequired()
                .HasMaxLength(2000);

            // Cozum: Opsiyonel
            builder.Property(ts => ts.Resolution)
                .HasMaxLength(2000);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Servis numarasi benzersiz
            builder.HasIndex(ts => ts.ServiceNumber)
                .IsUnique()
                .HasDatabaseName("IX_TechnicalServices_ServiceNumber");

            // Bildirim tarihi index
            builder.HasIndex(ts => ts.ReportedDate)
                .HasDatabaseName("IX_TechnicalServices_ReportedDate");

            // Durum index (acik talepleri bulmak icin)
            builder.HasIndex(ts => ts.Status)
                .HasDatabaseName("IX_TechnicalServices_Status");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // TechnicalService (N) --> Store (1)
            builder.HasOne(ts => ts.Store)
                .WithMany(s => s.TechnicalServices)
                .HasForeignKey(ts => ts.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // TechnicalService (N) --> Employee (Bildiren) (1)
            builder.HasOne(ts => ts.ReportedByEmployee)
                .WithMany()
                .HasForeignKey(ts => ts.ReportedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // TechnicalService (N) --> Employee (Atanan) (1) (Opsiyonel)
            builder.HasOne(ts => ts.AssignedToEmployee)
                .WithMany(e => e.AssignedTechnicalServices)
                .HasForeignKey(ts => ts.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // TechnicalService (N) --> Customer (1) (Opsiyonel)
            builder.HasOne(ts => ts.Customer)
                .WithMany()
                .HasForeignKey(ts => ts.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(ts => !ts.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
        }
    }
}
