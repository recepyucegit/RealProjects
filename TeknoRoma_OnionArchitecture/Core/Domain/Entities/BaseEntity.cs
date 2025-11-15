namespace Domain.Entities
{
    /// <summary>
    /// Tüm entity'lerin miras alacağı temel sınıf
    /// ABSTRACT olmasının nedeni: BaseEntity'den direkt nesne oluşturulmasını engellemek
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Constructor - Her entity oluşturulduğunda CreatedDate otomatik atanır
        /// NEDEN? Manuel her seferinde yazmak yerine otomatikleştirdik
        /// </summary>
        protected BaseEntity()
        {
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Primary Key - Her tablonun benzersiz kimliği
        /// Database'de IDENTITY(1,1) olarak ayarlanacak
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Kaydın oluşturulma tarihi
        /// Constructor'da otomatik atanıyor
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Kaydın son güncellenme tarihi
        /// Repository'de Update metodunda otomatik set edilecek
        /// Null olabilir çünkü yeni kayıtlarda henüz güncelleme olmamıştır
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Soft Delete için kullanılacak
        /// NEDEN SOFT DELETE? 
        /// - Veriyi fiziksel olarak silmek yerine "silinmiş" olarak işaretleriz
        /// - Raporlarda geçmiş verilere erişim sağlar
        /// - Yanlışlıkla silinen kayıtları geri getirilebilir
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}