using System.Diagnostics;
using CagriMerkeziUyg.Models;
using CagriMerkeziUyg.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CagriMerkeziUyg.Controllers
{
    [Authorize(Policy = "AllOperators")]
    public class HomeController(ILogger<HomeController> logger, CagriMerkeziDbContext context) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly CagriMerkeziDbContext _context = context;

        public async Task<IActionResult> Index()
        {
            // Debug için
            System.Diagnostics.Debug.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"User name: {User.Identity?.Name}");
            
            // Eğer kullanıcı giriş yapmamışsa login sayfasına yönlendir
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            // Temel istatistikleri hesapla
            var toplamMusteri = await _context.Musteriler.CountAsync();
            var bugunkuKayitlar = await _context.Musteriler
                .Where(m => m.KayitTarihi.Date == DateTime.Today)
                .CountAsync();

            // Aktivite istatistikleri
            var toplamAktivite = await _context.MusteriAktiviteleri.CountAsync();
            var bugunkuAktiviteler = await _context.MusteriAktiviteleri
                .Where(a => a.OlusturulmaTarihi.Date == DateTime.Today)
                .CountAsync();

            // Aktif müşteri sayısı (Son 30 gün içinde aktivitesi olan)
            var aktifMusteri = await _context.Musteriler
                .Where(m => m.Aktiviteler.Any(a => a.OlusturulmaTarihi >= DateTime.Now.AddDays(-30)))
                .CountAsync();

            // ViewBag ile view'a gönder
            ViewBag.ToplamMusteri = toplamMusteri;
            ViewBag.BugunkuKayit = bugunkuKayitlar;
            ViewBag.ToplamAktivite = toplamAktivite;
            ViewBag.BugunkuAktiviteler = bugunkuAktiviteler;
            ViewBag.AktifMusteri = aktifMusteri;
            
            // View'da eksik olan değişkenler
            ViewBag.BugunkuAramalar = bugunkuAktiviteler; // Arama = Aktivite
            ViewBag.CozulenTalepler = await _context.MusteriAktiviteleri
                .Where(a => a.Durum == AktiviteDurumu.Cozumlendi && a.OlusturulmaTarihi.Date == DateTime.Today)
                .CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
