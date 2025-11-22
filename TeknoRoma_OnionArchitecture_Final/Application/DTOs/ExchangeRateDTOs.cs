using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

/// <summary>
/// Exchange rate information
/// </summary>
public class ExchangeRateDto
{
    public Currency Currency { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public decimal BuyingRate { get; set; }
    public decimal SellingRate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Currency conversion result
/// </summary>
public class CurrencyConversionDto
{
    public Currency FromCurrency { get; set; }
    public Currency ToCurrency { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime ConversionDate { get; set; }
}

/// <summary>
/// All exchange rates response
/// </summary>
public class ExchangeRatesResponseDto
{
    public DateTime Date { get; set; }
    public string Source { get; set; } = "TCMB";
    public List<ExchangeRateDto> Rates { get; set; } = new();
}
