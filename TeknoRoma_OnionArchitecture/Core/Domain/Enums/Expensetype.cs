namespace Domain.Enums
{
    /// <summary>
    /// Gider Türü
    /// Muhasebe temsilcisinin takip ettiği gider kategorileri
    /// Haluk Bey'in (Şube Müdürü) istediği rapor kategorileri
    /// </summary>
    public enum ExpenseType
    {
        /// <summary>
        /// Çalışan maaşları ve primleri
        /// </summary>
        CalisanOdemesi = 1,

        /// <summary>
        /// Sunucu, network, donanım giderleri
        /// </summary>
        TeknikaltyapiGideri = 2,

        /// <summary>
        /// Elektrik, su, internet faturaları
        /// </summary>
        Fatura = 3,

        /// <summary>
        /// Diğer giderler (kırtasiye, temizlik vb.)
        /// </summary>
        DigerGider = 4
    }
}