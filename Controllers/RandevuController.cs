using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Data;
using CagriMerkeziUyg.Models;

namespace CagriMerkeziUyg.Controllers
{
    public class RandevuController : Controller
    {
        private readonly CagriMerkeziDbContext _context;

        public RandevuController(CagriMerkeziDbContext context)
        {
            _context = context;
        }

        // GET: Randevu (Takvim Görünümü)
        public async Task<IActionResult> Index()
        {
            // Operatör listesi
            ViewBag.Operatorler = await _context.Operatorler
                .Where(o => o.Aktif)
                .OrderBy(o => o.Ad)
                .Select(o => new { o.Id, o.TamAd })
                .ToListAsync();

            return View();
        }

        // GET: Randevu/Detay/5
        public async Task<IActionResult> Detay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Musteri)
                .Include(r => r.Operator)
                .Include(r => r.OlusturanOperator)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // GET: Randevu/Olustur
        public async Task<IActionResult> Olustur(int? musteriId)
        {
            await ViewBagYukle();
            
            var model = new Randevu
            {
                RandevuZamani = DateTime.Now.AddHours(1), // 1 saat sonrası
                BitisZamani = DateTime.Now.AddHours(2),   // 2 saat sonrası
                MusteriId = musteriId
            };

            // Eğer müşteri ID'si varsa müşteri bilgisini yükle
            if (musteriId.HasValue)
            {
                var musteri = await _context.Musteriler.FindAsync(musteriId.Value);
                if (musteri != null)
                {
                    ViewBag.MusteriAdi = musteri.TamAd;
                }
            }

            return View(model);
        }

        // POST: Randevu/Olustur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur([Bind("Baslik,Aciklama,RandevuZamani,BitisZamani,Tip,Durum,Oncelik,MusteriId,OperatorId,HatirlatmaAktif,HatirlatmaSuresi")] Randevu randevu)
        {
            // Geçmiş tarih kontrolü
            if (randevu.RandevuZamani <= DateTime.Now)
            {
                ModelState.AddModelError("RandevuZamani", "Randevu tarihi geçmiş bir tarih olamaz! Lütfen gelecek bir tarih seçiniz.");
            }

            // Bitiş zamanı kontrolü
            if (randevu.BitisZamani.HasValue && randevu.BitisZamani.Value <= randevu.RandevuZamani)
            {
                ModelState.AddModelError("BitisZamani", "Bitiş zamanı, başlangıç zamanından sonra olmalıdır!");
            }

            if (ModelState.IsValid)
            {
                // Operatör bilgisini Claims'den veya Session'dan al
                randevu.OlusturanOperatorId = GetCurrentOperatorId();
                randevu.OlusturulmaTarihi = DateTime.Now;
                randevu.SonGuncelleme = DateTime.Now;

                // Çakışma kontrolü
                var cakismaVar = await CakismaKontrolu(randevu);
                if (cakismaVar)
                {
                    ModelState.AddModelError("", "Seçilen operatör bu saatte başka bir randevusu var!");
                    await ViewBagYukle();
                    return View(randevu);
                }

                _context.Add(randevu);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Randevu başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Index));
            }
            await ViewBagYukle();
            return View(randevu);
        }

        // GET: Randevu/Duzenle/5
        public async Task<IActionResult> Duzenle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            await ViewBagYukle();
            return View(randevu);
        }

        // POST: Randevu/Duzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, [Bind("Id,Baslik,Aciklama,RandevuZamani,BitisZamani,Tip,Durum,Oncelik,MusteriId,OperatorId,HatirlatmaAktif,HatirlatmaSuresi,TamamlanmaNotu,OlusturanOperatorId,OlusturulmaTarihi")] Randevu randevu)
        {
            if (id != randevu.Id)
            {
                return NotFound();
            }

            // Geçmiş tarih kontrolü (sadece durumu Bekliyor ise)
            if (randevu.Durum == RandevuDurumu.Bekliyor && randevu.RandevuZamani <= DateTime.Now)
            {
                ModelState.AddModelError("RandevuZamani", "Bekleyen randevu için gelecek bir tarih seçmelisiniz!");
            }

            // Bitiş zamanı kontrolü
            if (randevu.BitisZamani.HasValue && randevu.BitisZamani.Value <= randevu.RandevuZamani)
            {
                ModelState.AddModelError("BitisZamani", "Bitiş zamanı, başlangıç zamanından sonra olmalıdır!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Çakışma kontrolü (kendisi hariç)
                    var cakismaVar = await CakismaKontrolu(randevu, id);
                    if (cakismaVar)
                    {
                        ModelState.AddModelError("", "Seçilen operatör bu saatte başka bir randevusu var!");
                        await ViewBagYukle();
                        return View(randevu);
                    }

                    // Eğer durum tamamlandı olarak değiştiyse
                    if (randevu.Durum == RandevuDurumu.Tamamlandi && !randevu.TamamlanmaTarihi.HasValue)
                    {
                        randevu.TamamlanmaTarihi = DateTime.Now;
                    }

                    randevu.SonGuncelleme = DateTime.Now;
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                    TempData["Mesaj"] = "Randevu başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            await ViewBagYukle();
            return View(randevu);
        }

        // POST: Randevu/Sil/5
        [HttpPost, ActionName("Sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SilOnay(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Randevu başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: API endpoint - Takvim için JSON data
        [HttpGet]
        public async Task<IActionResult> GetRandevular(DateTime? start, DateTime? end, int? operatorId)
        {
            var query = _context.Randevular
                .Include(r => r.Musteri)
                .Include(r => r.Operator)
                .AsQueryable();

            // Tarih filtresi
            if (start.HasValue)
            {
                query = query.Where(r => r.RandevuZamani >= start.Value);
            }

            if (end.HasValue)
            {
                query = query.Where(r => r.RandevuZamani <= end.Value);
            }

            // Operatör filtresi
            if (operatorId.HasValue && operatorId.Value > 0)
            {
                query = query.Where(r => r.OperatorId == operatorId.Value);
            }

            var randevular = await query
                .OrderBy(r => r.RandevuZamani)
                .Select(r => new
                {
                    id = r.Id,
                    title = r.Baslik,
                    start = r.RandevuZamani.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.BitisZamani.HasValue ? r.BitisZamani.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    color = r.RenkKodu,
                    description = r.Aciklama,
                    tip = r.Tip.ToString(),
                    durum = r.Durum.ToString(),
                    oncelik = r.Oncelik.ToString(),
                    musteriAdi = r.Musteri != null ? r.Musteri.TamAd : null,
                    operatorAdi = r.Operator != null ? r.Operator.TamAd : null,
                    url = Url.Action("Detay", new { id = r.Id })
                })
                .ToListAsync();

            return Json(randevular);
        }

        // Hızlı durum güncelleme
        [HttpPost]
        public async Task<IActionResult> DurumGuncelle(int id, RandevuDurumu durum)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            randevu.Durum = durum;
            randevu.SonGuncelleme = DateTime.Now;

            if (durum == RandevuDurumu.Tamamlandi && !randevu.TamamlanmaTarihi.HasValue)
            {
                randevu.TamamlanmaTarihi = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Durum güncellendi!" });
        }

        // Bugünün randevuları
        public async Task<IActionResult> BugunRandevular()
        {
            var bugun = DateTime.Today;
            var yarin = bugun.AddDays(1);

            var randevular = await _context.Randevular
                .Include(r => r.Musteri)
                .Include(r => r.Operator)
                .Where(r => r.RandevuZamani >= bugun && r.RandevuZamani < yarin)
                .OrderBy(r => r.RandevuZamani)
                .ToListAsync();

            return View(randevular);
        }

        // Yaklaşan randevular
        public async Task<IActionResult> YaklasanRandevular()
        {
            var simdi = DateTime.Now;
            var birSaatSonra = simdi.AddHours(1);

            var randevular = await _context.Randevular
                .Include(r => r.Musteri)
                .Include(r => r.Operator)
                .Where(r => r.RandevuZamani >= simdi && r.RandevuZamani <= birSaatSonra && r.Durum == RandevuDurumu.Bekliyor)
                .OrderBy(r => r.RandevuZamani)
                .ToListAsync();

            return View(randevular);
        }

        // Operatör randevuları
        public async Task<IActionResult> OperatorRandevulari(int? operatorId)
        {
            if (!operatorId.HasValue)
            {
                operatorId = GetCurrentOperatorId();
            }

            var operator_ = await _context.Operatorler.FindAsync(operatorId);
            ViewBag.OperatorAdi = operator_?.TamAd;

            var randevular = await _context.Randevular
                .Include(r => r.Musteri)
                .Include(r => r.Operator)
                .Where(r => r.OperatorId == operatorId && r.RandevuZamani >= DateTime.Today)
                .OrderBy(r => r.RandevuZamani)
                .ToListAsync();

            return View(randevular);
        }

        // Mevcut operatör ID'sini al
        private int GetCurrentOperatorId()
        {
            // Önce Claims'den dene
            var operatorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(operatorIdClaim) && int.TryParse(operatorIdClaim, out int operatorId))
            {
                return operatorId;
            }

            // Claims'de yoksa Session'dan dene
            var sessionOperatorId = HttpContext.Session.GetInt32("OperatorId");
            if (sessionOperatorId.HasValue)
            {
                return sessionOperatorId.Value;
            }

            // Her ikisi de yoksa varsayılan değer (geliştirme için)
            return 1;
        }

        // API: Müşteri Arama (AJAX için)
        [HttpGet]
        public async Task<IActionResult> MusteriAra(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var musteriler = await _context.Musteriler
                .Where(m => 
                    m.Ad.Contains(term) || 
                    m.Soyad.Contains(term) || 
                    m.TelefonNo.Contains(term) ||
                    m.Email.Contains(term)
                )
                .OrderBy(m => m.Ad)
                .Take(20) // En fazla 20 sonuç
                .Select(m => new
                {
                    id = m.Id,
                    text = $"{m.TamAd} - {m.TelefonNo}",
                    telefon = m.TelefonNo,
                    email = m.Email
                })
                .ToListAsync();

            return Json(musteriler);
        }

        // Yardımcı metodlar
        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.Id == id);
        }

        private async Task<bool> CakismaKontrolu(Randevu randevu, int? haricId = null)
        {
            var baslangic = randevu.RandevuZamani;
            var bitis = randevu.BitisZamani ?? randevu.RandevuZamani.AddMinutes(30); // Varsayılan 30 dk

            var query = _context.Randevular
                .Where(r => r.OperatorId == randevu.OperatorId && r.Durum != RandevuDurumu.Iptal);

            if (haricId.HasValue)
            {
                query = query.Where(r => r.Id != haricId.Value);
            }

            var cakisanRandevu = await query
                .Where(r =>
                    (r.RandevuZamani >= baslangic && r.RandevuZamani < bitis) ||
                    (r.BitisZamani.HasValue && r.BitisZamani.Value > baslangic && r.BitisZamani.Value <= bitis) ||
                    (r.RandevuZamani <= baslangic && r.BitisZamani.HasValue && r.BitisZamani.Value >= bitis)
                )
                .AnyAsync();

            return cakisanRandevu;
        }

        private async Task ViewBagYukle()
        {
            ViewBag.Operatorler = new SelectList(
                await _context.Operatorler.Where(o => o.Aktif).OrderBy(o => o.Ad).ToListAsync(),
                "Id", "TamAd"
            );

            ViewBag.Musteriler = new SelectList(
                await _context.Musteriler.OrderBy(m => m.Ad).ToListAsync(),
                "Id", "TamAd"
            );

            ViewBag.RandevuTipleri = Enum.GetValues(typeof(RandevuTipi))
                .Cast<RandevuTipi>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString()
                });

            ViewBag.RandevuDurumlari = Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(d => new SelectListItem
                {
                    Value = d.ToString(),
                    Text = d.ToString()
                });

            ViewBag.RandevuOncelikleri = Enum.GetValues(typeof(RandevuOncelik))
                .Cast<RandevuOncelik>()
                .Select(o => new SelectListItem
                {
                    Value = o.ToString(),
                    Text = o.ToString()
                });
        }
    }
}


