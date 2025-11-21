using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.Areas.KasaSatis.Controllers
{
    [Area("KasaSatis")]
    [Authorize(Roles = "KasaSatis")]
    public class CustomerController : BaseController
    {
        public CustomerController(IUnitOfWork unitOfWork, ILogger<CustomerController> logger)
            : base(unitOfWork, logger) { }

        // GET: /KasaSatis/Customer/Search
        public async Task<IActionResult> Search(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return Json(new { success = false, message = "TC Kimlik No giriniz" });
            }

            var customer = await _unitOfWork.Customers.GetByIdentityNumberAsync(identityNumber);

            if (customer == null)
            {
                return Json(new { success = false, message = "Müşteri bulunamadı" });
            }

            return Json(new
            {
                success = true,
                customer = new
                {
                    id = customer.ID,
                    fullName = customer.FullName,
                    identityNumber = customer.IdentityNumber,
                    phone = customer.Phone,
                    email = customer.Email
                }
            });
        }

        // POST: /KasaSatis/Customer/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Customer model)
        {
            try
            {
                // TC Kimlik kontrolü
                var existing = await _unitOfWork.Customers.GetByIdentityNumberAsync(model.IdentityNumber);
                if (existing != null)
                {
                    return Json(new { success = false, message = "Bu TC Kimlik No ile kayıtlı müşteri var" });
                }

                await _unitOfWork.Customers.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Müşteri kaydedildi",
                    customer = new
                    {
                        id = model.ID,
                        fullName = model.FullName,
                        identityNumber = model.IdentityNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri kaydedilirken hata");
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
    }
}
