namespace TeknoRoma.Entities;

/// <summary>
/// Mağaza/Şube Entity
/// TEKNOROMA'nın 55 mağazasını temsil eder
/// İstanbul: 20, İzmir: 13, Ankara: 13, Bursa: 9
///
/// NEDEN ÖNEMLİ?
/// - Her şube bağımsız bir profit center (kar merkezi)
/// - Haluk Bey her şubenin performansını ayrı raporlayıp karşılaştırır
/// - Çalışanlar bir şubeye atanır
/// - Satışlar ve giderler şube bazında takip edilir
/// </summary>
public class Store : BaseEntity
{
    /// <summary>
    /// Mağaza Adı
    /// Örn: "TEKNOROMA Kadıköy", "TEKNOROMA Bornova", "TEKNOROMA Kızılay"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mağaza Kodu
    /// Unique identifier - Kısa kod
    /// Örn: "IST-KDK-01", "IZM-BRN-01"
    /// </summary>
    public string StoreCode { get; set; } = string.Empty;

    /// <summary>
    /// Şehir
    /// Enum yerine string çünkü yeni şehirler eklenebilir
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// İlçe
    /// </summary>
    public string District { get; set; } = string.Empty;

    /// <summary>
    /// Detaylı adres
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Telefon numarası
    /// Müşteri iletişimi için
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email adresi
    /// Raporlar için kullanılır (Haluk Bey'e otomatik raporlar gönderilir)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mağaza aktif mi?
    /// Kapatılan mağazalar için false olur
    /// NEDEN? Mağaza kapatılınca veriler silinmez (Soft Delete mantığı)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Açılış tarihi
    /// İş analizi için önemli (yeni mağazalar farklı performans gösterir)
    /// </summary>
    public DateTime OpeningDate { get; set; }

    /// <summary>
    /// Mağaza metrekaresi
    /// Kira hesaplaması ve performans analizi için
    /// </summary>
    public decimal? SquareMeters { get; set; }


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
    /// Bir mağazada birden fazla departman olabilir (One-to-Many)
    /// Örn: Bilgisayar, Cep Telefonu, Kamera, Satış, Depo, Muhasebe
    /// </summary>
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    /// <summary>
    /// Bu mağazada yapılan satışlar
    /// Bir mağazada birden fazla satış olabilir (One-to-Many)
    /// Haluk Bey: "Hangi mağazam daha çok satış yapıyor"
    /// </summary>
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    /// <summary>
    /// Bu mağazanın giderleri
    /// Maaşlar, faturalar, teknik altyapı giderleri
    /// Feyza Paragöz: "Her mağazanın giderlerini ayrı takip etmeliyim"
    /// </summary>
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    /// <summary>
    /// Bu mağazanın teknik servis kayıtları
    /// Özgün Kablocu: "Hangi mağazada daha çok teknik sorun var"
    /// </summary>
    public virtual ICollection<TechnicalService> TechnicalServices { get; set; } = new List<TechnicalService>();
}
