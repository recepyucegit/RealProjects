using AutoMapper;
using TeknoRoma.Business.DTOs;
using TeknoRoma.Business.Services.Abstract;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.Entities;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Concrete;

/// <summary>
/// TechnicalService Service Implementation - Teknik servis business logic
/// Özgün Kablocu'nun (Teknik Servis Temsilcisi) kullandığı servis
///
/// ÖNEMLİ ÖZELLİKLER:
/// - SLA (Service Level Agreement) takibi
/// - Öncelik bazlı iş yönetimi
/// - Çözüm süresi hesaplama
/// - Otomatik ServiceNumber oluşturma
/// </summary>
public class TechnicalServiceService : ITechnicalServiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    // SLA hedefleri (saat olarak)
    private static readonly Dictionary<int, int> SlaTargetHours = new()
    {
        { 1, 72 },  // Düşük öncelik: 72 saat
        { 2, 24 },  // Orta öncelik: 24 saat
        { 3, 12 },  // Yüksek öncelik: 12 saat
        { 4, 4 }    // Kritik öncelik: 4 saat
    };

    public TechnicalServiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    // ====== CRUD OPERATIONS ======

    public async Task<IEnumerable<TechnicalServiceSummaryDto>> GetAllTechnicalServicesAsync(bool includeDeleted = false)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var result = new List<TechnicalServiceSummaryDto>();

        foreach (var service in services)
        {
            result.Add(await MapToSummaryDto(service));
        }

        return result;
    }

    public async Task<TechnicalServiceDto?> GetTechnicalServiceByIdAsync(int id)
    {
        var service = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
        if (service == null) return null;

        return await MapToDetailDto(service);
    }

    public async Task<TechnicalServiceDto?> GetTechnicalServiceByNumberAsync(string serviceNumber)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var service = services.FirstOrDefault(s => s.ServiceNumber == serviceNumber);

        if (service == null) return null;

        return await MapToDetailDto(service);
    }

    public async Task<TechnicalServiceDto?> CreateTechnicalServiceAsync(CreateTechnicalServiceDto createDto)
    {
        // VALIDASYON: Mağaza mevcut mu?
        var store = await _unitOfWork.Stores.GetByIdAsync(createDto.StoreId);
        if (store == null)
            throw new InvalidOperationException("Mağaza bulunamadı.");

        // VALIDASYON: Bildiren çalışan mevcut mu?
        var reportedBy = await _unitOfWork.Employees.GetByIdAsync(createDto.ReportedByEmployeeId);
        if (reportedBy == null)
            throw new InvalidOperationException("Bildiren çalışan bulunamadı.");

        // VALIDASYON: Müşteri sorunu ise müşteri zorunlu
        if (createDto.IsCustomerIssue && !createDto.CustomerId.HasValue)
            throw new InvalidOperationException("Müşteri sorunu için müşteri seçimi zorunludur.");

        // VALIDASYON: Müşteri mevcut mu?
        if (createDto.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId.Value);
            if (customer == null)
                throw new InvalidOperationException("Müşteri bulunamadı.");
        }

        // ServiceNumber oluştur
        var serviceNumber = await GenerateServiceNumberAsync();

        var technicalService = new TechnicalService
        {
            ServiceNumber = serviceNumber,
            Title = createDto.Title,
            Description = createDto.Description,
            StoreId = createDto.StoreId,
            ReportedByEmployeeId = createDto.ReportedByEmployeeId,
            AssignedToEmployeeId = createDto.AssignedToEmployeeId,
            IsCustomerIssue = createDto.IsCustomerIssue,
            CustomerId = createDto.CustomerId,
            ProductId = createDto.ProductId,
            Status = TechnicalServiceStatus.Acik,
            Priority = createDto.Priority,
            ReportedDate = DateTime.Now
        };

        await _unitOfWork.TechnicalServices.AddAsync(technicalService);
        var saved = await _unitOfWork.SaveChangesAsync();

        if (!saved) return null;

        return await GetTechnicalServiceByIdAsync(technicalService.Id);
    }

    public async Task<TechnicalServiceDto?> UpdateTechnicalServiceAsync(UpdateTechnicalServiceDto updateDto)
    {
        var service = await _unitOfWork.TechnicalServices.GetByIdAsync(updateDto.Id);
        if (service == null) return null;

        // Status değişikliği kontrolü
        bool wasResolved = service.Status == TechnicalServiceStatus.Tamamlandi ||
                          service.Status == TechnicalServiceStatus.Cozulemedi;
        bool isNowResolved = updateDto.Status == TechnicalServiceStatus.Tamamlandi ||
                            updateDto.Status == TechnicalServiceStatus.Cozulemedi;

        // Çözüldü olarak işaretleniyorsa Resolution zorunlu
        if (isNowResolved && !wasResolved && string.IsNullOrEmpty(updateDto.Resolution))
            throw new InvalidOperationException("Çözüm açıklaması zorunludur.");

        // Güncellenebilir alanlar
        service.Title = updateDto.Title;
        service.Description = updateDto.Description;
        service.AssignedToEmployeeId = updateDto.AssignedToEmployeeId;
        service.Status = updateDto.Status;
        service.Priority = updateDto.Priority;
        service.Resolution = updateDto.Resolution;
        service.Cost = updateDto.Cost;

        // Çözüldüyse ResolvedDate set et
        if (isNowResolved && !wasResolved)
        {
            service.ResolvedDate = DateTime.Now;
        }

        _unitOfWork.TechnicalServices.Update(service);
        var saved = await _unitOfWork.SaveChangesAsync();

        if (!saved) return null;

        return await GetTechnicalServiceByIdAsync(service.Id);
    }

    public async Task<bool> DeleteTechnicalServiceAsync(int id)
    {
        var service = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
        if (service == null) return false;

        _unitOfWork.TechnicalServices.Delete(service);
        return await _unitOfWork.SaveChangesAsync();
    }


    // ====== BUSINESS LOGIC METHODS ======

    public async Task<IEnumerable<TechnicalServiceSummaryDto>> GetTechnicalServicesByStoreAsync(int storeId, TechnicalServiceStatus? status = null)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var storeServices = services.Where(s => s.StoreId == storeId);

        if (status.HasValue)
            storeServices = storeServices.Where(s => s.Status == status.Value);

        var result = new List<TechnicalServiceSummaryDto>();
        foreach (var service in storeServices)
        {
            result.Add(await MapToSummaryDto(service));
        }

        return result;
    }

    public async Task<IEnumerable<TechnicalServiceDto>> GetTechnicalServicesByCustomerAsync(int customerId)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var customerServices = services.Where(s => s.CustomerId == customerId);

        var result = new List<TechnicalServiceDto>();
        foreach (var service in customerServices)
        {
            result.Add(await MapToDetailDto(service));
        }

        return result;
    }

    public async Task<IEnumerable<TechnicalServiceDto>> GetTechnicalServicesByProductAsync(int productId)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var productServices = services.Where(s => s.ProductId == productId);

        var result = new List<TechnicalServiceDto>();
        foreach (var service in productServices)
        {
            result.Add(await MapToDetailDto(service));
        }

        return result;
    }

    public async Task<IEnumerable<TechnicalServiceSummaryDto>> GetTechnicalServicesAssignedToAsync(int employeeId, TechnicalServiceStatus? status = null)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var assigned = services.Where(s => s.AssignedToEmployeeId == employeeId);

        if (status.HasValue)
            assigned = assigned.Where(s => s.Status == status.Value);

        var result = new List<TechnicalServiceSummaryDto>();
        foreach (var service in assigned)
        {
            result.Add(await MapToSummaryDto(service));
        }

        return result;
    }

    public async Task<IEnumerable<TechnicalServiceSummaryDto>> GetOpenTechnicalServicesAsync(int? storeId = null)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var open = services.Where(s =>
            s.Status == TechnicalServiceStatus.Acik ||
            s.Status == TechnicalServiceStatus.Islemde);

        if (storeId.HasValue)
            open = open.Where(s => s.StoreId == storeId.Value);

        var result = new List<TechnicalServiceSummaryDto>();
        foreach (var service in open)
        {
            result.Add(await MapToSummaryDto(service));
        }

        return result.OrderByDescending(s => s.Priority).ThenBy(s => s.ReportedDate).ToList();
    }

    public async Task<IEnumerable<TechnicalServiceSummaryDto>> GetSlaViolationsAsync(int? storeId = null)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var open = services.Where(s =>
            s.Status == TechnicalServiceStatus.Acik ||
            s.Status == TechnicalServiceStatus.Islemde);

        if (storeId.HasValue)
            open = open.Where(s => s.StoreId == storeId.Value);

        var violations = new List<TechnicalServiceSummaryDto>();

        foreach (var service in open)
        {
            var hoursSinceReported = (DateTime.Now - service.ReportedDate).TotalHours;
            var slaTarget = SlaTargetHours.GetValueOrDefault(service.Priority, 24);

            if (hoursSinceReported > slaTarget)
            {
                violations.Add(await MapToSummaryDto(service));
            }
        }

        return violations.OrderByDescending(s => s.Priority).ToList();
    }

    public async Task<IEnumerable<TechnicalServiceSummaryDto>> GetTechnicalServicesByPriorityAsync(int priority, int? storeId = null)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var priorityServices = services.Where(s => s.Priority == priority);

        if (storeId.HasValue)
            priorityServices = priorityServices.Where(s => s.StoreId == storeId.Value);

        var result = new List<TechnicalServiceSummaryDto>();
        foreach (var service in priorityServices)
        {
            result.Add(await MapToSummaryDto(service));
        }

        return result;
    }

    public async Task<bool> AssignTechnicalServiceAsync(int serviceId, int employeeId)
    {
        var service = await _unitOfWork.TechnicalServices.GetByIdAsync(serviceId);
        if (service == null)
            throw new InvalidOperationException("Teknik servis kaydı bulunamadı.");

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            throw new InvalidOperationException("Çalışan bulunamadı.");

        service.AssignedToEmployeeId = employeeId;

        // Açık ise İşlemde yap
        if (service.Status == TechnicalServiceStatus.Acik)
            service.Status = TechnicalServiceStatus.Islemde;

        _unitOfWork.TechnicalServices.Update(service);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ResolveTechnicalServiceAsync(int serviceId, string resolution, decimal? cost = null)
    {
        var service = await _unitOfWork.TechnicalServices.GetByIdAsync(serviceId);
        if (service == null)
            throw new InvalidOperationException("Teknik servis kaydı bulunamadı.");

        if (string.IsNullOrEmpty(resolution))
            throw new InvalidOperationException("Çözüm açıklaması zorunludur.");

        service.Status = TechnicalServiceStatus.Tamamlandi;
        service.Resolution = resolution;
        service.ResolvedDate = DateTime.Now;
        service.Cost = cost;

        _unitOfWork.TechnicalServices.Update(service);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> MarkAsUnresolvableAsync(int serviceId, string reason)
    {
        var service = await _unitOfWork.TechnicalServices.GetByIdAsync(serviceId);
        if (service == null)
            throw new InvalidOperationException("Teknik servis kaydı bulunamadı.");

        if (string.IsNullOrEmpty(reason))
            throw new InvalidOperationException("Çözülememe sebebi zorunludur.");

        service.Status = TechnicalServiceStatus.Cozulemedi;
        service.Resolution = $"[Çözülemedi] {reason}";
        service.ResolvedDate = DateTime.Now;

        _unitOfWork.TechnicalServices.Update(service);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<object> GetTechnicalServicePerformanceReportAsync(DateTime startDate, DateTime endDate, int? storeId = null)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var periodServices = services.Where(s =>
            s.ReportedDate >= startDate &&
            s.ReportedDate <= endDate);

        if (storeId.HasValue)
            periodServices = periodServices.Where(s => s.StoreId == storeId.Value);

        var servicesList = periodServices.ToList();

        var resolved = servicesList.Where(s =>
            s.Status == TechnicalServiceStatus.Tamamlandi ||
            s.Status == TechnicalServiceStatus.Cozulemedi).ToList();

        var avgResolutionTime = resolved.Any() && resolved.All(s => s.ResolvedDate.HasValue)
            ? resolved.Average(s => (s.ResolvedDate!.Value - s.ReportedDate).TotalHours)
            : 0;

        // SLA başarı oranı
        var slaSuccess = resolved.Any()
            ? resolved.Count(s =>
            {
                var hours = (s.ResolvedDate!.Value - s.ReportedDate).TotalHours;
                var target = SlaTargetHours.GetValueOrDefault(s.Priority, 24);
                return hours <= target;
            }) * 100.0 / resolved.Count
            : 0;

        return new
        {
            Period = new { StartDate = startDate, EndDate = endDate },
            TotalIssues = servicesList.Count,
            ResolvedCount = servicesList.Count(s => s.Status == TechnicalServiceStatus.Tamamlandi),
            UnresolvableCount = servicesList.Count(s => s.Status == TechnicalServiceStatus.Cozulemedi),
            OpenCount = servicesList.Count(s => s.Status == TechnicalServiceStatus.Acik || s.Status == TechnicalServiceStatus.Islemde),
            AverageResolutionTimeHours = Math.Round(avgResolutionTime, 2),
            SlaSuccessRate = Math.Round(slaSuccess, 2),
            ByPriority = servicesList
                .GroupBy(s => s.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .OrderBy(x => x.Priority),
            CustomerIssueCount = servicesList.Count(s => s.IsCustomerIssue),
            SystemIssueCount = servicesList.Count(s => !s.IsCustomerIssue)
        };
    }

    public async Task<object> GetEmployeeTechnicalServiceStatsAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var employeeServices = services.Where(s =>
            s.AssignedToEmployeeId == employeeId &&
            s.ReportedDate >= startDate &&
            s.ReportedDate <= endDate).ToList();

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);

        var resolved = employeeServices.Where(s => s.Status == TechnicalServiceStatus.Tamamlandi).ToList();
        var avgTime = resolved.Any() && resolved.All(s => s.ResolvedDate.HasValue)
            ? resolved.Average(s => (s.ResolvedDate!.Value - s.ReportedDate).TotalHours)
            : 0;

        return new
        {
            EmployeeId = employeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Bilinmiyor",
            Period = new { StartDate = startDate, EndDate = endDate },
            TotalAssigned = employeeServices.Count,
            ResolvedCount = resolved.Count,
            AverageResolutionTimeHours = Math.Round(avgTime, 2),
            OpenCount = employeeServices.Count(s => s.Status == TechnicalServiceStatus.Acik || s.Status == TechnicalServiceStatus.Islemde)
        };
    }


    // ====== PRIVATE HELPER METHODS ======

    private async Task<string> GenerateServiceNumberAsync()
    {
        var year = DateTime.Now.Year;
        var services = await _unitOfWork.TechnicalServices.GetAllAsync();
        var yearServices = services.Where(s => s.ServiceNumber.Contains($"TS-{year}-"));

        var maxNumber = 0;
        foreach (var service in yearServices)
        {
            var parts = service.ServiceNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"TS-{year}-{(maxNumber + 1):D5}";
    }

    private string CalculateSlaStatus(TechnicalService service)
    {
        var slaTarget = SlaTargetHours.GetValueOrDefault(service.Priority, 24);

        if (service.Status == TechnicalServiceStatus.Tamamlandi ||
            service.Status == TechnicalServiceStatus.Cozulemedi)
        {
            if (service.ResolvedDate.HasValue)
            {
                var hours = (service.ResolvedDate.Value - service.ReportedDate).TotalHours;
                return hours <= slaTarget ? "SLA İçinde" : "SLA İhlali";
            }
        }
        else
        {
            var hours = (DateTime.Now - service.ReportedDate).TotalHours;
            if (hours < slaTarget * 0.8) return "SLA İçinde";
            if (hours < slaTarget) return "SLA Riski";
            return "SLA İhlali";
        }

        return "Belirsiz";
    }

    private async Task<TechnicalServiceSummaryDto> MapToSummaryDto(TechnicalService service)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(service.StoreId);
        string? assignedToName = null;

        if (service.AssignedToEmployeeId.HasValue)
        {
            var assigned = await _unitOfWork.Employees.GetByIdAsync(service.AssignedToEmployeeId.Value);
            assignedToName = assigned != null ? $"{assigned.FirstName} {assigned.LastName}" : null;
        }

        return new TechnicalServiceSummaryDto
        {
            Id = service.Id,
            ServiceNumber = service.ServiceNumber,
            Title = service.Title,
            Status = service.Status,
            Priority = service.Priority,
            PriorityText = service.Priority switch
            {
                1 => "Düşük",
                2 => "Orta",
                3 => "Yüksek",
                4 => "Kritik",
                _ => "Belirsiz"
            },
            SlaStatus = CalculateSlaStatus(service),
            IsCustomerIssue = service.IsCustomerIssue,
            StoreName = store?.StoreName ?? "Bilinmiyor",
            AssignedToEmployeeName = assignedToName,
            ReportedDate = service.ReportedDate
        };
    }

    private async Task<TechnicalServiceDto> MapToDetailDto(TechnicalService service)
    {
        var dto = _mapper.Map<TechnicalServiceDto>(service);

        var store = await _unitOfWork.Stores.GetByIdAsync(service.StoreId);
        var reportedBy = await _unitOfWork.Employees.GetByIdAsync(service.ReportedByEmployeeId);

        dto.StoreName = store?.StoreName ?? "Bilinmiyor";
        dto.ReportedByEmployeeName = reportedBy != null ? $"{reportedBy.FirstName} {reportedBy.LastName}" : "Bilinmiyor";

        if (service.AssignedToEmployeeId.HasValue)
        {
            var assigned = await _unitOfWork.Employees.GetByIdAsync(service.AssignedToEmployeeId.Value);
            dto.AssignedToEmployeeName = assigned != null ? $"{assigned.FirstName} {assigned.LastName}" : null;
        }

        if (service.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(service.CustomerId.Value);
            dto.CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : null;
        }

        if (service.ProductId.HasValue)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(service.ProductId.Value);
            dto.ProductName = product?.Name;
        }

        dto.ResolutionTimeInHours = service.ResolvedDate.HasValue
            ? (service.ResolvedDate.Value - service.ReportedDate).TotalHours
            : null;

        return dto;
    }
}
