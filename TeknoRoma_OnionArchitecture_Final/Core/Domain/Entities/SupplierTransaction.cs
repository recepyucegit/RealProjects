// ============================================================================
// SupplierTransaction.cs - Tedarikçi İşlem/Hareket Entity
// ============================================================================
// AÇIKLAMA:
// TEKNOROMA'nın tedarikçilerden yaptığı alımları ve ödemeleri takip eden entity.
// Stok girişleri, borç takibi ve tedarikçi cari hesap yönetimi için kullanılır.
//
// İŞLEM TÜRLERİ:
// - Ürün Alımı: Tedarikçiden ürün satın alma
// - Ödeme: Tedarikçiye yapılan ödeme
// - İade: Tedarikçiye ürün iadesi (negatif işlem)
//
// İŞ KURALLARI:
// - Her işleme benzersiz numara verilir
// - Alım yapıldığında stok otomatik güncellenir
// - Vadeli ödemeler için IsPaid takibi
// - Tedarikçi bazlı borç/alacak raporlaması
// ============================================================================

namespace Domain.Entities
{
    /// <summary>
    /// Tedarikçi İşlem Entity Sınıfı
    ///
    /// CARİ HESAP YÖNETİMİ:
    /// - Borç: Alım yapıldı, ödeme yapılmadı (IsPaid = false)
    /// - Alacak: Ödeme yapıldı veya iade alındı
    /// - Bakiye: Toplam Borç - Toplam Ödeme
    ///
    /// STOK ENTEGRASYONU:
    /// - Alım kaydı = Stok girişi
    /// - Product.UnitsInStock += Quantity
    /// </summary>
    public class SupplierTransaction : BaseEntity
    {
        // ====================================================================
        // İŞLEM TANIMLAYICI
        // ====================================================================

        /// <summary>
        /// İşlem Numarası (Benzersiz)
        ///
        /// AÇIKLAMA:
        /// - Her tedarikçi işlemine verilen benzersiz referans
        /// - Muhasebe kayıtları ve takip için
        ///
        /// FORMAT: "TH-YYYY-NNNNN"
        /// - TH: Tedarikçi Hareketi prefix'i
        /// - YYYY: Yıl (2024)
        /// - NNNNN: Sıra numarası
        /// - Örn: "TH-2024-00001"
        ///
        /// NOT:
        /// - TH = Turkish için "Tedarikçi Hareketi"
        /// - İngilizce: ST (Supplier Transaction) de olabilir
        /// </summary>
        public string TransactionNumber { get; set; } = null!;

        /// <summary>
        /// İşlem Tarihi
        ///
        /// AÇIKLAMA:
        /// - Alımın/işlemin yapıldığı tarih
        /// - Genellikle tedarikçi fatura tarihi
        /// - Ödeme tarihi farklı olabilir (vadeli alım)
        ///
        /// STOK HAREKETİ:
        /// - Stok girişi bu tarihte kaydedilir
        /// - FIFO/LIFO hesabında kullanılır
        /// </summary>
        public DateTime TransactionDate { get; set; }

        // ====================================================================
        // İLİŞKİSEL ALANLAR (FOREIGN KEYS)
        // ====================================================================

        /// <summary>
        /// Tedarikçi ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Alımın yapıldığı tedarikçi
        /// - Suppliers tablosundaki Id ile eşleşir
        ///
        /// CARİ HESAP:
        /// - Tedarikçi bazlı borç/alacak takibi
        /// - supplier.SupplierTransactions ile erişim
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Ürün ID (Foreign Key)
        ///
        /// AÇIKLAMA:
        /// - Alınan ürün
        /// - Products tablosundaki Id ile eşleşir
        ///
        /// STOK GÜNCELLEME:
        /// - Bu ürünün stoğu artırılır
        /// - Product.UnitsInStock += Quantity
        ///
        /// ALTERNATİF TASARIM:
        /// - Tek işlemde birden fazla ürün için
        /// - SupplierTransactionDetail tablosu eklenebilir
        /// - Şu anki tasarım: Tek işlem = Tek ürün
        /// </summary>
        public int ProductId { get; set; }

        // ====================================================================
        // MİKTAR VE TUTAR
        // ====================================================================

        /// <summary>
        /// Alınan Miktar (Adet)
        ///
        /// AÇIKLAMA:
        /// - Tedarikçiden alınan ürün adedi
        /// - Pozitif değer: Alım
        /// - Negatif değer: İade (opsiyonel tasarım)
        ///
        /// STOK ETKİSİ:
        /// - Alım onaylandığında stok artar
        /// - Product.UnitsInStock += Quantity
        ///
        /// MINIMUM SİPARİŞ:
        /// - Tedarikçilerin minimum sipariş adetleri olabilir
        /// - Örn: "En az 10 adet sipariş"
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Birim Alış Fiyatı
        ///
        /// AÇIKLAMA:
        /// - Tedarikçiden alınan birim fiyat
        /// - Maliyet hesabı için kullanılır
        /// - Satış fiyatından farklı (Product.UnitPrice)
        ///
        /// KAR MARJI:
        /// - Kar = Satış Fiyatı - Alış Fiyatı
        /// - Margin % = (Kar / Satış Fiyatı) × 100
        ///
        /// ÖRNEK:
        /// - UnitPrice (alış): 40.000 TL
        /// - Product.UnitPrice (satış): 50.000 TL
        /// - Kar: 10.000 TL (%20 margin)
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Toplam Tutar
        ///
        /// AÇIKLAMA:
        /// - Quantity × UnitPrice
        /// - Tedarikçiye borçlanılan tutar
        ///
        /// HESAPLAMA:
        /// TotalAmount = Quantity × UnitPrice
        /// Örn: 10 adet × 40.000 TL = 400.000 TL
        ///
        /// NOT:
        /// - KDV dahil/hariç durumu netleştirilmeli
        /// - Şu anki tasarım: KDV dahil varsayılıyor
        /// </summary>
        public decimal TotalAmount { get; set; }

        // ====================================================================
        // BELGE BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Fatura Numarası (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Tedarikçinin kestiği fatura numarası
        /// - Muhasebe eşleşmesi için
        /// - KDV indirimi için gerekli
        ///
        /// ÖRNEKLER:
        /// - "TEK2024000123" (tedarikçi fatura no)
        /// - "GIB2024XXX" (e-fatura no)
        ///
        /// MUHASEBE:
        /// - Gider faturaları kayıt altına alınmalı
        /// - KDV mahsuplaşması için fatura zorunlu
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Açıklama/Not (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - İşlem hakkında ek bilgiler
        /// - Sipariş detayları, özel durumlar
        ///
        /// ÖRNEKLER:
        /// - "Kampanyalı alım - %10 indirimli"
        /// - "Acil sipariş - ekspres kargo"
        /// - "Numune ürün - ücretsiz"
        /// </summary>
        public string? Notes { get; set; }

        // ====================================================================
        // ÖDEME DURUMU
        // ====================================================================

        /// <summary>
        /// Ödeme Yapıldı mı?
        ///
        /// AÇIKLAMA:
        /// - true: Tedarikçiye ödeme yapıldı
        /// - false: Henüz ödenmedi (borç)
        ///
        /// VARSAYILAN:
        /// - "= false" ile borç olarak başlar
        ///
        /// VADELİ ALIMLAR:
        /// - Çoğu B2B alım vadeli (30-60-90 gün)
        /// - Vade tarihi için ayrı alan eklenebilir
        ///
        /// BORÇ TAKİBİ:
        /// var totalDebt = transactions
        ///     .Where(t => !t.IsPaid)
        ///     .Sum(t => t.TotalAmount);
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Ödeme Tarihi (Opsiyonel)
        ///
        /// AÇIKLAMA:
        /// - Tedarikçiye ödemenin yapıldığı tarih
        /// - IsPaid = false ise null
        ///
        /// NULLABLE (DateTime?):
        /// - Ödeme yapılmadıysa null
        /// - Ödeme yapıldığında set edilir
        ///
        /// KULLANIM:
        /// // Ödeme süresi hesaplama
        /// if (IsPaid && PaymentDate.HasValue)
        /// {
        ///     var paymentDays = (PaymentDate.Value - TransactionDate).Days;
        ///     // Ortalama ödeme süresi analizi
        /// }
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// Tedarikçi (Navigation Property)
        ///
        /// İLİŞKİ: SupplierTransaction (N) → Supplier (1)
        /// - Birden fazla işlem aynı tedarikçiye ait olabilir
        ///
        /// KULLANIM:
        /// var supplierName = transaction.Supplier.CompanyName;
        /// var supplierPhone = transaction.Supplier.Phone;
        /// </summary>
        public virtual Supplier Supplier { get; set; } = null!;

        /// <summary>
        /// Alınan Ürün (Navigation Property)
        ///
        /// İLİŞKİ: SupplierTransaction (N) → Product (1)
        /// - Aynı ürün birden fazla kez alınabilir
        ///
        /// KULLANIM:
        /// var productName = transaction.Product.Name;
        /// var currentStock = transaction.Product.UnitsInStock;
        ///
        /// MALİYET ANALİZİ:
        /// // Bu ürünün ortalama alış fiyatı
        /// var avgCost = product.SupplierTransactions
        ///     .Average(t => t.UnitPrice);
        /// </summary>
        public virtual Product Product { get; set; } = null!;
    }
}
