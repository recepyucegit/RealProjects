namespace Domain.Enums
{
    /// <summary>
    /// Ödeme Türü
    /// Müşteri satış yaparken hangi yöntemle ödeme yaptı
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// Nakit ödeme
        /// </summary>
        Nakit = 1,

        /// <summary>
        /// Kredi kartı ile ödeme
        /// </summary>
        KrediKarti = 2,

        /// <summary>
        /// Banka havalesi
        /// </summary>
        Havale = 3,

        /// <summary>
        /// Çek ile ödeme
        /// </summary>
        Cek = 4
    }
}