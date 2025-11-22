// ============================================================================
// ICustomerService.cs - Müşteri Servis Interface
// ============================================================================
// AÇIKLAMA:
// Müşteri yönetimi için iş mantığı katmanı.
// CRM (Customer Relationship Management) işlevleri.
//
// MÜŞTERİ TÜRLERİ:
// 1. Kayıtlı Müşteri: TC ile kayıt, puan kazanır, kampanyalardan yararlanır
// 2. Anonim Müşteri: Hızlı satış, kayıt gerekmez
//
// KVKK (Kişisel Verilerin Korunması Kanunu):
// - TC Kimlik no hassas veri
// - E-posta/telefon açık rıza gerektirir
// - Silme hakkı -> Soft Delete
// ============================================================================

using Domain.Entities;

namespace Application.Services
{
    /// <summary>
    /// Müşteri Servis Interface
    ///
    /// CRM: Müşteri ilişkileri yönetimi
    /// SADAKAT: Puan kazanma ve kullanma
    /// ANALİZ: VIP müşteri belirleme
    /// </summary>
    public interface ICustomerService
    {
        // ========================================================================
        // SORGULAMA (QUERY) METODLARI
        // ========================================================================

        /// <summary>
        /// ID ile Müşteri Getir
        ///
        /// DETAY SAYFASI: Müşteri profil görüntüleme
        /// İLİŞKİLİ VERİ: Satış geçmişi için Include
        /// </summary>
        Task<Customer?> GetByIdAsync(int id);

        /// <summary>
        /// TC Kimlik ile Müşteri Getir
        ///
        /// KASİYER KULLANIMI: "TC'niz var mı?"
        /// PUAN KULLANIMI: TC ile müşteri bulunur, puan sorgulanır
        ///
        /// HIZLI ARAMA: TC ile direkt erişim
        /// </summary>
        Task<Customer?> GetByIdentityNumberAsync(string identityNumber);

        /// <summary>
        /// Tüm Müşteriler
        ///
        /// ADMIN: Müşteri listesi sayfası
        /// CRM: Toplu pazarlama için liste
        /// </summary>
        Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>
        /// Aktif Müşteriler
        ///
        /// SOFT DELETE: IsDeleted = false
        /// PAZARLAMA: Aktif müşteri listesi
        /// </summary>
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();

        /// <summary>
        /// Müşteri Arama
        ///
        /// ARAMA KRİTERLERİ:
        /// - İsim/Soyisim içinde
        /// - TC içinde
        /// - Telefon içinde
        /// - Email içinde
        ///
        /// KASİYER: "Ahmet" yazınca tüm Ahmet'ler gelir
        /// FULL-TEXT: SQL LIKE veya Full-Text Index
        /// </summary>
        Task<IEnumerable<Customer>> SearchAsync(string searchTerm);

        /// <summary>
        /// En Çok Alışveriş Yapan Müşteriler (VIP)
        ///
        /// SADAKAT PROGRAMI: VIP müşteri belirleme
        /// ÖZEL KAMPANYA: Bu listeye özel indirim
        ///
        /// SIRALAMA: Toplam harcama tutarına göre
        ///
        /// ÖRNEK:
        /// // Bu yılın Top 100 müşterisi
        /// var vipList = await GetTopCustomersAsync(100,
        ///     new DateTime(2024, 1, 1),
        ///     DateTime.Now);
        /// </summary>
        Task<IEnumerable<Customer>> GetTopCustomersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);

        // ========================================================================
        // KOMUT (COMMAND) METODLARI
        // ========================================================================

        /// <summary>
        /// Yeni Müşteri Oluştur
        ///
        /// KAYIT SÜRECİ:
        /// 1. TC benzersizlik kontrolü
        /// 2. Email/telefon format kontrolü
        /// 3. KVKK onayı alınmalı (frontend'de)
        /// 4. Hoşgeldin puanı verilebilir
        ///
        /// GERİ DÖNÜŞ: ID atanmış Customer
        /// </summary>
        Task<Customer> CreateAsync(Customer customer);

        /// <summary>
        /// Müşteri Güncelle
        ///
        /// GÜNCELLEME SENARYOLARI:
        /// - Adres değişikliği
        /// - Telefon güncelleme
        /// - VIP statüsü değişikliği
        ///
        /// AUDİT: UpdatedAt otomatik güncellenir
        /// </summary>
        Task UpdateAsync(Customer customer);

        /// <summary>
        /// Müşteri Sil
        ///
        /// SOFT DELETE: IsDeleted = true
        ///
        /// KVKK "UNUTULMA HAKKI":
        /// - Satış kayıtları CustomerId = null yapılabilir
        /// - Veya anonimleştirme (isim -> "Silinmiş Müşteri")
        ///
        /// DİKKAT: Gerçek silme yerine anonimleştirme tercih edilir
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// TC Kimlik Benzersizlik Kontrolü
        ///
        /// İŞ KURALI: Aynı TC ile iki müşteri olamaz
        ///
        /// YENİ MÜŞTERİ: excludeId = null
        /// GÜNCELLEME: excludeId = mevcutId (kendi TC'si hariç kontrol)
        ///
        /// ÖRNEK:
        /// // Kayıt formunda
        /// if (await IsIdentityNumberTakenAsync(tcKimlik))
        /// {
        ///     // Mevcut müşteri bulundu, yeni kayıt oluşturma
        ///     var existingCustomer = await GetByIdentityNumberAsync(tcKimlik);
        ///     return RedirectToAction("Details", new { id = existingCustomer.Id });
        /// }
        /// </summary>
        Task<bool> IsIdentityNumberTakenAsync(string identityNumber, int? excludeId = null);
    }
}
