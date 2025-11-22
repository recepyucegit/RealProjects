// ===================================================================================
// TEKNOROMA - CUSTOMERS CONTROLLER
// ===================================================================================
//
// Musteri yonetimi icin API endpoint'leri.
//
// TEKNOROMA GEREKSINIMLERI:
// - TC Kimlik ile hizli musteri sorgulama
// - Yeni musteri kaydi
// - Musteri profili ve satin alma gecmisi
//
// SENARYO (Gul - Kasa):
// "Musterinin TC Kimlik Numarasini girdigim anda musteri bilgilerinin
// buyuk bir bolumu otomatik olarak ekrana gelmeli"
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Musteri Yonetimi API Controller
    /// </summary>
    [Authorize]
    public class CustomersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(IUnitOfWork unitOfWork, ILogger<CustomersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Tum musterileri listele
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Customer>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return Success(customers);
        }

        /// <summary>
        /// ID ile musteri getir
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return NotFoundResponse($"ID: {id} olan musteri bulunamadi");

            return Success(customer);
        }

        /// <summary>
        /// TC Kimlik ile musteri getir
        /// </summary>
        /// <remarks>
        /// HIZLI MUSTERI SORGULAMA:
        /// Kasa satis sirasinda TC girildiginde
        /// musteri bilgileri otomatik gelir.
        ///
        /// SENARYO (Gul - Kasa):
        /// TC gir -> Musteri bilgileri ekrana gelsin
        /// </remarks>
        [HttpGet("identity/{identityNumber}")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByIdentityNumber(string identityNumber)
        {
            var customer = await _unitOfWork.Customers.GetByIdentityNumberAsync(identityNumber);

            if (customer == null)
                return NotFoundResponse($"TC: {identityNumber} ile kayitli musteri bulunamadi");

            _logger.LogInformation("Musteri sorgusu: TC {IdentityNumber}", identityNumber);
            return Success(customer);
        }

        /// <summary>
        /// Musteri ara (isim ile)
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Customer>>), 200)]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequestResponse("Arama terimi gerekli");

            var customers = await _unitOfWork.Customers.SearchByNameAsync(name);
            return Success(customers);
        }

        /// <summary>
        /// En cok alisveris yapan musteriler
        /// </summary>
        /// <remarks>
        /// RAPOR (Haluk - Sube Muduru):
        /// "En cok satilan 10 urun ve bu urunu alan musteriler"
        /// </remarks>
        [HttpGet("top")]
        [Authorize(Policy = "RaporGorebilir")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Customer>>), 200)]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
        {
            var customers = await _unitOfWork.Customers.GetTopCustomersAsync(count);
            return Success(customers, $"En cok alisveris yapan {count} musteri");
        }

        /// <summary>
        /// Yeni musteri olustur
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "SatisYapabilir")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Customer customer)
        {
            // TC benzersizlik kontrolu
            if (!string.IsNullOrEmpty(customer.IdentityNumber))
            {
                var existing = await _unitOfWork.Customers.GetByIdentityNumberAsync(customer.IdentityNumber);
                if (existing != null)
                    return BadRequestResponse($"Bu TC ile kayitli musteri mevcut: {customer.IdentityNumber}");
            }

            var created = await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Yeni musteri olusturuldu: {CustomerName}", customer.FullName);
            return Created(created);
        }

        /// <summary>
        /// Musteri guncelle
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "SatisYapabilir")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] Customer customer)
        {
            var existing = await _unitOfWork.Customers.GetByIdAsync(id);
            if (existing == null)
                return NotFoundResponse($"ID: {id} olan musteri bulunamadi");

            existing.FirstName = customer.FirstName;
            existing.LastName = customer.LastName;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.Address = customer.Address;

            await _unitOfWork.Customers.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            return Success(existing, "Musteri bilgileri guncellendi");
        }

        /// <summary>
        /// Musteri sil (Soft Delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "SubeYonetimi")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return NotFoundResponse($"ID: {id} olan musteri bulunamadi");

            await _unitOfWork.Customers.SoftDeleteAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
