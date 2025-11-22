// ============================================================================
// UserRole.cs - Kullanıcı Rolleri Enum
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA sistemindeki kullanıcı rollerini tanımlayan enum.
// Her rol, sistemde farklı yetkilere sahiptir.
//
// ENUM NEDİR?
// - Enum (Enumeration): Sabit değerlerin adlandırılmış listesi
// - int yerine anlamlı isimler kullanmamızı sağlar
// - Kod okunabilirliğini artırır
// - Compile-time type safety sağlar (hatalı değer atanamaz)
//
// NEDEN MAGIC NUMBER KULLANMIYORUZ?
// ❌ Kötü: if (user.Role == 1) // 1 ne demek?
// ✅ İyi:  if (user.Role == UserRole.GenelMudur) // Anlaşılır!
//
// VERİTABANINDA NASIL SAKLANIR?
// - EF Core, enum'u otomatik olarak int'e çevirir
// - GenelMudur = 1, SubeMuduru = 2 şeklinde saklanır
// - Veritabanında: Role sütunu INT tipinde, değer 1-6 arası
//
// YETKİ HİYERARŞİSİ:
// 1. GenelMudur: Tüm sisteme tam erişim (Admin)
// 2. SubeMuduru: Kendi şubesinin tüm işlemleri
// 3. KasaSatis: Satış ve müşteri işlemleri
// 4. Depo: Stok ve ürün yönetimi
// 5. Muhasebe: Finansal işlemler ve raporlar
// 6. TeknikServis: Servis kayıtları ve takibi
//
// ASP.NET IDENTITY İLE ENTEGRASYON:
// - Employee entity'si IdentityUser'dan kalıtım alır
// - Bu enum, iç uygulama rollerini temsil eder
// - Identity'nin Role sistemiyle senkronize edilebilir
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Kullanıcı Rolleri Enum'u
    ///
    /// İŞ GEREKSİNİMİ (Haluk Bey):
    /// "Kullanıcı sadece yetkisi dahilindeki bölümlere erişebilmeli.
    ///  Kasadaki personel muhasebe ekranını görmesin."
    ///
    /// KULLANIM ÖRNEKLERİ:
    /// <code>
    /// // Rol kontrolü
    /// if (currentUser.Role == UserRole.GenelMudur)
    /// {
    ///     // Tüm verileri göster
    /// }
    ///
    /// // Yetki kontrolü
    /// [Authorize(Roles = nameof(UserRole.GenelMudur))]
    /// public IActionResult AdminPanel() { }
    ///
    /// // Switch expression (C# 8.0+)
    /// var dashboard = user.Role switch
    /// {
    ///     UserRole.GenelMudur => "AdminDashboard",
    ///     UserRole.SubeMuduru => "BranchDashboard",
    ///     UserRole.KasaSatis => "SalesDashboard",
    ///     _ => "DefaultDashboard"
    /// };
    /// </code>
    /// </summary>
    public enum UserRole
    {
        // ====================================================================
        // YÖNETİM KADEMESİ
        // ====================================================================

        /// <summary>
        /// Genel Müdür - En Üst Yetki Seviyesi
        ///
        /// YETKİLER:
        /// - Tüm mağazaların verilerine erişim
        /// - Tüm raporları görme
        /// - Kullanıcı yönetimi
        /// - Sistem ayarları
        /// - Fiyat politikaları belirleme
        ///
        /// TİPİK KULLANICILAR:
        /// - Şirket sahibi
        /// - CEO
        /// - Genel Müdür
        ///
        /// DEĞERİ NEDEN 1?
        /// - 0 yerine 1'den başlamak best practice
        /// - 0 genellikle "tanımsız" veya "varsayılan" için ayrılır
        /// - Veritabanında NULL ile karışmaması için
        /// </summary>
        GenelMudur = 1,

        /// <summary>
        /// Şube Müdürü - Mağaza Seviyesi Yetki
        ///
        /// YETKİLER:
        /// - Kendi mağazasının tüm işlemleri
        /// - Mağaza personeli yönetimi
        /// - Mağaza raporları
        /// - Stok onayları
        /// - İndirim/kampanya uygulama
        ///
        /// KISITLAMALAR:
        /// - Diğer mağazaların verilerine erişemez
        /// - Genel ayarları değiştiremez
        /// - Fiyat politikası belirleyemez
        ///
        /// VERİ FİLTRELEME:
        /// <code>
        /// // Şube müdürü sadece kendi mağazasını görür
        /// var sales = await _context.Sales
        ///     .Where(s => s.StoreId == currentUser.StoreId)
        ///     .ToListAsync();
        /// </code>
        /// </summary>
        SubeMuduru = 2,

        // ====================================================================
        // OPERASYONEL PERSONEL
        // ====================================================================

        /// <summary>
        /// Kasa/Satış Personeli - Satış Odaklı Yetki
        ///
        /// YETKİLER:
        /// - Satış işlemi yapma
        /// - Müşteri kaydı oluşturma
        /// - Ürün arama ve bilgi görme
        /// - İade işlemi (onay gerekebilir)
        /// - Günlük satış raporu
        ///
        /// KISITLAMALAR:
        /// - Fiyat değiştiremez
        /// - Stok düzeltme yapamaz
        /// - Finansal raporları göremez
        /// - Personel bilgilerine erişemez
        ///
        /// DEPARTMAN EŞLEŞMESİ:
        /// - Department.DepartmentType = UserRole.KasaSatis
        /// - Bu rol satış departmanına atanır
        /// </summary>
        KasaSatis = 3,

        /// <summary>
        /// Depo Personeli - Stok Yönetimi Yetkisi
        ///
        /// YETKİLER:
        /// - Stok girişi/çıkışı
        /// - Sayım işlemleri
        /// - Ürün transferi (mağazalar arası)
        /// - Tedarikçi siparişi hazırlama
        /// - Stok raporları
        ///
        /// KISITLAMALAR:
        /// - Satış yapamaz
        /// - Fiyat göremez/değiştiremez
        /// - Müşteri bilgilerine erişemez
        /// - Finansal işlemler yapamaz
        ///
        /// İŞ SÜRECİ:
        /// 1. Tedarikçiden mal gelir
        /// 2. Depo personeli stok girişi yapar
        /// 3. SupplierTransaction kaydı oluşur
        /// 4. Product.UnitsInStock güncellenir
        /// </summary>
        Depo = 4,

        /// <summary>
        /// Muhasebe Personeli - Finansal Yetki
        ///
        /// YETKİLER:
        /// - Gider kayıtları oluşturma
        /// - Tedarikçi ödemeleri
        /// - Finansal raporlar
        /// - Cari hesap takibi
        /// - Maaş hesaplamaları
        ///
        /// KISITLAMALAR:
        /// - Satış yapamaz
        /// - Stok işlemi yapamaz
        /// - Teknik servis kaydı oluşturamaz
        ///
        /// RAPORLAMA:
        /// - Günlük/aylık gelir-gider
        /// - Kar/zarar analizi
        /// - Tedarikçi borç durumu
        /// - Mağaza bazlı maliyet
        /// </summary>
        Muhasebe = 5,

        /// <summary>
        /// Teknik Servis Personeli - Servis Yetkisi
        ///
        /// YETKİLER:
        /// - Servis kaydı oluşturma/güncelleme
        /// - Arıza takibi
        /// - Müşteri cihaz bilgisi görme
        /// - Servis raporları
        /// - Parça talep etme
        ///
        /// KISITLAMALAR:
        /// - Satış yapamaz
        /// - Stok işlemi yapamaz (parça hariç)
        /// - Finansal verilere erişemez
        ///
        /// SERVİS WORKFLOW:
        /// 1. Müşteri arıza bildirir
        /// 2. Teknisyen servis kaydı açar
        /// 3. Durum güncellenir (Açık → İşlemde → Tamamlandı)
        /// 4. Müşteriye bilgi verilir
        /// </summary>
        TeknikServis = 6
    }
}
