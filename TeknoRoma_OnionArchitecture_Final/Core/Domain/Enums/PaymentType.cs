// ============================================================================
// PaymentType.cs - Ödeme Türü Enum
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'daki satış işlemlerinde kullanılan ödeme yöntemlerini tanımlar.
// Her satış, bu enum'dan bir değer ile ödeme türünü belirtir.
//
// ÖDEME YÖNTEMLERİ VE ÖZELLİKLERİ:
// 1. Nakit: Anında ödeme, işlem maliyeti yok
// 2. Kredi Kartı: Taksitli ödeme imkanı, komisyon var
// 3. Banka Kartı: Anında çekim, düşük komisyon
// 4. Havale/EFT: Büyük tutarlarda tercih edilir
// 5. Çek: Vadeli ödeme, B2B işlemlerde yaygın
//
// POS (Point of Sale) ENTEGRASYONU:
// - Kart ödemelerinde POS cihazı ile iletişim
// - Provizyon numarası kaydı
// - İptal/iade işlemleri
//
// MUHASEBE ETKİSİ:
// - Nakit: Kasa hesabına giriş
// - Kart: Banka hesabına (komisyon düşülerek)
// - Çek: Alacak hesabına (tahsil edilene kadar)
// ============================================================================

namespace Domain.Enums
{
    /// <summary>
    /// Ödeme Türü Enum'u
    ///
    /// SATIŞ İŞLEMİ İLE İLİŞKİ:
    /// - Sale entity'sindeki PaymentType alanında kullanılır
    /// - Her satış bir ödeme türü ile tamamlanır
    ///
    /// KULLANIM ÖRNEĞİ:
    /// <code>
    /// var sale = new Sale
    /// {
    ///     SaleNumber = "S-2024-00001",
    ///     PaymentType = PaymentType.KrediKarti,
    ///     TotalAmount = 1500m
    /// };
    /// </code>
    ///
    /// RAPORLAMA:
    /// <code>
    /// // Ödeme türüne göre satış dağılımı
    /// var paymentStats = sales
    ///     .GroupBy(s => s.PaymentType)
    ///     .Select(g => new
    ///     {
    ///         PaymentType = g.Key,
    ///         Count = g.Count(),
    ///         Total = g.Sum(s => s.TotalAmount)
    ///     });
    /// </code>
    /// </summary>
    public enum PaymentType
    {
        // ====================================================================
        // NAKİT ÖDEME
        // ====================================================================

        /// <summary>
        /// Nakit Ödeme
        ///
        /// AÇIKLAMA:
        /// - Fiziksel para ile ödeme
        /// - En hızlı işlem türü
        /// - İşlem maliyeti yok
        ///
        /// AVANTAJLARI:
        /// - Komisyon yok
        /// - Anında tahsilat
        /// - POS gerekmiyor
        ///
        /// DEZAVANTAJLARI:
        /// - Para sayma/bozuk para sorunu
        /// - Sahte para riski
        /// - Kasa sayım yükü
        ///
        /// KASA İŞLEMLERİ:
        /// <code>
        /// // Nakit satış = Kasa girişi
        /// if (sale.PaymentType == PaymentType.Nakit)
        /// {
        ///     cashRegister.Balance += sale.TotalAmount;
        /// }
        /// </code>
        /// </summary>
        Nakit = 1,

        // ====================================================================
        // KART İLE ÖDEME
        // ====================================================================

        /// <summary>
        /// Kredi Kartı ile Ödeme
        ///
        /// AÇIKLAMA:
        /// - Banka kredi kartı ile ödeme
        /// - Taksit imkanı var
        /// - POS cihazı gerektirir
        ///
        /// TAKSİT SEÇENEKLERİ:
        /// - Tek çekim
        /// - 3, 6, 9, 12 taksit
        /// - Kampanyalı taksitler
        ///
        /// KOMİSYON ORANLARI (Örnek):
        /// - Tek çekim: %1.5
        /// - 3 taksit: %2.0
        /// - 6 taksit: %2.5
        /// - 12 taksit: %3.5
        ///
        /// POS İŞLEMİ:
        /// <code>
        /// // Kredi kartı provizyon süreci
        /// var posResult = await _posService.ProcessPayment(
        ///     amount: sale.TotalAmount,
        ///     installments: 6  // 6 taksit
        /// );
        ///
        /// if (posResult.Approved)
        /// {
        ///     sale.Status = SaleStatus.Tamamlandi;
        ///     sale.ProvisionNumber = posResult.ProvisionCode;
        /// }
        /// </code>
        ///
        /// MUHASEBE:
        /// - Komisyon gider olarak kaydedilir
        /// - Net tutar banka hesabına geçer
        /// </summary>
        KrediKarti = 2,

        /// <summary>
        /// Banka Kartı (Debit Card) ile Ödeme
        ///
        /// AÇIKLAMA:
        /// - Müşterinin banka hesabından anında çekim
        /// - Taksit imkanı YOK
        /// - POS cihazı gerektirir
        ///
        /// KREDİ KARTI İLE FARKI:
        /// - Kredi Kartı: Borç verilir, ay sonunda/taksitle ödenir
        /// - Banka Kartı: Hesapta para varsa anında çekilir
        ///
        /// AVANTAJLARI:
        /// - Daha düşük komisyon (%0.5 - %1)
        /// - Anında tahsilat
        /// - Müşteri borçlanmaz
        ///
        /// DEZAVANTAJLARI:
        /// - Taksit seçeneği yok
        /// - Müşteri hesabında para olmalı
        ///
        /// ÖRNEK:
        /// <code>
        /// // Banka kartı ödemesi
        /// if (sale.PaymentType == PaymentType.BankaKarti)
        /// {
        ///     // Taksit seçeneği sunma
        ///     installmentOptions = new List&lt;int&gt;(); // Boş
        /// }
        /// </code>
        /// </summary>
        BankaKarti = 3,

        // ====================================================================
        // BANKA TRANSFERİ
        // ====================================================================

        /// <summary>
        /// Havale/EFT ile Ödeme
        ///
        /// AÇIKLAMA:
        /// - Banka hesabından doğrudan transfer
        /// - EFT: Elektronik Fon Transferi (anlık, 7/24)
        /// - Havale: Aynı banka içi transfer
        ///
        /// KULLANIM SENARYOLARI:
        /// - Yüksek tutarlı alımlar
        /// - Kurumsal satışlar (B2B)
        /// - Online sipariş ödemeleri
        /// - Kapıda ödeme istemeyenler
        ///
        /// İŞ SÜRECİ:
        /// 1. Satış kaydı oluşturulur (Beklemede)
        /// 2. Müşteriye IBAN ve açıklama verilir
        /// 3. Ödeme bankaya düşer
        /// 4. Muhasebe onaylar
        /// 5. Satış tamamlanır
        ///
        /// KOMİSYON:
        /// - EFT: Sabit ücret (örn: 5-10 TL)
        /// - Havale: Genellikle ücretsiz
        ///
        /// ÖRNEK MESAJ:
        /// <code>
        /// // Havale bilgisi mesajı
        /// var message = $@"
        ///     IBAN: TR12 0001 0012 3456 7890 1234 56
        ///     Açıklama: {sale.SaleNumber}
        ///     Tutar: {sale.TotalAmount:C}
        ///     Ödemenizi yaptıktan sonra siparişiniz kargoya verilecektir.";
        /// </code>
        /// </summary>
        Havale = 4,

        // ====================================================================
        // VADELİ ÖDEME
        // ====================================================================

        /// <summary>
        /// Çek ile Ödeme
        ///
        /// AÇIKLAMA:
        /// - Vadeli ödeme aracı
        /// - Genellikle B2B (kurumsal) işlemlerde
        /// - Belli bir tarihte bankadan tahsil edilir
        ///
        /// ÇEK TÜRLERİ:
        /// - Şirket Çeki: Müşterinin şirketinin çeki
        /// - Müşteri Çeki: Üçüncü taraf çeki
        ///
        /// RİSKLER:
        /// - Karşılıksız çek riski
        /// - Vade tarihine kadar bekleme
        /// - Tahsilat masrafları
        ///
        /// VADE TAKİBİ:
        /// <code>
        /// // Çekli satışlar için ödeme durumu
        /// public class Check
        /// {
        ///     public string CheckNumber { get; set; }
        ///     public decimal Amount { get; set; }
        ///     public DateTime DueDate { get; set; }  // Vade tarihi
        ///     public string BankName { get; set; }
        ///     public bool IsCollected { get; set; }  // Tahsil edildi mi?
        /// }
        /// </code>
        ///
        /// MUHASEBE:
        /// - Çek alındığında: Alacak Senetleri hesabına
        /// - Tahsil edildiğinde: Banka hesabına
        ///
        /// NOT:
        /// - TEKNOROMA perakende odaklı olduğu için
        /// - Çek genellikle kurumsal alımlarda kullanılır
        /// - Bireysel müşterilerde nadir
        /// </summary>
        Cek = 5
    }
}

// ============================================================================
// EK BİLGİLER VE BEST PRACTICES
// ============================================================================
//
// KARMA ÖDEME (SPLIT PAYMENT):
// Şu anki tasarımda bir satış tek ödeme türü ile yapılır.
// Karma ödeme için (örn: 500 TL nakit + 500 TL kart):
// <code>
// // Alternatif tasarım: SalePayment tablosu
// public class SalePayment
// {
//     public int SaleId { get; set; }
//     public PaymentType PaymentType { get; set; }
//     public decimal Amount { get; set; }
// }
// </code>
//
// YENİ ÖDEME YÖNTEMLERİ EKLENEBİLİR:
// <code>
// public enum PaymentType
// {
//     Nakit = 1,
//     KrediKarti = 2,
//     BankaKarti = 3,
//     Havale = 4,
//     Cek = 5,
//     // Yeni eklemeler:
//     Kripto = 6,      // Bitcoin, Ethereum
//     Papara = 7,      // Dijital cüzdan
//     Troy = 8,        // Yerli kart
//     Iyzico = 9,      // Online ödeme
//     Puan = 10        // Sadakat puanı ile ödeme
// }
// </code>
//
// POS ENTEGRASYONU NOTLARI:
// - Sanal POS: Online ödemeler için
// - Fiziksel POS: Mağaza içi kart ödemeleri
// - Yaygın sağlayıcılar: iyzico, PayTR, Param, Stripe
// ============================================================================
