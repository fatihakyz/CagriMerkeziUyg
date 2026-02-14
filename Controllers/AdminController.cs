using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Models;
using CagriMerkeziUyg.Data;
using CagriMerkeziUyg.Services;
using Microsoft.AspNetCore.Authorization;

namespace CagriMerkeziUyg.Controllers
{
    [Authorize(Policy = "AdminOrSupervisor")]
    public class AdminController : Controller
    {
        private readonly CagriMerkeziDbContext _context;
        private readonly SimpleExcelExportService _excelService;
        private readonly OperatorDurumService _durumService;

        public AdminController(CagriMerkeziDbContext context, SimpleExcelExportService excelService, OperatorDurumService durumService)
        {
            _context = context;
            _excelService = excelService;
            _durumService = durumService;
        }

        // Admin Dashboard
        public async Task<IActionResult> Index()
        {
            // Genel istatistikler
            var toplamMusteri = await _context.Musteriler.CountAsync();
            var toplamOperator = await _context.Operatorler.Where(o => o.Aktif).CountAsync();
            var toplamAktivite = await _context.MusteriAktiviteleri.CountAsync();
            var bugunkuAktiviteler = await _context.MusteriAktiviteleri
                .Where(a => a.OlusturulmaTarihi.Date == DateTime.Today)
                .CountAsync();

            // En iyi performans gösteren operatör (bu ay)
            var baslangicTarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var enIyiOperator = await _context.MusteriAktiviteleri
                .Where(a => a.OlusturulmaTarihi >= baslangicTarihi && a.OperatorId != null)
                .GroupBy(a => a.OperatorId)
                .Select(g => new {
                    OperatorId = g.Key,
                    ToplamAktivite = g.Count(),
                    CozulenAktivite = g.Count(a => a.Durum == AktiviteDurumu.Cozumlendi)
                })
                .OrderByDescending(x => x.CozulenAktivite)
                .FirstOrDefaultAsync();

            ViewBag.ToplamMusteri = toplamMusteri;
            ViewBag.ToplamOperator = toplamOperator;
            ViewBag.ToplamAktivite = toplamAktivite;
            ViewBag.BugunkuAktiviteler = bugunkuAktiviteler;

            if (enIyiOperator != null)
            {
                var operatorBilgi = await _context.Operatorler.FindAsync(enIyiOperator.OperatorId);
                ViewBag.EnIyiOperator = operatorBilgi?.TamAd ?? "Bilinmiyor";
                ViewBag.EnIyiOperatorSkor = enIyiOperator.CozulenAktivite;
            }

            return View();
        }

        // Operatör Yönetimi
        public async Task<IActionResult> Operatorler()
        {
            var operatorler = await _context.Operatorler.ToListAsync();
            return View(operatorler);
        }

        // Yeni Operatör Ekleme
        public IActionResult OperatorEkle()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OperatorEkle(Operator operatorModel)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı adı kontrolü
                var mevcutOperator = await _context.Operatorler
                    .FirstOrDefaultAsync(o => o.KullaniciAdi == operatorModel.KullaniciAdi);
                
                if (mevcutOperator != null)
                {
                    ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı zaten alınmış.");
                    return View(operatorModel);
                }

                // Email kontrolü
                var emailKontrol = await _context.Operatorler
                    .FirstOrDefaultAsync(o => o.Email == operatorModel.Email);
                
                if (emailKontrol != null)
                {
                    ModelState.AddModelError("Email", "Bu email adresi zaten kullanılıyor.");
                    return View(operatorModel);
                }

                operatorModel.KayitTarihi = DateTime.Now;
                _context.Operatorler.Add(operatorModel);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Operatör başarıyla eklendi!";
                return RedirectToAction("Operatorler");
            }

            return View(operatorModel);
        }

        // Operatör Detay ve Düzenleme 
        public async Task<IActionResult> OperatorDetayEkle(int id)
        {
            var operatorModel = await _context.Operatorler.FindAsync(id);
            if (operatorModel == null)
            {
                TempData["Error"] = "Operatör bulunamadı.";
                return RedirectToAction("Operatorler");
            }

            return View(operatorModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OperatorDetayEkle(Operator operatorModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanıcı adı kontrolü (kendisi hariç)
                    var kullaniciAdiKontrol = await _context.Operatorler
                        .FirstOrDefaultAsync(o => o.Id != operatorModel.Id && o.KullaniciAdi == operatorModel.KullaniciAdi);
                    
                    if (kullaniciAdiKontrol != null)
                    {
                        ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı başka bir operatör tarafından kullanılıyor.");
                        return View(operatorModel);
                    }

                    // Email kontrolü (kendisi hariç)
                    var emailKontrol = await _context.Operatorler
                        .FirstOrDefaultAsync(o => o.Id != operatorModel.Id && o.Email == operatorModel.Email);
                    
                    if (emailKontrol != null)
                    {
                        ModelState.AddModelError("Email", "Bu email adresi başka bir operatör tarafından kullanılıyor.");
                        return View(operatorModel);
                    }

                    _context.Operatorler.Update(operatorModel);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Operatör bilgileri başarıyla güncellendi!";
                    return RedirectToAction("Operatorler");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Güncelleme sırasında bir hata oluştu: " + ex.Message;
                    return View(operatorModel);
                }
            }

            return View(operatorModel);
        }

        // Operatör Düzenleme
        public async Task<IActionResult> OperatorDuzenle(int id)
        {
            var operatorModel = await _context.Operatorler.FindAsync(id);
            if (operatorModel == null)
            {
                TempData["Error"] = "Operatör bulunamadı.";
                return RedirectToAction("Operatorler");
            }

            return View(operatorModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OperatorDuzenle(Operator operatorModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanıcı adı kontrolü (kendisi hariç)
                    var kullaniciAdiKontrol = await _context.Operatorler
                        .FirstOrDefaultAsync(o => o.Id != operatorModel.Id && o.KullaniciAdi == operatorModel.KullaniciAdi);
                    
                    if (kullaniciAdiKontrol != null)
                    {
                        ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı başka bir operatör tarafından kullanılıyor.");
                        return View(operatorModel);
                    }

                    // Email kontrolü (kendisi hariç)
                    var emailKontrol = await _context.Operatorler
                        .FirstOrDefaultAsync(o => o.Id != operatorModel.Id && o.Email == operatorModel.Email);
                    
                    if (emailKontrol != null)
                    {
                        ModelState.AddModelError("Email", "Bu email adresi başka bir operatör tarafından kullanılıyor.");
                        return View(operatorModel);
                    }

                    _context.Update(operatorModel);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Operatör bilgileri güncellendi!";
                    return RedirectToAction("Operatorler");
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "Güncelleme sırasında bir hata oluştu.";
                    return View(operatorModel);
                }
            }

            return View(operatorModel);
        }

        // Operatör Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OperatorSil(int id)
        {
            var operatorModel = await _context.Operatorler.FindAsync(id);
            if (operatorModel != null)
            {
                // Operatörün aktif çağrıları var mı kontrol et
                var aktifCagrilar = await _context.MusteriAktiviteleri
                    .Where(a => a.OperatorId == id && a.Durum != AktiviteDurumu.Cozumlendi && a.Durum != AktiviteDurumu.IptalEdildi)
                    .CountAsync();

                if (aktifCagrilar > 0)
                {
                    TempData["Error"] = "Bu operatörün aktif çağrıları bulunmaktadır. Önce çağrıları tamamlayınız.";
                    return RedirectToAction("Operatorler");
                }

                // Operatörü pasif yap (silme yerine)
                operatorModel.Aktif = false;
                _context.Update(operatorModel);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Operatör pasif duruma getirildi.";
            }
            else
            {
                TempData["Error"] = "Operatör bulunamadı.";
            }

            return RedirectToAction("Operatorler");
        }

        // Performans Raporları
        public async Task<IActionResult> PerformansRaporlari()
        {
            var operatorler = await _context.Operatorler
                .Where(o => o.Aktif)
                .Select(o => new { o.Id, o.TamAd })
                .ToListAsync();

            ViewBag.Operatorler = operatorler;
            return View();
        }

        // Operatör Performans Detayı (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetOperatorPerformans(int operatorId, DateTime? baslangic, DateTime? bitis)
        {
            baslangic ??= DateTime.Today.AddDays(-30);
            bitis ??= DateTime.Today;

            var aktiviteler = await _context.MusteriAktiviteleri
                .Where(a => a.OperatorId == operatorId && 
                           a.OlusturulmaTarihi >= baslangic && 
                           a.OlusturulmaTarihi <= bitis)
                .ToListAsync();

            var performans = new OperatorPerformansOzeti
            {
                OperatorId = operatorId,
                OperatorAdi = (await _context.Operatorler.FindAsync(operatorId))?.TamAd ?? "Bilinmiyor",
                BaslangicTarihi = baslangic.Value,
                BitisTarihi = bitis.Value,
                ToplamCagri = aktiviteler.Count,
                CozulenCagri = aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi),
                CozumOrani = aktiviteler.Count > 0 ? 
                    (decimal)aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi) / aktiviteler.Count * 100 : 0,
                OrtalamaCagriSuresi = aktiviteler.Where(a => a.CagriSuresi.HasValue).Any() ? 
                    (decimal)aktiviteler.Where(a => a.CagriSuresi.HasValue).Average(a => a.CagriSuresi)! : 0,
                OrtalamaMemnuniyet = aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Any() ? 
                    (decimal)aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Average(a => a.MusteriMemnuniyet)! : 0,
                GenelPerformansPuani = 0 // Bu algoritma ile hesaplanacak
            };

            // Performans puanı hesaplama (basit algoritma)
            performans.GenelPerformansPuani = CalculatePerformanceScore(performans);

            return Json(performans);
        }

        // Günlük Aktivite Raporu
        public async Task<IActionResult> GunlukRapor(DateTime? tarih)
        {
            tarih ??= DateTime.Today;

            var gunlukAktiviteler = await _context.MusteriAktiviteleri
                .Include(a => a.Operator)
                .Include(a => a.Musteri)
                .Where(a => a.OlusturulmaTarihi.Date == tarih.Value.Date)
                .OrderByDescending(a => a.OlusturulmaTarihi)
                .ToListAsync();

            ViewBag.SeciliTarih = tarih.Value;
            ViewBag.ToplamAktivite = gunlukAktiviteler.Count;
            ViewBag.CozulenAktivite = gunlukAktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi);

            return View(gunlukAktiviteler);
        }

        // Aylık Özet Raporu
        public async Task<IActionResult> AylikRapor(int? yil, int? ay)
        {
            yil ??= DateTime.Now.Year;
            ay ??= DateTime.Now.Month;

            var baslangic = new DateTime(yil.Value, ay.Value, 1);
            var bitis = baslangic.AddMonths(1).AddDays(-1);

            // Aylık aktiviteler
            var aylikAktiviteler = await _context.MusteriAktiviteleri
                .Include(a => a.Operator)
                .Where(a => a.OlusturulmaTarihi >= baslangic && a.OlusturulmaTarihi <= bitis)
                .ToListAsync();

            // Operatör performansları
            var operatorPerformans = aylikAktiviteler
                .Where(a => a.OperatorId != null)
                .GroupBy(a => a.OperatorId)
                .Select(g => new AylikRaporOperatorPerformans
                {
                    OperatorAdi = g.First().Operator?.TamAd ?? "Bilinmiyor",
                    OperatorEmail = g.First().Operator?.Email ?? "",
                    ToplamAktivite = g.Count(),
                    CozulenAktivite = g.Count(a => a.Durum == AktiviteDurumu.Cozumlendi),
                    BekleyenAktivite = g.Count(a => a.Durum == AktiviteDurumu.Beklemede),
                    IptalEdilenAktivite = g.Count(a => a.Durum == AktiviteDurumu.IptalEdildi),
                    OrtalamaCagriSuresi = g.Where(a => a.CagriSuresi.HasValue).Any() ? 
                        (decimal)g.Where(a => a.CagriSuresi.HasValue).Average(a => a.CagriSuresi.Value) : 0m,
                    MusteriMemnuniyetPuani = g.Where(a => a.MusteriMemnuniyet.HasValue).Any() ? 
                        (decimal)g.Where(a => a.MusteriMemnuniyet.HasValue).Average(a => a.MusteriMemnuniyet.Value) : 0m
                })
                .ToList();

            // Günlük aktivite dağılımı
            var gunlukAktivite = aylikAktiviteler
                .GroupBy(a => a.OlusturulmaTarihi.Day)
                .ToDictionary(g => g.Key, g => g.Count());

            // Özet istatistikler
            ViewBag.ToplamAktivite = aylikAktiviteler.Count;
            ViewBag.CozulenAktivite = aylikAktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi);
            ViewBag.BekleyenAktivite = aylikAktiviteler.Count(a => a.Durum == AktiviteDurumu.Beklemede || a.Durum == AktiviteDurumu.Islemde);
            
            ViewBag.SecilenYil = yil.Value;
            ViewBag.SecilenAy = ay.Value;
            ViewBag.AyAdi = new DateTime(yil.Value, ay.Value, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
            ViewBag.OperatorPerformans = operatorPerformans;
            ViewBag.GunlukAktivite = gunlukAktivite;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportAylikRapor(int? yil, int? ay)
        {
            yil ??= DateTime.Now.Year;
            ay ??= DateTime.Now.Month;

            var baslangic = new DateTime(yil.Value, ay.Value, 1);
            var bitis = baslangic.AddMonths(1).AddDays(-1);

            var aylikAktiviteler = await _context.MusteriAktiviteleri
                .Include(a => a.Operator)
                .Include(a => a.Musteri)
                .Where(a => a.OlusturulmaTarihi >= baslangic && a.OlusturulmaTarihi <= bitis)
                .OrderBy(a => a.OlusturulmaTarihi)
                .ToListAsync();

            var ayAdi = new DateTime(yil.Value, ay.Value, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
            var dosyaAdi = $"Aylik_Rapor_{ayAdi.Replace(" ", "_")}.xlsx";

            var excelData = _excelService.ExportAylikRapor(aylikAktiviteler, ayAdi);
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", dosyaAdi);
        }

        // Performans puanı hesaplama algoritması
        private decimal CalculatePerformanceScore(OperatorPerformansOzeti performans)
        {
            decimal skor = 0;

            // Çözüm oranı (%40 ağırlık)
            skor += performans.CozumOrani * 0.4m;

            // Müşteri memnuniyeti (%30 ağırlık) - 5 üzerinden 100'e çevir
            if (performans.OrtalamaMemnuniyet > 0)
            {
                skor += (performans.OrtalamaMemnuniyet / 5 * 100) * 0.3m;
            }

            // Çağrı sayısı (%20 ağırlık) - 50 çağrı ve üzeri tam puan
            var cagriPuani = Math.Min(performans.ToplamCagri / 50m * 100, 100);
            skor += cagriPuani * 0.2m;

            // Ortalama çağrı süresi (%10 ağırlık) - 10 dk ve altı tam puan
            if (performans.OrtalamaCagriSuresi > 0)
            {
                var surePuani = Math.Max(100 - ((performans.OrtalamaCagriSuresi - 10) * 5), 0);
                skor += surePuani * 0.1m;
            }

            return Math.Round(skor, 2);
        }

        // Excel Export - Performans Raporu
        [HttpGet]
        public async Task<IActionResult> ExportPerformansRaporu(int? operatorId, DateTime? baslangic, DateTime? bitis)
        {
            baslangic ??= DateTime.Today.AddDays(-30);
            bitis ??= DateTime.Today;

            var performansListesi = new List<OperatorPerformansOzeti>();

            if (operatorId.HasValue)
            {
                // Tek operatör
                var aktiviteler = await _context.MusteriAktiviteleri
                    .Where(a => a.OperatorId == operatorId && 
                               a.OlusturulmaTarihi >= baslangic && 
                               a.OlusturulmaTarihi <= bitis)
                    .ToListAsync();

                var operatorBilgi = await _context.Operatorler.FindAsync(operatorId.Value);
                if (operatorBilgi != null)
                {
                    var performans = new OperatorPerformansOzeti
                    {
                        OperatorId = operatorId.Value,
                        OperatorAdi = operatorBilgi.TamAd,
                        BaslangicTarihi = baslangic.Value,
                        BitisTarihi = bitis.Value,
                        ToplamCagri = aktiviteler.Count,
                        CozulenCagri = aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi),
                        CozumOrani = aktiviteler.Count > 0 ? 
                            (decimal)aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi) / aktiviteler.Count * 100 : 0,
                        OrtalamaCagriSuresi = aktiviteler.Where(a => a.CagriSuresi.HasValue).Any() ? 
                            (decimal)aktiviteler.Where(a => a.CagriSuresi.HasValue).Average(a => a.CagriSuresi)! : 0,
                        OrtalamaMemnuniyet = aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Any() ? 
                            (decimal)aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Average(a => a.MusteriMemnuniyet)! : 0
                    };
                    performans.GenelPerformansPuani = CalculatePerformanceScore(performans);
                    performansListesi.Add(performans);
                }
            }
            else
            {
                // Tüm operatörler
                var tumOperatorler = await _context.Operatorler.Where(o => o.Aktif).ToListAsync();
                foreach (var op in tumOperatorler)
                {
                    var aktiviteler = await _context.MusteriAktiviteleri
                        .Where(a => a.OperatorId == op.Id && 
                                   a.OlusturulmaTarihi >= baslangic && 
                                   a.OlusturulmaTarihi <= bitis)
                        .ToListAsync();

                    var performans = new OperatorPerformansOzeti
                    {
                        OperatorId = op.Id,
                        OperatorAdi = op.TamAd,
                        BaslangicTarihi = baslangic.Value,
                        BitisTarihi = bitis.Value,
                        ToplamCagri = aktiviteler.Count,
                        CozulenCagri = aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi),
                        CozumOrani = aktiviteler.Count > 0 ? 
                            (decimal)aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi) / aktiviteler.Count * 100 : 0,
                        OrtalamaCagriSuresi = aktiviteler.Where(a => a.CagriSuresi.HasValue).Any() ? 
                            (decimal)aktiviteler.Where(a => a.CagriSuresi.HasValue).Average(a => a.CagriSuresi)! : 0,
                        OrtalamaMemnuniyet = aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Any() ? 
                            (decimal)aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Average(a => a.MusteriMemnuniyet)! : 0
                    };
                    performans.GenelPerformansPuani = CalculatePerformanceScore(performans);
                    performansListesi.Add(performans);
                }
            }

            var excelData = _excelService.ExportPerformansRaporu(performansListesi, "Performans Raporu");
            var fileName = $"PerformansRaporu_{baslangic.Value:yyyyMMdd}_{bitis.Value:yyyyMMdd}.xlsx";
            
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Excel Export - Günlük Rapor
        [HttpGet]
        public async Task<IActionResult> ExportGunlukRapor(DateTime? tarih)
        {
            tarih ??= DateTime.Today;

            var gunlukAktiviteler = await _context.MusteriAktiviteleri
                .Include(a => a.Operator)
                .Include(a => a.Musteri)
                .Where(a => a.OlusturulmaTarihi.Date == tarih.Value.Date)
                .OrderByDescending(a => a.OlusturulmaTarihi)
                .ToListAsync();

            var excelData = _excelService.ExportGunlukRapor(gunlukAktiviteler, tarih.Value);
            var fileName = $"GunlukRapor_{tarih.Value:yyyyMMdd}.xlsx";
            
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Excel Export - Operatör Listesi
        [HttpGet]
        public async Task<IActionResult> ExportOperatorListesi()
        {
            var operatorler = await _context.Operatorler.ToListAsync();
            var excelData = _excelService.ExportOperatorListesi(operatorler);
            var fileName = $"OperatorListesi_{DateTime.Today:yyyyMMdd}.xlsx";
            
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // ============================================
        // ŞABLON YÖNETİMİ
        // ============================================

        /// <summary>
        /// Şablon yönetimi ana sayfası
        /// </summary>
        public async Task<IActionResult> SablonYonetimi()
        {
            // İstatistikler
            var toplamKategori = await _context.CevapSablonKategorileri.CountAsync();
            var toplamSablon = await _context.CevapSablonlari.CountAsync();
            var aktifSablon = await _context.CevapSablonlari.Where(s => s.Aktif).CountAsync();
            var toplamKullanim = await _context.CevapSablonKullanimlar.CountAsync();

            // En çok kullanılan şablonlar
            var enCokKullanilanSablonlar = await _context.CevapSablonlari
                .Include(s => s.Kategori)
                .OrderByDescending(s => s.KullanimSayisi)
                .Take(5)
                .Select(s => new {
                    s.Id,
                    s.Baslik,
                    KategoriAd = s.Kategori.Ad,
                    s.KullanimSayisi,
                    s.Aktif
                })
                .ToListAsync();

            ViewBag.ToplamKategori = toplamKategori;
            ViewBag.ToplamSablon = toplamSablon;
            ViewBag.AktifSablon = aktifSablon;
            ViewBag.ToplamKullanim = toplamKullanim;
            ViewBag.EnCokKullanilanSablonlar = enCokKullanilanSablonlar;

            return View();
        }

        /// <summary>
        /// Kategori listesi (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetKategoriler()
        {
            try
            {
                var kategoriler = await _context.CevapSablonKategorileri
                    .OrderBy(k => k.Sira)
                    .Select(k => new
                    {
                        k.Id,
                        k.Ad,
                        k.Aciklama,
                        k.IconClass,
                        k.RenkKodu,
                        k.Sira,
                        k.Aktif,
                        k.AktiviteTuru,
                        SablonSayisi = k.Sablonlar.Count(),
                        k.OlusturulmaTarihi
                    })
                    .ToListAsync();

                return Json(new { success = true, data = kategoriler });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategoriler yüklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Şablon listesi (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSablonlar(int? kategoriId)
        {
            try
            {
                var query = _context.CevapSablonlari
                    .Include(s => s.Kategori)
                    .Include(s => s.OlusturanOperator)
                    .AsQueryable();

                if (kategoriId.HasValue)
                {
                    query = query.Where(s => s.KategoriId == kategoriId.Value);
                }

                var sablonlar = await query
                    .OrderBy(s => s.Kategori.Sira)
                    .ThenBy(s => s.Sira)
                    .Select(s => new
                    {
                        s.Id,
                        s.KategoriId,
                        KategoriAd = s.Kategori.Ad,
                        KategoriRenk = s.Kategori.RenkKodu,
                        s.Baslik,
                        s.Icerik,
                        s.Notlar,
                        s.Sira,
                        s.Aktif,
                        s.KullanimSayisi,
                        s.SonKullanimTarihi,
                        s.DegiskenIceriyor,
                        s.KisaYol,
                        s.OlusturulmaTarihi,
                        OlusturanAd = s.OlusturanOperator != null ? s.OlusturanOperator.TamAd : "Sistem"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = sablonlar });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Şablonlar yüklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Kategori ekleme (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KategoriEkle(CevapSablonKategori kategori)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(kategori.Ad))
                {
                    return Json(new { success = false, message = "Kategori adı zorunludur" });
                }

                // Varsayılan değerler
                kategori.OlusturulmaTarihi = DateTime.Now;
                if (string.IsNullOrWhiteSpace(kategori.RenkKodu))
                {
                    kategori.RenkKodu = "#007bff";
                }

                // Sıra numarası ata (en son + 1)
                var maxSira = await _context.CevapSablonKategorileri.AnyAsync() 
                    ? await _context.CevapSablonKategorileri.MaxAsync(k => k.Sira) 
                    : 0;
                kategori.Sira = maxSira + 1;

                _context.CevapSablonKategorileri.Add(kategori);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Kategori başarıyla eklendi", kategoriId = kategori.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategori eklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Kategori güncelleme (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KategoriGuncelle(CevapSablonKategori kategori)
        {
            try
            {
                var mevcutKategori = await _context.CevapSablonKategorileri.FindAsync(kategori.Id);
                if (mevcutKategori == null)
                {
                    return Json(new { success = false, message = "Kategori bulunamadı" });
                }

                mevcutKategori.Ad = kategori.Ad;
                mevcutKategori.Aciklama = kategori.Aciklama;
                mevcutKategori.IconClass = kategori.IconClass;
                mevcutKategori.RenkKodu = kategori.RenkKodu;
                mevcutKategori.Sira = kategori.Sira;
                mevcutKategori.Aktif = kategori.Aktif;
                mevcutKategori.AktiviteTuru = kategori.AktiviteTuru;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Kategori başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategori güncellenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Kategori silme (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KategoriSil(int id)
        {
            try
            {
                var kategori = await _context.CevapSablonKategorileri
                    .Include(k => k.Sablonlar)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (kategori == null)
                {
                    return Json(new { success = false, message = "Kategori bulunamadı" });
                }

                if (kategori.Sablonlar.Any())
                {
                    return Json(new { success = false, message = "Bu kategoriye ait şablonlar var. Önce şablonları silin veya taşıyın." });
                }

                _context.CevapSablonKategorileri.Remove(kategori);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Kategori başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategori silinirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Şablon ekleme (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SablonEkle(CevapSablonu sablon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sablon.Baslik) || string.IsNullOrWhiteSpace(sablon.Icerik))
                {
                    return Json(new { success = false, message = "Başlık ve içerik zorunludur" });
                }

                // Varsayılan değerler
                sablon.OlusturulmaTarihi = DateTime.Now;
                sablon.KullanimSayisi = 0;
                sablon.Aktif = true;

                // Sıra numarası ata (kategorideki son + 1)
                var maxSira = await _context.CevapSablonlari
                    .Where(s => s.KategoriId == sablon.KategoriId)
                    .AnyAsync()
                    ? await _context.CevapSablonlari
                        .Where(s => s.KategoriId == sablon.KategoriId)
                        .MaxAsync(s => s.Sira)
                    : 0;
                sablon.Sira = maxSira + 1;

                // Değişken kontrolü
                sablon.DegiskenIceriyor = sablon.Icerik.Contains("{") && sablon.Icerik.Contains("}");

                _context.CevapSablonlari.Add(sablon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Şablon başarıyla eklendi", sablonId = sablon.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Şablon eklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Şablon güncelleme (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SablonGuncelle(CevapSablonu sablon)
        {
            try
            {
                var mevcutSablon = await _context.CevapSablonlari.FindAsync(sablon.Id);
                if (mevcutSablon == null)
                {
                    return Json(new { success = false, message = "Şablon bulunamadı" });
                }

                mevcutSablon.KategoriId = sablon.KategoriId;
                mevcutSablon.Baslik = sablon.Baslik;
                mevcutSablon.Icerik = sablon.Icerik;
                mevcutSablon.Notlar = sablon.Notlar;
                mevcutSablon.Sira = sablon.Sira;
                mevcutSablon.Aktif = sablon.Aktif;
                mevcutSablon.KisaYol = sablon.KisaYol;
                mevcutSablon.DegiskenIceriyor = sablon.Icerik.Contains("{") && sablon.Icerik.Contains("}");

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Şablon başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Şablon güncellenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Şablon silme (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SablonSil(int id)
        {
            try
            {
                var sablon = await _context.CevapSablonlari.FindAsync(id);
                if (sablon == null)
                {
                    return Json(new { success = false, message = "Şablon bulunamadı" });
                }

                _context.CevapSablonlari.Remove(sablon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Şablon başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Şablon silinirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Şablon kullanım istatistikleri (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSablonIstatistikleri(DateTime? baslangic, DateTime? bitis)
        {
            try
            {
                baslangic ??= DateTime.Today.AddMonths(-1);
                bitis ??= DateTime.Today.AddDays(1);

                var kullanimlar = await _context.CevapSablonKullanimlar
                    .Include(k => k.Sablon)
                        .ThenInclude(s => s.Kategori)
                    .Include(k => k.Operator)
                    .Where(k => k.KullanimTarihi >= baslangic && k.KullanimTarihi < bitis)
                    .ToListAsync();

                // Şablon bazında istatistikler
                var sablonIstatistik = kullanimlar
                    .GroupBy(k => k.SablonId)
                    .Select(g => new
                    {
                        SablonId = g.Key,
                        SablonBaslik = g.First().Sablon.Baslik,
                        KategoriAd = g.First().Sablon.Kategori.Ad,
                        KullanimSayisi = g.Count(),
                        FarkliOperatorSayisi = g.Select(x => x.OperatorId).Distinct().Count(),
                        SonKullanim = g.Max(x => x.KullanimTarihi)
                    })
                    .OrderByDescending(x => x.KullanimSayisi)
                    .ToList();

                // Kategori bazında istatistikler
                var kategoriIstatistik = kullanimlar
                    .GroupBy(k => k.Sablon.KategoriId)
                    .Select(g => new
                    {
                        KategoriId = g.Key,
                        KategoriAd = g.First().Sablon.Kategori.Ad,
                        KullanimSayisi = g.Count()
                    })
                    .OrderByDescending(x => x.KullanimSayisi)
                    .ToList();

                // Operatör bazında istatistikler
                var operatorIstatistik = kullanimlar
                    .GroupBy(k => k.OperatorId)
                    .Select(g => new
                    {
                        OperatorId = g.Key,
                        OperatorAd = g.First().Operator.TamAd,
                        KullanimSayisi = g.Count(),
                        FarkliSablonSayisi = g.Select(x => x.SablonId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.KullanimSayisi)
                    .ToList();

                return Json(new
                {
                    success = true,
                    toplamKullanim = kullanimlar.Count,
                    sablonIstatistik = sablonIstatistik,
                    kategoriIstatistik = kategoriIstatistik,
                    operatorIstatistik = operatorIstatistik,
                    baslangicTarihi = baslangic,
                    bitisTarihi = bitis
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İstatistikler yüklenirken hata oluştu: " + ex.Message });
            }
        }

        // ============================================
        // DURUM YÖNETİMİ - SÜPERVIZÖR ÖZELLİKLERİ
        // ============================================

        /// <summary>
        /// Canlı operatör durumu dashboard'u
        /// </summary>
        public IActionResult CanliDurum()
        {
            return View();
        }

        /// <summary>
        /// Tüm operatörlerin anlık durumunu getir (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTumOperatorDurumlari()
        {
            try
            {
                var durumlar = await _durumService.TumOperatorDurumlariniGetir();

                // Durum bazında gruplama
                var durumGrupları = durumlar.GroupBy(d => d.MevcutDurum)
                    .Select(g => new
                    {
                        durum = g.Key.ToString(),
                        sayi = g.Count(),
                        operatorler = g.Select(o => o.TamAd).ToList()
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    toplamOperator = durumlar.Count,
                    operatorler = durumlar.Select(d => new
                    {
                        id = d.OperatorId,
                        ad = d.Ad,
                        soyad = d.Soyad,
                        tamAd = d.TamAd,
                        mevcutDurum = d.MevcutDurum.ToString(),
                        durumRengi = d.DurumRengi,
                        durumIkon = d.DurumIkon,
                        sonDegisiklik = d.SonDurumDegisikliği,
                        durumSuresi = Math.Round(d.DurumSuresi, 1),
                        durumNotu = d.DurumNotu
                    }),
                    durumGrupları = durumGrupları
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Durumlar yüklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Uzun süreli durumda kalan operatör uyarıları (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDurumUyarilari()
        {
            try
            {
                var uyarilar = await _durumService.UzunSureliDurumKontrol();

                return Json(new
                {
                    success = true,
                    uyariSayisi = uyarilar.Count,
                    uyarilar = uyarilar.Select(u => new
                    {
                        operatorId = u.OperatorId,
                        operatorAdi = u.OperatorAdi,
                        mevcutDurum = u.MevcutDurum.ToString(),
                        gecenSure = Math.Round(u.GecenSure, 1),
                        uyariMesaji = u.UyariMesaji,
                        oncelikSeviyesi = u.OncelikSeviyesi
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Uyarılar yüklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Operatörün durum geçmişini görüntüle
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OperatorDurumGecmisi(int operatorId, DateTime? tarih)
        {
            try
            {
                tarih ??= DateTime.Today;

                var operatorEntity = await _context.Operatorler.FindAsync(operatorId);
                if (operatorEntity == null)
                {
                    return Json(new { success = false, message = "Operatör bulunamadı" });
                }

                var gecmis = await _context.OperatorDurumGecmisleri
                    .Where(d => d.OperatorId == operatorId && d.GecisZamani.Date == tarih.Value.Date)
                    .OrderBy(d => d.GecisZamani)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    operatorAdi = operatorEntity.TamAd,
                    tarih = tarih.Value.ToString("dd.MM.yyyy"),
                    gecmis = gecmis.Select(g => new
                    {
                        id = g.Id,
                        oncekiDurum = g.OncekiDurum.ToString(),
                        yeniDurum = g.YeniDurum.ToString(),
                        gecisZamani = g.GecisZamani.ToString("HH:mm:ss"),
                        bitisZamani = g.BitisZamani?.ToString("HH:mm:ss"),
                        sureDakika = g.SureDakika.HasValue ? (decimal?)Math.Round(g.SureDakika.Value, 1) : null,
                        not = g.Not,
                        otomatikGecis = g.OtomatikGecis
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Geçmiş yüklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Operatörlerin günlük durum özetlerini getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GunlukDurumOzetleri(DateTime? tarih)
        {
            try
            {
                tarih ??= DateTime.Today;

                var ozetler = await _context.OperatorGunlukDurumOzetleri
                    .Include(o => o.Operator)
                    .Where(o => o.Tarih.Date == tarih.Value.Date)
                    .OrderByDescending(o => o.VerimlilıkOrani)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    tarih = tarih.Value.ToString("dd.MM.yyyy"),
                    ozetler = ozetler.Select(o => new
                    {
                        operatorId = o.OperatorId,
                        operatorAdi = o.Operator?.TamAd,
                        toplamCalismaSuresi = Math.Round(o.ToplamCalismaSuresi, 1),
                        cagrıdaGecenSure = Math.Round(o.CagrıdaGecenSure, 1),
                        araCalismaSuresi = Math.Round(o.AraCalismaSuresi, 1),
                        musaitSure = Math.Round(o.MusaitSure, 1),
                        molaSuresi = Math.Round(o.MolaSuresi, 1),
                        toplamCagriSayisi = o.ToplamCagriSayisi,
                        ortalamaCagriSuresi = o.OrtalamaCagriSuresi.HasValue ? Math.Round(o.OrtalamaCagriSuresi.Value, 1) : 0,
                        verimlilıkOrani = Math.Round(o.VerimlilıkOrani, 1),
                        kullanimOrani = Math.Round(o.KullanimOrani, 1)
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Özetler yüklenirken hata oluştu: " + ex.Message });
            }
        }

        /// <summary>
        /// Operatörün durumunu süpervizör tarafından değiştir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OperatorDurumDegistir(int operatorId, OperatorDurumu yeniDurum, string? not)
        {
            try
            {
                var result = await _durumService.DurumDegistir(
                    operatorId,
                    yeniDurum,
                    not: $"Süpervizör tarafından değiştirildi: {not}",
                    otomatikGecis: false
                );

                return Json(new
                {
                    success = result.success,
                    message = result.message,
                    yeniDurum = yeniDurum.ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Durum değiştirilemedi: " + ex.Message });
            }
        }
    }
}
