using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Models;
using CagriMerkeziUyg.Data;
using CagriMerkeziUyg.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CagriMerkeziUyg.Controllers
{
    [Authorize(Policy = "AllOperators")]
    public class OperatorController : Controller
    {
        private readonly CagriMerkeziDbContext _context;
        private readonly OperatorDurumService _durumService;

        public OperatorController(CagriMerkeziDbContext context, OperatorDurumService durumService)
        {
            _context = context;
            _durumService = durumService;
        }

        // Operatör Dashboard
        public async Task<IActionResult> Index()
        {
            var operatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (operatorIdClaim == null || !int.TryParse(operatorIdClaim.Value, out int aktifOperatorId))
            {
                TempData["Error"] = "Giriş bilgileriniz geçersiz. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }

            var operatorBilgi = await _context.Operatorler.FindAsync(aktifOperatorId);
            if (operatorBilgi == null || !operatorBilgi.Aktif)
            {
                TempData["Error"] = "Operatör bilgisi bulunamadı veya hesap aktif değil.";
                return RedirectToAction("Login", "Auth");
            }

            // Bu operatörün bugünkü istatistikleri
            var bugun = DateTime.Today;
            var bugunkuAktiviteler = await _context.MusteriAktiviteleri
                .Where(a => a.OperatorId == aktifOperatorId && a.OlusturulmaTarihi.Date == bugun)
                .ToListAsync();

            // Bu operatörün bekleyen çağrıları
            var bekleyenCagrilar = await _context.MusteriAktiviteleri
                .Include(a => a.Musteri)
                    .ThenInclude(m => m.EtiketAtamalari)
                        .ThenInclude(ea => ea.MusteriEtiketi)
                .Where(a => a.OperatorId == aktifOperatorId && 
                           (a.Durum == AktiviteDurumu.Yeni || a.Durum == AktiviteDurumu.Islemde || a.Durum == AktiviteDurumu.Beklemede))
                .OrderByDescending(a => a.Oncelik)
                .ThenBy(a => a.OlusturulmaTarihi)
                .Take(10)
                .ToListAsync();

            ViewBag.OperatorAdi = operatorBilgi.TamAd;
            ViewBag.BugunkuToplamCagri = bugunkuAktiviteler.Count;
            ViewBag.BugunkuCozulen = bugunkuAktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi);
            ViewBag.BekleyenCagriSayisi = bekleyenCagrilar.Count;
            ViewBag.OrtalamaMemnuniyet = bugunkuAktiviteler
                .Where(a => a.MusteriMemnuniyet.HasValue).Any() ?
                (decimal)bugunkuAktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Average(a => a.MusteriMemnuniyet)! : 0;

            return View(bekleyenCagrilar);
        }

        // Operatörün Çağrı Listesi
        public async Task<IActionResult> CagriListesi(string durum = "", int sayfa = 1)
        {
                    // Giriş yapmış operatörün ID'sini al
        var aktifOperatorId = GetCurrentOperatorId();
        if (aktifOperatorId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

            var query = _context.MusteriAktiviteleri
                .Include(a => a.Musteri)
                    .ThenInclude(m => m.EtiketAtamalari)
                        .ThenInclude(ea => ea.MusteriEtiketi)
                .Where(a => a.OperatorId == aktifOperatorId.Value);

            // Durum filtresi
            if (!string.IsNullOrEmpty(durum) && Enum.TryParse(durum, out AktiviteDurumu durumEnum))
            {
                query = query.Where(a => a.Durum == durumEnum);
            }

            // Sayfalama
            int sayfaBoyutu = 20;
            var toplamKayit = await query.CountAsync();
            var cagrilar = await query
                .OrderByDescending(a => a.OlusturulmaTarihi)
                .Skip((sayfa - 1) * sayfaBoyutu)
                .Take(sayfaBoyutu)
                .ToListAsync();

            ViewBag.SeciliDurum = durum;
            ViewBag.SayfaNo = sayfa;
            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplamKayit / sayfaBoyutu);
            ViewBag.ToplamKayit = toplamKayit;

            return View(cagrilar);
        }

        // Çağrı Detayı ve İşlem
        public async Task<IActionResult> CagriDetay(int id)
        {
            var aktivite = await _context.MusteriAktiviteleri
                .Include(a => a.Musteri)
                    .ThenInclude(m => m.EtiketAtamalari)
                        .ThenInclude(ea => ea.MusteriEtiketi)
                .Include(a => a.Operator)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aktivite == null)
            {
                TempData["Error"] = "Çağrı bulunamadı.";
                return RedirectToAction("CagriListesi");
            }

            // Müşterinin diğer aktivitelerini de getir
            var musteriDigerAktiviteler = await _context.MusteriAktiviteleri
                .Include(a => a.Operator)
                .Where(a => a.MusteriId == aktivite.MusteriId && a.Id != id)
                .OrderByDescending(a => a.OlusturulmaTarihi)
                .Take(5)
                .ToListAsync();

            ViewBag.MusteriDigerAktiviteler = musteriDigerAktiviteler;

            return View(aktivite);
        }

        // Çağrı Güncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CagriGuncelle(int id, AktiviteDurumu durum, int? memnuniyet, decimal? cagriSuresi, string aciklama)
        {
            var aktivite = await _context.MusteriAktiviteleri.FindAsync(id);
            if (aktivite == null)
            {
                TempData["Error"] = "Çağrı bulunamadı.";
                return RedirectToAction("CagriListesi");
            }

            // Güncelleme işlemleri
            aktivite.Durum = durum;
            if (memnuniyet.HasValue && memnuniyet >= 1 && memnuniyet <= 5)
            {
                aktivite.MusteriMemnuniyet = memnuniyet;
            }
            if (cagriSuresi.HasValue && cagriSuresi > 0)
            {
                aktivite.CagriSuresi = cagriSuresi;
            }
            if (!string.IsNullOrEmpty(aciklama))
            {
                aktivite.Aciklama = aciklama;
            }

            // Çözüm tarihi güncelle
            if (durum == AktiviteDurumu.Cozumlendi)
            {
                aktivite.CozumTarihi = DateTime.Now;
            }

            _context.Update(aktivite);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Çağrı başarıyla güncellendi!";
            return RedirectToAction("CagriDetay", new { id = id });
        }

        // Yeni Çağrı Oluşturma (Operatör tarafından)
        public async Task<IActionResult> YeniCagri()
        {
            // Son eklenen müşterileri getir (hızlı seçim için)
            var sonMusteriler = await _context.Musteriler
                .OrderByDescending(m => m.SonGuncelleme)
                .Take(10)
                .Select(m => new { m.Id, m.TamAd, m.TelefonNo })
                .ToListAsync();

            ViewBag.SonMusteriler = sonMusteriler;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniCagri(int musteriId, AktiviteTuru tur, AktiviteOncelik oncelik, string konu, string aciklama)
        {
                    // Giriş yapmış operatörün ID'sini al
        var aktifOperatorId = GetCurrentOperatorId();
        if (aktifOperatorId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

            if (string.IsNullOrEmpty(konu) || string.IsNullOrEmpty(aciklama))
            {
                TempData["Error"] = "Konu ve açıklama gereklidir.";
                return RedirectToAction("YeniCagri");
            }

            var musteri = await _context.Musteriler.FindAsync(musteriId);
            if (musteri == null)
            {
                TempData["Error"] = "Müşteri bulunamadı.";
                return RedirectToAction("YeniCagri");
            }

            var yeniAktivite = new MusteriAktiviteler
            {
                MusteriId = musteriId,
                OperatorId = aktifOperatorId,
                Tur = tur,
                Konu = konu,
                Oncelik = oncelik,
                Durum = AktiviteDurumu.Islemde,
                Aciklama = aciklama,
                OlusturulmaTarihi = DateTime.Now
            };

            _context.MusteriAktiviteleri.Add(yeniAktivite);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yeni çağrı başarıyla oluşturuldu!";
            return RedirectToAction("CagriDetay", new { id = yeniAktivite.Id });
        }

        // Operatörün Performans Özeti
        public async Task<IActionResult> PerformansOzeti()
        {
                    // Giriş yapmış operatörün ID'sini al
        var aktifOperatorId = GetCurrentOperatorId();
        if (aktifOperatorId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

            var operatorBilgi = await _context.Operatorler.FindAsync(aktifOperatorId);
            if (operatorBilgi == null)
            {
                TempData["Error"] = "Operatör bilgisi bulunamadı.";
                return RedirectToAction("Index");
            }

            // Son 30 günün performansı
            var baslangic = DateTime.Today.AddDays(-30);
            var aktiviteler = await _context.MusteriAktiviteleri
                .Where(a => a.OperatorId == aktifOperatorId.Value && a.OlusturulmaTarihi >= baslangic)
                .ToListAsync();

            var performans = new OperatorPerformansOzeti
            {
                OperatorId = aktifOperatorId.Value,
                OperatorAdi = operatorBilgi.TamAd,
                BaslangicTarihi = baslangic,
                BitisTarihi = DateTime.Today,
                ToplamCagri = aktiviteler.Count,
                CozulenCagri = aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi),
                CozumOrani = aktiviteler.Count > 0 ? 
                    (decimal)aktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi) / aktiviteler.Count * 100 : 0,
                OrtalamaCagriSuresi = aktiviteler.Where(a => a.CagriSuresi.HasValue).Any() ? 
                    (decimal)aktiviteler.Where(a => a.CagriSuresi.HasValue).Average(a => a.CagriSuresi)! : 0,
                OrtalamaMemnuniyet = aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Any() ? 
                    (decimal)aktiviteler.Where(a => a.MusteriMemnuniyet.HasValue).Average(a => a.MusteriMemnuniyet)! : 0
            };

            // Günlük istatistik (son 7 gün)
            var gunlukIstatistikler = new List<object>();
            for (int i = 6; i >= 0; i--)
            {
                var tarih = DateTime.Today.AddDays(-i);
                var gunlukAktiviteler = aktiviteler.Where(a => a.OlusturulmaTarihi.Date == tarih).ToList();
                
                gunlukIstatistikler.Add(new {
                    Tarih = tarih.ToString("dd.MM"),
                    ToplamCagri = gunlukAktiviteler.Count,
                    CozulenCagri = gunlukAktiviteler.Count(a => a.Durum == AktiviteDurumu.Cozumlendi)
                });
            }

            ViewBag.GunlukIstatistikler = gunlukIstatistikler;

            return View(performans);
        }

        // Müşteri Hızlı Arama (AJAX)
        [HttpGet]
        public async Task<IActionResult> MusteriAra(string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
                return Json(new List<object>());

            var musteriler = await _context.Musteriler
                .Include(m => m.EtiketAtamalari)
                    .ThenInclude(ea => ea.MusteriEtiketi)
                .Where(m => m.Ad.Contains(q) || m.Soyad.Contains(q) || m.TelefonNo.Contains(q))
                .Take(10)
                .Select(m => new { 
                    id = m.Id, 
                    text = $"{m.TamAd} - {m.TelefonNo}",
                    musteriTipi = m.MusteriTipi.HasValue ? m.MusteriTipi.Value.ToString() : "",
                    etiketler = m.EtiketAtamalari.Select(ea => new { 
                        ad = ea.MusteriEtiketi.Ad, 
                        renk = ea.MusteriEtiketi.RenkKodu 
                    }).ToList()
                })
                .ToListAsync();

            return Json(musteriler);
        }

        // Helper metod: Güvenli operatör ID alma
        private int? GetCurrentOperatorId()
        {
            var operatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (operatorIdClaim != null && int.TryParse(operatorIdClaim.Value, out int operatorId))
            {
                return operatorId;
            }
            return null;
        }

        // HIZLI MÜŞTERİ KAYIT SİSTEMİ
        /// Telefon numarasını kontrol et - kayıtlı mı değil mi?
        [HttpPost]
        public async Task<IActionResult> MusteriKontrol(string telefonNo)
        {
            try
            {
                var musteri = await _context.Musteriler
                    .Include(m => m.EtiketAtamalari)
                        .ThenInclude(e => e.MusteriEtiketi)
                    .FirstOrDefaultAsync(m => m.TelefonNo == telefonNo);

                if (musteri == null)
                {
                    return Json(new { 
                        success = true, 
                        musteriVar = false, 
                        telefonNo = telefonNo,
                        mesaj = "Bu numara sistemde kayıtlı değil"
                    });
                }

                return Json(new { 
                    success = true, 
                    musteriVar = true,
                    musteri = new {
                        musteri.Id,
                        musteri.Ad,
                        musteri.Soyad,
                        musteri.TamAd,
                        musteri.TelefonNo,
                        musteri.Email,
                        musteri.KayitDurumu,
                        musteri.GeciciKayit,
                        etiketler = musteri.EtiketAtamalari.Select(e => new {
                            e.MusteriEtiketi.Ad,
                            e.MusteriEtiketi.RenkKodu
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kontrol sırasında hata oluştu: " + ex.Message });
            }
        }

        /// Hızlı müşteri kaydı oluştur (Sadece ad, soyad, telefon)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizliMusteriOlustur(string ad, string soyad, string telefonNo)
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                // Telefon numarası kontrolü
                var mevcutMusteri = await _context.Musteriler
                    .FirstOrDefaultAsync(m => m.TelefonNo == telefonNo);

                if (mevcutMusteri != null)
                {
                    return Json(new { success = false, message = "Bu telefon numarası zaten kayıtlı" });
                }

                // Validasyon
                if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) || string.IsNullOrWhiteSpace(telefonNo))
                {
                    return Json(new { success = false, message = "Ad, soyad ve telefon alanları zorunludur" });
                }

                // Yeni müşteri oluştur
                var yeniMusteri = new Musteri
                {
                    Ad = ad.Trim(),
                    Soyad = soyad.Trim(),
                    TelefonNo = telefonNo.Trim(),
                    KayitDurumu = MusteriKayitDurumu.Kismi,
                    GeciciKayit = false,
                    KayitTamamlandi = false,
                    KayitTarihi = DateTime.Now,
                    SonGuncelleme = DateTime.Now
                };

                _context.Musteriler.Add(yeniMusteri);
                await _context.SaveChangesAsync();

                // Aktivite kaydı oluştur
                var aktivite = new MusteriAktiviteler
                {
                    MusteriId = yeniMusteri.Id,
                    OperatorId = aktifOperatorId,
                    Tur = AktiviteTuru.Diger,
                    Konu = "Hızlı müşteri kaydı",
                    Aciklama = "Çağrı sırasında hızlı kayıt yapıldı",
                    Durum = AktiviteDurumu.Cozumlendi,
                    OlusturulmaTarihi = DateTime.Now
                };

                _context.MusteriAktiviteleri.Add(aktivite);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Müşteri başarıyla kaydedildi",
                    musteri = new {
                        yeniMusteri.Id,
                        yeniMusteri.Ad,
                        yeniMusteri.Soyad,
                        yeniMusteri.TamAd,
                        yeniMusteri.TelefonNo
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kayıt oluşturulurken hata oluştu: " + ex.Message });
            }
        }

        /// Geçici (Anonim) müşteri oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeciciMusteriOlustur(string telefonNo)
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                // Validasyon
                if (string.IsNullOrWhiteSpace(telefonNo))
                {
                    return Json(new { success = false, message = "Telefon numarası zorunludur" });
                }

                // Geçici müşteri oluştur
                var geciciMusteri = new Musteri
                {
                    Ad = "Anonim",
                    Soyad = "Müşteri",
                    TelefonNo = telefonNo.Trim(),
                    KayitDurumu = MusteriKayitDurumu.Gecici,
                    GeciciKayit = true,
                    KayitTamamlandi = false,
                    KayitTarihi = DateTime.Now,
                    SonGuncelleme = DateTime.Now
                };

                _context.Musteriler.Add(geciciMusteri);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Geçici müşteri kaydı oluşturuldu",
                    musteri = new {
                        geciciMusteri.Id,
                        geciciMusteri.TamAd,
                        geciciMusteri.TelefonNo,
                        geciciMusteri.GeciciKayit
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Geçici kayıt oluşturulurken hata oluştu: " + ex.Message });
            }
        }

        /// Çağrı başlat - AramaLog kaydı oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CagriBaslat(string telefonNo, int? musteriId, bool musteriKayitliydi, bool cagriSirasindaKayitOlusturuldu)
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                var aramaLog = new AramaLog
                {
                    TelefonNo = telefonNo,
                    MusteriId = musteriId,
                    OperatorId = aktifOperatorId.Value,
                    Tip = AramaTipi.Gelen,
                    BaslangicZamani = DateTime.Now,
                    Durum = AramaDurumu.Devam,
                    MusteriKayitliydi = musteriKayitliydi,
                    CagriSirasindaKayitOlusturuldu = cagriSirasindaKayitOlusturuldu
                };

                _context.AramaLoglari.Add(aramaLog);
                await _context.SaveChangesAsync();

                // DURUM YÖNETİMİ: Çağrı başlayınca otomatik olarak "Çağrıda" durumuna geç
                await _durumService.DurumDegistir(
                    aktifOperatorId.Value,
                    OperatorDurumu.Cagirida,
                    $"Çağrı başladı - {telefonNo}",
                    otomatikGecis: true,
                    ilgiliAramaLogId: aramaLog.Id
                );

                return Json(new { 
                    success = true, 
                    message = "Çağrı başlatıldı",
                    aramaLogId = aramaLog.Id,
                    baslangicZamani = aramaLog.BaslangicZamani
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Çağrı başlatılırken hata oluştu: " + ex.Message });
            }
        }

        /// Çağrı bitir - AramaLog güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CagriBitir(int aramaLogId, AramaDurumu durum, string? notlar, int? memnuniyet)
        {
            try
            {
                var aramaLog = await _context.AramaLoglari.FindAsync(aramaLogId);
                if (aramaLog == null)
                {
                    return Json(new { success = false, message = "Arama kaydı bulunamadı" });
                }

                aramaLog.BitisZamani = DateTime.Now;
                aramaLog.Durum = durum;
                aramaLog.Notlar = notlar;
                aramaLog.MusteriMemnuniyet = memnuniyet;
                
                // Süreyi hesapla (dakika cinsinden)
                if (aramaLog.BaslangicZamani != null && aramaLog.BitisZamani != null)
                {
                    var fark = aramaLog.BitisZamani.Value - aramaLog.BaslangicZamani;
                    aramaLog.Sure = fark.TotalMinutes;
                }

                await _context.SaveChangesAsync();

                //  DURUM YÖNETİMİ: Çağrı bitince otomatik olarak "Ara Çalışma" durumuna geç
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId.HasValue)
                {
                    await _durumService.DurumDegistir(
                        aktifOperatorId.Value,
                        OperatorDurumu.AraCalısma,
                        "Çağrı sonlandırıldı, notlar alınıyor",
                        otomatikGecis: true,
                        ilgiliAramaLogId: aramaLogId
                    );
                }

                return Json(new { 
                    success = true, 
                    message = "Çağrı sonlandırıldı",
                    sure = aramaLog.Sure
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Çağrı sonlandırılırken hata oluştu: " + ex.Message });
            }
        }

        // ============================================
        // DURUM YÖNETİMİ
        // ============================================

        /// <summary>
        /// Operatör durumunu değiştir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumDegistir(OperatorDurumu yeniDurum, string? not)
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                var result = await _durumService.DurumDegistir(
                    aktifOperatorId.Value, 
                    yeniDurum, 
                    not, 
                    otomatikGecis: false
                );

                return Json(new { 
                    success = result.success, 
                    message = result.message,
                    yeniDurum = yeniDurum.ToString(),
                    degisimZamani = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Durum değiştirilemedi: " + ex.Message });
            }
        }

        /// <summary>
        /// Operatörün mevcut durumunu getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MevcutDurumGetir()
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                var operatorEntity = await _context.Operatorler.FindAsync(aktifOperatorId.Value);
                if (operatorEntity == null)
                {
                    return Json(new { success = false, message = "Operatör bulunamadı" });
                }

                var durumSuresi = operatorEntity.SonDurumDegisikliği.HasValue
                    ? (DateTime.Now - operatorEntity.SonDurumDegisikliği.Value).TotalMinutes
                    : 0;

                return Json(new
                {
                    success = true,
                    durum = operatorEntity.MevcutDurum.ToString(),
                    durumMetni = operatorEntity.MevcutDurum.ToString(),
                    durumRengi = operatorEntity.DurumRengi,
                    durumIkon = operatorEntity.DurumIkon,
                    sonDegisiklik = operatorEntity.SonDurumDegisikliği,
                    durumSuresi = Math.Round(durumSuresi, 1),
                    durumNotu = operatorEntity.DurumNotu
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Durum getirilemedi: " + ex.Message });
            }
        }

        /// <summary>
        /// Operatörün bugünkü durum geçmişini getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BugunkuDurumGecmisi()
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                var gecmis = await _durumService.BugunkuDurumGecmisiniGetir(aktifOperatorId.Value);

                var sonuc = gecmis.Select(g => new
                {
                    id = g.Id,
                    oncekiDurum = g.OncekiDurum.ToString(),
                    yeniDurum = g.YeniDurum.ToString(),
                    gecisZamani = g.GecisZamani.ToString("HH:mm:ss"),
                    bitisZamani = g.BitisZamani?.ToString("HH:mm:ss"),
                    sureDakika = g.SureDakika.HasValue ? (decimal?)Math.Round(g.SureDakika.Value, 1) : null,
                    not = g.Not,
                    otomatikGecis = g.OtomatikGecis
                }).ToList();

                return Json(new { success = true, data = sonuc });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Geçmiş getirilemedi: " + ex.Message });
            }
        }

        /// <summary>
        /// Operatörün bugünkü durum özetini getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BugunkuDurumOzeti()
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                var ozet = await _durumService.GunlukOzetHesapla(aktifOperatorId.Value, DateTime.Today);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        toplamCalismaSuresi = Math.Round(ozet.ToplamCalismaSuresi, 1),
                        cagrıdaGecenSure = Math.Round(ozet.CagrıdaGecenSure, 1),
                        araCalismaSuresi = Math.Round(ozet.AraCalismaSuresi, 1),
                        musaitSure = Math.Round(ozet.MusaitSure, 1),
                        molaSuresi = Math.Round(ozet.MolaSuresi, 1),
                        ogleYemegiSuresi = Math.Round(ozet.OgleYemegiSuresi, 1),
                        toplantiSuresi = Math.Round(ozet.ToplantiSuresi, 1),
                        egitimSuresi = Math.Round(ozet.EgitimSuresi, 1),
                        toplamCagriSayisi = ozet.ToplamCagriSayisi,
                        ortalamaCagriSuresi = ozet.OrtalamaCagriSuresi.HasValue ? Math.Round(ozet.OrtalamaCagriSuresi.Value, 1) : 0,
                        verimlilıkOrani = Math.Round(ozet.VerimlilıkOrani, 1),
                        kullanimOrani = Math.Round(ozet.KullanimOrani, 1)
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Özet getirilemedi: " + ex.Message });
            }
        }

        /// <summary>
        /// Hızlı durum değiştirme butonları için
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizliDurumDegistir(string durum)
        {
            try
            {
                var aktifOperatorId = GetCurrentOperatorId();
                if (aktifOperatorId == null)
                {
                    return Json(new { success = false, message = "Operatör bilgisi bulunamadı" });
                }

                // String'i enum'a çevir
                if (!Enum.TryParse<OperatorDurumu>(durum, out var yeniDurum))
                {
                    return Json(new { success = false, message = "Geçersiz durum" });
                }

                var result = await _durumService.DurumDegistir(aktifOperatorId.Value, yeniDurum);

                return Json(new { 
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
