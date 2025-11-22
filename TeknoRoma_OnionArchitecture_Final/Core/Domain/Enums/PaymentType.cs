namespace Domain.Enums
{
    /// <summary>
    /// Ödeme Türü
    /// Satışlarda kullanılan ödeme yöntemleri
    /// </summary>
    public enum PaymentType
    {
        Nakit = 1,
        KrediKarti = 2,
        BankaKarti = 3,
        Havale = 4,
        Cek = 5
    }
}
