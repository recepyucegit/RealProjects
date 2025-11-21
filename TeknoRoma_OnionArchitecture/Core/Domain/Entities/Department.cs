using Domain.Enums;
using static System.Formats.Asn1.AsnWriter;

namespace Domain.Entities
{
    /// <summary>
    /// Departman Entity
    /// TEKNOROMA'da toplam 30 departman var
    /// Örn: Bilgisayar Donanımları, Cep Telefonları, Kameralar, Satış, Depo, Muhasebe
    /// </summary>
    public class Department : BaseEntity
    {
        /// <summary>
        /// Departman Adı
        /// Örn: "Bilgisayar Donanımları", "Satış Departmanı", "Depo"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Departman Açıklaması
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Hangi mağazaya ait? (Foreign Key)
        /// Her departman bir mağazaya aittir
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Departmanın rolü/türü
        /// Örn: SubeYoneticisi, KasaSatis, Depo
        /// NEDEN? Yetkilendirme için departman türünü bilmemiz gerekiyor
        /// </summary>
        public UserRole DepartmentType { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Departmanın bağlı olduğu mağaza
        /// Many-to-One ilişki (Birden fazla departman bir mağazaya ait)
        /// </summary>
        public virtual Store Store { get; set; } = null!;

        /// <summary>
        /// Bu departmandaki çalışanlar
        /// One-to-Many ilişki (Bir departmanda birden fazla çalışan)
        /// </summary>
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}