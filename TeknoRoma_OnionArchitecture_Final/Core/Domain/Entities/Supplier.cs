// ============================================================================
// Supplier.cs - Tedarikçi Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın ürün tedarik ettiği firma ve distribütörleri temsil eder.
// Her ürün bir tedarikçiden alınır, tedarikçi bilgileri sipariş ve
// borç/alacak takibi için kullanılır.
//
// ÖRNEK TEDARİKÇİLER:
// - Apple Türkiye Distribütörü
// - Samsung Electronics Turkey
// - Xiaomi Yetkili Distribütör
// - HP/Dell Kurumsal Satış
// - Sony Electronics
// - LG Electronics
//
// İŞ KURALLARI:
// - Her tedarikçinin benzersiz vergi numarası olmalı
// - Tedarikçi silinmeden önce ürünler başka tedarikçiye atanmalı
// - Tedarikçi işlemleri (borç/alacak) takip edilir
// ============================================================================

namespace Domain.Entities
{
    /// <summary>
    /// Tedarikçi Entity Sınıfı
    ///
    /// B2B (Business-to-Business) İLİŞKİSİ:
    /// - TEKNOROMA tedarikçilerden toptan alım yapar
    /// - Vadeli ödeme ve cari hesap yönetimi
    /// - Ürün iade ve değişim süreçleri
    /// </summary>
    public class Supplier : BaseEntity
    {
        // ====================================================================
        // FİRMA BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Tedarikçi Firma Adı
        ///
        /// AÇIKLAMA:
        /// - Tedarikçi firmanın ticari unvanı
        /// - Fatura ve resmi yazışmalarda kullanılır
        ///
        /// ÖRNEKLER:
        /// - "Apple Türkiye Ticaret A.Ş."
        /// - "Samsung Electronics Istanbul"
        /// - "Teknosa İç ve Dış Ticaret A.Ş."
        /// </summary>
        public string CompanyName { get; set; } = null!;

        /// <summary>
        /// Yetkili Kişi Adı
        ///
        /// AÇIKLAMA:
        /// - Tedarikçi firmadaki irtibat kişisi
        /// - Sipariş ve sorun çözümünde iletişim kurulacak kişi
        ///
        /// ÖRNEK:
        /// - "Ahmet Yıldız - Satış Müdürü"
        /// - "Ayşe Kaya"
        /// </summary>
        public string ContactName { get; set; } = null!;

        // ====================================================================
        // İLETİŞİM BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Telefon Numarası
        ///
        /// AÇIKLAMA:
        /// - Tedarikçinin telefon numarası
        /// - Acil sipariş ve sorun çözümü için
        ///
        /// FORMAT:
        /// - Sabit hat: "0212 XXX XX XX"
        /// - Cep: "05XX XXX XX XX"
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// E-posta Adresi
        ///
        /// AÇIKLAMA:
        /// - Tedarikçinin kurumsal e-postası
        /// - Sipariş, fatura ve resmi yazışmalar için
        ///
        /// ÖRNEK:
        /// - "siparis@apple-turkiye.com"
        /// - "b2b@samsung.com.tr"
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Adres (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Tedarikçinin fiziksel adresi
        /// - Ürün teslim alma veya ziyaret için
        ///
        /// NULLABLE (string?):
        /// - Adres zorunlu değil
        /// - Bazı tedarikçiler sadece online çalışıyor olabilir
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Şehir (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Tedarikçinin bulunduğu şehir
        /// - Lojistik planlaması için
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Ülke
        ///
        /// AÇIKLAMA:
        /// - Tedarikçinin ülkesi
        /// - Uluslararası tedarikçiler için önemli
        ///
        /// VARSAYILAN DEĞER:
        /// - "= \"Türkiye\"" ile Türkiye olarak başlar
        /// - string literal içinde tırnak: \" kullanılır
        ///
        /// ULUSLARARASI TEDARİKÇİLER:
        /// - "Çin" (Xiaomi, Huawei)
        /// - "Güney Kore" (Samsung, LG)
        /// - "ABD" (Apple, HP, Dell)
        /// </summary>
        public string Country { get; set; } = "Türkiye";

        // ====================================================================
        // VERGİ BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Vergi Numarası (Tax Number)
        ///
        /// AÇIKLAMA:
        /// - Tedarikçinin vergi kimlik numarası
        /// - Fatura düzenleme ve vergi beyanı için zorunlu
        /// - Türkiye'de 10 haneli
        ///
        /// DOĞRULAMA:
        /// - 10 hane kontrolü
        /// - Vergi dairesi sorgulaması (e-devlet)
        ///
        /// UNIQUE CONSTRAINT:
        /// - Aynı vergi numarası ile birden fazla tedarikçi olamaz
        ///
        /// E-FATURA:
        /// - E-fatura mükellefiyeti kontrolü
        /// - GİB (Gelir İdaresi Başkanlığı) sorgulaması
        /// </summary>
        public string TaxNumber { get; set; } = null!;

        // ====================================================================
        // DURUM
        // ====================================================================

        /// <summary>
        /// Tedarikçi Aktif mi?
        ///
        /// AÇIKLAMA:
        /// - true: Aktif tedarikçi, sipariş verilebilir
        /// - false: Pasif tedarikçi, yeni sipariş verilmez
        ///
        /// PASİF YAPILMA NEDENLERİ:
        /// - Tedarikçi ile iş ilişkisi sonlandı
        /// - Ödeme sorunları
        /// - Kalite problemleri
        /// - Tedarikçi kapandı
        ///
        /// VARSAYILAN:
        /// - "= true" ile aktif başlar
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Tedarikçiden Alınan Ürünler
        ///
        /// İLİŞKİ: Supplier (1) → Product (N)
        /// - Bir tedarikçiden birden fazla ürün alınabilir
        ///
        /// KULLANIM:
        /// // Tedarikçinin ürün sayısı
        /// var productCount = supplier.Products.Count;
        ///
        /// // Tedarikçiden alınan toplam ürün değeri
        /// var totalValue = supplier.Products
        ///     .Sum(p => p.UnitsInStock * p.UnitPrice);
        /// </summary>
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        /// <summary>
        /// Tedarikçi İşlemleri (Alım/Ödeme Hareketleri)
        ///
        /// İLİŞKİ: Supplier (1) → SupplierTransaction (N)
        /// - Bir tedarikçi ile birden fazla işlem olabilir
        ///
        /// KULLANIM:
        /// // Tedarikçiye toplam borç
        /// var totalDebt = supplier.SupplierTransactions
        ///     .Where(t => !t.IsPaid)
        ///     .Sum(t => t.Amount);
        ///
        /// // Ödenen toplam tutar
        /// var totalPaid = supplier.SupplierTransactions
        ///     .Where(t => t.IsPaid)
        ///     .Sum(t => t.Amount);
        ///
        /// // Bakiye (Borç - Ödenen)
        /// var balance = totalDebt - totalPaid;
        /// </summary>
        public virtual ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();
    }
}
