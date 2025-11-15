using Domain.Enums;
using static System.Formats.Asn1.AsnWriter;

namespace Domain.Entities
{
    /// <summary>
    /// Gider Entity
    /// Feyza Paragöz'ün (Muhasebe Temsilcisi) yönettiği giderler
    /// 
    /// Haluk Bey'in istediği Gider Raporu:
    /// - Çalışan ödemeleri
    /// - Teknik altyapı giderleri
    /// - Faturalar
    /// - Diğer giderler
    /// </summary>
    public class Expense : BaseEntity
    {
        /// <summary>
        /// Gider Numarası (Benzersiz)
        /// Format: "G-2024-00001"
        /// </summary>
        public string ExpenseNumber { get; set; }

        /// <summary>
        /// Gider Tarihi
        /// </summary>
        public DateTime ExpenseDate { get; set; }

        /// <summary>
        /// Gider Türü
        /// CalisanOdemesi, TeknikaltyapiGideri, Fatura, DigerGider
        /// </summary>
        public ExpenseType ExpenseType { get; set; }

        /// <summary>
        /// Hangi mağaza? (Foreign Key)
        /// Her mağaza kendi giderlerini takip eder
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Hangi çalışana ödeme? (Foreign Key)
        /// Sadece ExpenseType = CalisanOdemesi ise dolu
        /// Maaş ödemelerinde ilgili çalışan
        /// Null olabilir (diğer gider türlerinde)
        /// </summary>
        public int? EmployeeId { get; set; }

        /// <summary>
        /// Gider Tutarı
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Para Birimi
        /// TRY, USD, EUR
        /// Feyza Paragöz: "O tarihteki döviz kurunu da görmek istiyorum"
        /// </summary>
        public Currency Currency { get; set; } = Currency.TRY;

        /// <summary>
        /// Döviz Kuru (sadece USD ve EUR için)
        /// O tarihteki kur
        /// Null ise TRY (varsayılan kur = 1)
        /// </summary>
        public decimal? ExchangeRate { get; set; }

        /// <summary>
        /// TL karşılığı
        /// Hesaplama: Amount * ExchangeRate
        /// Currency = TRY ise Amount ile aynı
        /// </summary>
        public decimal AmountInTRY { get; set; }

        /// <summary>
        /// Gider Açıklaması
        /// "Ocak 2024 Maaşı - Recep Öztürk"
        /// "Elektrik Faturası - Aralık 2023"
        /// "Sunucu Bakım Ücreti"
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Fatura/Evrak Numarası
        /// Opsiyonel
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Ödeme yapıldı mı?
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Ödeme Tarihi
        /// Null olabilir (henüz ödeme yapılmadıysa)
        /// </summary>
        public DateTime? PaymentDate { get; set; }


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Giderin ait olduğu mağaza
        /// Many-to-One ilişki
        /// </summary>
        public virtual Store Store { get; set; }

        /// <summary>
        /// Giderin ait olduğu çalışan (maaş ödemelerinde)
        /// Many-to-One ilişki
        /// Null olabilir
        /// </summary>
        public virtual Employee Employee { get; set; }
    }
}