using Application.Services;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

/// <summary>
/// NotificationService Implementation - SignalR ile Gerçek Zamanlı Bildirimler
///
/// NASIL ÇALIŞIR?
/// 1. Service katmanı (ISaleService, ITechnicalServiceService vb.) bu servisi çağırır
/// 2. Bu servis SignalR Hub üzerinden ilgili gruplara/kullanıcılara mesaj gönderir
/// 3. Client tarafındaki JavaScript mesajı alır ve kullanıcıya gösterir
///
/// MESAJ TİPLERİ:
/// - ReceiveNotification: Genel bildirim (başlık, mesaj, tip)
/// - ReceiveNewSale: Yeni satış bildirimi (Depo için)
/// - ReceiveCriticalStock: Kritik stok bildirimi (Şube Müdürü için)
/// - ReceiveNewTechnicalService: Yeni teknik servis talebi (Teknik Servis için)
/// - ReceiveMobileSale: Mobil satış bildirimi (Kasa için)
/// - ReceivePaymentConfirmed: Ödeme onayı (Depo için)
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Yeni satış bildirimi - Depo personeline gönderilir
    /// Kerim Zulacı anında görür ve ürünleri hazırlar
    /// </summary>
    public async Task NotifyNewSaleAsync(string saleNumber, int? cashRegisterNumber, string productSummary)
    {
        var notification = new
        {
            SaleNumber = saleNumber,
            CashRegisterNumber = cashRegisterNumber ?? 0,
            ProductSummary = productSummary,
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = $"Yeni Satış! {saleNumber} - Kasa {cashRegisterNumber}"
        };

        await _hubContext.Clients.Group("Depo").SendAsync("ReceiveNewSale", notification);
        _logger.LogInformation("Yeni satış bildirimi gönderildi: {SaleNumber}", saleNumber);
    }

    /// <summary>
    /// Kritik stok bildirimi - Şube Müdürüne gönderilir
    /// Haluk Bey hemen tedarik işlemi başlatabilir
    /// </summary>
    public async Task NotifyCriticalStockAsync(string productName, int currentStock, int criticalLevel)
    {
        var notification = new
        {
            ProductName = productName,
            CurrentStock = currentStock,
            CriticalLevel = criticalLevel,
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = $"Kritik Stok Uyarısı! {productName}: {currentStock} adet (Kritik: {criticalLevel})"
        };

        await _hubContext.Clients.Group("SubeYoneticisi").SendAsync("ReceiveCriticalStock", notification);
        await _hubContext.Clients.Group("Depo").SendAsync("ReceiveCriticalStock", notification);
        _logger.LogWarning("Kritik stok bildirimi: {ProductName} - {CurrentStock}/{CriticalLevel}",
            productName, currentStock, criticalLevel);
    }

    /// <summary>
    /// Yeni teknik servis bildirimi - Teknik Servis personeline gönderilir
    /// Özgün Kablocu anında görür ve müdahale eder
    /// </summary>
    public async Task NotifyNewTechnicalServiceAsync(string serviceNumber, string title, int priority, bool isCustomerIssue)
    {
        var priorityText = priority switch
        {
            4 => "KRİTİK",
            3 => "YÜKSEK",
            2 => "ORTA",
            _ => "DÜŞÜK"
        };

        var issueType = isCustomerIssue ? "Müşteri Sorunu" : "Sistem Sorunu";

        var notification = new
        {
            ServiceNumber = serviceNumber,
            Title = title,
            Priority = priority,
            PriorityText = priorityText,
            IsCustomerIssue = isCustomerIssue,
            IssueType = issueType,
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = $"[{priorityText}] {issueType}: {title}"
        };

        await _hubContext.Clients.Group("TeknikServis").SendAsync("ReceiveNewTechnicalService", notification);

        // Kritik öncelik ise Şube Müdürüne de bildir
        if (priority >= 3)
        {
            await _hubContext.Clients.Group("SubeYoneticisi").SendAsync("ReceiveNewTechnicalService", notification);
        }

        _logger.LogInformation("Teknik servis bildirimi: {ServiceNumber} - Öncelik: {Priority}",
            serviceNumber, priority);
    }

    /// <summary>
    /// Mobil satış bildirimi - Kasa personeline gönderilir
    /// Gül Satar anında görür ve fatura hazırlar
    /// </summary>
    public async Task NotifyMobileSaleAsync(string saleNumber, string employeeName, string customerName, decimal totalAmount)
    {
        var notification = new
        {
            SaleNumber = saleNumber,
            EmployeeName = employeeName,
            CustomerName = customerName,
            TotalAmount = totalAmount,
            TotalAmountText = $"{totalAmount:N2} TL",
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = $"Mobil Satış! {saleNumber} - {employeeName} tarafından {customerName}'e"
        };

        await _hubContext.Clients.Group("KasaSatis").SendAsync("ReceiveMobileSale", notification);
        _logger.LogInformation("Mobil satış bildirimi: {SaleNumber} - {EmployeeName}", saleNumber, employeeName);
    }

    /// <summary>
    /// Ödeme onayı bildirimi - Depo personeline gönderilir
    /// Kerim Zulacı ürünleri hazırlamaya başlar
    /// </summary>
    public async Task NotifyPaymentConfirmedAsync(string saleNumber, int? cashRegisterNumber)
    {
        var notification = new
        {
            SaleNumber = saleNumber,
            CashRegisterNumber = cashRegisterNumber ?? 0,
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = $"Ödeme Alındı! {saleNumber} - Kasa {cashRegisterNumber} - Ürünleri hazırlayın"
        };

        await _hubContext.Clients.Group("Depo").SendAsync("ReceivePaymentConfirmed", notification);
        _logger.LogInformation("Ödeme onayı bildirimi: {SaleNumber}", saleNumber);
    }

    /// <summary>
    /// Teknik servis atama bildirimi - Atanan personele gönderilir
    /// </summary>
    public async Task NotifyServiceAssignedAsync(string serviceNumber, int assignedEmployeeId)
    {
        var notification = new
        {
            ServiceNumber = serviceNumber,
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = $"Size yeni bir teknik servis talebi atandı: {serviceNumber}"
        };

        await _hubContext.Clients.Group($"User_{assignedEmployeeId}").SendAsync("ReceiveServiceAssigned", notification);
        _logger.LogInformation("Teknik servis atama bildirimi: {ServiceNumber} -> Employee {EmployeeId}",
            serviceNumber, assignedEmployeeId);
    }

    /// <summary>
    /// Teknik servis çözüldü bildirimi - Bildiren personele gönderilir
    /// </summary>
    public async Task NotifyServiceResolvedAsync(string serviceNumber, string resolution)
    {
        var notification = new
        {
            ServiceNumber = serviceNumber,
            Resolution = resolution,
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = $"Teknik servis talebi çözüldü: {serviceNumber}"
        };

        // Tüm teknik servis personeline bildir
        await _hubContext.Clients.Group("TeknikServis").SendAsync("ReceiveServiceResolved", notification);
        _logger.LogInformation("Teknik servis çözüm bildirimi: {ServiceNumber}", serviceNumber);
    }

    /// <summary>
    /// Belirli bir role bildirim gönder
    /// </summary>
    public async Task NotifyRoleAsync(string role, string title, string message, string type = "info")
    {
        var notification = new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification", notification);
        _logger.LogInformation("Rol bildirimi: {Role} - {Title}", role, title);
    }

    /// <summary>
    /// Belirli bir kullanıcıya bildirim gönder
    /// </summary>
    public async Task NotifyUserAsync(string userId, string title, string message, string type = "info")
    {
        var notification = new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        _logger.LogInformation("Kullanıcı bildirimi: User_{UserId} - {Title}", userId, title);
    }

    /// <summary>
    /// Tüm bağlı kullanıcılara bildirim gönder
    /// </summary>
    public async Task NotifyAllAsync(string title, string message, string type = "info")
    {
        var notification = new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        _logger.LogInformation("Genel bildirim: {Title}", title);
    }
}
