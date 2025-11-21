using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(IUnitOfWork unitOfWork, ILogger<CustomersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return NotFound(new { message = "Müşteri bulunamadı" });

            return Ok(customer);
        }

        /// <summary>
        /// TC Kimlik ile ara (Gül Satar için)
        /// </summary>
        [HttpGet("identity/{identityNumber}")]
        public async Task<IActionResult> GetByIdentityNumber(string identityNumber)
        {
            var customer = await _unitOfWork.Customers.GetByIdentityNumberAsync(identityNumber);
            if (customer == null)
                return NotFound(new { message = "Müşteri bulunamadı" });

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Customer customer)
        {
            try
            {
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = customer.ID }, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri eklenirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
