namespace Domain.Enums
{
    /// <summary>
    /// Gider Türü
    /// Haluk Bey'in gider raporu kategorileri
    /// </summary>
    public enum ExpenseType
    {
        /// <summary>
        /// Çalışan maaş ödemeleri
        /// </summary>
        CalisanOdemesi = 1,

        /// <summary>
        /// Teknik altyapı giderleri (sunucu, yazılım, bakım)
        /// </summary>
        TeknikAltyapiGideri = 2,

        /// <summary>
        /// Faturalar (elektrik, su, doğalgaz, internet)
        /// </summary>
        Fatura = 3,

        /// <summary>
        /// Diğer giderler
        /// </summary>
        DigerGider = 4
    }
}
