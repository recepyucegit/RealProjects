// ============================================================================
// IExpenseService.cs - Gider Servis Interface
// ============================================================================
// AÇIKLAMA:
// Gider yönetimi için iş mantığı katmanı.
// Maliyet analizi, ödeme takibi ve finansal raporlama.
//
// GİDER KATEGORİLERİ (ExpenseType):
// - Kira: Mağaza kirası
// - Fatura: Elektrik, su, internet
// - Maas: Personel maaşları
// - Malzeme: Ofis malzemeleri
// - Teknik: Bakım onarım
// - Diger: Sınıflandırılamayan
//
// FİNANSAL KONTROL:
// Gelir (Satış) - Gider = Kar/Zarar
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    /// <summary>
    /// Gider Servis Interface
    ///
    /// MALİYET KONTROLÜ: Tüm giderlerin takibi
    /// NAKİT AKIŞI: Ödenmemiş giderlerin planlanması
    /// BÜTÇE: Gider analizi ve bütçe karşılaştırma
    /// </summary>
    public interface IExpenseService
    {
        // ========================================================================
        // SORGULAMA (QUERY) METODLARI
        // ========================================================================

        /// <summary>
        /// ID ile Gider Getir
        ///
        /// DETAY SAYFASI: Gider detayı görüntüleme
        /// İLİŞKİLİ VERİ: Store, Employee (maaş ise) include
        /// </summary>
        Task<Expense?> GetByIdAsync(int id);

        /// <summary>
        /// Gider Numarası ile Getir
        ///
        /// FATURA TAKİBİ: "G-2024-00123 hangi giderdi?"
        /// FORMAT: "G-YYYY-NNNNN" (Gider numarası)
        /// </summary>
        Task<Expense?> GetByExpenseNumberAsync(string expenseNumber);

        /// <summary>
        /// Tüm Giderler
        ///
        /// MUHASEBE: Tüm gider kayıtları
        /// DİKKAT: Sayfalama gerekebilir
        /// </summary>
        Task<IEnumerable<Expense>> GetAllAsync();

        /// <summary>
        /// Mağaza Giderleri
        ///
        /// ŞUBE MALİYETİ: "Ankara-1'in giderleri ne kadar?"
        /// KAR/ZARAR: Şube bazlı kar hesabı için
        /// </summary>
        Task<IEnumerable<Expense>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Türe Göre Giderler
        ///
        /// KATEGORİ ANALİZİ:
        /// - GetByTypeAsync(ExpenseType.Kira) -> Tüm kira giderleri
        /// - GetByTypeAsync(ExpenseType.Fatura) -> Fatura giderleri
        ///
        /// BÜTÇE PLANLAMA: Kategori bazlı bütçe takibi
        /// </summary>
        Task<IEnumerable<Expense>> GetByTypeAsync(ExpenseType expenseType);

        /// <summary>
        /// Tarih Aralığına Göre Giderler
        ///
        /// DÖNEMSEL RAPOR: "Ocak ayı giderleri"
        /// VERGİ HESABI: Yıllık gider toplamı
        /// </summary>
        Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ödenmemiş Giderler
        ///
        /// NAKİT AKIŞI KRİTİK!
        /// BORÇ LİSTESİ: IsPaid = false olan tüm giderler
        ///
        /// PLANLAMA:
        /// - Bu hafta vadesi dolanlar
        /// - Gecikmiş ödemeler (alarm)
        /// </summary>
        Task<IEnumerable<Expense>> GetUnpaidExpensesAsync();

        // ========================================================================
        // KOMUT (COMMAND) METODLARI
        // ========================================================================

        /// <summary>
        /// Yeni Gider Oluştur
        ///
        /// GİDER KAYDI:
        /// 1. Expense kaydı oluştur
        /// 2. Gider numarası otomatik ata (G-2024-XXXXX)
        /// 3. ExpenseDate = işlem tarihi
        /// 4. IsPaid = false (varsayılan)
        ///
        /// VALİDASYON:
        /// - Tutar > 0
        /// - ExpenseType geçerli olmalı
        /// - StoreId geçerli olmalı
        /// </summary>
        Task<Expense> CreateAsync(Expense expense);

        /// <summary>
        /// Gider Güncelle
        ///
        /// DÜZELTME SENARYOLARI:
        /// - Tutar düzeltme
        /// - Açıklama ekleme
        /// - Kategori değiştirme
        /// </summary>
        Task UpdateAsync(Expense expense);

        /// <summary>
        /// Gider Sil
        ///
        /// SOFT DELETE: IsDeleted = true
        ///
        /// MUHASEBE KURALI:
        /// Fiili gider silinmez, iptal kaydı tutulur
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Ödendi Olarak İşaretle
        ///
        /// ÖDEME İŞLEMİ:
        /// 1. IsPaid = true
        /// 2. PaymentDate = Now (veya belirtilen tarih)
        /// 3. Kasa çıkışı kaydı (nakit akış raporları için)
        ///
        /// ÖRNEK:
        /// // Elektrik faturası ödendi
        /// await MarkAsPaidAsync(expenseId: 123);
        /// </summary>
        Task MarkAsPaidAsync(int expenseId);

        // ========================================================================
        // RAPORLAMA METODLARI
        // ========================================================================

        /// <summary>
        /// Aylık Toplam Gider
        ///
        /// KAR/ZARAR HESABI:
        /// Kar = Aylık Satış - Aylık Gider
        ///
        /// BÜTÇE KARŞILAŞTIRMA:
        /// Planlanan gider vs gerçekleşen gider
        ///
        /// ÖRNEK:
        /// var tumGider = await GetMonthlyTotalAsync(2024, 1);
        /// var ankara1Gider = await GetMonthlyTotalAsync(2024, 1, storeId: 1);
        /// </summary>
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);
    }
}
