// ===================================================================================
// TEKNOROMA - TECHNICAL SERVICES CONTROLLER
// ===================================================================================
//
// Teknik servis talepleri icin API endpoint'leri.
// Ticket sistemi ve sorun takibi.
//
// TEKNOROMA GEREKSINIMLERI:
// - Sorun bildirimi (telefon yerine sistem uzerinden)
// - Otomatik uyari (yeni sorun bildirildiginde)
// - Durum takibi (Tamamlandi, Tamamlanamadi)
// - Musteri ve ic sorunlar ayri takip
//
// SENARYO (Ozgun - Teknik Servis):
// "Ozellikle Satis bolumunde teknik bir sorun ciktiginda
// bizi telefonla arayip sorunu bildiriyorlar, sorunu bu sekilde
// takip etmek cok guc. Windows Uygulamasini kullanarak sorun
// bildirimi yapmalarini ve yeni sorun bildirildiginde
// bizi otomatik olarak uyarmasini istiyoruz."
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Teknik Servis Talepleri API Controller
    ///
    /// Ticket sistemi, sorun takibi
    /// </summary>
    [Authorize]
    public class TechnicalServicesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TechnicalServicesController> _logger;

        public TechnicalServicesController(
            IUnitOfWork unitOfWork,
            ILogger<TechnicalServicesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Tum teknik servis taleplerini listele
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TechnicalService>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await _unitOfWork.TechnicalServices.GetAllAsync();
            return Success(tickets);
        }

        /// <summary>
        /// ID ile talep getir
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<TechnicalService>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
            if (ticket == null)
                return NotFoundResponse($"ID: {id} olan talep bulunamadi");

            return Success(ticket);
        }

        /// <summary>
        /// Servis numarasi ile talep getir
        /// </summary>
        /// <param name="serviceNumber">Servis numarasi (TS-YYYY-NNNNN)</param>
        [HttpGet("number/{serviceNumber}")]
        [ProducesResponseType(typeof(ApiResponse<TechnicalService>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByServiceNumber(string serviceNumber)
        {
            var ticket = await _unitOfWork.TechnicalServices.GetByServiceNumberAsync(serviceNumber);
            if (ticket == null)
                return NotFoundResponse($"Servis no: {serviceNumber} bulunamadi");

            return Success(ticket);
        }

        /// <summary>
        /// Duruma gore talepler
        /// </summary>
        /// <param name="status">Talep durumu</param>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TechnicalService>>), 200)]
        public async Task<IActionResult> GetByStatus(TechnicalServiceStatus status)
        {
            var tickets = await _unitOfWork.TechnicalServices.GetByStatusAsync(status);
            return Success(tickets);
        }

        /// <summary>
        /// Acik talepler
        /// </summary>
        /// <remarks>
        /// Tamamlanmamis ve cozulemedi disindaki tum talepler.
        /// Dashboard uyarisi icin kullanilir.
        /// </remarks>
        [HttpGet("open")]
        [Authorize(Policy = "TeknikDestek")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TechnicalService>>), 200)]
        public async Task<IActionResult> GetOpenIssues()
        {
            var tickets = await _unitOfWork.TechnicalServices.GetOpenIssuesAsync();
            return Success(tickets, $"{tickets.Count} acik talep var");
        }

        /// <summary>
        /// Acik talep sayisi
        /// </summary>
        /// <remarks>
        /// Dashboard badge/counter icin hizli sorgu
        /// </remarks>
        [HttpGet("open/count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        public async Task<IActionResult> GetOpenIssuesCount()
        {
            var count = await _unitOfWork.TechnicalServices.GetOpenIssuesCountAsync();
            return Success(new { OpenIssuesCount = count });
        }

        /// <summary>
        /// Atanmamis talepler
        /// </summary>
        /// <remarks>
        /// SENARYO (Ozgun - Teknik Servis):
        /// Henuz bir teknisyene atanmamis talepler.
        /// Is dagilimi icin kritik.
        /// </remarks>
        [HttpGet("unassigned")]
        [Authorize(Policy = "TeknikDestek")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TechnicalService>>), 200)]
        public async Task<IActionResult> GetUnassigned()
        {
            var tickets = await _unitOfWork.TechnicalServices.GetUnassignedAsync();
            return Success(tickets, $"{tickets.Count} atanmamis talep var");
        }

        /// <summary>
        /// Teknisyene atanan talepler
        /// </summary>
        /// <param name="employeeId">Teknisyen ID</param>
        [HttpGet("assigned/{employeeId:int}")]
        [Authorize(Policy = "TeknikDestek")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TechnicalService>>), 200)]
        public async Task<IActionResult> GetByAssignedEmployee(int employeeId)
        {
            var tickets = await _unitOfWork.TechnicalServices.GetByAssignedEmployeeAsync(employeeId);
            return Success(tickets);
        }

        /// <summary>
        /// Musteri talepleri
        /// </summary>
        /// <param name="customerId">Musteri ID</param>
        [HttpGet("customer/{customerId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TechnicalService>>), 200)]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var tickets = await _unitOfWork.TechnicalServices.GetByCustomerAsync(customerId);
            return Success(tickets);
        }

        /// <summary>
        /// Magaza talepleri
        /// </summary>
        /// <param name="storeId">Magaza ID</param>
        [HttpGet("store/{storeId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TechnicalService>>), 200)]
        public async Task<IActionResult> GetByStore(int storeId)
        {
            var tickets = await _unitOfWork.TechnicalServices.GetByStoreAsync(storeId);
            return Success(tickets);
        }

        /// <summary>
        /// Yeni teknik servis talebi olustur
        /// </summary>
        /// <remarks>
        /// SORUN BILDIRIMI:
        /// Herkes sistem uzerinden sorun bildirebilir.
        /// Telefon yerine bu endpoint kullanilir.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TechnicalService>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] TechnicalService ticket)
        {
            // Servis numarasi olustur
            ticket.ServiceNumber = await _unitOfWork.TechnicalServices.GenerateServiceNumberAsync();
            ticket.Status = TechnicalServiceStatus.Acik;
            ticket.CreatedDate = DateTime.Now;

            var created = await _unitOfWork.TechnicalServices.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("YENI TEKNIK SERVIS TALEBI: {ServiceNumber} - {Description}",
                ticket.ServiceNumber, ticket.Description);

            return Created(created);
        }

        /// <summary>
        /// Talep durumunu guncelle
        /// </summary>
        /// <param name="id">Talep ID</param>
        /// <param name="status">Yeni durum</param>
        [HttpPut("{id:int}/status")]
        [Authorize(Policy = "TeknikDestek")]
        [ProducesResponseType(typeof(ApiResponse<TechnicalService>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] TechnicalServiceStatus status)
        {
            var ticket = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
            if (ticket == null)
                return NotFoundResponse($"ID: {id} olan talep bulunamadi");

            ticket.Status = status;

            if (status == TechnicalServiceStatus.Tamamlandi ||
                status == TechnicalServiceStatus.Cozulemedi)
            {
                ticket.ResolvedDate = DateTime.Now;
            }

            await _unitOfWork.TechnicalServices.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Teknik servis durumu guncellendi: {ServiceNumber} -> {Status}",
                ticket.ServiceNumber, status);

            return Success(ticket, $"Talep durumu guncellendi: {status}");
        }

        /// <summary>
        /// Talebi teknisyene ata
        /// </summary>
        /// <param name="id">Talep ID</param>
        /// <param name="employeeId">Teknisyen ID</param>
        [HttpPut("{id:int}/assign/{employeeId:int}")]
        [Authorize(Policy = "TeknikDestek")]
        [ProducesResponseType(typeof(ApiResponse<TechnicalService>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignToEmployee(int id, int employeeId)
        {
            var ticket = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
            if (ticket == null)
                return NotFoundResponse($"ID: {id} olan talep bulunamadi");

            ticket.AssignedToEmployeeId = employeeId;
            ticket.Status = TechnicalServiceStatus.Islemde;

            await _unitOfWork.TechnicalServices.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Teknik servis atandi: {ServiceNumber} -> Calisan {EmployeeId}",
                ticket.ServiceNumber, employeeId);

            return Success(ticket, "Talep teknisyene atandi");
        }

        /// <summary>
        /// Talep sil (Soft Delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "SubeYonetimi")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
            if (ticket == null)
                return NotFoundResponse($"ID: {id} olan talep bulunamadi");

            await _unitOfWork.TechnicalServices.SoftDeleteAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
