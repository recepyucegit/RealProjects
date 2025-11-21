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
        /// Enum yerine string çünkü yeni şehirler eklenebilir
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
        /// Email adresi (raporlar için)
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Mağaza aktif mi?
        /// Kapatılan mağazalar için false olur
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== NAVIGATION PROPERTIES ======
        // NEDEN? Entity Framework ilişkileri yönetmek için kullanır
        // Lazy/Eager Loading için gerekli

        /// <summary>
        /// Bu mağazadaki çalışanlar
        /// Bir mağazanın birden fazla çalışanı olabilir (One-to-Many)
        /// VIRTUAL: Lazy Loading için (ihtiyaç olduğunda yüklenir)
        /// </summary>
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

        /// <summary>
        /// Bu mağazadaki departmanlar
        /// Bir mağazada birden fazla departman olabilir
        /// </summary>
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

        /// <summary>
        /// Bu mağazada yapılan satışlar
        /// Bir mağazada birden fazla satış olabilir
        /// </summary>
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

        /// <summary>
        /// Bu mağazanın giderleri
        /// </summary>
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        /// <summary>
        /// Bu mağazanın teknik servis kayıtları
        /// </summary>
        public virtual ICollection<TechnicalService> TechnicalServices { get; set; } = new List<TechnicalService>();
    }
}