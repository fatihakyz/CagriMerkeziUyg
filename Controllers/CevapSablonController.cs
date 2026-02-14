using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Models;
using CagriMerkeziUyg.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CagriMerkeziUyg.Controllers
{
    /// Hızlı yanıt şablonları için API Controller
    /// AJAX isteklerine JSON formatında cevap verir
    [Authorize(Policy = "AllOperators")]
    public class CevapSablonController : Controller
    {
        private readonly CagriMerkeziDbContext _context;

        public CevapSablonController(CagriMerkeziDbContext context)
        {
            _context = context;
        }
        /// Tüm aktif kategorileri getir (Operatör ekranı için)
        [HttpGet]
        public async Task<IActionResult> GetKategoriler()
        {
            try
            {
                var kategoriler = await _context.CevapSablonKategorileri
                    .Where(k => k.Aktif)
                    .OrderBy(k => k.Sira)
                    .Select(k => new
                    {
                        k.Id,
                        k.Ad,
                        k.Aciklama,
                        k.IconClass,
                        k.RenkKodu,
                        k.Sira,
                        SablonSayisi = k.Sablonlar.Count(s => s.Aktif)
                    })
                    .ToListAsync();

                return Json(new { success = true, data = kategoriler });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategoriler yüklenirken hata oluştu: " + ex.Message });
            }
        }
        /// Belirli bir kategoriye ait şablonları getir
        [HttpGet]
        public async Task<IActionResult> GetSablonlar(int kategoriId)
        {
            try
            {
                var sablonlar = await _context.CevapSablonlari
                    .Where(s => s.KategoriId == kategoriId && s.Aktif)
                    .OrderBy(s => s.Sira)
                    .Select(s => new
                    {
                        s.Id,
                        s.Baslik,
                        s.Icerik,
                        s.Notlar,
                        s.DegiskenIceriyor,
                        s.KisaYol,
                        s.KullanimSayisi
                    })
                    .ToListAsync();

                return Json(new { success = true, data = sablonlar });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Şablonlar yüklenirken hata oluştu: " + ex.Message });
            }
        }
        /// Tüm aktif şablonları getir (Arama için)
        [HttpGet]
        public async Task<IActionResult> GetTumSablonlar()
        {
            try
            {
                var sablonlar = await _context.CevapSablonlari
                    .Include(s => s.Kategori)
                    .Where(s => s.Aktif && s.Kategori.Aktif)
                    .OrderBy(s => s.Kategori.Sira)
                    .ThenBy(s => s.Sira)
                    .Select(s => new
                    {
                        s.Id,
                        s.KategoriId,
                        KategoriAdi = s.Kategori.Ad,
                        s.Baslik,
                        s.Icerik,
                        s.Notlar,
                        s.DegiskenIceriyor,
                        s.KisaYol,
                        s.KullanimSayisi
                    })
                    .ToListAsync();

                return Json(new { success = true, data = sablonlar });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Şablonlar yüklenirken hata oluştu: " + ex.Message });
            }
        }
        /// Belirli bir şablonun detayını getir
        [HttpGet]
        public async Task<IActionResult> GetSablon(int id)
        {
            try
            {
                var sablon = await _context.CevapSablonlari
                    .Include(s => s.Kategori)
                    .Where(s => s.Id == id && s.Aktif)
                    .Select(s => new
                    {
                        s.Id,
                        s.KategoriId,
                        KategoriAdi = s.Kategori.Ad,
                        KategoriRenk = s.Kategori.RenkKodu,
                        s.Baslik,
                        s.Icerik,
                        s.Notlar,
                        s.DegiskenIceriyor,
                        s.KisaYol,
                        s.KullanimSayisi,
                        s.SonKullanimTarihi
                    })
                    .FirstOrDefaultAsync();

                if (sablon == null)
                {
                    return Json(new { success = false, message = "Şablon bulunamadı" });
                }

                return Json(new { success = true, data = sablon });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Şablon yüklenirken hata oluştu: " + ex.Message });
            }
        }
        /// Şablon kullanıldığında istatistiği güncelle ve log kaydet
        [HttpPost]
        public async Task<IActionResult> SablonKullan(int sablonId, int? musteriId, int? aktiviteId)
        {
            try
            {
                var operatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (operatorIdClaim == null || !int.TryParse(operatorIdClaim.Value, out int operatorId))
                {
                    return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });
                }

                // Şablonu bul
                var sablon = await _context.CevapSablonlari.FindAsync(sablonId);
                if (sablon == null)
                {
                    return Json(new { success = false, message = "Şablon bulunamadı" });
                }
                // Kullanım istatistiğini güncelle
                sablon.KullanimSayisi++;
                sablon.SonKullanimTarihi = DateTime.Now;
                // Kullanım kaydı oluştur
                var kullanimKaydi = new CevapSablonKullanim
                {
                    SablonId = sablonId,
                    OperatorId = operatorId,
                    MusteriId = musteriId,
                    AktiviteId = aktiviteId,
                    KullanimTarihi = DateTime.Now
                };

                _context.CevapSablonKullanimlar.Add(kullanimKaydi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Kullanım kaydedildi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kullanım kaydedilirken hata oluştu: " + ex.Message });
            }
        }
        /// En çok kullanılan şablonları getir
        [HttpGet]
        public async Task<IActionResult> GetPopulerSablonlar(int adet = 5)
        {
            try
            {
                var sablonlar = await _context.CevapSablonlari
                    .Include(s => s.Kategori)
                    .Where(s => s.Aktif && s.Kategori.Aktif)
                    .OrderByDescending(s => s.KullanimSayisi)
                    .Take(adet)
                    .Select(s => new
                    {
                        s.Id,
                        KategoriAdi = s.Kategori.Ad,
                        s.Baslik,
                        s.Icerik,
                        s.KullanimSayisi,
                        s.KisaYol
                    })
                    .ToListAsync();

                return Json(new { success = true, data = sablonlar });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Popüler şablonlar yüklenirken hata oluştu: " + ex.Message });
            }
        }
        /// Şablon arama (Başlık ve içerikte ara)
        [HttpGet]
        public async Task<IActionResult> AramaSablonlar(string arama)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(arama))
                {
                    return Json(new { success = true, data = new List<object>() });
                }

                var sablonlar = await _context.CevapSablonlari
                    .Include(s => s.Kategori)
                    .Where(s => s.Aktif && s.Kategori.Aktif &&
                               (s.Baslik.Contains(arama) || s.Icerik.Contains(arama)))
                    .OrderByDescending(s => s.KullanimSayisi)
                    .Take(10)
                    .Select(s => new
                    {
                        s.Id,
                        KategoriAdi = s.Kategori.Ad,
                        KategoriRenk = s.Kategori.RenkKodu,
                        s.Baslik,
                        s.Icerik,
                        s.KisaYol,
                        s.KullanimSayisi
                    })
                    .ToListAsync();

                return Json(new { success = true, data = sablonlar });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Arama yapılırken hata oluştu: " + ex.Message });
            }
        }
        // ADMIN İŞLEMLERİ      
        /// Admin: Tüm kategorileri getir (pasif dahil)       
        [HttpGet]
        [Authorize(Policy = "AdminOrSupervisor")]
        public async Task<IActionResult> AdminGetKategoriler()
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
                        SablonSayisi = k.Sablonlar.Count()
                    })
                    .ToListAsync();

                return Json(new { success = true, data = kategoriler });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategoriler yüklenirken hata oluştu: " + ex.Message });
            }
        }
        /// Admin: Kullanım istatistikleri
        [HttpGet]
        [Authorize(Policy = "AdminOrSupervisor")]
        public async Task<IActionResult> AdminGetIstatistikler(DateTime? baslangic, DateTime? bitis)
        {
            try
            {
                baslangic ??= DateTime.Today.AddMonths(-1);
                bitis ??= DateTime.Today;

                var kullanimlar = await _context.CevapSablonKullanimlar
                    .Include(k => k.Sablon)
                    .Include(k => k.Operator)
                    .Where(k => k.KullanimTarihi >= baslangic && k.KullanimTarihi <= bitis)
                    .GroupBy(k => k.SablonId)
                    .Select(g => new
                    {
                        SablonId = g.Key,
                        SablonAdi = g.First().Sablon.Baslik,
                        KullanimSayisi = g.Count(),
                        FarkliOperatorSayisi = g.Select(x => x.OperatorId).Distinct().Count(),
                        SonKullanim = g.Max(x => x.KullanimTarihi)
                    })
                    .OrderByDescending(x => x.KullanimSayisi)
                    .ToListAsync();

                var toplamKullanim = await _context.CevapSablonKullanimlar
                    .Where(k => k.KullanimTarihi >= baslangic && k.KullanimTarihi <= bitis)
                    .CountAsync();

                return Json(new { 
                    success = true, 
                    data = new {
                        sablonlar = kullanimlar,
                        toplamKullanim = toplamKullanim,
                        baslangicTarihi = baslangic,
                        bitisTarihi = bitis
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İstatistikler yüklenirken hata oluştu: " + ex.Message });
            }
        }
    }
}