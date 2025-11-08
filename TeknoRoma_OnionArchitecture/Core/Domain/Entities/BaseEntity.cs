using System;

namespace TEKNOROMA.Core.Domain.Entities
{
    /// <summary>
    /// Tüm entity sınıflarının türeyeceği base sınıf.
    /// SOLID Prensipleri:
    /// - Single Responsibility: Sadece ortak özellikleri tutar
    /// - Open/Closed: Yeni entity'ler eklemek için extend edilir, değiştirilmez
    /// - Liskov Substitution: BaseEntity yerine her entity kullanılabilir
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Constructor: Her entity oluşturulduğunda çalışır
        /// NEDEN: CreatedDate otomatik atanmalı, manuel atamayı unutmayalım diye
        /// </summary>
        protected BaseEntity()
        {
            // NEDEN DateTime.Now: Entity oluşturulduğu anı kaydet
            // NOT: UTC kullanmak daha iyi olabilir (farklı zaman dilimleri için)
            CreatedDate = DateTime.Now;

            // NEDEN false: Yeni oluşturulan entity silinmemiş olarak başlamalı
            IsDeleted = false;
        }

        /// <summary>
        /// Primary Key - Her entity'nin benzersiz kimliği
        /// NEDEN int: SQL Server'da Identity column için standart
        /// NEDEN ID: Convention over Configuration (EF Core otomatik tanır)
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Entity ne zaman oluşturuldu?
        /// NEDEN: Audit trail (denetim izi) için gerekli
        /// NEDEN Constructor'da atanıyor: Otomatik olsun
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Entity en son ne zaman güncellendi?
        /// NEDEN nullable: İlk oluşturulduğunda null (henüz güncellenmemiş)
        /// KULLANIM: Her Update işleminde bu değer set edilmeli
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Kim oluşturdu?
        /// NEDEN string: Username veya UserID tutabiliriz (flexible)
        /// NEDEN nullable: Opsiyonel - bazı durumlarda sistem oluşturabilir
        /// KULLANIM: Login olan kullanıcının username'i atanmalı
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Kim güncelledi?
        /// NEDEN string: Username veya UserID tutabiliriz
        /// NEDEN nullable: Henüz güncellenmemişse null
        /// KULLANIM: Her Update işleminde login olan user atanmalı
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// SOFT DELETE: Kayıt silinmiş mi?
        /// NEDEN Soft Delete: TEKNOROMA'da "eskiden sattığımız ürünler" raporu isteniyor!
        /// - Hard Delete: Veritabanından kalıcı siler (GERİ GETİRİLEMEZ!)
        /// - Soft Delete: Sadece flag değişir (GERİ GETİRİLEBİLİR!)
        /// AVANTAJLAR:
        /// - Silinen kayıtları raporlarda gösterebiliriz
        /// - Yanlışlıkla silmeleri geri alabiliriz
        /// - Audit trail bozulmaz
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Ne zaman silindi?
        /// NEDEN nullable: Silinmemişse null
        /// KULLANIM: Delete işleminde DateTime.Now atanmalı
        /// FAYDA: "Bu ürün 2 ay önce silinmiş" gibi bilgiler
        /// </summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// Kim sildi?
        /// NEDEN: Sorumluluğu takip edebilmek için
        /// ÖRNEK: "Bu kategoriyi Haluk Bey silmiş, ona soralım"
        /// </summary>
        public string? DeletedBy { get; set; }

        /// <summary>
        /// Aktif mi?
        /// NEDEN IsDeleted'dan farklı: 
        /// - IsDeleted: Kalıcı silme (arşivleme)
        /// - IsActive: Geçici pasifleştirme (aktif/pasif yapma)
        /// ÖRNEK: 
        /// - Yaz sezonunda kışlık ürünleri pasif yap (IsActive=false)
        /// - Ama silme (IsDeleted=false)
        /// - İlkbaharda tekrar aktif yap
        /// </summary>
        public bool IsActive { get; set; } = true;  // Default olarak aktif

        // =====================================================
        // NEDEN bu kadar property?
        // =====================================================
        // 1. ID, CreatedDate, ModifiedDate: TEMEL - Her entity'de olmalı
        // 2. CreatedBy, ModifiedBy: AUDİT - Kim ne yaptı? (Önemli!)
        // 3. IsDeleted, DeletedDate, DeletedBy: SOFT DELETE - Geri alınabilir silme
        // 4. IsActive: İŞ MANTIĞI - Geçici pasifleştirme
        //
        // SOLID - Single Responsibility ✅
        // Bu sınıf sadece "ortak özellikler"den sorumlu
        //
        // SOLID - Open/Closed ✅
        // Yeni entity eklemek için extend et, bu sınıfı değiştirme
        //
        // SOLID - Dependency Inversion ✅
        // Hiçbir external dependency yok (EF, SQL, vb.)
    }
}