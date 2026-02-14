using Microsoft.AspNetCore.Mvc;
using CagriMerkeziUyg.Data;
using CagriMerkeziUyg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CagriMerkeziUyg.Controllers
{
    [Authorize(Policy = "AllOperators")]
    public class TelefonController(CagriMerkeziDbContext context) : Controller
    {
        private readonly CagriMerkeziDbContext _context = context;

        // Debug: Arama kayıtlarını kontrol et
        [HttpGet]
        public async Task<IActionResult> DebugAramalar()
        {
            var aramalar = await _context.AramaLoglari
                .Include(a => a.Musteri)
                .Include(a => a.Operator)
                .OrderByDescending(a => a.BaslangicZamani)
                .Take(10)
                .ToListAsync();
            
            return Json(new { 
                count = aramalar.Count,
                data = aramalar.Select(a => new {
                    Id = a.Id,
                    TelefonNo = a.TelefonNo,
                    OperatorAdi = a.Operator?.TamAd,
                    BaslangicZamani = a.BaslangicZamani,
                    Durum = a.DurumMetni,
                    Tip = a.TipMetni,
                    Notlar = a.Notlar
                })
            });
        }

        // Giden arama başlatma simülasyonu
        [HttpPost]
        public async Task<IActionResult> AramaBaslat(string telefonNo, int? musteriId, int operatorId)
        {
            Console.WriteLine($"=== ARAMA BASLAT === TelefonNo: {telefonNo}, MusteriId: {musteriId}, OperatorId: {operatorId}");
            
            // Validation
            if (string.IsNullOrEmpty(telefonNo))
            {
                Console.WriteLine("ERROR: Telefon numarası boş");
                return Json(new { success = false, message = "Telefon numarası gereklidir." });
            }

            if (operatorId <= 0)
            {
                Console.WriteLine("ERROR: Geçersiz operatör ID");
                return Json(new { success = false, message = "Geçersiz operatör ID." });
            }

            try
            {
                var aramaLog = new AramaLog
                {
                    TelefonNo = telefonNo.Trim(),
                    MusteriId = musteriId,
                    OperatorId = operatorId,
                    Tip = AramaTipi.Giden,
                    BaslangicZamani = DateTime.Now,
                    Durum = AramaDurumu.Baslatildi,
                    Notlar = "WebRTC üzerinden başlatıldı"
                };

                Console.WriteLine($"AramaLog oluşturuldu: {aramaLog.TelefonNo}");
                _context.AramaLoglari.Add(aramaLog);
                
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges sonucu: {result} kayıt etkilendi. AramaLog ID: {aramaLog.Id}");

                return Json(new { success = true, aramaId = aramaLog.Id, baslangicZamani = aramaLog.BaslangicZamani });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HATA: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Arama başlatılırken hata oluştu: " + ex.Message });
            }
        }

        // Gelen arama simülasyonu
        [HttpPost]
        public async Task<IActionResult> GelenAramaSimulasyonu()
        {
            // Rastgele telefon numarası üret
            var randomNumbers = new[] { "05321234567", "05551234567", "05431234567", "02121234567", "03121234567" };
            var randomTelefon = randomNumbers[new Random().Next(randomNumbers.Length)];

            // Müşteriyi kontrol et
            var musteri = await _context.Musteriler.FirstOrDefaultAsync(m => m.TelefonNo == randomTelefon);
            
            // Rastgele operatör seç (aktif olanlardan)
            var operatorler = await _context.Operatorler.Where(o => o.Aktif).ToListAsync();
            var randomOperator = operatorler[new Random().Next(operatorler.Count)];

            var aramaLog = new AramaLog
            {
                TelefonNo = randomTelefon,
                MusteriId = musteri?.Id,
                OperatorId = randomOperator.Id,
                Tip = AramaTipi.Gelen,
                BaslangicZamani = DateTime.Now,
                Durum = AramaDurumu.Baslatildi,
                Notlar = "Sistem tarafından simüle edildi"
            };

            _context.AramaLoglari.Add(aramaLog);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                aramaId = aramaLog.Id,
                telefonNo = randomTelefon,
                musteriAdi = musteri?.TamAd ?? "Bilinmiyor Arayan",
                operatorAdi = randomOperator.TamAd,
                operatorId = randomOperator.Id,
                tip = "Gelen",
                baslangicZamani = aramaLog.BaslangicZamani
            });
        }

        // Aramayı sonlandır
        [HttpPost]
        public async Task<IActionResult> AramaSonlandir(int aramaId, string notlar, int? musteriMemnuniyet)
        {
            var aramaLog = await _context.AramaLoglari.FindAsync(aramaId);
            if (aramaLog == null)
            {
                return Json(new { success = false, error = "Arama bulunamadı" });
            }

            aramaLog.BitisZamani = DateTime.Now;
            aramaLog.Sure = (aramaLog.BitisZamani.Value - aramaLog.BaslangicZamani).TotalMinutes;
            aramaLog.Durum = AramaDurumu.Tamamlandi;
            aramaLog.Notlar = notlar;
            aramaLog.MusteriMemnuniyet = musteriMemnuniyet;

            _context.AramaLoglari.Update(aramaLog);
            await _context.SaveChangesAsync();

            return Json(new { success = true, aramaId = aramaLog.Id, sure = aramaLog.Sure });
        }

        // Aramayı reddet/iptal et
        [HttpPost]
        public async Task<IActionResult> AramaReddet(int aramaId, string sebep = "Operatör reddettti")
        {
            var aramaLog = await _context.AramaLoglari.FindAsync(aramaId);
            if (aramaLog == null)
            {
                return Json(new { success = false, error = "Arama bulunamadı" });
            }

            aramaLog.BitisZamani = DateTime.Now;
            aramaLog.Sure = (aramaLog.BitisZamani.Value - aramaLog.BaslangicZamani).TotalMinutes;
            aramaLog.Durum = AramaDurumu.Iptal;
            aramaLog.Notlar = sebep;

            _context.AramaLoglari.Update(aramaLog);
            await _context.SaveChangesAsync();

            return Json(new { success = true, aramaId = aramaLog.Id, message = "Arama reddedildi" });
        }

        // Aramayı cevapsız olarak işaretle
        [HttpPost]
        public async Task<IActionResult> AramaCevapsiz(int aramaId)
        {
            var aramaLog = await _context.AramaLoglari.FindAsync(aramaId);
            if (aramaLog == null)
            {
                return Json(new { success = false, error = "Arama bulunamadı" });
            }

            aramaLog.BitisZamani = DateTime.Now;
            aramaLog.Sure = (aramaLog.BitisZamani.Value - aramaLog.BaslangicZamani).TotalMinutes;
            aramaLog.Durum = AramaDurumu.Cevapsiz;
            aramaLog.Notlar = "Zaman aşımı - cevap verilmedi";

            _context.AramaLoglari.Update(aramaLog);
            await _context.SaveChangesAsync();

            return Json(new { success = true, aramaId = aramaLog.Id, message = "Arama cevapsız" });
        }

        // Arama geçmişi
        public async Task<IActionResult> AramaGecmisi()
        {
            var aramaLoglari = await _context.AramaLoglari
                .Include(a => a.Musteri)
                .Include(a => a.Operator)
                .OrderByDescending(a => a.BaslangicZamani)
                .ToListAsync();
            return View(aramaLoglari);
        }

        // Gerçek zamanlı arama durumu kontrolü
        [HttpGet]
        public async Task<IActionResult> AktifAramalar()
        {
            var aktifAramalar = await _context.AramaLoglari
                .Include(a => a.Musteri)
                .Include(a => a.Operator)
                .Where(a => a.Durum == AramaDurumu.Baslatildi || a.Durum == AramaDurumu.Devam)
                .OrderByDescending(a => a.BaslangicZamani)
                .ToListAsync();

            return Json(aktifAramalar.Select(a => new {
                Id = a.Id,
                TelefonNo = a.TelefonNo,
                MusteriAdi = a.Musteri?.TamAd ?? "Bilinmiyor",
                OperatorAdi = a.Operator?.TamAd,
                Tip = a.TipMetni,
                BaslangicZamani = a.BaslangicZamani.ToString("HH:mm:ss"),
                Durum = a.DurumMetni,
                Sure = a.Sure?.ToString("F1") ?? "0"
            }));
        }

        // Operatörlerin arama durumlarını getir
        [HttpGet]
        public async Task<IActionResult> OperatorAramaDurumlari()
        {
            var operatorler = await _context.Operatorler
                .Where(o => o.Aktif)
                .Select(o => new {
                    Id = o.Id,
                    Ad = o.TamAd,
                    AktifArama = _context.AramaLoglari
                        .Where(a => a.OperatorId == o.Id && 
                               (a.Durum == AramaDurumu.Baslatildi || a.Durum == AramaDurumu.Devam))
                        .FirstOrDefault(),
                    BugunkuAramaSayisi = _context.AramaLoglari
                        .Count(a => a.OperatorId == o.Id && a.BaslangicZamani.Date == DateTime.Today)
                })
                .ToListAsync();

            return Json(operatorler);
        }
    }

    public class WebRTCSignalModel
    {
        public string Type { get; set; } = string.Empty;
        public object Data { get; set; } = new();
        public string TargetUser { get; set; } = string.Empty;
    }
}

