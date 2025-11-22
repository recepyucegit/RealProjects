// ============================================================================
// ISaleService.cs - Satış Servis Interface
// ============================================================================
// AÇIKLAMA:
// Satış işlemleri için iş mantığı katmanı.
// Satış oluşturma, iptal etme ve raporlama işlevleri.
//
// KRİTİK İŞ KURALLARI:
// 1. Satış oluşturulunca stok düşürülmeli
// 2. İptal edilince stok geri eklenmeli
// 3. Müşteri puanı güncellenmeli
// 4. Fiş numarası otomatik oluşturulmalı
//
// TRANSACTION YÖNETİMİ:
// CreateAsync içinde birden fazla tablo güncellenir:
// - Sales tablosu
// - SaleDetails tablosu
// - Products tablosu (stok)
// - Customers tablosu (puan)
// Hepsi tek transaction içinde olmalı (Unit of Work)
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    /// <summary>
    /// Satış Servis Interface
    ///
    /// POS SİSTEMİ: Kasada satış yapma
    /// E-TİCARET: Online sipariş oluşturma
    /// RAPORLAMA: Satış analizi
    /// </summary>
    public interface ISaleService
    {
        // ========================================================================
        // SORGULAMA (QUERY) METODLARI
        // ========================================================================

        /// <summary>
        /// ID ile Satış Getir
        ///
        /// İLİŞKİLİ VERİ: Include ile Customer, Employee, SaleDetails gelir
        /// FİŞ GÖRÜNTÜLEME: Satış detayı sayfası için
        /// </summary>
        Task<Sale?> GetByIdAsync(int id);

        /// <summary>
        /// Fiş Numarası ile Satış Getir
        ///
        /// MÜŞTERİ SORGUSU: "FS-2024-00123 numaralı fişim nerede?"
        /// İADE İŞLEMİ: Fiş numarası ile satış bulunur
        ///
        /// FORMAT: "FS-2024-00001" (Fiş-Yıl-SıraNo)
        /// </summary>
        Task<Sale?> GetBySaleNumberAsync(string saleNumber);

        /// <summary>
        /// Tüm Satışları Getir
        ///
        /// ADMIN: Satış listesi sayfası
        /// DİKKAT: Sayfalama gerekli - çok fazla kayıt olabilir
        /// </summary>
        Task<IEnumerable<Sale>> GetAllAsync();

        /// <summary>
        /// Müşteri Satışları
        ///
        /// MÜŞTERİ GEÇMİŞİ: Bu müşteri neleri aldı?
        /// VIP ANALİZİ: Toplam harcama hesaplama
        /// </summary>
        Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// Çalışan Satışları
        ///
        /// PERFORMANS: Bu kasiyer bugün kaç satış yaptı?
        /// PRİM HESAPLAMA: Aylık satış tutarı toplamı
        /// </summary>
        Task<IEnumerable<Sale>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Mağaza Satışları
        ///
        /// ŞUBE PERFORMANSI: Mağaza bazlı satış listesi
        /// YÖNETİCİ RAPORU: Şube müdürü kendi mağazasını görür
        /// </summary>
        Task<IEnumerable<Sale>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Duruma Göre Satışlar
        ///
        /// DURUM DEĞERLERİ:
        /// - Tamamlandi: Normal satış
        /// - Beklemede: Kredi onayı bekleniyor
        /// - IptalEdildi: İade yapılmış
        ///
        /// KARGO: Beklemede olanlar sevk bekliyor
        /// </summary>
        Task<IEnumerable<Sale>> GetByStatusAsync(SaleStatus status);

        /// <summary>
        /// Tarih Aralığına Göre Satışlar
        ///
        /// DÖNEMSEL RAPOR: "Ocak ayı satışları"
        /// KAMPANYA ANALİZİ: "Black Friday döneminde ne kadar sattık?"
        ///
        /// ÖRNEK:
        /// var ocakSatislari = await GetByDateRangeAsync(
        ///     new DateTime(2024, 1, 1),
        ///     new DateTime(2024, 1, 31));
        /// </summary>
        Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        // ========================================================================
        // KOMUT (COMMAND) METODLARI
        // ========================================================================

        /// <summary>
        /// Yeni Satış Oluştur
        ///
        /// EN KRİTİK METOD - Transaction içinde yapılanlar:
        /// 1. Sale kaydı oluştur (fiş numarası otomatik)
        /// 2. SaleDetail kayıtları oluştur (her ürün kalemi)
        /// 3. Her ürün için stok düşür
        /// 4. Müşteri varsa puan ekle
        /// 5. Kasa raporuna ekle
        ///
        /// PARAMETRE AÇIKLAMASI:
        /// - sale: Satış ana bilgileri (müşteri, toplam, ödeme tipi)
        /// - details: Satış kalemleri (ürün, adet, fiyat)
        ///
        /// ÖRNEK KULLANIM:
        /// var sale = new Sale { CustomerId = 1, PaymentType = PaymentType.KrediKarti };
        /// var details = new List&lt;SaleDetail&gt;
        /// {
        ///     new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 100 },
        ///     new SaleDetail { ProductId = 2, Quantity = 1, UnitPrice = 200 }
        /// };
        /// var createdSale = await _saleService.CreateAsync(sale, details);
        /// </summary>
        Task<Sale> CreateAsync(Sale sale, IEnumerable<SaleDetail> details);

        /// <summary>
        /// Satış Durumu Güncelle
        ///
        /// DURUM GEÇİŞLERİ (State Machine):
        /// Beklemede -> Tamamlandi (Ödeme alındı)
        /// Beklemede -> IptalEdildi (Müşteri vazgeçti)
        /// Tamamlandi -> IptalEdildi (İade)
        ///
        /// GEÇERSİZ GEÇİŞ: IptalEdildi -> Tamamlandi (HATA!)
        /// </summary>
        Task UpdateStatusAsync(int saleId, SaleStatus status);

        /// <summary>
        /// Satış İptal
        ///
        /// İADE İŞLEMİ - Transaction içinde:
        /// 1. Sale.Status = IptalEdildi
        /// 2. Her ürün için stok geri eklenir
        /// 3. Müşteri puanı düşürülür
        /// 4. Kasa raporundan çıkarılır
        ///
        /// NEDEN AYRI METOD?
        /// İptal işlemi özel iş kuralları gerektirir
        /// (yetki kontrolü, fatura iptali, vb.)
        /// </summary>
        Task CancelAsync(int saleId);

        // ========================================================================
        // RAPORLAMA METODLARI
        // ========================================================================

        /// <summary>
        /// Günlük Satış Toplamı
        ///
        /// KASA KAPANIŞ: Gün sonu ciro kontrolü
        /// storeId: Belirli mağaza için (null = tüm mağazalar)
        ///
        /// ÖRNEK:
        /// var bugunCiro = await GetDailyTotalAsync(DateTime.Today);
        /// var ankara1Ciro = await GetDailyTotalAsync(DateTime.Today, storeId: 1);
        /// </summary>
        Task<decimal> GetDailyTotalAsync(DateTime date, int? storeId = null);

        /// <summary>
        /// Aylık Satış Toplamı
        ///
        /// PERFORMANS RAPORU: "Ocak ayında ne kadar sattık?"
        /// HEDEF KARŞILAŞTIRMA: Aylık hedef vs gerçekleşen
        ///
        /// ÖRNEK:
        /// var ocakCirosu = await GetMonthlyTotalAsync(2024, 1);
        /// </summary>
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);
    }
}
