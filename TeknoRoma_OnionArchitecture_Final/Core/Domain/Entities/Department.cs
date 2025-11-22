using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Departman Entity
    /// TEKNOROMA'da toplam 30 departman var
    /// </summary>
    public class Department : BaseEntity
    {
        /// <summary>
        /// Departman Adı
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Departman Açıklaması
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Hangi mağazaya ait? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Departmanın rolü/türü
        /// </summary>
        public UserRole DepartmentType { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
