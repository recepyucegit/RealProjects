// ===================================================================================
// TEKNOROMA - BASE API CONTROLLER
// ===================================================================================
//
// Tum API controller'larinin miras aldigi temel sinif.
// Ortak ozellikler ve yardimci metodlar burada tanimlanir.
//
// CONTROLLER ATTRIBUTE'LARI:
// [ApiController]: Model validation, automatic 400 response
// [Route]: API route sablonu
//
// ===================================================================================

using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Temel API Controller
    ///
    /// Tum controller'lar bu siniftan turetilir.
    /// Ortak ozellikler:
    /// - Route template: api/[controller]
    /// - ApiController attribute
    /// - Yardimci metodlar
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Basarili response dondur (200 OK)
        /// </summary>
        protected IActionResult Success<T>(T data, string? message = null)
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Message = message ?? "Islem basarili",
                Data = data
            });
        }

        /// <summary>
        /// Olusturma basarili response (201 Created)
        /// </summary>
        protected IActionResult Created<T>(T data, string? location = null)
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Message = "Kayit basariyla olusturuldu",
                Data = data
            };

            if (!string.IsNullOrEmpty(location))
            {
                return Created(location, response);
            }

            return StatusCode(201, response);
        }

        /// <summary>
        /// Kayit bulunamadi response (404 Not Found)
        /// </summary>
        protected IActionResult NotFoundResponse(string message = "Kayit bulunamadi")
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null
            });
        }

        /// <summary>
        /// Hatali istek response (400 Bad Request)
        /// </summary>
        protected IActionResult BadRequestResponse(string message)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null
            });
        }
    }

    /// <summary>
    /// Standart API Response Wrapper
    ///
    /// Tum API response'lari bu formatta doner.
    /// Frontend gelistiriciler icin tutarli bir yapi saglar.
    /// </summary>
    /// <typeparam name="T">Data tipi</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Islem basarili mi?
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mesaj (basari veya hata aciklamasi)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Response olusturma zamani
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Sayfalama icin response wrapper
    /// </summary>
    public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        /// <summary>
        /// Mevcut sayfa numarasi
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Sayfa basi kayit sayisi
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Toplam kayit sayisi
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Toplam sayfa sayisi
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
