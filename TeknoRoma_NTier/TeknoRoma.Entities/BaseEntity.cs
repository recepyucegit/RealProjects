namespace TeknoRoma.Entities;

/// <summary>
/// Tüm entity sınıflarının miras aldığı temel sınıf
/// Ortak özellikleri (Id, CreatedDate, UpdatedDate, IsDeleted) içerir
/// Abstract olduğu için direkt örneklenemez, sadece kalıtım için kullanılır
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary Key - Her entity için benzersiz kimlik
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kaydın oluşturulma tarihi - Entity ilk kez veritabanına kaydedildiğinde otomatik atanır
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Kaydın güncellenme tarihi - Entity her güncellendiğinde bu alan güncellenir
    /// Nullable olduğu için ilk başta null'dır
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Soft Delete için kullanılır - True ise kayıt silinmiş sayılır ama fiziksel olarak veritabanında kalır
    /// Bu sayede veri kaybı önlenir ve gerektiğinde geri getirilebilir
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
