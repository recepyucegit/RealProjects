using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Account;

namespace Web.Controllers
{
    /// <summary>
    /// Account Controller
    ///
    /// AMAÇ:
    /// - Login/Logout işlemleri
    /// - Kullanıcı kimlik doğrulama
    /// - Role göre yönlendirme
    ///
    /// KULLANICI TİPLERİ:
    /// - SubeYoneticisi → /SubeYoneticisi/Dashboard
    /// - KasaSatis → /KasaSatis/Dashboard
    /// - MobilSatis → /MobilSatis/Dashboard
    /// - Depo → /Depo/Dashboard
    /// - Muhasebe → /Muhasebe/Dashboard
    /// - TeknikServis → /TeknikServis/Dashboard
    /// </summary>
    [AllowAnonymous] // Login sayfası herkes erişebilir
    public class AccountController : BaseController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(
            IUnitOfWork unitOfWork,
            ILogger<AccountController> logger,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
            : base(unitOfWork, logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Email ile giriş yap
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true // 5 hatalı denemede hesap kilitlenir
            );

            if (result.Succeeded)
            {
                _logger.LogInformation($"Kullanıcı giriş yaptı: {model.Email}");
                ShowSuccessMessage("Hoş geldiniz!");

                // ReturnUrl varsa oraya yönlendir
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                // Role göre dashboard'a yönlendir
                return RedirectToRoleDashboard();
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning($"Hesap kilitlendi: {model.Email}");
                ModelState.AddModelError(string.Empty, "Hesabınız çok fazla hatalı deneme nedeniyle kilitlendi. Lütfen 5 dakika sonra tekrar deneyin.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Kullanıcı çıkış yaptı");
            ShowInfoMessage("Çıkış yapıldı.");
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            ShowErrorMessage("Bu sayfaya erişim yetkiniz yok.");
            return View();
        }

        /// <summary>
        /// Kullanıcının rolüne göre dashboard'a yönlendir
        /// </summary>
        private IActionResult RedirectToRoleDashboard()
        {
            if (User.IsInRole("SubeYoneticisi"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "SubeYoneticisi" });
            }
            else if (User.IsInRole("KasaSatis"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "KasaSatis" });
            }
            else if (User.IsInRole("MobilSatis"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "MobilSatis" });
            }
            else if (User.IsInRole("Depo"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Depo" });
            }
            else if (User.IsInRole("Muhasebe"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Muhasebe" });
            }
            else if (User.IsInRole("TeknikServis"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "TeknikServis" });
            }

            // Rol bulunamazsa login'e geri dön
            _logger.LogWarning($"Kullanıcının rolü bulunamadı: {User.Identity?.Name}");
            return RedirectToAction(nameof(Login));
        }
    }
}
