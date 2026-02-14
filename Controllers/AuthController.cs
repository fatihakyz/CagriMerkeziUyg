using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Data;
using CagriMerkeziUyg.Models;
using CagriMerkeziUyg.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CagriMerkeziUyg.Controllers
{
    public class AuthController : Controller
    {
        private readonly CagriMerkeziDbContext _context;
        private readonly OperatorDurumService _durumService;

        public AuthController(CagriMerkeziDbContext context, OperatorDurumService durumService)
        {
            _context = context;
            _durumService = durumService;
        }

        // Giriş sayfası
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Giriş işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre, string? returnUrl = null)
        {
            // Console'a da yazdır ki görebilsin
            Console.WriteLine($"=== LOGIN ATTEMPT ===");
            Console.WriteLine($"User: '{kullaniciAdi}', Password: '{sifre}'");
            System.Diagnostics.Debug.WriteLine($"Login attempt - User: '{kullaniciAdi}', Password: '{sifre}'");
            
            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                Console.WriteLine("ERROR: Empty username or password");
                System.Diagnostics.Debug.WriteLine("Empty username or password");
                ModelState.AddModelError("", "Kullanıcı adı ve şifre gereklidir.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Operatör kontrolü (şimdilik basit şifre kontrolü)
            var operatorUser = await _context.Operatorler
                .FirstOrDefaultAsync(o => o.KullaniciAdi == kullaniciAdi && o.Aktif);

            Console.WriteLine($"Found operator: {operatorUser?.KullaniciAdi}, Active: {operatorUser?.Aktif}, Role: {operatorUser?.Rol}");
            System.Diagnostics.Debug.WriteLine($"Found operator: {operatorUser?.KullaniciAdi}, Active: {operatorUser?.Aktif}");
            
            var passwordValid = operatorUser != null && VerifyPassword(sifre, operatorUser);
            Console.WriteLine($"Password valid: {passwordValid}");
            System.Diagnostics.Debug.WriteLine($"Password valid: {passwordValid}");

            if (passwordValid)
            {
                Console.WriteLine("SUCCESS: Login successful, signing in user");
                System.Diagnostics.Debug.WriteLine("Login successful, signing in user");
                await SignInUser(operatorUser!);

                // Son giriş zamanını güncelle
                operatorUser!.SonGiris = DateTime.Now;
                await _context.SaveChangesAsync();

                //  DURUM YÖNETİMİ: Giriş yapınca otomatik olarak "Müsait" durumuna geç
                await _durumService.DurumDegistir(
                    operatorUser.Id, 
                    OperatorDurumu.Musait, 
                    "Sisteme giriş yapıldı", 
                    otomatikGecis: true
                );

                // Return URL varsa oraya yönlendir, yoksa role göre yönlendir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    Console.WriteLine($"Redirecting to return URL: {returnUrl}");
                    System.Diagnostics.Debug.WriteLine($"Redirecting to return URL: {returnUrl}");
                    return Redirect(returnUrl);
                }

                var redirectAction = operatorUser.Rol switch
                {
                    OperatorRolu.Admin => "Index",
                    OperatorRolu.Supervisor => "Index",
                    _ => "Index"
                };
                
                var redirectController = operatorUser.Rol switch
                {
                    OperatorRolu.Admin => "Admin",
                    OperatorRolu.Supervisor => "Admin", 
                    _ => "Operator"
                };
                
                Console.WriteLine($"SUCCESS: Redirecting to {redirectController}/{redirectAction}");
                System.Diagnostics.Debug.WriteLine($"Redirecting to {redirectController}/{redirectAction}");
                return RedirectToAction(redirectAction, redirectController);
            }

            Console.WriteLine("ERROR: Login failed - Invalid credentials");
            System.Diagnostics.Debug.WriteLine("Login failed");
            ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Çıkış işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // DURUM YÖNETİMİ: Çıkış yaparken otomatik olarak "Offline" durumuna geç
            var operatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (operatorIdClaim != null && int.TryParse(operatorIdClaim.Value, out int operatorId))
            {
                await _durumService.DurumDegistir(
                    operatorId, 
                    OperatorDurumu.Offline, 
                    "Sistemden çıkış yapıldı", 
                    otomatikGecis: true
                );
            }

            // Session'ı temizle
            HttpContext.Session.Clear();
            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Yetki yok sayfası
        public IActionResult AccessDenied()
        {
            return View();
        }


        // Kullanıcı oturum açma
        private async Task SignInUser(Operator operatorUser)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, operatorUser.Id.ToString()),
                new Claim(ClaimTypes.Name, operatorUser.TamAd),
                new Claim(ClaimTypes.Email, operatorUser.Email ?? ""),
                new Claim("KullaniciAdi", operatorUser.KullaniciAdi),
                new Claim(ClaimTypes.Role, operatorUser.Rol.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) // 8 saat oturum süresi
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Session'a OperatorId kaydet
            HttpContext.Session.SetInt32("OperatorId", operatorUser.Id);
            HttpContext.Session.SetString("OperatorAdi", operatorUser.TamAd);
            HttpContext.Session.SetString("OperatorRol", operatorUser.Rol.ToString());
            
            Console.WriteLine($"Session set - OperatorId: {operatorUser.Id}, Name: {operatorUser.TamAd}");
        }

        // Şifre doğrulama
        private bool VerifyPassword(string sifre, Operator operatorUser)
        {
            Console.WriteLine($"VerifyPassword - User: '{operatorUser.KullaniciAdi}', Password: '{sifre}'");
            System.Diagnostics.Debug.WriteLine($"VerifyPassword - User: '{operatorUser.KullaniciAdi}', Password: '{sifre}'");
            
            bool result;
            
            if (string.IsNullOrEmpty(operatorUser.Sifre))
            {
                result = operatorUser.KullaniciAdi switch
                {
                    "admin" => sifre == "admin123",
                    "supervisor" => sifre == "super123", 
                    "operator1" => sifre == "op123",
                    "operator2" => sifre == "op123",
                    "mehmet.demir" => sifre == "op123",
                    _ => sifre == "123456" 
                };
            }
            else
            {
                result = operatorUser.Sifre == sifre;
            }
            
            Console.WriteLine($"Password verification result: {result} for user '{operatorUser.KullaniciAdi}'");
            System.Diagnostics.Debug.WriteLine($"Password verification result: {result}");
            return result;
        }

        // Mevcut kullanıcı bilgisi
        public IActionResult CurrentUser()
        {
            if (!(User.Identity?.IsAuthenticated == true))
            {
                return Json(new { authenticated = false });
            }

            return Json(new
            {
                authenticated = true,
                id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                name = User.FindFirst(ClaimTypes.Name)?.Value,
                username = User.FindFirst("KullaniciAdi")?.Value,
                role = User.FindFirst(ClaimTypes.Role)?.Value,
                email = User.FindFirst(ClaimTypes.Email)?.Value
            });
        }
    }
}
