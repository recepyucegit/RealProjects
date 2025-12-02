// ===================================================================================
// TEKNOROMA - ACCOUNT CONTROLLER (Kimlik Doğrulama)
// ===================================================================================
//
// BU CONTROLLER'IN AMACI:
// Kullanıcı girişi, çıkışı ve kimlik doğrulama işlemlerini yönetir
//
// ÖNEMLİ:
// Şu an basit bir login sayfası oluşturuyoruz
// İleride ASP.NET Identity entegrasyonu yapılacak
//
// ===================================================================================

using Microsoft.AspNetCore.Mvc;
using Presentation.WebMVC.Models;

namespace Presentation.WebMVC.Controllers
{
    /// <summary>
    /// Kullanıcı kimlik doğrulama controller'ı
    /// Login, Logout, Register işlemlerini yönetir
    /// </summary>
    public class AccountController : Controller
    {
        // TODO: IAuthenticationService inject edilecek (Identity kurulunca)

        /// <summary>
        /// Login sayfasını gösterir
        /// GET: /Account/Login
        ///
        /// KULLANIM SENARYOSU:
        /// - Kullanıcı siteye ilk girdiğinde bu sayfa açılır
        /// - Kimlik doğrulama gerektiren sayfaya erişmeye çalışırsa buraya yönlendirilir
        /// </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Return URL: Giriş yaptıktan sonra gitmek istediği sayfa
            // Örnek: /Products/Index sayfasına gitmek isterken login sayfasına düştü
            // Giriş yaptıktan sonra tekrar /Products/Index'e yönlendirilecek
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Login form'unu işler (POST)
        /// POST: /Account/Login
        ///
        /// YAPILACAKLAR:
        /// 1. Kullanıcı adı ve şifre doğrulaması
        /// 2. ASP.NET Identity ile kimlik doğrulama
        /// 3. Cookie oluşturma ve session başlatma
        /// 4. Başarılıysa ana sayfaya yönlendirme
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF koruması
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                // Form validasyon hatası varsa aynı sayfayı göster
                return View(model);
            }

            // TODO: ASP.NET Identity ile kullanıcı doğrulama
            // GEÇICI: Basit kontrol (gerçek uygulama değil!)
            if (model.Email == "admin@teknoroma.com" && model.Password == "Admin123!")
            {
                // Başarılı giriş!
                // TODO: SignInManager.PasswordSignInAsync kullanılacak

                // Session'a kullanıcı bilgilerini kaydet
                // GEÇICI: ASP.NET Identity kurulunca burası değişecek
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("UserName", "Admin Kullanıcı");
                HttpContext.Session.SetString("UserRole", "Yönetici");

                // ReturnUrl varsa oraya git, yoksa Home/Index'e
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            // Hatalı giriş
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı!");
            return View(model);
        }

        /// <summary>
        /// Kullanıcı çıkışı
        /// POST: /Account/Logout
        ///
        /// GÜVENLIK:
        /// - Sadece POST metodu (CSRF önlemi)
        /// - Logout link'leri form içinde olmalı
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // TODO: SignInManager.SignOutAsync() kullanılacak

            // Session'ı temizle
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Erişim reddedildi sayfası
        /// GET: /Account/AccessDenied
        ///
        /// KULLANIM:
        /// Kullanıcı yetkisi olmayan bir sayfaya erişmeye çalıştığında
        /// [Authorize(Roles = "Admin")] attribute'ü buraya yönlendirir
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
