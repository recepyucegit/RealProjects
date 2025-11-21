using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Web.Hubs;

/// <summary>
/// SignalR Notification Hub - Gerçek Zamanlı Bildirimler
///
/// NASIL ÇALIŞIR?
/// 1. Client (tarayıcı) bu Hub'a WebSocket ile bağlanır
/// 2. Bağlantı sırasında kullanıcı rol grubuna eklenir
/// 3. Server tarafında bir olay olduğunda ilgili gruba mesaj gönderilir
/// 4. Client mesajı alır ve ekranda gösterir
///
/// GRUPLAR (Rol Bazlı):
/// - SubeYoneticisi: Haluk Bey - Kritik stok, genel raporlar
/// - KasaSatis: Gül Satar - Mobil satışlar
/// - Depo: Kerim Zulacı - Yeni satışlar, ödeme onayları
/// - Muhasebe: Feyza Paragöz - Gider onayları
/// - TeknikServis: Özgün Kablocu - Teknik servis talepleri
/// - MobilSatis: Fahri Cepçi - Satış onayları
///
/// GÜVENLİK: [Authorize] ile sadece giriş yapmış kullanıcılar bağlanabilir
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı Hub'a bağlandığında çağrılır
    /// Kullanıcıyı rol gruplarına ekler
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var connectionId = Context.ConnectionId;
        var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = user?.Identity?.Name ?? "Anonymous";

        _logger.LogInformation("Kullanıcı bağlandı: {UserName} (ConnectionId: {ConnectionId})", userName, connectionId);

        // Kullanıcıyı kendi ID'sine göre gruba ekle (bireysel bildirimler için)
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(connectionId, $"User_{userId}");
            _logger.LogInformation("Kullanıcı bireysel gruba eklendi: User_{UserId}", userId);
        }

        // Kullanıcıyı rol gruplarına ekle
        if (user?.IsInRole("SubeYoneticisi") == true)
        {
            await Groups.AddToGroupAsync(connectionId, "SubeYoneticisi");
            _logger.LogInformation("Kullanıcı SubeYoneticisi grubuna eklendi");
        }

        if (user?.IsInRole("KasaSatis") == true)
        {
            await Groups.AddToGroupAsync(connectionId, "KasaSatis");
            _logger.LogInformation("Kullanıcı KasaSatis grubuna eklendi");
        }

        if (user?.IsInRole("Depo") == true)
        {
            await Groups.AddToGroupAsync(connectionId, "Depo");
            _logger.LogInformation("Kullanıcı Depo grubuna eklendi");
        }

        if (user?.IsInRole("Muhasebe") == true)
        {
            await Groups.AddToGroupAsync(connectionId, "Muhasebe");
            _logger.LogInformation("Kullanıcı Muhasebe grubuna eklendi");
        }

        if (user?.IsInRole("TeknikServis") == true)
        {
            await Groups.AddToGroupAsync(connectionId, "TeknikServis");
            _logger.LogInformation("Kullanıcı TeknikServis grubuna eklendi");
        }

        if (user?.IsInRole("MobilSatis") == true)
        {
            await Groups.AddToGroupAsync(connectionId, "MobilSatis");
            _logger.LogInformation("Kullanıcı MobilSatis grubuna eklendi");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Kullanıcı Hub'dan ayrıldığında çağrılır
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userName = Context.User?.Identity?.Name ?? "Anonymous";
        _logger.LogInformation("Kullanıcı ayrıldı: {UserName}", userName);

        if (exception != null)
        {
            _logger.LogError(exception, "Bağlantı hatası ile ayrıldı: {UserName}", userName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client'tan mesaj alma (opsiyonel - debug için)
    /// </summary>
    public async Task SendMessage(string message)
    {
        var userName = Context.User?.Identity?.Name ?? "Anonymous";
        _logger.LogInformation("Mesaj alındı: {UserName} - {Message}", userName, message);

        // Echo back (test için)
        await Clients.Caller.SendAsync("ReceiveMessage", userName, message);
    }
}
