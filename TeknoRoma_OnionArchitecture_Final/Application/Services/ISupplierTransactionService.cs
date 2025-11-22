// ============================================================================
// ISupplierTransactionService.cs - Tedarikçi İşlem Servis Interface
// ============================================================================
// AÇIKLAMA:
// Tedarikçi alım ve ödeme işlemlerini yönetmek için iş mantığı katmanı.
// Cari hesap takibi, stok giriş kayıtları ve borç yönetimi.
//
// CARİ HESAP SİSTEMİ:
// Tedarikçiden mal alındığında:
// - SupplierTransaction kaydı oluşur (Borç)
// - Ödeme yapıldığında IsPaid = true
//
// STOK ENTEGRASYonu:
// Alım işlemi = Stok girişi
// CreateAsync içinde Product.CurrentStock güncellenir
// ============================================================================

using Domain.Entities;

namespace Application.Services
{
    /// <summary>
    /// Tedarikçi İşlem Servis Interface
    ///
    /// CARİ HESAP: Tedarikçi borç/alacak takibi
    /// NAKİT AKIŞI: Ödenmemiş faturaların planlanması
    /// MALİYET ANALİZİ: Ürün maliyet hesaplama
    /// </summary>
    public interface ISupplierTransactionService
    {
        // ========================================================================
        // SORGULAMA (QUERY) METODLARI
        // ========================================================================

        /// <summary>
        /// ID ile İşlem Getir
        ///
        /// DETAY SAYFASI: İşlem detayı görüntüleme
        /// İLİŞKİLİ VERİ: Supplier, Product include edilir
        /// </summary>
        Task<SupplierTransaction?> GetByIdAsync(int id);

        /// <summary>
        /// İşlem Numarası ile Getir
        ///
        /// FATURA TAKİBİ: "TH-2024-00123 işlemi neydi?"
        /// FORMAT: "TH-YYYY-NNNNN" (Tedarikçi Hareketi)
        ///
        /// KULLANIM: Tedarikçi fatura numarası ile eşleştirme
        /// </summary>
        Task<SupplierTransaction?> GetByTransactionNumberAsync(string transactionNumber);

        /// <summary>
        /// Tüm İşlemler
        ///
        /// MUHASEBE: Tüm tedarikçi hareketleri
        /// FİLTRELEME: Dönem bazlı filtreleme için temel
        /// </summary>
        Task<IEnumerable<SupplierTransaction>> GetAllAsync();

        /// <summary>
        /// Tedarikçi İşlemleri
        ///
        /// CARİ EKSTRE: Bu tedarikçi ile tüm işlemler
        /// BORÇ HESAPLAMA: Toplam borç = Ödenmemiş işlemler toplamı
        ///
        /// ÖRNEK:
        /// var samsungIslemleri = await GetBySupplierAsync(1);
        /// var toplamBorc = samsungIslemleri.Where(x => !x.IsPaid).Sum(x => x.TotalAmount);
        /// </summary>
        Task<IEnumerable<SupplierTransaction>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// Ürün Alım Geçmişi
        ///
        /// MALİYET ANALİZİ: Bu ürün nereden, ne fiyata alındı?
        /// FİYAT KARŞILAŞTIRMA: En ucuz tedarikçiyi bulma
        ///
        /// ÖRNEK:
        /// var iphoneAlimlari = await GetByProductAsync(productId: 5);
        /// var ortalamaFiyat = iphoneAlimlari.Average(x => x.UnitPrice);
        /// </summary>
        Task<IEnumerable<SupplierTransaction>> GetByProductAsync(int productId);

        /// <summary>
        /// Tarih Aralığına Göre İşlemler
        ///
        /// DÖNEMSEL RAPOR: "Ocak ayında ne kadar mal aldık?"
        /// VERGİ HESABI: KDV beyanı için dönemsel toplam
        /// </summary>
        Task<IEnumerable<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ödenmemiş İşlemler
        ///
        /// KRİTİK: Nakit akış yönetimi için
        /// BORÇ LİSTESİ: IsPaid = false olan tüm işlemler
        ///
        /// PLANLAMA: Bu hafta ödenmesi gerekenler
        /// TEDARİKÇİ İLİŞKİSİ: Geciken ödemeler uyarısı
        /// </summary>
        Task<IEnumerable<SupplierTransaction>> GetUnpaidTransactionsAsync();

        // ========================================================================
        // KOMUT (COMMAND) METODLARI
        // ========================================================================

        /// <summary>
        /// Yeni Alım İşlemi Oluştur
        ///
        /// STOK GİRİŞİ - Transaction içinde:
        /// 1. SupplierTransaction kaydı oluştur
        /// 2. İşlem numarası otomatik ata (TH-2024-XXXXX)
        /// 3. Product.CurrentStock += Quantity
        /// 4. Tedarikçi cari bakiyesini güncelle
        ///
        /// VALİDASYON:
        /// - Miktar > 0
        /// - Birim fiyat >= 0
        /// - Geçerli tedarikçi ve ürün ID
        /// </summary>
        Task<SupplierTransaction> CreateAsync(SupplierTransaction transaction);

        /// <summary>
        /// İşlem Güncelle
        ///
        /// DÜZELTME SENARYOLARI:
        /// - Fiyat düzeltme (yanlış girilmiş)
        /// - Miktar düzeltme
        /// - Açıklama ekleme
        ///
        /// DİKKAT: Stok güncelleme gerekebilir (fark hesabı)
        /// </summary>
        Task UpdateAsync(SupplierTransaction transaction);

        /// <summary>
        /// İşlem Sil
        ///
        /// SOFT DELETE veya İPTAL:
        /// - IsDeleted = true
        /// - Stok geri düşürülmeli
        ///
        /// MUHASEBE: İptal kaydı tutulur, fiziksel silme yapılmaz
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Ödendi Olarak İşaretle
        ///
        /// ÖDEME İŞLEMİ:
        /// 1. IsPaid = true
        /// 2. PaymentDate = Now
        /// 3. Kasa çıkışı kaydı (opsiyonel)
        ///
        /// KURAL: Ödeme yapılınca geri alınamaz (iptal için yeni kayıt)
        /// </summary>
        Task MarkAsPaidAsync(int transactionId);

        // ========================================================================
        // RAPORLAMA METODLARI
        // ========================================================================

        /// <summary>
        /// Aylık Toplam Alım Tutarı
        ///
        /// MALİYET ANALİZİ: "Bu ay tedarikçilere ne kadar ödedik?"
        /// BÜTÇE KARŞILAŞTIRMA: Planlanan vs gerçekleşen
        /// VERGİ: KDV hesaplama için toplam
        ///
        /// ÖRNEK:
        /// var ocakAlim = await GetMonthlyTotalAsync(2024, 1);
        /// </summary>
        Task<decimal> GetMonthlyTotalAsync(int year, int month);
    }
}
