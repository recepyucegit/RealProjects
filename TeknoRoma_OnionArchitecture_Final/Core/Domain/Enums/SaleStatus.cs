// ============================================================================
// SaleStatus.cs - Satış Durumu Enum
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'daki satış işlemlerinin durumunu takip eden enum.
// Her satış, yaşam döngüsü boyunca bu durumlardan birinde bulunur.
//
// SATIŞ YAŞAM DÖNGÜSÜ (State Machine):
// ┌──────────┐    ┌──────────────┐    ┌────────────┐
// │ Beklemede │ -> │ Hazirlaniyor │ -> │ Tamamlandi │
// └──────────┘    └──────────────┘    └────────────┘
//       │                 │
//       └─────────────────┴──────────> ┌────────┐
//                                      │ Iptal  │
//                                      └────────┘
//
// İŞ KURALLARI:
// - Satış oluşturulduğunda "Beklemede" olarak başlar
// - Ödeme alındığında "Hazirlaniyor" olur
// - Teslim edildiğinde "Tamamlandi" olur
// - Her aşamada "Iptal" edilebilir (iş kuralına bağlı)
//
// STOK ETKİSİ:
// - Beklemede: Stok rezerve edilebilir
// - Tamamlandi: Stok düşülür
// - Iptal: Rezerv kaldırılır / Stok geri eklenir
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Satış Durumu Enum'u
    ///
    /// STATE MACHINE (Durum Makinesi):
    /// - Her durumdan hangi duruma geçilebileceği tanımlıdır
    /// - Geçersiz geçişler engellenmelidir
    ///
    /// KULLANIM ÖRNEKLERİ:
    /// <code>
    /// // Satış oluşturma
    /// var sale = new Sale
    /// {
    ///     Status = SaleStatus.Beklemede
    /// };
    ///
    /// // Durum güncelleme
    /// public void CompletePayment(Sale sale)
    /// {
    ///     if (sale.Status != SaleStatus.Beklemede)
    ///         throw new InvalidOperationException("Sadece bekleyen satışlar için ödeme alınabilir");
    ///
    ///     sale.Status = SaleStatus.Hazirlaniyor;
    /// }
    ///
    /// // Filtreli sorgulama
    /// var pendingSales = await _context.Sales
    ///     .Where(s => s.Status == SaleStatus.Beklemede)
    ///     .ToListAsync();
    /// </code>
    /// </summary>
    public enum SaleStatus
    {
        // ====================================================================
        // BAŞLANGIÇ DURUMU
        // ====================================================================

        /// <summary>
        /// Beklemede - Satış Oluşturuldu, Ödeme Bekleniyor
        ///
        /// AÇIKLAMA:
        /// - Satış kaydı oluşturuldu
        /// - Henüz ödeme alınmadı
        /// - Sepet oluşturuldu ama tamamlanmadı
        ///
        /// KULLANIM SENARYOLARI:
        /// 1. Online sipariş: Sepete eklendi, ödeme bekleniyor
        /// 2. Havale/EFT: Sipariş alındı, ödeme bankaya düşmedi
        /// 3. Mağaza: Ürünler barkodlandı, müşteri kasada
        ///
        /// GEÇİŞLER:
        /// - -> Hazirlaniyor: Ödeme alındığında
        /// - -> Iptal: Müşteri vazgeçtiğinde / Timeout
        ///
        /// STOK YÖNETİMİ:
        /// <code>
        /// // Opsiyonel: Stok rezervasyonu
        /// if (sale.Status == SaleStatus.Beklemede)
        /// {
        ///     product.ReservedStock += quantity;
        ///     product.AvailableStock -= quantity;
        /// }
        /// </code>
        ///
        /// TIMEOUT SENARYOSU:
        /// - Online siparişlerde 30 dakika içinde ödeme yapılmazsa
        /// - Otomatik olarak Iptal durumuna geçebilir
        /// </summary>
        Beklemede = 1,

        // ====================================================================
        // İŞLEM DURUMU
        // ====================================================================

        /// <summary>
        /// Hazırlanıyor - Ödeme Alındı, Ürünler Hazırlanıyor
        ///
        /// AÇIKLAMA:
        /// - Ödeme başarıyla alındı
        /// - Ürünler paketleniyor / hazırlanıyor
        /// - Teslimata hazır hale getiriliyor
        ///
        /// KULLANIM SENARYOLARI:
        /// 1. Online sipariş: Depodan ürünler toplanıyor
        /// 2. Mağaza: Büyük ürünler depoya gitmiş, getiriliyor
        /// 3. Teknik ürün: Kurulum/kontrol yapılıyor
        ///
        /// GEÇİŞLER:
        /// - Beklemede -> Hazirlaniyor: Ödeme alındı
        /// - Hazirlaniyor -> Tamamlandi: Teslim edildi
        /// - Hazirlaniyor -> Iptal: İade talebi (nadir)
        ///
        /// BİLDİRİM:
        /// <code>
        /// // Müşteriye bildirim gönder
        /// if (oldStatus == SaleStatus.Beklemede &&
        ///     newStatus == SaleStatus.Hazirlaniyor)
        /// {
        ///     await _notificationService.SendSms(
        ///         sale.Customer.Phone,
        ///         $"Siparişiniz {sale.SaleNumber} hazırlanıyor."
        ///     );
        /// }
        /// </code>
        ///
        /// KARGO ENTEGRASYONU:
        /// - Bu aşamada kargo etiketi basılabilir
        /// - Takip numarası oluşturulabilir
        /// </summary>
        Hazirlaniyor = 2,

        // ====================================================================
        // SONUÇ DURUMLARI
        // ====================================================================

        /// <summary>
        /// Tamamlandı - Satış Başarıyla Sonuçlandı
        ///
        /// AÇIKLAMA:
        /// - Ürünler müşteriye teslim edildi
        /// - Satış işlemi kapandı
        /// - Bu son durum, geri dönüş yok (sadece iade)
        ///
        /// ETKİLER:
        /// 1. Stok: Product.UnitsInStock azaltılır
        /// 2. Ciro: Günlük/aylık satış raporlarına eklenir
        /// 3. Müşteri: Sadakat puanı eklenir
        /// 4. Çalışan: Satış primi hesabına yansır
        ///
        /// STOK DÜŞÜMÜ:
        /// <code>
        /// // Satış tamamlandığında stok güncelleme
        /// if (sale.Status == SaleStatus.Tamamlandi)
        /// {
        ///     foreach (var detail in sale.SaleDetails)
        ///     {
        ///         detail.Product.UnitsInStock -= detail.Quantity;
        ///     }
        /// }
        /// </code>
        ///
        /// RAPORLAMA:
        /// <code>
        /// // Günlük ciro hesaplama
        /// var dailyRevenue = await _context.Sales
        ///     .Where(s => s.Status == SaleStatus.Tamamlandi)
        ///     .Where(s => s.SaleDate.Date == DateTime.Today)
        ///     .SumAsync(s => s.TotalAmount);
        /// </code>
        ///
        /// İADE DURUMU:
        /// - Tamamlandı satışlar için ayrı iade süreci başlatılabilir
        /// - Return/Refund entity'si oluşturulabilir
        /// </summary>
        Tamamlandi = 3,

        /// <summary>
        /// İptal - Satış İptal Edildi
        ///
        /// AÇIKLAMA:
        /// - Satış herhangi bir nedenle iptal edildi
        /// - Ödeme alındıysa iade edilmeli
        /// - Stok geri eklenmeli
        ///
        /// İPTAL NEDENLERİ:
        /// 1. Müşteri vazgeçti
        /// 2. Ödeme alınamadı (timeout)
        /// 3. Stok yetersiz (satış sonrası tespit)
        /// 4. Fiyat hatası
        /// 5. Sahte sipariş (fraud)
        ///
        /// SOFT DELETE vs İPTAL:
        /// - IsDeleted = true: Kayıt tamamen gizlenir
        /// - Status = Iptal: Kayıt görünür, raporlanabilir
        /// - İptal edilen satışlar analiz için önemli
        ///
        /// STOK GERİ EKLEME:
        /// <code>
        /// // İptal durumunda stok geri ekleme
        /// if (sale.Status == SaleStatus.Iptal)
        /// {
        ///     foreach (var detail in sale.SaleDetails)
        ///     {
        ///         // Eğer stok zaten düşülmüşse geri ekle
        ///         if (previousStatus == SaleStatus.Tamamlandi)
        ///         {
        ///             detail.Product.UnitsInStock += detail.Quantity;
        ///         }
        ///     }
        /// }
        /// </code>
        ///
        /// İADE İŞLEMİ:
        /// <code>
        /// // Ödeme iadesi
        /// if (sale.PaymentType == PaymentType.KrediKarti)
        /// {
        ///     await _posService.Refund(
        ///         originalTransactionId: sale.TransactionId,
        ///         amount: sale.TotalAmount
        ///     );
        /// }
        /// </code>
        ///
        /// RAPORLAMA:
        /// - İptal oranı takibi
        /// - İptal nedeni analizi
        /// - Fraud tespiti
        /// </summary>
        Iptal = 4
    }
}

// ============================================================================
// EK BİLGİLER VE BEST PRACTICES
// ============================================================================
//
// GEÇERLİ DURUM GEÇİŞLERİ (State Transitions):
// <code>
// public bool CanTransitionTo(SaleStatus from, SaleStatus to)
// {
//     return (from, to) switch
//     {
//         (SaleStatus.Beklemede, SaleStatus.Hazirlaniyor) => true,
//         (SaleStatus.Beklemede, SaleStatus.Iptal) => true,
//         (SaleStatus.Hazirlaniyor, SaleStatus.Tamamlandi) => true,
//         (SaleStatus.Hazirlaniyor, SaleStatus.Iptal) => true,
//         // Tamamlandi -> başka duruma geçemez
//         // Iptal -> başka duruma geçemez
//         _ => false
//     };
// }
// </code>
//
// GENİŞLETME ÖNERİSİ:
// Daha detaylı takip için ek durumlar eklenebilir:
// <code>
// public enum SaleStatus
// {
//     Beklemede = 1,
//     OdemeAlindi = 2,
//     Hazirlaniyor = 3,
//     KargoyaVerildi = 4,
//     TeslimEdildi = 5,
//     Tamamlandi = 6,
//     IadeTalep = 7,
//     IadeTamamlandi = 8,
//     Iptal = 9
// }
// </code>
//
// DURUM DEĞİŞİKLİĞİ LOGLAMA:
// Her durum değişikliği audit trail için kaydedilebilir.
// SaleStatusHistory tablosu oluşturulabilir.
// ============================================================================
