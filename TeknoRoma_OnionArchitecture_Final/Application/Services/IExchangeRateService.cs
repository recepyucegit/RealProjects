namespace Application.Services
{
    /// <summary>
    /// Döviz kuru servisi - TCMB entegrasyonu
    /// </summary>
    public interface IExchangeRateService
    {
        Task<decimal> GetUsdRateAsync();
        Task<decimal> GetEurRateAsync();
        Task<decimal> ConvertToTryAsync(decimal amount, string currency);
    }
}
