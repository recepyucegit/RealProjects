// ===================================================================================
// TEKNOROMA - DEPARTMAN ENTITY KONFIGURASYONU (DepartmentConfiguration.cs)
// ===================================================================================
//
// Departman bilgilerinin veritabani konfigurasyonu.
// Her magaza kendi departmanlarina sahiptir.
// Ornek: Satis, Teknik Servis, Depo, Muhasebe vb.
//
// ===================================================================================

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Department Entity Konfigurasyonu
    /// </summary>
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");
            builder.HasKey(d => d.Id);

            // =================================================================
            // PROPERTY KONFIGURASYONLARI
            // =================================================================

            // Departman Adi: Zorunlu
            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Aciklama: Opsiyonel
            builder.Property(d => d.Description)
                .HasMaxLength(500);

            // =================================================================
            // INDEX TANIMLARI
            // =================================================================

            // Magaza + Departman adi birlikte benzersiz
            // Ayni magazada ayni isimde iki departman olamaz
            builder.HasIndex(d => new { d.StoreId, d.Name })
                .IsUnique()
                .HasDatabaseName("IX_Departments_StoreId_Name");

            // =================================================================
            // ILISKI KONFIGURASYONLARI
            // =================================================================

            // Department (N) --> Store (1)
            builder.HasOne(d => d.Store)
                .WithMany(s => s.Departments)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================================================================
            // GLOBAL QUERY FILTER
            // =================================================================
            builder.HasQueryFilter(d => !d.IsDeleted);

            // =================================================================
            // SEED DATA
            // =================================================================
        }
    }
}
