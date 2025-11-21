using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TechnicalServicesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TechnicalServicesController> _logger;

        public TechnicalServicesController(IUnitOfWork unitOfWork, ILogger<TechnicalServicesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all technical service issues
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> GetAll()
        {
            var issues = await _unitOfWork.TechnicalServices.GetAllAsync();
            return Ok(issues);
        }

        /// <summary>
        /// Get issue by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> GetById(int id)
        {
            var issue = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
            if (issue == null)
                return NotFound(new { message = "Teknik servis kaydı bulunamadı" });

            return Ok(issue);
        }

        /// <summary>
        /// Get open issues (Özgün Kablocu için)
        /// </summary>
        [HttpGet("open")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> GetOpenIssues()
        {
            var openIssues = await _unitOfWork.TechnicalServices.GetOpenIssuesAsync();
            return Ok(openIssues);
        }

        /// <summary>
        /// Get issues assigned to specific employee
        /// </summary>
        [HttpGet("assigned/{employeeId}")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> GetAssignedIssues(int employeeId)
        {
            var assignedIssues = await _unitOfWork.TechnicalServices.GetAssignedIssuesAsync(employeeId);
            return Ok(assignedIssues);
        }

        /// <summary>
        /// Get issues by priority level
        /// </summary>
        [HttpGet("priority/{priority}")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> GetByPriority(int priority)
        {
            if (priority < 1 || priority > 4)
                return BadRequest(new { message = "Öncelik seviyesi 1-4 arası olmalıdır" });

            var issues = await _unitOfWork.TechnicalServices.GetByPriorityAsync(priority);
            return Ok(issues);
        }

        /// <summary>
        /// Get critical issues (Priority 4)
        /// </summary>
        [HttpGet("critical")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> GetCriticalIssues()
        {
            var criticalIssues = await _unitOfWork.TechnicalServices.GetByPriorityAsync(4);
            return Ok(criticalIssues);
        }

        /// <summary>
        /// Get open issues count
        /// </summary>
        [HttpGet("open/count")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> GetOpenIssuesCount()
        {
            var count = await _unitOfWork.TechnicalServices.GetOpenIssuesCountAsync();
            return Ok(new { openIssuesCount = count });
        }

        /// <summary>
        /// Create new technical service issue
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "TeknikServis")]
        public async Task<IActionResult> Create([FromBody] TechnicalServiceCreateDto dto)
        {
            try
            {
                var issue = new TechnicalService
                {
                    StoreId = dto.StoreId,
                    CustomerId = dto.CustomerId,
                    AssignedEmployeeId = dto.AssignedEmployeeId,
                    IssueType = dto.IssueType,
                    Title = dto.Title,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = IssueStatus.Acik, // Yeni kayıt her zaman Açık
                    ReportedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                await _unitOfWork.TechnicalServices.AddAsync(issue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Yeni teknik servis kaydı oluşturuldu: {IssueId}, Öncelik: {Priority}",
                    issue.ID, issue.Priority);

                return CreatedAtAction(nameof(GetById), new { id = issue.ID }, issue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servis kaydı eklenirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Assign issue to employee
        /// </summary>
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> AssignToEmployee(int id, [FromBody] AssignIssueDto dto)
        {
            try
            {
                var issue = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
                if (issue == null)
                    return NotFound(new { message = "Teknik servis kaydı bulunamadı" });

                issue.AssignedEmployeeId = dto.EmployeeId;
                issue.ModifiedDate = DateTime.Now;

                _unitOfWork.TechnicalServices.Update(issue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Teknik servis kaydı atandı: {IssueId} -> Çalışan: {EmployeeId}",
                    id, dto.EmployeeId);

                return Ok(new { message = "Kayıt başarıyla atandı", issue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt atama sırasında hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update issue status
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var issue = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
                if (issue == null)
                    return NotFound(new { message = "Teknik servis kaydı bulunamadı" });

                issue.Status = dto.Status;
                issue.ModifiedDate = DateTime.Now;

                // Kapandı veya Çözüldü durumunda kapanış tarihi ekle
                if (dto.Status == IssueStatus.Kapandi || dto.Status == IssueStatus.Cozuldu)
                {
                    issue.ClosedDate = DateTime.Now;
                }

                _unitOfWork.TechnicalServices.Update(issue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Teknik servis durumu güncellendi: {IssueId} -> {Status}",
                    id, dto.Status);

                return Ok(new { message = "Durum başarıyla güncellendi", issue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Durum güncelleme sırasında hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add solution to issue (close issue)
        /// </summary>
        [HttpPut("{id}/solve")]
        [Authorize(Roles = "TeknikServis")]
        public async Task<IActionResult> SolveIssue(int id, [FromBody] SolveIssueDto dto)
        {
            try
            {
                var issue = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
                if (issue == null)
                    return NotFound(new { message = "Teknik servis kaydı bulunamadı" });

                issue.Solution = dto.Solution;
                issue.Status = IssueStatus.Cozuldu;
                issue.ClosedDate = DateTime.Now;
                issue.ModifiedDate = DateTime.Now;

                _unitOfWork.TechnicalServices.Update(issue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Teknik servis sorunu çözüldü: {IssueId}", id);
                return Ok(new { message = "Sorun çözüldü olarak işaretlendi", issue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sorun çözme işlemi sırasında hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete issue
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "TeknikServis,SubeYoneticisi")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var issue = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
                if (issue == null)
                    return NotFound(new { message = "Teknik servis kaydı bulunamadı" });

                await _unitOfWork.TechnicalServices.SoftDeleteAsync(issue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Teknik servis kaydı silindi: {IssueId}", id);
                return Ok(new { message = "Kayıt başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt silinirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs
    public class TechnicalServiceCreateDto
    {
        public int StoreId { get; set; }
        public int? CustomerId { get; set; }
        public int? AssignedEmployeeId { get; set; }
        public IssueType IssueType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; } // 1-4
    }

    public class AssignIssueDto
    {
        public int EmployeeId { get; set; }
    }

    public class UpdateStatusDto
    {
        public IssueStatus Status { get; set; }
    }

    public class SolveIssueDto
    {
        public string Solution { get; set; }
    }
}
