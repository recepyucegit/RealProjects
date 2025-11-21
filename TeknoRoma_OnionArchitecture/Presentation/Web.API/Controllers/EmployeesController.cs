using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IUnitOfWork unitOfWork, ILogger<EmployeesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all employees (SubeYoneticisi only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _unitOfWork.Employees.GetAllAsync();
            return Ok(employees);
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Çalışan bulunamadı" });

            return Ok(employee);
        }

        /// <summary>
        /// Get employee by Identity User ID
        /// </summary>
        [HttpGet("identity/{identityUserId}")]
        public async Task<IActionResult> GetByIdentityUserId(string identityUserId)
        {
            var employee = await _unitOfWork.Employees.GetByIdentityUserIdAsync(identityUserId);
            if (employee == null)
                return NotFound(new { message = "Çalışan bulunamadı" });

            return Ok(employee);
        }

        /// <summary>
        /// Get employees by store
        /// </summary>
        [HttpGet("store/{storeId}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetByStore(int storeId)
        {
            var employees = await _unitOfWork.Employees.GetByStoreAsync(storeId);
            return Ok(employees);
        }

        /// <summary>
        /// Get employees by department
        /// </summary>
        [HttpGet("department/{departmentId}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var employees = await _unitOfWork.Employees.GetByDepartmentAsync(departmentId);
            return Ok(employees);
        }

        /// <summary>
        /// Get active employees
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetActiveEmployees()
        {
            var activeEmployees = await _unitOfWork.Employees.GetActiveEmployeesAsync();
            return Ok(activeEmployees);
        }

        /// <summary>
        /// Get employee sales performance (Haluk Bey için)
        /// </summary>
        [HttpGet("{id}/performance/{year}/{month}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetPerformance(int id, int year, int month)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Çalışan bulunamadı" });

            var totalSales = await _unitOfWork.Employees.GetEmployeeSalesPerformanceAsync(id, year, month);
            var commission = await _unitOfWork.Employees.CalculateEmployeeCommissionAsync(id, year, month);

            var performance = new
            {
                employeeId = id,
                employeeName = $"{employee.FirstName} {employee.LastName}",
                year,
                month,
                totalSales,
                salesQuota = employee.SalesQuota ?? 0,
                commission,
                quotaPercentage = employee.SalesQuota.HasValue && employee.SalesQuota.Value > 0
                    ? Math.Round((totalSales / employee.SalesQuota.Value) * 100, 2)
                    : 0
            };

            return Ok(performance);
        }

        /// <summary>
        /// Get employee commission calculation
        /// </summary>
        [HttpGet("{id}/commission/{year}/{month}")]
        [Authorize(Roles = "SubeYoneticisi,KasaSatis,MobilSatis")]
        public async Task<IActionResult> GetCommission(int id, int year, int month)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Çalışan bulunamadı" });

            var commission = await _unitOfWork.Employees.CalculateEmployeeCommissionAsync(id, year, month);
            var totalSales = await _unitOfWork.Employees.GetEmployeeSalesPerformanceAsync(id, year, month);

            return Ok(new
            {
                employeeId = id,
                year,
                month,
                totalSales,
                salesQuota = employee.SalesQuota ?? 0,
                commissionAmount = commission,
                commissionRate = 0.10m // %10 prim oranı
            });
        }

        /// <summary>
        /// Get all employees performance report (Haluk Bey için)
        /// </summary>
        [HttpGet("performance/report/{year}/{month}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetPerformanceReport(int year, int month)
        {
            var employees = await _unitOfWork.Employees.GetActiveEmployeesAsync();
            var performanceList = new List<object>();

            foreach (var employee in employees)
            {
                // Sadece satış personeli için (SalesQuota olanlar)
                if (!employee.SalesQuota.HasValue)
                    continue;

                var totalSales = await _unitOfWork.Employees.GetEmployeeSalesPerformanceAsync(employee.ID, year, month);
                var commission = await _unitOfWork.Employees.CalculateEmployeeCommissionAsync(employee.ID, year, month);

                performanceList.Add(new
                {
                    employeeId = employee.ID,
                    employeeName = $"{employee.FirstName} {employee.LastName}",
                    department = employee.Department?.Name,
                    totalSales,
                    salesQuota = employee.SalesQuota.Value,
                    commission,
                    quotaPercentage = Math.Round((totalSales / employee.SalesQuota.Value) * 100, 2),
                    performanceLevel = totalSales >= employee.SalesQuota.Value ? "Hedef Aşıldı" : "Hedef Altında"
                });
            }

            return Ok(new
            {
                year,
                month,
                totalEmployees = performanceList.Count,
                performances = performanceList.OrderByDescending(p => ((dynamic)p).totalSales)
            });
        }

        /// <summary>
        /// Get employee monthly sales total
        /// </summary>
        [HttpGet("{id}/sales/{year}/{month}")]
        public async Task<IActionResult> GetMonthlySales(int id, int year, int month)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Çalışan bulunamadı" });

            var totalSales = await _unitOfWork.Employees.GetEmployeeSalesPerformanceAsync(id, year, month);

            return Ok(new
            {
                employeeId = id,
                employeeName = $"{employee.FirstName} {employee.LastName}",
                year,
                month,
                totalSales
            });
        }

        /// <summary>
        /// Create new employee (SubeYoneticisi only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
        {
            try
            {
                // Check if Identity Number already exists
                var existing = await _unitOfWork.Employees.GetByIdentityNumberAsync(dto.IdentityNumber);
                if (existing != null)
                    return BadRequest(new { message = "Bu TC Kimlik No ile kayıtlı çalışan zaten var" });

                var employee = new Employee
                {
                    StoreId = dto.StoreId,
                    DepartmentId = dto.DepartmentId,
                    IdentityUserId = dto.IdentityUserId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    IdentityNumber = dto.IdentityNumber,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    HireDate = dto.HireDate,
                    Salary = dto.Salary,
                    SalesQuota = dto.SalesQuota,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                await _unitOfWork.Employees.AddAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Yeni çalışan oluşturuldu: {EmployeeId} - {Name}",
                    employee.ID, $"{employee.FirstName} {employee.LastName}");

                return CreatedAtAction(nameof(GetById), new { id = employee.ID }, employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalışan eklenirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update employee (SubeYoneticisi only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateDto dto)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                    return NotFound(new { message = "Çalışan bulunamadı" });

                employee.DepartmentId = dto.DepartmentId;
                employee.Phone = dto.Phone;
                employee.Email = dto.Email;
                employee.Salary = dto.Salary;
                employee.SalesQuota = dto.SalesQuota;
                employee.IsActive = dto.IsActive;
                employee.ModifiedDate = DateTime.Now;

                _unitOfWork.Employees.Update(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Çalışan güncellendi: {EmployeeId}", id);
                return Ok(new { message = "Çalışan başarıyla güncellendi", employee });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalışan güncellenirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivate employee (SubeYoneticisi only)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                    return NotFound(new { message = "Çalışan bulunamadı" });

                employee.IsActive = false;
                employee.ModifiedDate = DateTime.Now;

                _unitOfWork.Employees.Update(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Çalışan pasif hale getirildi: {EmployeeId}", id);
                return Ok(new { message = "Çalışan pasif hale getirildi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalışan pasif edilirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete employee (SubeYoneticisi only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                    return NotFound(new { message = "Çalışan bulunamadı" });

                await _unitOfWork.Employees.SoftDeleteAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Çalışan silindi: {EmployeeId}", id);
                return Ok(new { message = "Çalışan başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalışan silinirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs
    public class EmployeeCreateDto
    {
        public int StoreId { get; set; }
        public int DepartmentId { get; set; }
        public string? IdentityUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdentityNumber { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public decimal? SalesQuota { get; set; }
    }

    public class EmployeeUpdateDto
    {
        public int DepartmentId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal Salary { get; set; }
        public decimal? SalesQuota { get; set; }
        public bool IsActive { get; set; }
    }
}
