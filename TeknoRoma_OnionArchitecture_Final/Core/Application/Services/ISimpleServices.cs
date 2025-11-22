// ============================================================================
// ISimpleServices.cs - Basit Servis Interface'leri
// ============================================================================
// AÇIKLAMA:
// Daha az karmaşık entity'ler için servis interface'lerini içerir.
// Bu entity'ler genellikle lookup (referans) tabloları olarak kullanılır.
//
// NEDEN TEK DOSYADA?
// Category, Supplier, Store, Department entity'leri:
// - Basit CRUD operasyonları
// - Karmaşık iş kuralları yok
// - Az sayıda metod
//
// Her biri için ayrı dosya oluşturmak gereksiz olacaktı.
// Ancak büyük projelerde ayrı dosyalara bölünebilir.
// ============================================================================

using Domain.Entities;

namespace Application.Services
{
    // ========================================================================
    // KATEGORİ SERVİSİ
    // ========================================================================

    /// <summary>
    /// Kategori Servis Interface
    ///
    /// ÜRÜN SINIFLANDIRMA: Telefonlar, Tabletler, Aksesuarlar
    /// MENÜ YAPISI: E-ticaret menüsü için
    ///
    /// HİYERARŞİ: Şu an tek seviye (Parent-Child ilişkisi eklenebilir)
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// ID ile Kategori Getir
        ///
        /// DETAY/DÜZENLEME: Admin kategori düzenleme sayfası
        /// </summary>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm Kategoriler
        ///
        /// ADMIN: Kategori yönetim sayfası
        /// DROPDOWN: Ürün ekleme formunda kategori seçimi
        /// </summary>
        Task<IEnumerable<Category>> GetAllAsync();

        /// <summary>
        /// Aktif Kategoriler
        ///
        /// WEB SİTESİ: Müşteriye gösterilen menü
        /// SOFT DELETE: IsDeleted = false
        /// </summary>
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();

        /// <summary>
        /// Yeni Kategori Oluştur
        ///
        /// VALİDASYON: Kategori adı benzersiz olmalı
        /// </summary>
        Task<Category> CreateAsync(Category category);

        /// <summary>
        /// Kategori Güncelle
        ///
        /// AD DEĞİŞİKLİĞİ: Ürünlerdeki kategori adı otomatik güncellenmez
        /// (normalizasyon - sadece ID tutulur)
        /// </summary>
        Task UpdateAsync(Category category);

        /// <summary>
        /// Kategori Sil
        ///
        /// DİKKAT: Kategoride ürün varsa silinememeli!
        /// Önce GetProductCountAsync ile kontrol edilmeli
        ///
        /// SOFT DELETE: IsDeleted = true
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Kategorideki Ürün Sayısı
        ///
        /// SİLME KONTROLÜ: Ürün varsa kategori silinemez
        /// İSTATİSTİK: Dashboard'da kategori bazlı özet
        ///
        /// ÖRNEK:
        /// var count = await GetProductCountAsync(categoryId);
        /// if (count > 0)
        ///     return BadRequest("Kategoride ürün var, silinemez!");
        /// </summary>
        Task<int> GetProductCountAsync(int categoryId);
    }

    // ========================================================================
    // TEDARİKÇİ SERVİSİ
    // ========================================================================

    /// <summary>
    /// Tedarikçi Servis Interface
    ///
    /// MAL TEDARİĞİ: Ürün tedarik edilen firmalar
    /// CARİ HESAP: Borç/alacak takibi (SupplierTransaction ile)
    /// </summary>
    public interface ISupplierService
    {
        /// <summary>
        /// ID ile Tedarikçi Getir
        ///
        /// DETAY SAYFASI: Tedarikçi profili
        /// </summary>
        Task<Supplier?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm Tedarikçiler
        ///
        /// LİSTE SAYFASI: Admin tedarikçi yönetimi
        /// </summary>
        Task<IEnumerable<Supplier>> GetAllAsync();

        /// <summary>
        /// Aktif Tedarikçiler
        ///
        /// ALIM FORMU: Dropdown'da sadece aktif olanlar
        /// İŞ İLİŞKİSİ DEVAMİ: IsDeleted = false
        /// </summary>
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();

        /// <summary>
        /// Yeni Tedarikçi Oluştur
        ///
        /// VALİDASYON:
        /// - Firma adı zorunlu
        /// - Vergi no benzersiz (opsiyonel)
        /// - İletişim bilgisi zorunlu
        /// </summary>
        Task<Supplier> CreateAsync(Supplier supplier);

        /// <summary>
        /// Tedarikçi Güncelle
        ///
        /// GÜNCELLEME: İletişim bilgisi, adres, banka bilgisi
        /// </summary>
        Task UpdateAsync(Supplier supplier);

        /// <summary>
        /// Tedarikçi Sil
        ///
        /// SOFT DELETE: IsDeleted = true
        /// DİKKAT: Açık borç varsa silinemez (iş kuralı)
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Tedarikçiden Toplam Alım
        ///
        /// CARİ ANALİZ: "Samsung'dan bu yıl ne kadar mal aldık?"
        /// TEDARİKÇİ DEĞERLENDİRME: En çok iş yaptığımız tedarikçiler
        ///
        /// ÖRNEK:
        /// var yillikAlim = await GetTotalPurchaseAsync(supplierId: 1,
        ///     startDate: new DateTime(2024, 1, 1),
        ///     endDate: DateTime.Now);
        /// </summary>
        Task<decimal> GetTotalPurchaseAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
    }

    // ========================================================================
    // MAĞAZA SERVİSİ
    // ========================================================================

    /// <summary>
    /// Mağaza (Şube) Servis Interface
    ///
    /// ÇOK ŞUBE YÖNETİMİ: Zincir mağaza yapısı
    /// STOK TAKİBİ: Şube bazlı stok (genişletilebilir)
    /// PERSONEL: Şube bazlı personel ataması
    /// </summary>
    public interface IStoreService
    {
        /// <summary>
        /// ID ile Mağaza Getir
        ///
        /// DETAY: Mağaza bilgileri
        /// </summary>
        Task<Store?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm Mağazalar
        ///
        /// ADMIN: Şube yönetim sayfası
        /// </summary>
        Task<IEnumerable<Store>> GetAllAsync();

        /// <summary>
        /// Aktif Mağazalar
        ///
        /// DROPDOWN: Şube seçimi (satış formu, personel atama)
        /// </summary>
        Task<IEnumerable<Store>> GetActiveStoresAsync();

        /// <summary>
        /// Şehre Göre Mağazalar
        ///
        /// COĞRAFİ FİLTRE: "Ankara'daki mağazalar"
        /// RAPORLAMA: İl bazlı performans analizi
        /// </summary>
        Task<IEnumerable<Store>> GetByCityAsync(string city);

        /// <summary>
        /// Yeni Mağaza Oluştur
        ///
        /// ŞUBE AÇILIŞI:
        /// - Adres zorunlu
        /// - Şehir zorunlu
        /// - İletişim bilgisi zorunlu
        /// </summary>
        Task<Store> CreateAsync(Store store);

        /// <summary>
        /// Mağaza Güncelle
        ///
        /// GÜNCELLEME: Adres, telefon, çalışma saatleri
        /// </summary>
        Task UpdateAsync(Store store);

        /// <summary>
        /// Mağaza Sil (Kapat)
        ///
        /// ŞUBE KAPANIŞI:
        /// - Açık satış varsa tamamlanmalı
        /// - Personel başka şubeye transfer edilmeli
        /// - Stok başka şubeye devredilmeli (manuel)
        ///
        /// SOFT DELETE: IsDeleted = true
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Mağaza Toplam Satışı
        ///
        /// PERFORMANS: "Ankara-1 mağazası bu ay ne kadar sattı?"
        /// ŞUBE KARŞILAŞTIRMA: En çok satan şube
        ///
        /// ÖRNEK:
        /// var ankara1Satis = await GetTotalSalesAsync(storeId: 1,
        ///     startDate: new DateTime(2024, 1, 1),
        ///     endDate: new DateTime(2024, 1, 31));
        /// </summary>
        Task<decimal> GetTotalSalesAsync(int storeId, DateTime? startDate = null, DateTime? endDate = null);
    }

    // ========================================================================
    // DEPARTMAN SERVİSİ
    // ========================================================================

    /// <summary>
    /// Departman Servis Interface
    ///
    /// ORGANİZASYON YAPISI: Satış, Teknik Servis, Depo, vb.
    /// PERSONEL ATAMA: Çalışan -> Departman ilişkisi
    ///
    /// ŞUBE BAĞIMLI: Her departman bir şubeye bağlı
    /// (Ankara-1 Satış, Ankara-1 Teknik, İstanbul-1 Satış, ...)
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// ID ile Departman Getir
        ///
        /// DETAY: Departman bilgileri
        /// </summary>
        Task<Department?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm Departmanlar
        ///
        /// ADMIN: Organizasyon yapısı görüntüleme
        /// </summary>
        Task<IEnumerable<Department>> GetAllAsync();

        /// <summary>
        /// Mağazanın Departmanları
        ///
        /// ŞUBE BAZLI: "Ankara-1 mağazasında hangi departmanlar var?"
        /// PERSONEL ATAMA: Dropdown'da sadece o şubenin departmanları
        ///
        /// ÖRNEK:
        /// var ankara1Deps = await GetByStoreAsync(storeId: 1);
        /// // Satış, Teknik Servis, Depo, Muhasebe...
        /// </summary>
        Task<IEnumerable<Department>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Yeni Departman Oluştur
        ///
        /// VALİDASYON:
        /// - Departman adı zorunlu
        /// - StoreId geçerli olmalı
        /// - Aynı şubede aynı isimde departman olamaz
        /// </summary>
        Task<Department> CreateAsync(Department department);

        /// <summary>
        /// Departman Güncelle
        ///
        /// GÜNCELLEME: Ad değişikliği, açıklama
        /// </summary>
        Task UpdateAsync(Department department);

        /// <summary>
        /// Departman Sil
        ///
        /// SİLME KONTROLÜ: Personel varsa silinemez
        /// Önce GetEmployeeCountAsync ile kontrol
        ///
        /// SOFT DELETE: IsDeleted = true
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Departmandaki Çalışan Sayısı
        ///
        /// SİLME KONTROLÜ: count > 0 ise silinmez
        /// ORGANİZASYON ŞEMASI: Departman başına kaç kişi
        ///
        /// ÖRNEK:
        /// var count = await GetEmployeeCountAsync(depId);
        /// if (count > 0)
        ///     return BadRequest("Departmanda çalışan var!");
        /// </summary>
        Task<int> GetEmployeeCountAsync(int departmentId);
    }
}
