namespace Application.Services;

/// <summary>
/// Bildirim Servisi Interface
///
/// KULLANIM SENARYOLARI:
/// 1. Gül Satar satış onayladığında → Kerim Zulacı'ya (Depo) bildirim
/// 2. Stok kritik seviyeye düştüğünde → Haluk Bey'e bildirim
/// 3. Teknik servis kaydı açıldığında → Özgün Kablocu'ya bildirim
/// 4. Mobil satış yapıldığında → Gül Satar'a bildirim
///
/// TEKNOLOJİ: SignalR (WebSocket tabanlı gerçek zamanlı iletişim)
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Yeni satış bildirimi - Depo personeline
    /// Kerim Zulacı: "Satış yapıldığı anda anında görmem gerekiyor"
    /// </summary>
    /// <param name="saleNumber">Satış numarası (S-2024-00001)</param>
    /// <param name="cashRegisterNumber">Kasa numarası (1-10)</param>
    /// <param name="productSummary">Ürün özeti</param>
    Task NotifyNewSaleAsync(string saleNumber, int? cashRegisterNumber, string productSummary);

    /// <summary>
    /// Kritik stok bildirimi - Şube Müdürüne
    /// Gül Satar: "Kritik seviyenin altına düştüğü anda Haluk Bey'in uygulaması uyarı vermeli"
    /// </summary>
    /// <param name="productName">Ürün adı</param>
    /// <param name="currentStock">Mevcut stok</param>
    /// <param name="criticalLevel">Kritik seviye</param>
    Task NotifyCriticalStockAsync(string productName, int currentStock, int criticalLevel);

    /// <summary>
    /// Yeni teknik servis bildirimi - Teknik Servis personeline
    /// Özgün Kablocu: "Sorun bildirildiğinde otomatik uyarı istiyoruz"
    /// </summary>
    /// <param name="serviceNumber">Servis numarası (TS-2024-00001)</param>
    /// <param name="title">Sorun başlığı</param>
    /// <param name="priority">Öncelik (1-4)</param>
    /// <param name="isCustomerIssue">Müşteri sorunu mu?</param>
    Task NotifyNewTechnicalServiceAsync(string serviceNumber, string title, int priority, bool isCustomerIssue);

    /// <summary>
    /// Mobil satış bildirimi - Kasa personeline
    /// Gül Satar: "Cep bilgisayarıyla satış yapıldığında hemen beni uyarmalı"
    /// </summary>
    /// <param name="saleNumber">Satış numarası</param>
    /// <param name="employeeName">Satışı yapan mobil satış temsilcisi</param>
    /// <param name="customerName">Müşteri adı</param>
    /// <param name="totalAmount">Toplam tutar</param>
    Task NotifyMobileSaleAsync(string saleNumber, string employeeName, string customerName, decimal totalAmount);

    /// <summary>
    /// Ödeme alındı bildirimi - Depo personeline
    /// Kerim Zulacı: "Ödeme alındığında ürünleri hazırlayabilirim"
    /// </summary>
    /// <param name="saleNumber">Satış numarası</param>
    /// <param name="cashRegisterNumber">Kasa numarası</param>
    Task NotifyPaymentConfirmedAsync(string saleNumber, int? cashRegisterNumber);

    /// <summary>
    /// Teknik servis atama bildirimi - Atanan personele
    /// </summary>
    /// <param name="serviceNumber">Servis numarası</param>
    /// <param name="assignedEmployeeId">Atanan çalışan ID</param>
    Task NotifyServiceAssignedAsync(string serviceNumber, int assignedEmployeeId);

    /// <summary>
    /// Teknik servis çözüldü bildirimi - Bildiren personele
    /// </summary>
    /// <param name="serviceNumber">Servis numarası</param>
    /// <param name="resolution">Çözüm açıklaması</param>
    Task NotifyServiceResolvedAsync(string serviceNumber, string resolution);

    /// <summary>
    /// Belirli bir role bildirim gönder
    /// </summary>
    /// <param name="role">Hedef rol (SubeYoneticisi, Depo, vs.)</param>
    /// <param name="title">Bildirim başlığı</param>
    /// <param name="message">Bildirim mesajı</param>
    /// <param name="type">Bildirim tipi (info, warning, success, error)</param>
    Task NotifyRoleAsync(string role, string title, string message, string type = "info");

    /// <summary>
    /// Belirli bir kullanıcıya bildirim gönder
    /// </summary>
    /// <param name="userId">Hedef kullanıcı ID</param>
    /// <param name="title">Bildirim başlığı</param>
    /// <param name="message">Bildirim mesajı</param>
    /// <param name="type">Bildirim tipi</param>
    Task NotifyUserAsync(string userId, string title, string message, string type = "info");

    /// <summary>
    /// Tüm bağlı kullanıcılara bildirim gönder (broadcast)
    /// </summary>
    /// <param name="title">Bildirim başlığı</param>
    /// <param name="message">Bildirim mesajı</param>
    /// <param name="type">Bildirim tipi</param>
    Task NotifyAllAsync(string title, string message, string type = "info");
}
