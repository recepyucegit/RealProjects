// ===================================================================================
// TEKNOROMA - CUSTOM JAVASCRIPT
// ===================================================================================
//
// Modern admin panel JavaScript fonksiyonlari.
// jQuery ve Bootstrap 5 ile uyumlu.
//
// ICERIK:
// - Sidebar toggle
// - DataTables yapilandirmasi
// - Toast bildirimleri
// - Form validasyonlari
// - AJAX islemleri
// - Chart.js grafikleri
// - Utility fonksiyonlar
//
// ===================================================================================

'use strict';

// ===================================================================================
// GLOBAL NAMESPACE
// ===================================================================================
const TeknoRoma = {
    // Yapilandirma
    config: {
        apiBaseUrl: '/api',
        dateFormat: 'DD.MM.YYYY',
        currencySymbol: 'TL',
        toastDuration: 3000
    },

    // Durum
    state: {
        sidebarCollapsed: false,
        currentPage: 1,
        isLoading: false
    }
};

// ===================================================================================
// DOCUMENT READY
// ===================================================================================
$(document).ready(function () {
    console.log('TeknoRoma Admin Panel yuklendi');

    // Baslangic islemleri
    TeknoRoma.init();
});

// ===================================================================================
// INITIALIZATION
// ===================================================================================
TeknoRoma.init = function () {
    // Sidebar toggle
    this.initSidebar();

    // Tooltips
    this.initTooltips();

    // DataTables
    this.initDataTables();

    // Form validasyonlari
    this.initFormValidation();

    // Animasyonlar
    this.initAnimations();

    // Keyboard shortcuts
    this.initKeyboardShortcuts();

    // Auto-refresh for dashboard
    this.initAutoRefresh();
};

// ===================================================================================
// SIDEBAR
// ===================================================================================
TeknoRoma.initSidebar = function () {
    const sidebar = $('.sidebar');
    const toggleBtn = $('.sidebar-toggle');
    const mainContent = $('.main-content');

    // Toggle butonu
    toggleBtn.on('click', function () {
        sidebar.toggleClass('collapsed');
        TeknoRoma.state.sidebarCollapsed = sidebar.hasClass('collapsed');

        // Local storage'a kaydet
        localStorage.setItem('sidebarCollapsed', TeknoRoma.state.sidebarCollapsed);
    });

    // Local storage'dan yukle
    if (localStorage.getItem('sidebarCollapsed') === 'true') {
        sidebar.addClass('collapsed');
        TeknoRoma.state.sidebarCollapsed = true;
    }

    // Mobil menu toggle
    $('.mobile-menu-toggle').on('click', function () {
        sidebar.toggleClass('show');
    });

    // Mobilde disari tiklandiginda kapat
    $(document).on('click', function (e) {
        if ($(window).width() < 992) {
            if (!$(e.target).closest('.sidebar, .mobile-menu-toggle').length) {
                sidebar.removeClass('show');
            }
        }
    });
};

// ===================================================================================
// TOOLTIPS & POPOVERS
// ===================================================================================
TeknoRoma.initTooltips = function () {
    // Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Bootstrap popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
};

// ===================================================================================
// DATATABLES
// ===================================================================================
TeknoRoma.initDataTables = function () {
    // Varsayilan DataTables ayarlari
    if ($.fn.DataTable) {
        $.extend(true, $.fn.dataTable.defaults, {
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json',
                search: '',
                searchPlaceholder: 'Ara...',
                lengthMenu: '_MENU_ kayit goster',
                info: '_TOTAL_ kayittan _START_ - _END_ arasi gosteriliyor',
                infoEmpty: 'Kayit bulunamadi',
                infoFiltered: '(_MAX_ kayit icinden filtrelendi)',
                paginate: {
                    first: '<i class="bi bi-chevron-double-left"></i>',
                    previous: '<i class="bi bi-chevron-left"></i>',
                    next: '<i class="bi bi-chevron-right"></i>',
                    last: '<i class="bi bi-chevron-double-right"></i>'
                }
            },
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'Tumu']],
            responsive: true,
            processing: true,
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                '<"row"<"col-sm-12"tr>>' +
                '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            drawCallback: function () {
                // Her cizimden sonra tooltipleri yenile
                TeknoRoma.initTooltips();
            }
        });

        // Otomatik DataTable baslat
        $('.datatable').each(function () {
            if (!$.fn.DataTable.isDataTable(this)) {
                $(this).DataTable();
            }
        });
    }
};

// DataTable'i yeniden yukle
TeknoRoma.reloadDataTable = function (tableId) {
    const table = $('#' + tableId).DataTable();
    table.ajax.reload(null, false);
};

// ===================================================================================
// FORM VALIDATION
// ===================================================================================
TeknoRoma.initFormValidation = function () {
    // Bootstrap form validasyonu
    const forms = document.querySelectorAll('.needs-validation');

    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');
        }, false);
    });

    // Real-time validation
    $('input[required], select[required], textarea[required]').on('blur', function () {
        const $this = $(this);
        if ($this.val().trim() === '') {
            $this.addClass('is-invalid').removeClass('is-valid');
        } else {
            $this.addClass('is-valid').removeClass('is-invalid');
        }
    });
};

// ===================================================================================
// TOAST BILDIRIMLERI
// ===================================================================================
TeknoRoma.toast = {
    container: null,

    init: function () {
        if (!this.container) {
            this.container = $('<div class="toast-container"></div>');
            $('body').append(this.container);
        }
    },

    show: function (message, type = 'info', duration = TeknoRoma.config.toastDuration) {
        this.init();

        const icons = {
            success: 'bi-check-circle-fill',
            error: 'bi-x-circle-fill',
            warning: 'bi-exclamation-triangle-fill',
            info: 'bi-info-circle-fill'
        };

        const bgColors = {
            success: 'bg-success',
            error: 'bg-danger',
            warning: 'bg-warning',
            info: 'bg-info'
        };

        const toastId = 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white ${bgColors[type]} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi ${icons[type]} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        this.container.append(toastHtml);

        const toastEl = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastEl, { delay: duration });
        toast.show();

        // Otomatik kaldir
        toastEl.addEventListener('hidden.bs.toast', function () {
            $(this).remove();
        });
    },

    success: function (message) {
        this.show(message, 'success');
    },

    error: function (message) {
        this.show(message, 'error');
    },

    warning: function (message) {
        this.show(message, 'warning');
    },

    info: function (message) {
        this.show(message, 'info');
    }
};

// ===================================================================================
// AJAX HELPER
// ===================================================================================
TeknoRoma.ajax = {
    request: function (options) {
        const defaults = {
            method: 'GET',
            dataType: 'json',
            contentType: 'application/json',
            beforeSend: function () {
                TeknoRoma.loading.show();
            },
            complete: function () {
                TeknoRoma.loading.hide();
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error:', error);
                TeknoRoma.toast.error('Bir hata olustu. Lutfen tekrar deneyin.');
            }
        };

        return $.ajax($.extend({}, defaults, options));
    },

    get: function (url, data) {
        return this.request({ url: url, method: 'GET', data: data });
    },

    post: function (url, data) {
        return this.request({
            url: url,
            method: 'POST',
            data: JSON.stringify(data)
        });
    },

    put: function (url, data) {
        return this.request({
            url: url,
            method: 'PUT',
            data: JSON.stringify(data)
        });
    },

    delete: function (url) {
        return this.request({ url: url, method: 'DELETE' });
    }
};

// ===================================================================================
// LOADING INDICATOR
// ===================================================================================
TeknoRoma.loading = {
    overlay: null,

    show: function (message = 'Yukleniyor...') {
        if (!this.overlay) {
            this.overlay = $(`
                <div class="loading-overlay">
                    <div class="text-center">
                        <div class="spinner-border spinner-border-lg text-primary" role="status"></div>
                        <p class="mt-3 text-muted loading-message">${message}</p>
                    </div>
                </div>
            `);
            $('body').append(this.overlay);
        }

        this.overlay.find('.loading-message').text(message);
        this.overlay.fadeIn(200);
        TeknoRoma.state.isLoading = true;
    },

    hide: function () {
        if (this.overlay) {
            this.overlay.fadeOut(200);
        }
        TeknoRoma.state.isLoading = false;
    }
};

// ===================================================================================
// CONFIRMATION DIALOG
// ===================================================================================
TeknoRoma.confirm = function (options) {
    const defaults = {
        title: 'Emin misiniz?',
        message: 'Bu islemi geri alamazsiniz.',
        confirmText: 'Evet',
        cancelText: 'Iptal',
        confirmClass: 'btn-danger',
        onConfirm: function () { },
        onCancel: function () { }
    };

    const settings = $.extend({}, defaults, options);

    const modalHtml = `
        <div class="modal fade" id="confirmModal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="bi bi-question-circle text-warning me-2"></i>
                            ${settings.title}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p class="mb-0">${settings.message}</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            ${settings.cancelText}
                        </button>
                        <button type="button" class="btn ${settings.confirmClass}" id="confirmBtn">
                            ${settings.confirmText}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Mevcut modali kaldir
    $('#confirmModal').remove();

    // Yeni modal ekle
    $('body').append(modalHtml);

    const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
    modal.show();

    // Event handlers
    $('#confirmBtn').on('click', function () {
        modal.hide();
        settings.onConfirm();
    });

    $('#confirmModal').on('hidden.bs.modal', function () {
        $(this).remove();
    });
};

// ===================================================================================
// ANIMATIONS
// ===================================================================================
TeknoRoma.initAnimations = function () {
    // Fade-in animasyonu icin Intersection Observer
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    document.querySelectorAll('.animate-on-scroll').forEach(el => {
        observer.observe(el);
    });

    // Counter animasyonu
    $('.counter').each(function () {
        const $this = $(this);
        const target = parseInt($this.data('target'));

        $({ count: 0 }).animate({ count: target }, {
            duration: 1500,
            easing: 'swing',
            step: function () {
                $this.text(Math.floor(this.count).toLocaleString('tr-TR'));
            },
            complete: function () {
                $this.text(target.toLocaleString('tr-TR'));
            }
        });
    });
};

// ===================================================================================
// KEYBOARD SHORTCUTS
// ===================================================================================
TeknoRoma.initKeyboardShortcuts = function () {
    $(document).on('keydown', function (e) {
        // Ctrl + S: Kaydet
        if (e.ctrlKey && e.key === 's') {
            e.preventDefault();
            $('form .btn-primary[type="submit"]').first().click();
        }

        // Ctrl + N: Yeni kayit
        if (e.ctrlKey && e.key === 'n') {
            e.preventDefault();
            const newBtn = $('a[href*="Create"], a[href*="New"]').first();
            if (newBtn.length) {
                window.location.href = newBtn.attr('href');
            }
        }

        // Escape: Modal kapat
        if (e.key === 'Escape') {
            $('.modal.show').modal('hide');
        }

        // F2: Arama kutusuna odaklan
        if (e.key === 'F2') {
            e.preventDefault();
            $('.dataTables_filter input, input[type="search"]').first().focus();
        }
    });
};

// ===================================================================================
// AUTO REFRESH (Dashboard icin)
// ===================================================================================
TeknoRoma.initAutoRefresh = function () {
    // Dashboard ise her 5 dakikada bir yenile
    if ($('.dashboard-page').length) {
        setInterval(function () {
            TeknoRoma.refreshDashboard();
        }, 300000); // 5 dakika
    }
};

TeknoRoma.refreshDashboard = function () {
    // Dashboard verilerini AJAX ile guncelle
    console.log('Dashboard yenileniyor...');
    // Burada AJAX cagrilari yapilabilir
};

// ===================================================================================
// UTILITY FUNCTIONS
// ===================================================================================
TeknoRoma.utils = {
    // Para formatla
    formatCurrency: function (amount) {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY',
            minimumFractionDigits: 2
        }).format(amount);
    },

    // Tarih formatla
    formatDate: function (date) {
        return new Date(date).toLocaleDateString('tr-TR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    },

    // Tarih ve saat formatla
    formatDateTime: function (date) {
        return new Date(date).toLocaleString('tr-TR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Sayi formatla
    formatNumber: function (number) {
        return new Intl.NumberFormat('tr-TR').format(number);
    },

    // URL'den parametre al
    getUrlParam: function (param) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(param);
    },

    // Debounce
    debounce: function (func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    // Clipboard'a kopyala
    copyToClipboard: function (text) {
        navigator.clipboard.writeText(text).then(() => {
            TeknoRoma.toast.success('Kopyalandi!');
        }).catch(err => {
            console.error('Kopyalama hatasi:', err);
            TeknoRoma.toast.error('Kopyalanamadi');
        });
    }
};

// ===================================================================================
// CHART HELPERS
// ===================================================================================
TeknoRoma.charts = {
    // Varsayilan renk paleti
    colors: {
        primary: '#667eea',
        success: '#38ef7d',
        warning: '#ffc107',
        danger: '#dc3545',
        info: '#17a2b8',
        purple: '#764ba2'
    },

    // Gradient olustur
    createGradient: function (ctx, color1, color2) {
        const gradient = ctx.createLinearGradient(0, 0, 0, 300);
        gradient.addColorStop(0, color1);
        gradient.addColorStop(1, color2);
        return gradient;
    },

    // Varsayilan chart ayarlari
    defaultOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'bottom',
                labels: {
                    padding: 20,
                    usePointStyle: true
                }
            }
        }
    }
};

// ===================================================================================
// DELETE CONFIRMATION
// ===================================================================================
TeknoRoma.deleteConfirm = function (url, itemName, callback) {
    TeknoRoma.confirm({
        title: 'Silme Onay',
        message: `<strong>${itemName}</strong> kaydini silmek istediginize emin misiniz?`,
        confirmText: 'Sil',
        confirmClass: 'btn-danger',
        onConfirm: function () {
            TeknoRoma.ajax.delete(url)
                .done(function (response) {
                    TeknoRoma.toast.success('Kayit basariyla silindi');
                    if (typeof callback === 'function') {
                        callback(response);
                    } else {
                        location.reload();
                    }
                })
                .fail(function () {
                    TeknoRoma.toast.error('Silme islemi basarisiz oldu');
                });
        }
    });
};

// ===================================================================================
// EXPORT (Dis kullanim icin)
// ===================================================================================
window.TeknoRoma = TeknoRoma;
