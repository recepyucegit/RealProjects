// ============================================================================
// IEmployeeService.cs - Çalışan Servis Interface
// ============================================================================
// AÇIKLAMA:
// Çalışan yönetimi için iş mantığı katmanı.
// ASP.NET Identity ile entegrasyon ve insan kaynakları işlevleri.
//
// IDENTITY ENTEGRASYONU:
// Her Employee kaydı bir IdentityUser'a bağlıdır.
// Login: IdentityUser -> IdentityUserId -> Employee
//
// YETKİ YÖNETİMİ:
// Identity Roles kullanılır (Admin, Manager, Cashier, Technician)
// Employee.Role <-> IdentityUser.Roles senkronize tutulmalı
//
// GÜVENLİK:
// TC Kimlik no KVKK kapsamında hassas veri!
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    /// <summary>
    /// Çalışan Servis Interface
    ///
    /// İK YÖNETİMİ: Personel kayıt, güncelleme
    /// KİMLİK DOĞRULAMA: Login sonrası çalışan bilgisi
    /// PERFORMANS: En çok satan personel
    /// </summary>
    public interface IEmployeeService
    {
        // ========================================================================
        // SORGULAMA (QUERY) METODLARI
        // ========================================================================

        /// <summary>
        /// ID ile Çalışan Getir
        ///
        /// İLİŞKİLİ VERİ: Store, Department include edilir
        /// DETAY SAYFASI: Çalışan profil görüntüleme
        /// </summary>
        Task<Employee?> GetByIdAsync(int id);

        /// <summary>
        /// TC Kimlik ile Çalışan Getir
        ///
        /// BORDRO İŞLEMİ: TC ile personel eşleştirme
        /// SGK BİLDİRGESİ: TC bazlı sorgulama
        ///
        /// TC DOĞRULAMA: 11 haneli, algoritma kontrolü yapılabilir
        /// </summary>
        Task<Employee?> GetByIdentityNumberAsync(string identityNumber);

        /// <summary>
        /// Identity User ID ile Çalışan Getir
        ///
        /// LOGIN SONRASI KRİTİK METOD!
        ///
        /// AKIŞ:
        /// 1. Kullanıcı login olur -> IdentityUser
        /// 2. User.FindFirstValue(ClaimTypes.NameIdentifier) -> UserId
        /// 3. GetByIdentityUserIdAsync(userId) -> Employee
        /// 4. Employee bilgisi session'da tutulur
        ///
        /// ÖRNEK (Controller'da):
        /// var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        /// var employee = await _employeeService.GetByIdentityUserIdAsync(userId);
        /// </summary>
        Task<Employee?> GetByIdentityUserIdAsync(string identityUserId);

        /// <summary>
        /// Tüm Çalışanları Getir
        ///
        /// İK LİSTESİ: Admin personel listesi sayfası
        /// FİLTRELEME: Client-side veya sonraki sorgularla
        /// </summary>
        Task<IEnumerable<Employee>> GetAllAsync();

        /// <summary>
        /// Mağaza Çalışanları
        ///
        /// ŞUBE YÖNETİMİ: "Ankara-1 mağazasında kim çalışıyor?"
        /// YETKİ: Şube müdürü sadece kendi personelini görür
        ///
        /// ÖRNEK:
        /// var ankaraPersonel = await GetByStoreAsync(1);
        /// </summary>
        Task<IEnumerable<Employee>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Departman Çalışanları
        ///
        /// ORGANİZASYON ŞEMASI: Departman bazlı listeleme
        /// "Teknik Servis departmanında kimler var?"
        /// </summary>
        Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);

        /// <summary>
        /// Role Göre Çalışanlar
        ///
        /// YETKİ BAZLI LİSTE:
        /// - GetByRoleAsync(UserRole.Kasiyer) -> Tüm kasiyerler
        /// - GetByRoleAsync(UserRole.Teknisyen) -> Tüm teknisyenler
        ///
        /// İŞ DAĞILIMI: Teknisyenlere iş atama listesi
        /// </summary>
        Task<IEnumerable<Employee>> GetByRoleAsync(UserRole role);

        /// <summary>
        /// Aktif Çalışanlar
        ///
        /// İŞTEN ÇIKMAMIŞLAR: IsDeleted = false
        /// DROPDOWN: Çalışan seçim listelerinde
        /// </summary>
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();

        /// <summary>
        /// En Çok Satan Personel
        ///
        /// PRİM HESAPLAMA: Satış tutarı toplamına göre sıralı
        /// PERFORMANS TABLOSU: Dashboard widget
        ///
        /// PARAMETRELER:
        /// - count: İlk kaç kişi? (Top 5, Top 10)
        /// - startDate/endDate: Dönem filtresi
        ///
        /// ÖRNEK:
        /// // Bu ayın en iyi 5 satıcısı
        /// var top5 = await GetTopSellersAsync(5,
        ///     new DateTime(2024, 1, 1),
        ///     new DateTime(2024, 1, 31));
        /// </summary>
        Task<IEnumerable<Employee>> GetTopSellersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);

        // ========================================================================
        // KOMUT (COMMAND) METODLARI
        // ========================================================================

        /// <summary>
        /// Yeni Çalışan Oluştur
        ///
        /// İŞE ALIM SÜRECİ:
        /// 1. Employee kaydı oluştur
        /// 2. IdentityUser oluştur
        /// 3. Role ata
        /// 4. Employee.IdentityUserId güncelle
        ///
        /// VALİDASYON:
        /// - TC benzersiz olmalı
        /// - Email geçerli format
        /// - Maaş >= asgari ücret
        /// </summary>
        Task<Employee> CreateAsync(Employee employee);

        /// <summary>
        /// Çalışan Güncelle
        ///
        /// GÜNCELLEME SENARYOLARI:
        /// - Terfi: Role değişikliği -> IdentityUser role güncelle
        /// - Transfer: StoreId değişikliği
        /// - Zam: Salary değişikliği
        ///
        /// AUDİT: UpdatedAt otomatik güncellenir
        /// </summary>
        Task UpdateAsync(Employee employee);

        /// <summary>
        /// Çalışan Sil (İşten Çıkarma)
        ///
        /// SOFT DELETE: IsDeleted = true
        ///
        /// İŞTEN ÇIKIŞ İŞLEMLERİ:
        /// - IdentityUser deaktif edilir
        /// - Bordro geçmişi korunur
        /// - Atanan teknik servis talepleri devredilir
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// TC Kimlik Benzersizlik Kontrolü
        ///
        /// AÇIKLAMA: Repository'deki ile aynı mantık
        ///
        /// YENİ PERSONEL: excludeId = null
        /// GÜNCELLEME: excludeId = mevcut ID
        ///
        /// ÖRNEK:
        /// if (await IsIdentityNumberTakenAsync("12345678901"))
        ///     return BadRequest("Bu TC zaten kayıtlı");
        /// </summary>
        Task<bool> IsIdentityNumberTakenAsync(string identityNumber, int? excludeId = null);
    }
}
