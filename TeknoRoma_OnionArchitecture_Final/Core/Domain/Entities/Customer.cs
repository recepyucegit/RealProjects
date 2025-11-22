using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Müşteri Entity
    /// </summary>
    public class Customer : BaseEntity
    {
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
        /// Doğum Tarihi
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Cinsiyet
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// Email adresi
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Telefon numarası
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Adres
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Şehir
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Müşteri aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== CALCULATED PROPERTIES ======

        public string FullName => $"{FirstName} {LastName}";

        public int? Age => BirthDate.HasValue
            ? DateTime.Now.Year - BirthDate.Value.Year
            : null;


        // ====== NAVIGATION PROPERTIES ======

        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
