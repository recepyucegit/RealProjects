// ============================================================================
// BaseEntity.cs
// ============================================================================
// AÇIKLAMA:
// Bu dosya, projedeki TÜM entity'lerin (veritabanı tablolarına karşılık gelen
// sınıfların) miras alacağı temel sınıfı içerir.
//
// NEDEN GEREKLİ?
// - Her tabloda tekrar eden alanları (Id, CreatedDate vs.) tek yerde tanımlarız
// - DRY (Don't Repeat Yourself) prensibine uyarız
// - Generic Repository pattern'i kullanabilmek için ortak bir tip gerekir
//
// KULLANIM:
// public class Product : BaseEntity { ... }
// Bu sayede Product otomatik olarak Id, CreatedDate vs. alanlarına sahip olur
// ============================================================================

namespace Domain.Entities
{
    /// <summary>
    /// Tüm Entity'lerin miras alacağı soyut (abstract) temel sınıf.
    ///
    /// Abstract olmasının sebebi:
    /// - Doğrudan new BaseEntity() ile oluşturulamaz
    /// - Sadece miras alınarak kullanılabilir (Product, Customer vs.)
    /// - Ortak davranışları ve özellikleri merkezi bir yerde toplar
    /// </summary>
    public abstract class BaseEntity
    {
        // ====================================================================
        // PRIMARY KEY - TANIMLAYICI ALAN
        // ====================================================================
        /// <summary>
        /// Birincil Anahtar (Primary Key)
        ///
        /// AÇIKLAMA:
        /// - Her kaydı benzersiz şekilde tanımlar
        /// - Veritabanında otomatik artan (IDENTITY) olarak ayarlanır
        /// - Entity Framework bu alanı otomatik olarak PK olarak algılar
        ///   (Convention: "Id" veya "EntityNameId" formatındaki alanlar PK kabul edilir)
        ///
        /// TİP SEÇİMİ:
        /// - int: 2.1 milyar kayda kadar destekler, çoğu proje için yeterli
        /// - long: Çok büyük tablolar için
        /// - Guid: Dağıtık sistemler için (merge conflict önler)
        ///
        /// { get; set; } AÇIKLAMASI:
        /// - get: Değeri okumaya izin verir (product.Id)
        /// - set: Değeri yazmaya izin verir (product.Id = 5)
        /// - Auto-property: Compiler arka planda private field oluşturur
        /// </summary>
        public int Id { get; set; }

        // ====================================================================
        // AUDIT ALANLARI - KAYIT TAKİBİ
        // ====================================================================
        /// <summary>
        /// Kaydın oluşturulma tarihi ve saati
        ///
        /// AÇIKLAMA:
        /// - Kayıt ilk eklendiğinde otomatik set edilir
        /// - Genellikle DbContext.SaveChanges() override edilerek ayarlanır
        /// - Bir kez set edildikten sonra değiştirilmemeli
        ///
        /// KULLANIM ALANLARI:
        /// - Raporlama: "Bu ay kaç müşteri eklendi?"
        /// - Sıralama: "En yeni ürünler"
        /// - Denetim: "Bu kayıt ne zaman oluşturuldu?"
        ///
        /// DateTime TİPİ:
        /// - Tarih ve saat bilgisini birlikte tutar
        /// - SQL Server'da datetime2 olarak saklanır
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Kaydın son güncellenme tarihi ve saati
        ///
        /// AÇIKLAMA:
        /// - Kayıt her güncellendiğinde otomatik set edilir
        /// - İlk oluşturmada null olabilir (henüz güncellenmemiş)
        ///
        /// DateTime? (NULLABLE) AÇIKLAMASI:
        /// - "?" işareti bu alanın null olabileceğini belirtir
        /// - Kayıt hiç güncellenmemişse null kalır
        /// - SQL Server'da NULL değer olarak saklanır
        ///
        /// KULLANIM ALANLARI:
        /// - "Son güncelleme: 2 saat önce" gibi bilgiler
        /// - Senkronizasyon: "Son sync'den sonra değişen kayıtlar"
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        // ====================================================================
        // SOFT DELETE - YUMUŞAK SİLME
        // ====================================================================
        /// <summary>
        /// Soft Delete (Yumuşak Silme) için bayrak
        ///
        /// SOFT DELETE NEDİR?
        /// - Kayıt veritabanından fiziksel olarak silinmez
        /// - Sadece IsDeleted = true yapılır
        /// - Sorgularda WHERE IsDeleted = false filtresi uygulanır
        ///
        /// NEDEN SOFT DELETE?
        /// 1. Veri Kurtarma: Yanlışlıkla silinen veriler geri getirilebilir
        /// 2. Denetim İzi: Silinen kayıtların geçmişi korunur
        /// 3. İlişkisel Bütünlük: Foreign key ile bağlı kayıtlar bozulmaz
        /// 4. Yasal Gereklilik: Bazı sektörlerde veri saklama zorunluluğu
        ///
        /// VARSAYILAN DEĞER:
        /// - bool tipi varsayılan olarak false'dur
        /// - Yeni kayıtlar otomatik olarak IsDeleted = false ile oluşur
        ///
        /// ALTERNATİF YAKLAŞIM:
        /// - DeletedDate: DateTime? olarak silme tarihi tutulabilir
        /// - DeletedBy: Kimin sildiği bilgisi tutulabilir
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Kaydın silinme tarihi ve saati (Soft Delete için)
        ///
        /// AÇIKLAMA:
        /// - Kayıt soft delete yapıldığında set edilir
        /// - IsDeleted = true ise bu alan dolu olmalı
        /// - Hard delete yapılmadığı sürece kayıt korunur
        ///
        /// DateTime? (NULLABLE) AÇIKLAMASI:
        /// - "?" işareti bu alanın null olabileceğini belirtir
        /// - Silinmemiş kayıtlarda null kalır
        /// </summary>
        public DateTime? DeletedDate { get; set; }
    }
}
