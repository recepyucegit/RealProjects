using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Çalışan Entity
    /// TEKNOROMA'da toplam 258 çalışan var
    /// </summary>
    public class Employee : BaseEntity
    {
        /// <summary>
        /// ASP.NET Identity User ID
        /// </summary>
        public string IdentityUserId { get; set; } = null!;

        /// <summary>
        /// TC Kimlik Numarası (UNIQUE)
        /// </summary>
        public string IdentityNumber { get; set; } = null!;

        /// <summary>
        /// Ad
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Soyad
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Email adresi
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Telefon numarası
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Doğum Tarihi
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// İşe başlama tarihi
        /// </summary>
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Maaş
        /// </summary>
        public decimal Salary { get; set; }

        /// <summary>
        /// Hangi mağazada çalışıyor? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Hangi departmanda çalışıyor? (Foreign Key)
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// Çalışanın rolü
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Satış kotası (sadece satış ekibi için)
        /// </summary>
        public decimal? SalesQuota { get; set; }

        /// <summary>
        /// Çalışan aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== CALCULATED PROPERTY ======

        public string FullName => $"{FirstName} {LastName}";


        // ====== NAVIGATION PROPERTIES ======

        public virtual Store Store { get; set; } = null!;
        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<TechnicalService> AssignedTechnicalServices { get; set; } = new List<TechnicalService>();
    }
}
