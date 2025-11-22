// ============================================================================
// Employee.cs - Çalışan Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'da çalışan personelleri temsil eden entity.
// Satış personeli, teknik servis, depo, muhasebe ve yöneticileri kapsar.
//
// İŞ KURALLARI:
// - TEKNOROMA'da toplam 258 çalışan var
// - Her çalışan bir mağazaya ve departmana bağlı
// - TC Kimlik numarası benzersiz olmalı
// - ASP.NET Identity ile authentication entegrasyonu var
//
// GÜVENLİK:
// - IdentityUserId ile ASP.NET Identity sistemine bağlanır
// - Role alanı yetkilendirme için kullanılır
// - Şifre bu tabloda tutulmaz (Identity tablosunda)
// ============================================================================

using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Çalışan Entity Sınıfı
    ///
    /// İKİ TABLO ARASINDA KÖPRÜ:
    /// - AspNetUsers (Identity): Kimlik doğrulama bilgileri (email, password hash)
    /// - Employees: İş ile ilgili bilgiler (maaş, departman, satış kotası)
    ///
    /// Bu pattern, Identity sistemini genişletmenin tercih edilen yoludur.
    /// IdentityUser'ı doğrudan extend etmek yerine ayrı bir tablo kullanılır.
    /// </summary>
    public class Employee : BaseEntity
    {
        // ====================================================================
        // KİMLİK BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// ASP.NET Identity User ID
        ///
        /// AÇIKLAMA:
        /// - AspNetUsers tablosundaki Id ile eşleşir
        /// - ASP.NET Identity GUID formatında Id kullanır
        /// - Authentication (giriş) işlemleri için bu ID kullanılır
        ///
        /// NEDEN STRING?
        /// - Identity User Id'si string tipinde (GUID.ToString())
        /// - Örn: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
        ///
        /// KULLANIM:
        /// - Login sonrası: User.FindFirstValue(ClaimTypes.NameIdentifier)
        /// - Employee bulma: employees.First(e => e.IdentityUserId == userId)
        /// </summary>
        public string IdentityUserId { get; set; } = null!;

        /// <summary>
        /// TC Kimlik Numarası (UNIQUE)
        ///
        /// AÇIKLAMA:
        /// - 11 haneli Türkiye Cumhuriyeti kimlik numarası
        /// - Her çalışan için benzersiz (unique constraint)
        /// - SGK, maaş bordrosu ve resmi işlemler için gerekli
        ///
        /// DOĞRULAMA KURALLARI:
        /// - 11 hane olmalı
        /// - İlk hane 0 olamaz
        /// - Son hane algoritma ile doğrulanabilir
        ///
        /// GÜVENLİK NOTU:
        /// - KVKK kapsamında hassas veri
        /// - Sadece yetkili kullanıcılar görebilmeli
        /// - Log'lara yazılmamalı, maskelenmeli
        /// </summary>
        public string IdentityNumber { get; set; } = null!;

        /// <summary>
        /// Çalışan Adı
        ///
        /// AÇIKLAMA:
        /// - Çalışanın adı (ilk isim)
        /// - Birden fazla ad varsa hepsi bu alanda tutulur
        /// - Örn: "Mehmet", "Ayşe Nur"
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Çalışan Soyadı
        ///
        /// AÇIKLAMA:
        /// - Çalışanın soyadı
        /// - Türkiye'de tek soyad kullanılır
        /// - Örn: "Yılmaz", "Öztürk"
        /// </summary>
        public string LastName { get; set; } = null!;

        // ====================================================================
        // İLETİŞİM BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// E-posta Adresi
        ///
        /// AÇIKLAMA:
        /// - Çalışanın kurumsal veya kişisel e-postası
        /// - Login için kullanılır (Identity ile senkron)
        /// - Şifre sıfırlama linkleri bu adrese gönderilir
        ///
        /// UNIQUE CONSTRAINT:
        /// - Aynı email ile birden fazla hesap açılamaz
        /// - Identity tablosunda da unique
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Telefon Numarası
        ///
        /// AÇIKLAMA:
        /// - Çalışanın cep telefonu
        /// - Acil durum iletişimi ve SMS bildirimleri için
        /// - Format: "05XX XXX XX XX"
        /// </summary>
        public string Phone { get; set; } = null!;

        // ====================================================================
        // KİŞİSEL BİLGİLER
        // ====================================================================

        /// <summary>
        /// Doğum Tarihi
        ///
        /// AÇIKLAMA:
        /// - Çalışanın doğum tarihi
        /// - Yaş hesaplama ve doğum günü hatırlatmaları için
        /// - İş kanunu: 18 yaşından küçük çalıştırılamaz (bazı istisnalar hariç)
        ///
        /// VERİTABANI:
        /// - SQL Server'da date veya datetime2 olarak saklanır
        /// - Saat bilgisi önemsiz, sadece tarih
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// İşe Başlama Tarihi
        ///
        /// AÇIKLAMA:
        /// - Çalışanın TEKNOROMA'da işe başladığı tarih
        /// - Kıdem hesaplaması için kullanılır
        /// - Yıllık izin hakkı hesabı için gerekli
        ///
        /// HESAPLAMALAR:
        /// - Kıdem: (DateTime.Now - HireDate).TotalDays / 365
        /// - 1-5 yıl: 14 gün izin
        /// - 5+ yıl: 20 gün izin
        /// </summary>
        public DateTime HireDate { get; set; }

        // ====================================================================
        // MAAŞ VE PERFORMANS
        // ====================================================================

        /// <summary>
        /// Aylık Maaş (TL)
        ///
        /// AÇIKLAMA:
        /// - Brüt aylık maaş tutarı (TL)
        /// - SGK ve vergi kesintileri bu tutar üzerinden hesaplanır
        /// - Net maaş = Brüt - SGK - Gelir Vergisi - Damga Vergisi
        ///
        /// GÜVENLİK:
        /// - Hassas bilgi, sadece HR ve yöneticiler görebilir
        /// - Raporlarda maskelenmeli veya yetki kontrolü yapılmalı
        ///
        /// decimal SEÇİMİ:
        /// - Para birimi için her zaman decimal
        /// - Kuruş hassasiyeti için 2 ondalık hane
        /// </summary>
        public decimal Salary { get; set; }

        /// <summary>
        /// Satış Kotası (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Sadece satış personeli için geçerli
        /// - Aylık hedef satış tutarı (TL)
        /// - Prim hesabı için kullanılır
        ///
        /// NULLABLE (decimal?):
        /// - Satış personeli olmayan çalışanlar için null
        /// - Örn: Muhasebeci, depo personeli → SalesQuota = null
        /// - Satışçı → SalesQuota = 100000 (100K TL aylık hedef)
        ///
        /// PRİM HESABI:
        /// - Kota aşımında %X prim
        /// - Örn: Kota 100K, satış 120K → 20K üzerinden prim
        /// </summary>
        public decimal? SalesQuota { get; set; }

        // ====================================================================
        // İŞ İLİŞKİLERİ (FOREIGN KEYS)
        // ====================================================================

        /// <summary>
        /// Mağaza ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Çalışanın görev yaptığı mağaza
        /// - Stores tablosundaki Id ile eşleşir
        /// - Bir çalışan aynı anda tek mağazada çalışabilir
        ///
        /// TRANSFER DURUMU:
        /// - Mağaza değişikliğinde bu alan güncellenir
        /// - Transfer geçmişi için ayrı tablo tutulabilir
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Departman ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Çalışanın bağlı olduğu departman
        /// - Departments tablosundaki Id ile eşleşir
        ///
        /// ÖRNEK DEPARTMANLAR:
        /// - Satış, Kasa, Depo, Teknik Servis, Muhasebe, Yönetim
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// Kullanıcı Rolü (Enum)
        ///
        /// AÇIKLAMA:
        /// - Çalışanın sistemdeki yetki seviyesi
        /// - UserRole enum'undan değer alır
        /// - Authorization (yetkilendirme) için kullanılır
        ///
        /// ROL DEĞERLERİ:
        /// - GenelMudur: Tüm sisteme erişim
        /// - SubeMuduru: Kendi mağazasının işlemleri
        /// - KasaSatis: Satış ve kasa işlemleri
        /// - Depo: Stok ve ürün yönetimi
        /// - Muhasebe: Finansal raporlar ve giderler
        /// - TeknikServis: Servis kayıtları
        ///
        /// KULLANIM:
        /// [Authorize(Roles = "GenelMudur,SubeMuduru")]
        /// public IActionResult AdminPanel() { }
        /// </summary>
        public UserRole Role { get; set; }

        // ====================================================================
        // DURUM
        // ====================================================================

        /// <summary>
        /// Çalışan Aktif mi?
        ///
        /// AÇIKLAMA:
        /// - true: Aktif çalışan, sisteme giriş yapabilir
        /// - false: Pasif çalışan, sisteme giriş yapamaz
        ///
        /// KULLANIM SENARYOLARI:
        /// - İşten ayrılan çalışan: IsActive = false (soft delete yerine)
        /// - Uzun süreli izindeki çalışan: IsActive = false
        /// - Askere giden çalışan: IsActive = false
        ///
        /// LOGIN KONTROLÜ:
        /// if (!employee.IsActive)
        ///     return "Hesabınız pasif durumda";
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ====================================================================
        // CALCULATED PROPERTIES - HESAPLANAN ÖZELLİKLER
        // ====================================================================

        /// <summary>
        /// Tam Ad (Calculated)
        ///
        /// AÇIKLAMA:
        /// - Ad ve soyadı birleştirir
        /// - UI'da kullanıcı adı gösterimi için
        ///
        /// STRING INTERPOLATION ($"..."):
        /// - C# 6.0 ile gelen özellik
        /// - String birleştirmenin modern yolu
        /// - $"{FirstName} {LastName}" = FirstName + " " + LastName
        ///
        /// EXPRESSION BODY (=>):
        /// - Tek satırlık property/method tanımı
        /// - Get-only property oluşturur
        /// - Veritabanında saklanmaz
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Çalıştığı Mağaza (Navigation Property)
        ///
        /// İLİŞKİ: Employee (N) → Store (1)
        /// - Birden fazla çalışan aynı mağazada çalışabilir
        /// </summary>
        public virtual Store Store { get; set; } = null!;

        /// <summary>
        /// Bağlı Olduğu Departman (Navigation Property)
        ///
        /// İLİŞKİ: Employee (N) → Department (1)
        /// </summary>
        public virtual Department Department { get; set; } = null!;

        /// <summary>
        /// Yaptığı Satışlar (Collection Navigation Property)
        ///
        /// İLİŞKİ: Employee (1) → Sale (N)
        /// - Bir çalışan birden fazla satış yapabilir
        ///
        /// PERFORMANS RAPORU İÇİN:
        /// var totalSales = employee.Sales.Sum(s => s.TotalAmount);
        /// var saleCount = employee.Sales.Count;
        /// </summary>
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

        /// <summary>
        /// Kaydettiği Giderler (Collection Navigation Property)
        ///
        /// İLİŞKİ: Employee (1) → Expense (N)
        /// - Muhasebe personeli gider kaydı oluşturabilir
        /// </summary>
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        /// <summary>
        /// Atandığı Teknik Servis Kayıtları
        ///
        /// İLİŞKİ: Employee (1) → TechnicalService (N)
        /// - Teknik servis personeline atanan tamir işleri
        ///
        /// İSİMLENDİRME:
        /// - "Assigned" prefix'i ilişkinin yönünü belirtir
        /// - Çalışan bu servis kayıtlarına ATANMİŞ
        /// </summary>
        public virtual ICollection<TechnicalService> AssignedTechnicalServices { get; set; } = new List<TechnicalService>();
    }
}
