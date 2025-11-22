// ============================================================================
// Department.cs - Departman Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA mağazalarındaki departmanları temsil eden entity.
// Her mağazanın kendi departmanları vardır ve çalışanlar departmanlara atanır.
//
// ÖRNEK DEPARTMANLAR:
// - Satış Departmanı (KasaSatis)
// - Teknik Servis Departmanı (TeknikServis)
// - Depo/Stok Departmanı (Depo)
// - Muhasebe Departmanı (Muhasebe)
// - Yönetim (SubeMuduru, GenelMudur)
//
// İŞ KURALLARI:
// - TEKNOROMA'da toplam 30 departman var
// - Her departman bir mağazaya bağlı
// - DepartmentType ile departmanın işlev alanı belirlenir
// - Çalışanlar rollerine uygun departmanlara atanır
// ============================================================================

using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Departman Entity Sınıfı
    ///
    /// DEPARTMAN vs ROL:
    /// - Departman: Fiziksel/organizasyonel birim (Satış Departmanı)
    /// - Rol: Sistemdeki yetki seviyesi (KasaSatis)
    /// - DepartmentType ile departman-rol eşleşmesi sağlanır
    ///
    /// MAĞAZA-DEPARTMAN İLİŞKİSİ:
    /// - Her mağazanın kendi departmanları var
    /// - Aynı isimli departmanlar farklı mağazalarda olabilir
    /// - Örn: "Kadıköy - Satış Dept.", "Bornova - Satış Dept."
    /// </summary>
    public class Department : BaseEntity
    {
        // ====================================================================
        // TEMEL BİLGİLER
        // ====================================================================

        /// <summary>
        /// Departman Adı
        ///
        /// AÇIKLAMA:
        /// - Departmanın görünen adı
        /// - Mağaza içinde benzersiz olmalı
        ///
        /// ÖRNEKLER:
        /// - "Satış ve Pazarlama"
        /// - "Teknik Servis"
        /// - "Depo ve Lojistik"
        /// - "Muhasebe ve Finans"
        /// - "Yönetim"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Departman Açıklaması (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Departmanın görev tanımı
        /// - İş tanımları ve sorumluluklar
        ///
        /// ÖRNEK:
        /// "Mağaza içi ve online satış operasyonlarını yürütür.
        ///  Müşteri ilişkileri ve satış sonrası destek sağlar."
        /// </summary>
        public string? Description { get; set; }

        // ====================================================================
        // İLİŞKİSEL ALAN (FOREIGN KEY)
        // ====================================================================

        /// <summary>
        /// Mağaza ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Departmanın bağlı olduğu mağaza
        /// - Stores tablosundaki Id ile eşleşir
        /// - Her departman bir mağazaya ait
        ///
        /// NOT:
        /// - Merkezi departmanlar için "Genel Müdürlük" mağazası oluşturulabilir
        /// - Veya StoreId nullable yapılabilir
        /// </summary>
        public int StoreId { get; set; }

        // ====================================================================
        // DEPARTMAN TİPİ
        // ====================================================================

        /// <summary>
        /// Departman Türü/Rolü (Enum)
        ///
        /// AÇIKLAMA:
        /// - Departmanın işlevsel kategorisi
        /// - UserRole enum'undan değer alır
        /// - Çalışan atamaları için kullanılır
        ///
        /// EŞLEŞTİRME:
        /// - DepartmentType = KasaSatis → Satış personeli atanır
        /// - DepartmentType = TeknikServis → Teknisyenler atanır
        /// - DepartmentType = SubeMuduru → Yöneticiler atanır
        ///
        /// KULLANIM:
        /// // Çalışan departmana atanırken rol kontrolü
        /// if (employee.Role != department.DepartmentType)
        ///     throw new InvalidOperationException("Rol uyumsuzluğu!");
        ///
        /// NEDEN USERROLE KULLANILDI?
        /// - Departman tipleri rol tipleriyle birebir örtüşüyor
        /// - Ayrı enum oluşturmak gereksiz tekrar olurdu
        /// - DRY prensibi (Don't Repeat Yourself)
        /// </summary>
        public UserRole DepartmentType { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Bağlı Olduğu Mağaza (Navigation Property)
        ///
        /// İLİŞKİ: Department (N) → Store (1)
        /// - Birden fazla departman aynı mağazada olabilir
        ///
        /// KULLANIM:
        /// var storeName = department.Store.Name;
        /// var storeCity = department.Store.City;
        /// </summary>
        public virtual Store Store { get; set; } = null!;

        /// <summary>
        /// Departmandaki Çalışanlar (Collection Navigation Property)
        ///
        /// İLİŞKİ: Department (1) → Employee (N)
        /// - Bir departmanda birden fazla çalışan olabilir
        ///
        /// KULLANIM:
        /// // Departmandaki çalışan sayısı
        /// var employeeCount = department.Employees.Count;
        ///
        /// // Departmanın toplam maaş gideri
        /// var totalSalary = department.Employees
        ///     .Sum(e => e.Salary);
        ///
        /// // Aktif çalışanlar
        /// var activeEmployees = department.Employees
        ///     .Where(e => e.IsActive)
        ///     .ToList();
        /// </summary>
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
