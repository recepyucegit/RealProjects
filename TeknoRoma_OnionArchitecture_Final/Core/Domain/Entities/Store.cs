namespace Domain.Entities
{
    /// <summary>
    /// Mağaza/Şube Entity
    /// TEKNOROMA'nın 55 mağazasını temsil eder
    /// İstanbul: 20, İzmir: 13, Ankara: 13, Bursa: 9
    /// </summary>
    public class Store : BaseEntity
    {
        /// <summary>
        /// Mağaza Adı
        /// Örn: "TEKNOROMA Kadıköy", "TEKNOROMA Bornova"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Şehir
        /// </summary>
        public string City { get; set; } = null!;

        /// <summary>
        /// İlçe
        /// </summary>
        public string District { get; set; } = null!;

        /// <summary>
        /// Detaylı adres
        /// </summary>
        public string Address { get; set; } = null!;

        /// <summary>
        /// Telefon numarası
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Email adresi
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Mağaza aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== NAVIGATION PROPERTIES ======

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<TechnicalService> TechnicalServices { get; set; } = new List<TechnicalService>();
    }
}
