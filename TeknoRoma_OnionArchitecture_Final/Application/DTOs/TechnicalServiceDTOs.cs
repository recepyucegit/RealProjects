using Domain.Enums;


namespace TeknoRoma.Application.DTOs;

public class TechnicalServiceDto
{
    public int Id { get; set; }
    public string DeviceType { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string ProblemDescription { get; set; } = null!;
    public string? DiagnosticNotes { get; set; }
    public string? RepairNotes { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? FinalCost { get; set; }
    public TechnicalServiceStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public DateTime ReceivedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public int StoreId { get; set; }
    public string? StoreName { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateTechnicalServiceDto
{
    public string DeviceType { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string ProblemDescription { get; set; } = null!;
    public decimal? EstimatedCost { get; set; }
    public int? CustomerId { get; set; }
    public int? TechnicianId { get; set; }
    public int StoreId { get; set; }

    // For new customers
    public string? CustomerFirstName { get; set; }
    public string? CustomerLastName { get; set; }
    public string? CustomerPhone { get; set; }
}

public class UpdateTechnicalServiceDto
{
    public int Id { get; set; }
    public string DeviceType { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string ProblemDescription { get; set; } = null!;
    public string? DiagnosticNotes { get; set; }
    public string? RepairNotes { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? FinalCost { get; set; }
    public TechnicalServiceStatus Status { get; set; }
    public int? TechnicianId { get; set; }
}

public class TechnicalServiceStatusUpdateDto
{
    public int Id { get; set; }
    public TechnicalServiceStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public decimal? FinalCost { get; set; }
}

public class TechnicalServiceSummaryDto
{
    public int TotalTickets { get; set; }
    public int PendingTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int DeliveredTickets { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRepairTime { get; set; }
}
