namespace TeknoRoma.Application.DTOs;

/// <summary>
/// Generic paginated result wrapper
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Generic API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}

/// <summary>
/// Simple result for operations without return data
/// </summary>
public class OperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static OperationResult SuccessResult(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static OperationResult ErrorResult(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}

/// <summary>
/// Pagination request parameters
/// </summary>
public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

/// <summary>
/// Search/filter parameters
/// </summary>
public class SearchRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Date range for reports
/// </summary>
public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Dropdown/select list item
/// </summary>
public class SelectItemDto
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsSelected { get; set; }
}

/// <summary>
/// Key-value pair for dashboard statistics
/// </summary>
public class StatisticItemDto
{
    public string Label { get; set; } = null!;
    public decimal Value { get; set; }
    public string? FormattedValue { get; set; }
    public decimal? ChangePercent { get; set; }
    public bool IsPositiveChange { get; set; }
}

/// <summary>
/// Dashboard summary
/// </summary>
public class DashboardSummaryDto
{
    public decimal TodaySales { get; set; }
    public decimal MonthlySales { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int PendingTechnicalServices { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public List<DailySalesSummaryDto> Last7DaysSales { get; set; } = new();
    public List<StatisticItemDto> TopSellingProducts { get; set; } = new();
}
