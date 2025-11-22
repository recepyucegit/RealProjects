// ============================================================================
// ITechnicalServiceService.cs - Teknik Servis Servis Interface
// ============================================================================
// AÇIKLAMA:
// Teknik servis taleplerini yönetmek için iş mantığı katmanı.
// Ticket sistemi, SLA (Service Level Agreement) takibi.
//
// TİCKET SİSTEMİ AKIŞI:
// 1. Müşteri başvurur -> Ticket açılır (Status: Acik)
// 2. Yönetici inceler -> Teknisyene atar (Status: Islemde)
// 3. Teknisyen çözer -> Tamamlanır (Status: Tamamlandi)
// 4. Çözülemezse -> Üst merkeze iletilir (Status: Cozulemedi)
//
// SLA (Service Level Agreement):
// - Açık ticket süresi: Max 24 saat
// - İşlemde kalma süresi: Max 72 saat
// - Müşteri memnuniyeti hedefi: %95
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    /// <summary>
    /// Teknik Servis Servis Interface
    ///
    /// NOT: Interface adı "ITechnicalServiceService" - Service Service gibi görünse de
    /// TechnicalService entity'si için Service katmanı olduğundan bu şekilde.
    /// Alternatif: ITechnicalServiceManager
    /// </summary>
    public interface ITechnicalServiceService
    {
        // ========================================================================
        // SORGULAMA (QUERY) METODLARI
        // ========================================================================

        /// <summary>
        /// ID ile Talep Getir
        ///
        /// İLİŞKİLİ VERİ: Customer, Store, AssignedEmployee
        /// DETAY SAYFASI: Ticket detay görüntüleme
        /// </summary>
        Task<TechnicalService?> GetByIdAsync(int id);

        /// <summary>
        /// Servis Numarası ile Talep Getir
        ///
        /// MÜŞTERİ TAKİBİ: "TS-2024-00123 nolu talebim ne durumda?"
        /// ÇAĞRI MERKEZİ: Numara ile hızlı erişim
        ///
        /// FORMAT: "TS-2024-00001" (TeknikServis-Yıl-SıraNo)
        /// </summary>
        Task<TechnicalService?> GetByServiceNumberAsync(string serviceNumber);

        /// <summary>
        /// Tüm Talepleri Getir
        ///
        /// ADMIN: Tüm ticket listesi
        /// RAPORLAMA: Genel istatistikler için
        /// </summary>
        Task<IEnumerable<TechnicalService>> GetAllAsync();

        /// <summary>
        /// Duruma Göre Talepler
        ///
        /// FİLTRELEME:
        /// - GetByStatusAsync(Acik) -> Yeni açılan, bekleyen
        /// - GetByStatusAsync(Islemde) -> İşleme alınmış
        /// - GetByStatusAsync(Tamamlandi) -> Çözülmüş
        ///
        /// DASHBOARD: Durum bazlı sayaçlar
        /// </summary>
        Task<IEnumerable<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status);

        /// <summary>
        /// Mağaza Talepleri
        ///
        /// ŞUBE YÖNETİMİ: "Ankara-1'de kaç açık talep var?"
        /// YETKİ: Şube müdürü kendi mağazasını görür
        /// </summary>
        Task<IEnumerable<TechnicalService>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Teknisyene Atanan Talepler
        ///
        /// İŞ LİSTESİ: "Bugün ne işlerim var?"
        /// PERFORMANS: Teknisyen verimliliği analizi
        ///
        /// ÖRNEK:
        /// var myTasks = await GetByAssignedEmployeeAsync(currentEmployeeId);
        /// </summary>
        Task<IEnumerable<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId);

        /// <summary>
        /// Açık Talepler
        ///
        /// KRİTİK: Status != Tamamlandi &amp;&amp; Status != Cozulemedi
        /// DASHBOARD: Bekleyen iş sayısı uyarısı
        /// SLA TAKİBİ: Geciken talepler öne çıkar
        /// </summary>
        Task<IEnumerable<TechnicalService>> GetOpenIssuesAsync();

        /// <summary>
        /// Atanmamış Talepler
        ///
        /// ACİL LİSTE: AssignedToEmployeeId = null
        /// İŞ DAĞITIMI: Yönetici bu listeyi teknisyenlere dağıtır
        ///
        /// ALARM: Atanmamış talep > 2 saat ise bildirim
        /// </summary>
        Task<IEnumerable<TechnicalService>> GetUnassignedAsync();

        // ========================================================================
        // KOMUT (COMMAND) METODLARI
        // ========================================================================

        /// <summary>
        /// Yeni Talep Oluştur
        ///
        /// TİCKET AÇMA SÜRECİ:
        /// 1. Servis numarası otomatik oluşturulur (TS-2024-XXXXX)
        /// 2. Status = Acik (başlangıç durumu)
        /// 3. ReceivedDate = Now
        /// 4. Bildirim gönderilir (yöneticiye)
        ///
        /// GERİ DÖNÜŞ: Oluşturulan TechnicalService (ID + ServiceNumber)
        /// </summary>
        Task<TechnicalService> CreateAsync(TechnicalService technicalService);

        /// <summary>
        /// Talep Güncelle
        ///
        /// GÜNCELLEME SENARYOLARI:
        /// - Problem tanımı detaylandırma
        /// - Müşteri bilgisi düzeltme
        /// - Öncelik değiştirme
        ///
        /// AUDİT: UpdatedAt otomatik güncellenir
        /// </summary>
        Task UpdateAsync(TechnicalService technicalService);

        /// <summary>
        /// Teknisyene Ata
        ///
        /// İŞ DAĞITIMI METODU:
        /// 1. AssignedToEmployeeId = employeeId
        /// 2. Status = Islemde (otomatik geçiş)
        /// 3. Teknisyene bildirim gönderilir
        ///
        /// YETKİ: Sadece Yönetici yapabilir
        ///
        /// ÖRNEK:
        /// await AssignToEmployeeAsync(ticketId: 123, employeeId: 5);
        /// </summary>
        Task AssignToEmployeeAsync(int serviceId, int employeeId);

        /// <summary>
        /// Durum Güncelle
        ///
        /// DURUM GEÇİŞLERİ (State Machine):
        ///
        /// [Acik] --atama--> [Islemde] --çözüm--> [Tamamlandi]
        ///                        |
        ///                        +--çözülemez--> [Cozulemedi]
        ///
        /// PARAMETRELER:
        /// - serviceId: Ticket ID
        /// - status: Yeni durum
        /// - resolution: Çözüm açıklaması (Tamamlandi durumunda zorunlu)
        ///
        /// ÖRNEK:
        /// // Ticket çözüldü
        /// await UpdateStatusAsync(123, TechnicalServiceStatus.Tamamlandi,
        ///     resolution: "Ekran kartı değiştirildi, test edildi.");
        /// </summary>
        Task UpdateStatusAsync(int serviceId, TechnicalServiceStatus status, string? resolution = null);

        // ========================================================================
        // DASHBOARD METODLARI
        // ========================================================================

        /// <summary>
        /// Açık Talep Sayısı
        ///
        /// DASHBOARD BADGE: Menüde "Teknik Servis (5)" şeklinde
        /// HIZLI SORGU: Count(*) - tüm veriyi çekmez
        ///
        /// KULLANIM:
        /// var openCount = await GetOpenIssuesCountAsync();
        /// // Badge'de göster: openCount > 0 ise kırmızı badge
        /// </summary>
        Task<int> GetOpenIssuesCountAsync();
    }
}
