/**
 * TeknoRoma Bildirim Sistemi - SignalR Client
 *
 * KULLANIM:
 * 1. Sayfa yüklendiğinde otomatik Hub'a bağlanır
 * 2. Rol bazlı bildirimleri dinler
 * 3. Bildirim geldiğinde ekranda gösterir (toast notification)
 * 4. Ses ile uyarı verir (önemli bildirimler için)
 *
 * GEREKSINIMLER:
 * - signalr.js kütüphanesi (_Layout.cshtml'de yüklenmeli)
 * - Bootstrap Toast bileşeni
 */

// SignalR bağlantısı
let connection = null;

// Bildirim sesleri
const notificationSound = new Audio('/sounds/notification.mp3');
const criticalSound = new Audio('/sounds/critical.mp3');

/**
 * SignalR Hub'a bağlan
 */
async function connectToNotificationHub() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Otomatik yeniden bağlan
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Bağlantı durumu değişiklikleri
    connection.onreconnecting(error => {
        console.log("Yeniden bağlanıyor...", error);
        showToast("Bağlantı", "Sunucuya yeniden bağlanıyor...", "warning");
    });

    connection.onreconnected(connectionId => {
        console.log("Yeniden bağlandı:", connectionId);
        showToast("Bağlantı", "Sunucuya yeniden bağlandı!", "success");
    });

    connection.onclose(error => {
        console.log("Bağlantı kapandı:", error);
        // 5 saniye sonra tekrar dene
        setTimeout(() => startConnection(), 5000);
    });

    // Bildirim dinleyicileri
    setupNotificationListeners();

    // Bağlantıyı başlat
    await startConnection();
}

/**
 * Bağlantıyı başlat
 */
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Hub'a bağlandı");
    } catch (err) {
        console.error("Bağlantı hatası:", err);
        // 5 saniye sonra tekrar dene
        setTimeout(() => startConnection(), 5000);
    }
}

/**
 * Bildirim dinleyicilerini ayarla
 */
function setupNotificationListeners() {
    // Genel bildirim
    connection.on("ReceiveNotification", (notification) => {
        console.log("Bildirim alındı:", notification);
        showToast(notification.title, notification.message, notification.type);
        playNotificationSound(notification.type);
    });

    // Yeni satış (Depo için)
    connection.on("ReceiveNewSale", (notification) => {
        console.log("Yeni satış:", notification);
        showToast(
            `🛒 Yeni Satış - Kasa ${notification.cashRegisterNumber}`,
            `${notification.saleNumber}: ${notification.productSummary}`,
            "info"
        );
        playNotificationSound("info");
        updatePendingSalesCounter();
    });

    // Kritik stok (Şube Müdürü için)
    connection.on("ReceiveCriticalStock", (notification) => {
        console.log("Kritik stok:", notification);
        showToast(
            `⚠️ Kritik Stok Uyarısı!`,
            `${notification.productName}: ${notification.currentStock} adet kaldı (Kritik: ${notification.criticalLevel})`,
            "warning"
        );
        playCriticalSound();
        updateCriticalStockBadge();
    });

    // Yeni teknik servis (Teknik Servis için)
    connection.on("ReceiveNewTechnicalService", (notification) => {
        console.log("Yeni teknik servis:", notification);
        const icon = notification.priority >= 3 ? "🔴" : "🟡";
        showToast(
            `${icon} ${notification.issueType} - ${notification.priorityText}`,
            `${notification.serviceNumber}: ${notification.title}`,
            notification.priority >= 3 ? "error" : "warning"
        );
        if (notification.priority >= 3) {
            playCriticalSound();
        } else {
            playNotificationSound("warning");
        }
        updateTechnicalServiceBadge();
    });

    // Mobil satış (Kasa için)
    connection.on("ReceiveMobileSale", (notification) => {
        console.log("Mobil satış:", notification);
        showToast(
            `📱 Mobil Satış Geldi!`,
            `${notification.saleNumber} - ${notification.employeeName} → ${notification.customerName} (${notification.totalAmountText})`,
            "success"
        );
        playNotificationSound("success");
        updateMobileSalesCounter();
    });

    // Ödeme onayı (Depo için)
    connection.on("ReceivePaymentConfirmed", (notification) => {
        console.log("Ödeme onayı:", notification);
        showToast(
            `💳 Ödeme Alındı - Kasa ${notification.cashRegisterNumber}`,
            `${notification.saleNumber} - Ürünleri hazırlayın!`,
            "success"
        );
        playNotificationSound("success");
        updatePendingSalesCounter();
    });

    // Teknik servis atama
    connection.on("ReceiveServiceAssigned", (notification) => {
        console.log("Servis atandı:", notification);
        showToast(
            `📋 Yeni Görev Atandı`,
            notification.message,
            "info"
        );
        playNotificationSound("info");
    });

    // Teknik servis çözüm
    connection.on("ReceiveServiceResolved", (notification) => {
        console.log("Servis çözüldü:", notification);
        showToast(
            `✅ Sorun Çözüldü`,
            `${notification.serviceNumber}: ${notification.resolution.substring(0, 50)}...`,
            "success"
        );
        playNotificationSound("success");
    });
}

/**
 * Toast bildirimi göster
 */
function showToast(title, message, type = "info") {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        console.warn("Toast container bulunamadı");
        return;
    }

    const bgClass = {
        "info": "bg-info",
        "success": "bg-success",
        "warning": "bg-warning",
        "error": "bg-danger"
    }[type] || "bg-secondary";

    const toastId = `toast-${Date.now()}`;
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong><br>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: 8000 // 8 saniye görünür
    });
    toast.show();

    // 10 saniye sonra DOM'dan kaldır
    setTimeout(() => {
        toastElement.remove();
    }, 10000);
}

/**
 * Bildirim sesi çal
 */
function playNotificationSound(type) {
    try {
        notificationSound.currentTime = 0;
        notificationSound.play().catch(e => console.log("Ses çalınamadı:", e));
    } catch (e) {
        console.log("Ses hatası:", e);
    }
}

/**
 * Kritik uyarı sesi çal
 */
function playCriticalSound() {
    try {
        criticalSound.currentTime = 0;
        criticalSound.play().catch(e => console.log("Kritik ses çalınamadı:", e));
    } catch (e) {
        console.log("Ses hatası:", e);
    }
}

/**
 * Bekleyen satış sayacını güncelle (varsa)
 */
function updatePendingSalesCounter() {
    const badge = document.getElementById('pending-sales-badge');
    if (badge) {
        let count = parseInt(badge.textContent) || 0;
        badge.textContent = count + 1;
        badge.classList.remove('d-none');
    }
}

/**
 * Kritik stok badge'ini güncelle (varsa)
 */
function updateCriticalStockBadge() {
    const badge = document.getElementById('critical-stock-badge');
    if (badge) {
        let count = parseInt(badge.textContent) || 0;
        badge.textContent = count + 1;
        badge.classList.remove('d-none');
    }
}

/**
 * Teknik servis badge'ini güncelle (varsa)
 */
function updateTechnicalServiceBadge() {
    const badge = document.getElementById('technical-service-badge');
    if (badge) {
        let count = parseInt(badge.textContent) || 0;
        badge.textContent = count + 1;
        badge.classList.remove('d-none');
    }
}

/**
 * Mobil satış sayacını güncelle (varsa)
 */
function updateMobileSalesCounter() {
    const badge = document.getElementById('mobile-sales-badge');
    if (badge) {
        let count = parseInt(badge.textContent) || 0;
        badge.textContent = count + 1;
        badge.classList.remove('d-none');
    }
}

// Sayfa yüklendiğinde bağlan
document.addEventListener('DOMContentLoaded', function() {
    // Kullanıcı giriş yapmışsa bağlan
    if (document.querySelector('[data-user-authenticated="true"]') || window.userAuthenticated) {
        connectToNotificationHub();
    }
});
