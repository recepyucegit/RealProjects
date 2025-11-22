using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Supplier Transaction Repository Interface
    /// Tedarikçi hareketleri (alımları) için özel metodlar
    ///
    /// AMAÇ:
    /// - Tedarikçilerden yapılan ürün alımlarını takip eder
    /// - Haluk Bey'in tedarikçi raporlarını destekler
    /// - Muhasebe için ödeme takibi sağlar
    ///
    /// HALUK BEY'İN İSTEĞİ:
    /// "Hangi tedarikçiden bu ay hangi ürünleri ne kadar almışız,
    /// toplamda ne kadar alım yapmışız görmek istiyorum"
    ///
    /// KULLANIM ALANLARI:
    /// - Tedarikçi performans analizi
    /// - Ödeme takibi ve muhasebe raporları
    /// - Stok giriş takibi
    /// </summary>
    public interface ISupplierTransactionRepository : IRepository<SupplierTransaction>
    {
        /// <summary>
        /// İşlem numarasına göre kayıt bulur
        /// NEDEN?
        /// - Fatura takibi için işlem numarası ile sorgu
        /// - Unique field olduğu için özel metod
        /// </summary>
        /// <param name="transactionNumber">İşlem numarası (TH-2024-00001)</param>
        Task<SupplierTransaction> GetByTransactionNumberAsync(string transactionNumber);

        /// <summary>
        /// Tedarikçiye göre işlemleri getirir
        /// NEDEN?
        /// - Haluk Bey: "Bu tedarikçiden ne kadar alım yapmışız?"
        /// - Tedarikçi bazlı rapor için
        /// </summary>
        /// <param name="supplierId">Tedarikçi ID</param>
        Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// Ürüne göre işlemleri getirir
        /// NEDEN?
        /// - "Bu ürünü hangi tedarikçilerden almışız?"
        /// - Ürün tedarik geçmişi için
        /// </summary>
        /// <param name="productId">Ürün ID</param>
        Task<IReadOnlyList<SupplierTransaction>> GetByProductAsync(int productId);

        /// <summary>
        /// Tarih aralığındaki işlemleri getirir
        /// NEDEN?
        /// - Aylık/dönemsel tedarik raporları için
        /// - "Bu ay kaç TL'lik alım yaptık?"
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        Task<IReadOnlyList<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Tedarikçinin belirli tarih aralığındaki işlemlerini getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Bu tedarikçiden bu ay ne almışız?"
        /// - Dönemsel tedarikçi analizi
        /// </summary>
        /// <param name="supplierId">Tedarikçi ID</param>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAndDateRangeAsync(
            int supplierId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ödenmemiş işlemleri getirir
        /// NEDEN?
        /// - Muhasebe: "Ödememiz gereken tedarikçi faturaları"
        /// - Nakit akışı planlaması için
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetUnpaidAsync();

        /// <summary>
        /// Tedarikçinin ödenmemiş işlemlerini getirir
        /// NEDEN?
        /// - "Bu tedarikçiye ne kadar borcumuz var?"
        /// - Tedarikçi bazlı borç takibi
        /// </summary>
        /// <param name="supplierId">Tedarikçi ID</param>
        Task<IReadOnlyList<SupplierTransaction>> GetUnpaidBySupplierAsync(int supplierId);

        /// <summary>
        /// Fatura numarasına göre işlem bulur
        /// NEDEN?
        /// - Tedarikçi faturası ile eşleştirme için
        /// - Muhasebe doğrulaması için
        /// </summary>
        /// <param name="invoiceNumber">Fatura numarası</param>
        Task<SupplierTransaction> GetByInvoiceNumberAsync(string invoiceNumber);

        /// <summary>
        /// İşlemi ilişkili verilerle getirir (Eager Loading)
        /// NEDEN?
        /// - Detay sayfasında Supplier ve Product bilgilerini göstermek için
        /// - N+1 Query problemini önler
        /// </summary>
        /// <param name="transactionId">İşlem ID</param>
        Task<SupplierTransaction> GetWithDetailsAsync(int transactionId);

        /// <summary>
        /// Tedarikçinin toplam alım tutarını hesaplar
        /// NEDEN?
        /// - Haluk Bey: "Bu tedarikçiden toplamda ne kadar alım yapmışız?"
        /// - Tedarikçi değerlendirmesi için
        /// </summary>
        /// <param name="supplierId">Tedarikçi ID</param>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<decimal> GetTotalAmountBySupplierAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Toplam borç tutarını hesaplar (tüm tedarikçiler)
        /// NEDEN?
        /// - Muhasebe: "Toplam tedarikçi borcumuz ne kadar?"
        /// - Mali durum analizi
        /// </summary>
        Task<decimal> GetTotalUnpaidAmountAsync();

        /// <summary>
        /// Tedarikçiye olan borç tutarını hesaplar
        /// NEDEN?
        /// - "Bu tedarikçiye ne kadar borçluyuz?"
        /// - Ödeme planlaması için
        /// </summary>
        /// <param name="supplierId">Tedarikçi ID</param>
        Task<decimal> GetUnpaidAmountBySupplierAsync(int supplierId);

        /// <summary>
        /// En çok alım yapılan tedarikçileri getirir
        /// NEDEN?
        /// - Tedarikçi performans sıralaması
        /// - Stratejik tedarikçi ilişkileri için
        /// </summary>
        /// <param name="count">Kaç tedarikçi getirileceği</param>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<IReadOnlyList<(int SupplierId, string SupplierName, decimal TotalAmount, int TransactionCount)>> GetTopSuppliersAsync(
            int count, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Ürün bazlı alım istatistiklerini getirir
        /// NEDEN?
        /// - "Hangi ürünleri ne kadar almışız?"
        /// - Envanter planlaması için
        /// </summary>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<IReadOnlyList<(int ProductId, string ProductName, int TotalQuantity, decimal TotalCost)>> GetPurchasesByProductAsync(
            DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Yeni işlem numarası oluşturur
        /// Format: TH-2024-00001, TH-2024-00002
        /// </summary>
        Task<string> GenerateTransactionNumberAsync();

        /// <summary>
        /// Ödeme durumunu günceller
        /// NEDEN?
        /// - Muhasebe ödeme kaydı için
        /// - Toplu ödeme işlemleri için
        /// </summary>
        /// <param name="transactionId">İşlem ID</param>
        /// <param name="paymentDate">Ödeme tarihi</param>
        Task MarkAsPaidAsync(int transactionId, DateTime paymentDate);

        /// <summary>
        /// Birden fazla işlemi ödenmiş olarak işaretler
        /// NEDEN?
        /// - Toplu ödeme işlemleri için
        /// - "Bu tedarikçiye topluca ödeme yaptık"
        /// </summary>
        /// <param name="transactionIds">İşlem ID'leri</param>
        /// <param name="paymentDate">Ödeme tarihi</param>
        Task MarkRangeAsPaidAsync(IEnumerable<int> transactionIds, DateTime paymentDate);
    }
}
