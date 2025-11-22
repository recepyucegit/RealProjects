namespace Domain.Entities
{
    /// <summary>
    /// Tüm Entity'lerin temel sınıfı
    /// Ortak alanları içerir: Id, CreatedDate, ModifiedDate, IsDeleted
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Primary Key - Otomatik artan
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kayıt oluşturulma tarihi
        /// SaveChanges'de otomatik set edilir
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Son güncelleme tarihi
        /// SaveChanges'de otomatik set edilir
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Soft Delete için
        /// true: Kayıt silinmiş (görünmez)
        /// false: Kayıt aktif
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
