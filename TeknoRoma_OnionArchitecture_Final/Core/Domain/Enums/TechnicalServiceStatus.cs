// ============================================================================
// TechnicalServiceStatus.cs - Teknik Servis Durumu Enum
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın teknik servis/destek taleplerinin durumunu takip eden enum.
// Ticket sistemi mantığıyla çalışır ve her talep yaşam döngüsü boyunca
// bu durumlardan birinde bulunur.
//
// SERVİS TALEBİ YAŞAM DÖNGÜSÜ:
// ┌───────┐    ┌──────────┐    ┌────────────┐
// │ Acik  │ -> │ Islemde  │ -> │ Tamamlandi │
// └───────┘    └──────────┘    └────────────┘
//                   │
//                   └───────────> ┌────────────┐
//                                 │ Cozulemedi │
//                                 └────────────┘
//
// SLA (Service Level Agreement) TAKİBİ:
// - Her durum için maksimum süre tanımlanabilir
// - Süre aşımlarında otomatik eskalasyon
// - Müşteri memnuniyeti için kritik
//
// TICKET SİSTEMİ KAVRAMLARI:
// - Talep (Ticket): Bildirilen sorun
// - Atama (Assignment): Teknisyene yönlendirme
// - Eskalasyon: Üst seviyeye iletme
// - Çözüm (Resolution): Sorunun giderilmesi
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Teknik Servis Durumu Enum'u
    ///
    /// TİCKET YÖNETİMİ:
    /// - Sorun takibi
    /// - Teknisyen ataması
    /// - Çözüm süresi analizi
    ///
    /// KULLANIM ÖRNEĞİ:
    /// <code>
    /// var serviceTicket = new TechnicalService
    /// {
    ///     Title = "POS Cihazı Çalışmıyor",
    ///     Status = TechnicalServiceStatus.Acik,
    ///     Priority = 1  // Kritik
    /// };
    /// </code>
    ///
    /// DURUM TAKİBİ:
    /// <code>
    /// // Açık talepleri listele
    /// var openTickets = await _context.TechnicalServices
    ///     .Where(t => t.Status == TechnicalServiceStatus.Acik)
    ///     .OrderBy(t => t.Priority)
    ///     .ThenBy(t => t.ReportedDate)
    ///     .ToListAsync();
    /// </code>
    /// </summary>
    public enum TechnicalServiceStatus
    {
        // ====================================================================
        // BAŞLANGIÇ DURUMU
        // ====================================================================

        /// <summary>
        /// Açık - Yeni Talep, Henüz İşleme Alınmadı
        ///
        /// AÇIKLAMA:
        /// - Sorun yeni bildirildi
        /// - Henüz bir teknisyene atanmadı
        /// - İlk değerlendirme bekleniyor
        ///
        /// OTOMATİK DAVRANIŞLAR:
        /// - AssignedToEmployeeId = null
        /// - ResolvedDate = null
        /// - Resolution = null
        ///
        /// SLA (Service Level Agreement):
        /// <code>
        /// // Önceliğe göre yanıt süresi
        /// var maxResponseTime = ticket.Priority switch
        /// {
        ///     1 => TimeSpan.FromHours(2),   // Kritik: 2 saat
        ///     2 => TimeSpan.FromHours(4),   // Yüksek: 4 saat
        ///     3 => TimeSpan.FromHours(24),  // Normal: 24 saat
        ///     4 => TimeSpan.FromHours(72),  // Düşük: 72 saat
        ///     _ => TimeSpan.FromHours(24)
        /// };
        ///
        /// var isOverdue = DateTime.Now - ticket.ReportedDate > maxResponseTime;
        /// </code>
        ///
        /// ATAMA SÜRECİ:
        /// <code>
        /// // Teknisyen atama
        /// public void AssignTechnician(TechnicalService ticket, int employeeId)
        /// {
        ///     if (ticket.Status != TechnicalServiceStatus.Acik)
        ///         throw new InvalidOperationException("Sadece açık talepler atanabilir");
        ///
        ///     ticket.AssignedToEmployeeId = employeeId;
        ///     ticket.Status = TechnicalServiceStatus.Islemde;
        /// }
        /// </code>
        ///
        /// DASHBOARD GÖSTERIMI:
        /// - Kırmızı/turuncu uyarı ile gösterilir
        /// - Acil müdahale gerektiren talepler listelenir
        /// </summary>
        Acik = 1,

        // ====================================================================
        // İŞLEM DURUMU
        // ====================================================================

        /// <summary>
        /// İşlemde - Sorun Üzerinde Çalışılıyor
        ///
        /// AÇIKLAMA:
        /// - Teknisyen atandı ve çalışmaya başladı
        /// - Sorun analiz ediliyor veya çözülüyor
        /// - En aktif durum
        ///
        /// GEÇİŞ KOŞULLARI:
        /// - Acik -> Islemde: Teknisyen atandığında
        /// - Islemde -> Tamamlandi: Sorun çözüldüğünde
        /// - Islemde -> Cozulemedi: Çözüm bulunamadığında
        ///
        /// İŞ TAKİBİ:
        /// <code>
        /// // Teknisyenin aktif talepleri
        /// var myTickets = await _context.TechnicalServices
        ///     .Where(t => t.AssignedToEmployeeId == currentUserId)
        ///     .Where(t => t.Status == TechnicalServiceStatus.Islemde)
        ///     .ToListAsync();
        ///
        /// // İş yükü kontrolü
        /// var workload = myTickets.Count;
        /// if (workload >= 5)
        /// {
        ///     // Yeni atama yapma, başka teknisyene yönlendir
        /// }
        /// </code>
        ///
        /// PROGRESS GÜNCELLEME:
        /// <code>
        /// // Not ekleme (Resolution alanına geçici notlar)
        /// ticket.Resolution = $"[{DateTime.Now:dd.MM.yyyy HH:mm}] " +
        ///     "Sorun tespit edildi: Güç adaptörü arızalı. " +
        ///     "Yedek parça sipariş edildi.";
        /// </code>
        ///
        /// MÜŞTERİ BİLDİRİMİ:
        /// - Durum değişikliğinde müşteriye SMS/e-posta
        /// - "Talebiniz işleme alındı, teknisyenimiz çalışıyor"
        /// </summary>
        Islemde = 2,

        // ====================================================================
        // SONUÇ DURUMLARI
        // ====================================================================

        /// <summary>
        /// Tamamlandı - Sorun Başarıyla Çözüldü
        ///
        /// AÇIKLAMA:
        /// - Sorun giderildi
        /// - Müşteri/kullanıcı onayladı
        /// - Talep kapatıldı
        ///
        /// ZORUNLU ALANLAR:
        /// - Resolution: Çözümün açıklaması (boş olamaz)
        /// - ResolvedDate: Çözüm tarihi (otomatik set)
        ///
        /// ÇÖZÜM KAYDI:
        /// <code>
        /// public void CompleteTicket(TechnicalService ticket, string resolution)
        /// {
        ///     if (string.IsNullOrWhiteSpace(resolution))
        ///         throw new ArgumentException("Çözüm açıklaması zorunludur");
        ///
        ///     ticket.Status = TechnicalServiceStatus.Tamamlandi;
        ///     ticket.Resolution = resolution;
        ///     ticket.ResolvedDate = DateTime.Now;
        /// }
        /// </code>
        ///
        /// ÖRNEK ÇÖZÜMLER:
        /// - "POS cihazının güç adaptörü değiştirildi. Cihaz çalışıyor."
        /// - "Internet modem resetlendi, bağlantı sağlandı."
        /// - "Yazılım güncellendi, hata giderildi."
        ///
        /// PERFORMANS METRİKLERİ:
        /// <code>
        /// // Ortalama çözüm süresi
        /// var avgResolutionTime = completedTickets
        ///     .Where(t => t.ResolvedDate.HasValue)
        ///     .Average(t => (t.ResolvedDate!.Value - t.ReportedDate).TotalHours);
        ///
        /// // Teknisyen bazında çözüm sayısı
        /// var technicianStats = completedTickets
        ///     .GroupBy(t => t.AssignedToEmployee.FullName)
        ///     .Select(g => new
        ///     {
        ///         Technician = g.Key,
        ///         SolvedCount = g.Count(),
        ///         AvgTime = g.Average(t =>
        ///             (t.ResolvedDate!.Value - t.ReportedDate).TotalHours)
        ///     });
        /// </code>
        ///
        /// BİLGİ BANKASI:
        /// - Çözümler bilgi bankasına eklenir
        /// - Benzer sorunlarda referans olarak kullanılır
        /// - Yeni teknisyenler için eğitim materyali
        /// </summary>
        Tamamlandi = 3,

        /// <summary>
        /// Çözülemedi - Sorun Giderilemedi
        ///
        /// AÇIKLAMA:
        /// - Sorun teknik olarak çözülemedi
        /// - Dış faktörler nedeniyle müdahale edilemedi
        /// - Müşteri talebi geri çekti
        ///
        /// NEDENLERİ:
        /// 1. Teknik imkansızlık (donanım tamiri mümkün değil)
        /// 2. Garanti dışı (müşteri ücret ödemek istemedi)
        /// 3. Parça temin edilemedi
        /// 4. Müşteri vazgeçti / iletişim kurulamadı
        /// 5. Üçüncü taraf sorunu (ISP, üretici vs.)
        ///
        /// ZORUNLU ALANLAR:
        /// - Resolution: Neden çözülemediğinin açıklaması
        /// - ResolvedDate: Kapanış tarihi
        ///
        /// <code>
        /// public void MarkAsUnresolved(TechnicalService ticket, string reason)
        /// {
        ///     if (string.IsNullOrWhiteSpace(reason))
        ///         throw new ArgumentException("Çözülememe nedeni zorunludur");
        ///
        ///     ticket.Status = TechnicalServiceStatus.Cozulemedi;
        ///     ticket.Resolution = $"ÇÖZÜLEMEME NEDENİ: {reason}";
        ///     ticket.ResolvedDate = DateTime.Now;
        /// }
        /// </code>
        ///
        /// ÖRNEK NEDENLER:
        /// - "Cihaz anakartı arızalı, tamir ekonomik değil. Yeni cihaz önerildi."
        /// - "Müşteri garanti dışı tamir ücretini kabul etmedi."
        /// - "Yedek parça üretici tarafından temin edilemiyor."
        ///
        /// ESKALasyon:
        /// <code>
        /// // Çözülemeyen taleplerin analizi
        /// var unresolvedTickets = tickets
        ///     .Where(t => t.Status == TechnicalServiceStatus.Cozulemedi);
        ///
        /// var unresolvedRate = (decimal)unresolvedTickets.Count() /
        ///                      tickets.Count() * 100;
        ///
        /// // %10'dan fazlaysa yöneticiye rapor
        /// if (unresolvedRate > 10)
        /// {
        ///     await _reportService.GenerateEscalationReport(unresolvedTickets);
        /// }
        /// </code>
        ///
        /// MÜŞTERİ İLETİŞİMİ:
        /// - Neden çözülemediği açıkça bildirilmeli
        /// - Alternatif çözümler önerilmeli
        /// - Müşteri memnuniyeti anketi yapılabilir
        /// </summary>
        Cozulemedi = 4
    }
}

// ============================================================================
// EK BİLGİLER VE BEST PRACTICES
// ============================================================================
//
// GEÇERLİ DURUM GEÇİŞLERİ:
// <code>
// public static bool CanTransitionTo(TechnicalServiceStatus from, TechnicalServiceStatus to)
// {
//     return (from, to) switch
//     {
//         (TechnicalServiceStatus.Acik, TechnicalServiceStatus.Islemde) => true,
//         (TechnicalServiceStatus.Islemde, TechnicalServiceStatus.Tamamlandi) => true,
//         (TechnicalServiceStatus.Islemde, TechnicalServiceStatus.Cozulemedi) => true,
//         // Tamamlandi ve Cozulemedi son durumlar, geçiş yok
//         _ => false
//     };
// }
// </code>
//
// GENİŞLETME ÖNERİSİ:
// Daha detaylı takip için ek durumlar:
// <code>
// public enum TechnicalServiceStatus
// {
//     Acik = 1,
//     Atandi = 2,
//     Islemde = 3,
//     ParcaBekleniyor = 4,
//     MusteriBekleniyor = 5,
//     TestEdiliyor = 6,
//     Tamamlandi = 7,
//     Cozulemedi = 8,
//     Iptal = 9
// }
// </code>
//
// SLA KONFIGÜRASYONU:
// <code>
// public class SLAConfiguration
// {
//     public Dictionary<int, TimeSpan> ResponseTimeByPriority = new()
//     {
//         { 1, TimeSpan.FromHours(2) },   // Kritik
//         { 2, TimeSpan.FromHours(4) },   // Yüksek
//         { 3, TimeSpan.FromHours(24) },  // Normal
//         { 4, TimeSpan.FromHours(72) }   // Düşük
//     };
//
//     public Dictionary<int, TimeSpan> ResolutionTimeByPriority = new()
//     {
//         { 1, TimeSpan.FromHours(4) },   // Kritik
//         { 2, TimeSpan.FromHours(8) },   // Yüksek
//         { 3, TimeSpan.FromHours(48) },  // Normal
//         { 4, TimeSpan.FromHours(168) }  // Düşük (1 hafta)
//     };
// }
// </code>
//
// DASHBOARD METRİKLERİ:
// - Açık talep sayısı
// - Ortalama çözüm süresi
// - SLA uyum oranı
// - Teknisyen performansı
// - Çözülemeyen talep oranı
// ============================================================================
