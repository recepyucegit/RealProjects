// ============================================================================
// Customer.cs - Müşteri Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA mağazalarından alışveriş yapan müşterileri temsil eden entity.
// Müşteri bilgileri satış, fatura ve pazarlama amaçlı kullanılır.
//
// İŞ KURALLARI:
// - Her müşterinin TC kimlik numarası benzersiz olmalı
// - Telefon numarası zorunlu (fatura ve iletişim için)
// - Diğer alanlar opsiyonel (hızlı müşteri kaydı için)
//
// KVKK UYUMU:
// - Kişisel veriler KVKK kapsamında korunmalı
// - Müşteri onayı olmadan pazarlama yapılamaz
// - Veri silme talebi için IsDeleted kullanılır
// ============================================================================

using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Müşteri Entity Sınıfı
    ///
    /// MÜŞTERI KAYIT SENARYOLARI:
    /// 1. Hızlı Kayıt: Sadece ad, soyad, TC, telefon alınır
    /// 2. Detaylı Kayıt: Tüm bilgiler alınır (üyelik avantajları için)
    /// 3. Anonim Satış: Müşteri kaydı yapılmaz (CustomerId nullable olabilir Sale'de)
    /// </summary>
    public class Customer : BaseEntity
    {
        // ====================================================================
        // KİMLİK BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// TC Kimlik Numarası (UNIQUE)
        ///
        /// AÇIKLAMA:
        /// - 11 haneli TC kimlik numarası
        /// - Fatura düzenleme için zorunlu (vergi mevzuatı)
        /// - Müşteri tekil tanımlama için kullanılır
        ///
        /// E-FATURA ZORUNLULUĞU:
        /// - 5.000 TL üzeri satışlarda TC zorunlu
        /// - E-fatura/e-arşiv fatura için gerekli
        ///
        /// UNIQUE CONSTRAINT:
        /// - Aynı TC ile birden fazla müşteri kaydı oluşturulamaz
        /// - Mevcut müşteri tekrar kayıt olmaya çalışırsa uyarı verilir
        /// </summary>
        public string IdentityNumber { get; set; } = null!;

        /// <summary>
        /// Müşteri Adı
        ///
        /// AÇIKLAMA:
        /// - Müşterinin adı
        /// - Fatura "ad soyad" alanı için gerekli
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Müşteri Soyadı
        ///
        /// AÇIKLAMA:
        /// - Müşterinin soyadı
        /// - Fatura ve resmi yazışmalar için
        /// </summary>
        public string LastName { get; set; } = null!;

        // ====================================================================
        // KİŞİSEL BİLGİLER (OPSİYONEL)
        // ====================================================================

        /// <summary>
        /// Doğum Tarihi (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin doğum tarihi
        /// - Doğum günü kampanyaları için kullanılır
        /// - Yaş bazlı segmentasyon için
        ///
        /// NULLABLE (DateTime?):
        /// - Müşteri vermek istemeyebilir (KVKK hakkı)
        /// - Hızlı kayıtta alınmayabilir
        /// - "?" ile null değer alabilir
        ///
        /// HasValue KONTROLÜ:
        /// if (customer.BirthDate.HasValue)
        ///     var date = customer.BirthDate.Value;
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Cinsiyet (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Gender enum'undan değer alır
        /// - Pazarlama segmentasyonu için
        ///
        /// NULLABLE ENUM (Gender?):
        /// - Enum değeri de nullable olabilir
        /// - Belirtilmek istenmeyebilir
        /// - null: Belirtilmemiş
        ///
        /// GENDER ENUM DEĞERLERİ:
        /// - Erkek = 0
        /// - Kadin = 1
        /// - Diger = 2 (tercih etmiyorum)
        /// </summary>
        public Gender? Gender { get; set; }

        // ====================================================================
        // İLETİŞİM BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// E-posta Adresi (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin e-posta adresi
        /// - E-fatura gönderimi için
        /// - Kampanya ve duyurular için (onay gerekli)
        ///
        /// NULLABLE (string?):
        /// - Müşteri vermeyebilir
        /// - SMS tercih edebilir
        ///
        /// EMAIL MARKETING:
        /// - KVKK onayı olmadan mail gönderilemez
        /// - EmailConsent alanı eklenebilir
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Telefon Numarası (ZORUNLU)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin cep telefonu
        /// - ZORUNLU: Satış ve fatura için gerekli
        /// - SMS bilgilendirme için kullanılır
        ///
        /// FORMAT:
        /// - "05XX XXX XX XX" formatında
        /// - Uluslararası: "+90 5XX XXX XX XX"
        ///
        /// DOĞRULAMA:
        /// - 10-11 hane kontrolü
        /// - 05 ile başlamalı (Türkiye GSM)
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Adres (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin ev/iş adresi
        /// - Teslimat ve fatura adresi için
        /// - Detaylı adres (mahalle, sokak, no)
        ///
        /// ALTERNATİF TASARIM:
        /// - Birden fazla adres için ayrı Address tablosu
        /// - CustomerAddress (1-N ilişki)
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Şehir (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Müşterinin yaşadığı şehir
        /// - Bölgesel kampanyalar için
        /// - Teslimat lojistiği planlaması için
        /// </summary>
        public string? City { get; set; }

        // ====================================================================
        // DURUM
        // ====================================================================

        /// <summary>
        /// Müşteri Aktif mi?
        ///
        /// AÇIKLAMA:
        /// - true: Aktif müşteri
        /// - false: Pasif/Kara listedeki müşteri
        ///
        /// KULLANIM:
        /// - Sorunlu müşteri: IsActive = false
        /// - Dolandırıcılık şüphesi: IsActive = false
        /// - KVKK silme talebi: IsDeleted = true (BaseEntity'den)
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
        /// - Fatura, UI ve raporlarda kullanılır
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Yaş (Calculated, Nullable)
        ///
        /// AÇIKLAMA:
        /// - Doğum tarihinden yaş hesaplar
        /// - BirthDate null ise null döner
        ///
        /// NULLABLE CONDITIONAL:
        /// - BirthDate.HasValue: null kontrolü
        /// - true ise: yaş hesapla
        /// - false ise: null döndür
        ///
        /// TERNARY OPERATOR:
        /// koşul ? doğru_değer : yanlış_değer
        ///
        /// NOT:
        /// - Bu basit hesaplama, doğum günü geçip geçmediğini kontrol etmez
        /// - Daha doğru hesap için ay/gün kontrolü gerekir
        ///
        /// DOĞRU HESAPLAMA:
        /// var age = DateTime.Now.Year - BirthDate.Value.Year;
        /// if (DateTime.Now < BirthDate.Value.AddYears(age))
        ///     age--;
        /// </summary>
        public int? Age => BirthDate.HasValue
            ? DateTime.Now.Year - BirthDate.Value.Year
            : null;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Müşterinin Satışları (Collection Navigation Property)
        ///
        /// İLİŞKİ: Customer (1) → Sale (N)
        /// - Bir müşteri birden fazla alışveriş yapabilir
        ///
        /// KULLANIM:
        /// // Müşterinin toplam harcaması
        /// var totalSpent = customer.Sales.Sum(s => s.TotalAmount);
        ///
        /// // Satış sayısı
        /// var purchaseCount = customer.Sales.Count;
        ///
        /// // Son alışveriş tarihi
        /// var lastPurchase = customer.Sales.Max(s => s.SaleDate);
        ///
        /// MÜŞTERİ SEGMENTİ:
        /// - VIP: 10+ alışveriş veya 50K+ harcama
        /// - Regular: 3-9 alışveriş
        /// - New: 1-2 alışveriş
        /// </summary>
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
