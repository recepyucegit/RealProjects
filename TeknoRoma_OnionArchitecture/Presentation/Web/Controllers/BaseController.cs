using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    /// <summary>
    /// Base Controller
    ///
    /// AMAÇ:
    /// - Tüm controller'ların miras alacağı temel sınıf
    /// - Ortak metodlar ve özellikler
    /// - UnitOfWork ve Logger erişimi
    ///
    /// KULLANIM:
    /// public class HomeController : BaseController
    /// {
    ///     public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger)
    ///         : base(unitOfWork, logger) { }
    /// }
    /// </summary>
    [Authorize] // Varsayılan olarak tüm controller'lar login gerektirir
    public abstract class BaseController : Controller
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger _logger;

        protected BaseController(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Success mesajı göster
        /// </summary>
        protected void ShowSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        /// <summary>
        /// Error mesajı göster
        /// </summary>
        protected void ShowErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Warning mesajı göster
        /// </summary>
        protected void ShowWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        /// <summary>
        /// Info mesajı göster
        /// </summary>
        protected void ShowInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        /// <summary>
        /// Giriş yapmış kullanıcının ID'sini al
        /// </summary>
        protected string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Giriş yapmış kullanıcının email'ini al
        /// </summary>
        protected string GetCurrentUserEmail()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Giriş yapmış kullanıcının rolünü al
        /// </summary>
        protected string GetCurrentUserRole()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
