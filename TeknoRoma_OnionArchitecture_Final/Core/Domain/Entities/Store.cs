// ============================================================================
// Store.cs - Mağaza/Şube Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın fiziksel mağazalarını temsil eden entity.
// Her mağaza bağımsız bir satış noktasıdır ve kendi çalışanları,
// departmanları, satışları ve giderleri vardır.
//
// İŞ KURALI:
// - TEKNOROMA'nın 55 mağazası var: İstanbul(20), İzmir(13), Ankara(13), Bursa(9)
// - Her mağaza benzersiz bir isim ve adrese sahip olmalı
// - Mağaza silindiğinde (soft delete) ilişkili kayıtlar korunur
//
// VERİTABANI TABLOSU:
// Bu sınıf EF Core tarafından "Stores" tablosuna map edilir
// (Convention: Sınıf adının çoğul hali tablo adı olur)
// ============================================================================

namespace Domain.Entities
{
    /// <summary>
    /// Mağaza/Şube Entity Sınıfı
    ///
    /// BaseEntity'den Miras Alma:
    /// - ": BaseEntity" ifadesi bu sınıfın BaseEntity'den türediğini belirtir
    /// - Store otomatik olarak Id, CreatedDate, ModifiedDate, IsDeleted alanlarına sahip olur
    /// - Inheritance (Kalıtım) OOP'nin temel prensiplerinden biridir
    /// </summary>
    public class Store : BaseEntity
    {
        // ====================================================================
        // TEMEL BİLGİLER
        // ====================================================================

        /// <summary>
        /// Mağaza Adı
        ///
        /// AÇIKLAMA:
        /// - Mağazanın resmi adı (Örn: "TEKNOROMA Kadıköy Şubesi")
        /// - Benzersiz olmalı (aynı isimde iki mağaza olamaz)
        /// - UI'da dropdown listelerinde ve raporlarda görünür
        ///
        /// "= null!" AÇIKLAMASI:
        /// - C# 8.0 Nullable Reference Types özelliği ile geldi
        /// - "null!" derleyiciye "bu alan null olmayacak, güven bana" der
        /// - EF Core entity'yi veritabanından yüklerken değeri atar
        /// - Null-forgiving operator (!) kullanılır
        ///
        /// ALTERNATİF:
        /// - required keyword (C# 11): public required string Name { get; set; }
        /// - Constructor'da zorunlu parametre olarak almak
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Şehir Bilgisi
        ///
        /// AÇIKLAMA:
        /// - Mağazanın bulunduğu il
        /// - Bölgesel raporlama için kullanılır
        /// - Örn: "İstanbul", "İzmir", "Ankara", "Bursa"
        ///
        /// RAPORLAMA KULLANIMI:
        /// - "İstanbul'daki mağazaların toplam satışı"
        /// - "Şehir bazlı performans karşılaştırması"
        /// </summary>
        public string City { get; set; } = null!;

        /// <summary>
        /// İlçe Bilgisi
        ///
        /// AÇIKLAMA:
        /// - Mağazanın bulunduğu ilçe
        /// - Daha detaylı coğrafi filtreleme için
        /// - Örn: "Kadıköy", "Bornova", "Çankaya"
        /// </summary>
        public string District { get; set; } = null!;

        /// <summary>
        /// Detaylı Adres
        ///
        /// AÇIKLAMA:
        /// - Mağazanın tam açık adresi
        /// - Fatura, teslimat ve navigasyon için kullanılır
        /// - Sokak, numara, bina adı gibi bilgileri içerir
        /// </summary>
        public string Address { get; set; } = null!;

        // ====================================================================
        // İLETİŞİM BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Telefon Numarası
        ///
        /// AÇIKLAMA:
        /// - Mağazanın sabit telefon numarası
        /// - Müşteri hizmetleri ve iç iletişim için
        /// - Format: "0216 XXX XX XX" veya "+90 216 XXX XX XX"
        ///
        /// NOT:
        /// - Telefon string olarak saklanır (int değil)
        /// - Çünkü başındaki 0 kaybolmamalı
        /// - Uluslararası format (+90) desteklenmeli
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// E-posta Adresi
        ///
        /// AÇIKLAMA:
        /// - Mağazanın kurumsal email adresi
        /// - Örn: "kadikoy@teknoroma.com"
        /// - Otomatik bildirimler bu adrese gönderilir
        /// </summary>
        public string Email { get; set; } = null!;

        // ====================================================================
        // DURUM BİLGİSİ
        // ====================================================================

        /// <summary>
        /// Mağaza Aktif mi?
        ///
        /// AÇIKLAMA:
        /// - true: Mağaza açık ve faaliyette
        /// - false: Mağaza geçici olarak kapalı (tadilat, sezonluk kapanış vs.)
        ///
        /// "= true" VARSAYILAN DEĞER:
        /// - Yeni oluşturulan mağaza varsayılan olarak aktif
        /// - Property initializer kullanılarak set edilir
        ///
        /// IsActive vs IsDeleted FARKI:
        /// - IsActive=false: Geçici kapalı, tekrar açılabilir
        /// - IsDeleted=true: Kalıcı olarak kapatılmış (soft delete)
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ====================================================================
        // NAVIGATION PROPERTIES - İLİŞKİSEL GEZİNME ÖZELLİKLERİ
        // ====================================================================
        // Navigation Property Nedir?
        // - İlişkili entity'lere kod üzerinden erişim sağlar
        // - EF Core bu property'leri kullanarak JOIN işlemi yapar
        // - Lazy Loading veya Eager Loading ile yüklenir
        //
        // Collection Navigation Property:
        // - Bir mağazanın BİRDEN FAZLA çalışanı olabilir (One-to-Many)
        // - ICollection<T> interface'i kullanılır
        // - List<T> ile initialize edilir (null reference hatası önlenir)
        //
        // "virtual" Anahtar Kelimesi:
        // - Lazy Loading için gereklidir
        // - EF Core proxy sınıfı oluşturarak override eder
        // - İlişkili veriler sadece erişildiğinde yüklenir
        //
        // "new List<T>()" ile Initialize:
        // - Null reference exception önlenir
        // - store.Employees.Add() güvenle çağrılabilir
        // - Boş koleksiyon ile başlar, veriler sonra eklenir
        // ====================================================================

        /// <summary>
        /// Bu mağazada çalışan personeller
        ///
        /// İLİŞKİ: Store (1) → Employee (N) - Bir mağazada birden fazla çalışan
        ///
        /// KULLANIM:
        /// var employees = store.Employees;  // Mağazanın tüm çalışanları
        /// var count = store.Employees.Count; // Çalışan sayısı
        /// </summary>
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

        /// <summary>
        /// Bu mağazadaki departmanlar
        ///
        /// İLİŞKİ: Store (1) → Department (N) - Bir mağazada birden fazla departman
        ///
        /// ÖRNEK DEPARTMANLAR:
        /// - Bilgisayar, Telefon, Tablet, TV, Beyaz Eşya, Küçük Ev Aletleri
        /// </summary>
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

        /// <summary>
        /// Bu mağazada yapılan satışlar
        ///
        /// İLİŞKİ: Store (1) → Sale (N) - Bir mağazada birden fazla satış
        ///
        /// RAPORLAMA:
        /// - "Kadıköy şubesinin aylık satışları"
        /// - "En çok satış yapan mağaza"
        /// </summary>
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

        /// <summary>
        /// Bu mağazanın giderleri
        ///
        /// İLİŞKİ: Store (1) → Expense (N) - Bir mağazanın birden fazla gideri
        ///
        /// ÖRNEK GİDERLER:
        /// - Kira, elektrik, personel maaşları, temizlik, bakım
        /// </summary>
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        /// <summary>
        /// Bu mağazadaki teknik servis kayıtları
        ///
        /// İLİŞKİ: Store (1) → TechnicalService (N)
        ///
        /// AÇIKLAMA:
        /// - Müşteriler cihazlarını tamir için mağazaya getirir
        /// - Her mağaza kendi servis kayıtlarını tutar
        /// </summary>
        public virtual ICollection<TechnicalService> TechnicalServices { get; set; } = new List<TechnicalService>();
    }
}
