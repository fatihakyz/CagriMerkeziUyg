using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Models;
using CagriMerkeziUyg.Data;
using Microsoft.AspNetCore.Authorization;

namespace CagriMerkeziUyg.Controllers
{
    [Authorize(Policy = "AllOperators")]
    public class MusteriController : Controller
    {
        private readonly CagriMerkeziDbContext _context;

        public MusteriController(CagriMerkeziDbContext context)
        {
            _context = context;
        }

        // Müşteri arama sayfası
        public async Task<IActionResult> Ara()
        {
            await SetViewBagStatistics();
            return View();
        }
        
        // Telefon numarası ile arama (POST)
        [HttpPost]
        public async Task<IActionResult> Ara(string telefonNo)
        {
            if (string.IsNullOrWhiteSpace(telefonNo))
            {
                ModelState.AddModelError("", "Lütfen bir telefon numarası giriniz.");
                // İstatistikleri yeniden hesapla
                await SetViewBagStatistics();
                return View();
            }
            
            // Telefon numarasını temizle (boşluk, tire vb. karakterleri kaldır)
            var temizTelefonNo = telefonNo.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            
            // Müşteri arama
            var musteri = await _context.Musteriler.FirstOrDefaultAsync(m => 
                m.TelefonNo.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "") == temizTelefonNo);
            
            if (musteri != null)
            {
                // Müşteri bulundu, detay sayfasına yönlendir
                TempData["Success"] = "Müşteri bulundu!";
                return RedirectToAction("Detay", new { id = musteri.Id });
            }
            else
            {
                // Müşteri bulunamadı
                ViewBag.TelefonNo = telefonNo;
                ViewBag.Message = "Bu telefon numarası ile kayıtlı müşteri bulunamadı.";
                // İstatistikleri yeniden hesapla
                await SetViewBagStatistics();
                return View();
            }
        }
        
        // Yeni müşteri kayıt sayfası
        public IActionResult YeniKayit(string telefonNo = "")
        {
            var model = new Musteri();
            if (!string.IsNullOrEmpty(telefonNo))
            {
                model.TelefonNo = telefonNo;
            }
            return View(model);
        }
        
        // Yeni müşteri kaydetme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniKayit(Musteri musteri)
        {
            if (ModelState.IsValid)
            {
                // Telefon numarası kontrolü
                var temizTelefonNo = musteri.TelefonNo.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                var mevcutMusteri = await _context.Musteriler.FirstOrDefaultAsync(m => 
                    m.TelefonNo.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "") == temizTelefonNo);
                
                if (mevcutMusteri != null)
                {
                    ModelState.AddModelError("TelefonNo", "Bu telefon numarası zaten kayıtlı.");
                    return View(musteri);
                }
                
                // Yeni müşteri ekle
                musteri.KayitTarihi = DateTime.Now;
                musteri.SonGuncelleme = DateTime.Now;
                _context.Musteriler.Add(musteri);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Müşteri başarıyla kaydedildi!";
                return RedirectToAction("Detay", new { id = musteri.Id });
            }
            
            return View(musteri);
        }
        
        // Müşteri detay sayfası
        public async Task<IActionResult> Detay(int id)
        {
            var musteri = await _context.Musteriler
                .Include(m => m.Aktiviteler)
                .Include(m => m.EtiketAtamalari)
                    .ThenInclude(ea => ea.MusteriEtiketi)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (musteri == null)
            {
                TempData["Error"] = "Müşteri bulunamadı.";
                return RedirectToAction("Ara");
            }
            
            // Mevcut etiketleri ve kullanılabilir etiketleri ViewBag'e ekle
            ViewBag.MevcutEtiketler = musteri.EtiketAtamalari.Select(ea => ea.MusteriEtiketi).ToList();
            
            // Kullanılabilir etiketleri ayrı sorgu ile al
            var mevcutEtiketIdleri = musteri.EtiketAtamalari.Select(ea => ea.MusteriEtiketiId).ToList();
            ViewBag.KullanilabilirEtiketler = await _context.MusteriEtiketleri
                .Where(e => e.Aktif && !mevcutEtiketIdleri.Contains(e.Id))
                .ToListAsync();
            
            return View(musteri);
        }
        
        // Müşteri düzenleme sayfası
        public async Task<IActionResult> Duzenle(int id)
        {
            var musteri = await _context.Musteriler.FirstOrDefaultAsync(m => m.Id == id);
            if (musteri == null)
            {
                TempData["Error"] = "Müşteri bulunamadı.";
                return RedirectToAction("Ara");
            }
            
            return View(musteri);
        }
        
        // Müşteri düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Musteri musteri)
        {
            if (ModelState.IsValid)
            {
                var mevcutMusteri = await _context.Musteriler.FirstOrDefaultAsync(m => m.Id == musteri.Id);
                if (mevcutMusteri == null)
                {
                    TempData["Error"] = "Müşteri bulunamadı.";
                    return RedirectToAction("Ara");
                }
                
                // Telefon numarası kontrolü (kendisi hariç)
                var temizTelefonNo = musteri.TelefonNo.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                var telefonKontrolu = await _context.Musteriler.FirstOrDefaultAsync(m => 
                    m.Id != musteri.Id && 
                    m.TelefonNo.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "") == temizTelefonNo);
                
                if (telefonKontrolu != null)
                {
                    ModelState.AddModelError("TelefonNo", "Bu telefon numarası başka bir müşteri tarafından kullanılıyor.");
                    return View(musteri);
                }
                
                // Güncelle
                mevcutMusteri.TelefonNo = musteri.TelefonNo;
                mevcutMusteri.Ad = musteri.Ad;
                mevcutMusteri.Soyad = musteri.Soyad;
                mevcutMusteri.Email = musteri.Email;
                mevcutMusteri.Adres = musteri.Adres;
                mevcutMusteri.DogumTarihi = musteri.DogumTarihi;
                mevcutMusteri.Notlar = musteri.Notlar;
                mevcutMusteri.MusteriTipi = musteri.MusteriTipi;
                mevcutMusteri.OzelNotlar = musteri.OzelNotlar;
                // SonGuncelleme otomatik olarak DbContext'te güncelleniyor
                
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Müşteri bilgileri güncellendi!";
                return RedirectToAction("Detay", new { id = musteri.Id });
            }
            
            return View(musteri);
        }
        
        // Müşteri listesi
        public async Task<IActionResult> Liste(string arama = "")
        {
            var musteriler = _context.Musteriler.AsQueryable();
            
            if (!string.IsNullOrEmpty(arama))
            {
                musteriler = musteriler.Where(m => 
                    m.Ad.Contains(arama) ||
                    m.Soyad.Contains(arama) ||
                    m.TelefonNo.Contains(arama) ||
                    (m.Email != null && m.Email.Contains(arama)));
            }
            
            ViewBag.Arama = arama;
            var result = await musteriler.OrderByDescending(m => m.KayitTarihi).ToListAsync();
            return View(result);
        }
        
        // Müşteri silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var musteri = await _context.Musteriler.FirstOrDefaultAsync(m => m.Id == id);
            if (musteri != null)
            {
                _context.Musteriler.Remove(musteri);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Müşteri silindi.";
            }
            else
            {
                TempData["Error"] = "Müşteri bulunamadı.";
            }
            
            return RedirectToAction("Liste");
        }

        // Aktivite ekleme metodu
        [HttpPost]
        public async Task<IActionResult> AktiviteEkle(int musteriId, string tur, string aciklama)
        {
            // Açıklama boş mu kontrol et
            if (string.IsNullOrEmpty(aciklama))
            {
                TempData["Error"] = "Açıklama yazın!";
                return RedirectToAction("Detay", new { id = musteriId });
            }

            // Müşteri var mı bak
            var musteri = await _context.Musteriler.FindAsync(musteriId);
            if (musteri == null)
            {
                TempData["Error"] = "Müşteri yok!";
                return RedirectToAction("Ara");
            }

            // String'i enum'a çevir
            if (!Enum.TryParse(tur, out AktiviteTuru aktiviteTuru))
            {
                TempData["Error"] = "Geçersiz aktivite türü!";
                return RedirectToAction("Detay", new { id = musteriId });
            }

            // Yeni aktivite yap
            var aktivite = new MusteriAktiviteler
            {
                MusteriId = musteriId,
                Tur = aktiviteTuru,
                Aciklama = aciklama,
                OlusturulmaTarihi = DateTime.Now
            };

            // Kaydet
            _context.MusteriAktiviteleri.Add(aktivite);
            await _context.SaveChangesAsync();

            // Başarı mesajı
            TempData["Success"] = "Aktivite eklendi!";
            return RedirectToAction("Detay", new { id = musteriId });
        }
        //Aktivite düzenleme sayfasına yönlendirme
        [HttpGet]
        public async Task<IActionResult> AktiviteDuzenle(int id)
        {
            var aktivite = await _context.MusteriAktiviteleri
                .Include(a => a.Musteri)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aktivite == null)
            {
                TempData["Error"] = "Aktivite bulunamadı!";
                return RedirectToAction("Liste");
            }

            return View(aktivite);
        }
        [HttpPost] // Aktivite düzenleme form'unu kaydet
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AktiviteDuzenle(int id, MusteriAktiviteler aktivite)
        {
            if (id != aktivite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var mevcutAktivite = await _context.MusteriAktiviteleri.FindAsync(id);
                    if (mevcutAktivite == null)
                    {
                        TempData["Error"] = "Aktivite bulunamadı!";
                        return RedirectToAction("Liste");
                    }

                    mevcutAktivite.Tur = aktivite.Tur;
                    mevcutAktivite.Aciklama = aktivite.Aciklama;

                    _context.Update(mevcutAktivite);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Aktivite güncellendi!";
                    return RedirectToAction("Detay", new { id = mevcutAktivite.MusteriId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AktiviteVarMi(aktivite.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(aktivite);
        }
        // Aktivite silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AktiviteSil(int id)
        {
            var aktivite = await _context.MusteriAktiviteleri.FindAsync(id);
            if (aktivite == null)
            {
                TempData["Error"] = "Aktivite bulunamadı!";
                return RedirectToAction("Liste");
            }

            int musteriId = aktivite.MusteriId;

            _context.MusteriAktiviteleri.Remove(aktivite);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Aktivite silindi!";
            return RedirectToAction("Detay", new { id = musteriId });
        }

        // Yardımcı method
        private bool AktiviteVarMi(int id)
        {
            return _context.MusteriAktiviteleri.Any(e => e.Id == id);
        }

        // Helper method - ViewBag istatistiklerini set eder
        private async Task SetViewBagStatistics()
        {
            ViewBag.ToplamMusteri = await _context.Musteriler.CountAsync();
            ViewBag.BugunkuKayit = await _context.Musteriler
                .Where(m => m.KayitTarihi.Date == DateTime.Today)
                .CountAsync();
            ViewBag.SonArama = "Az önce"; // Şimdilik sabit değer
        }
   

    // Aktivite filtreleme metodu 
    [HttpGet]
        public async Task<IActionResult> AktiviteFiltreleme(int musteriId, string tur = "", string baslangicTarihi = "", string bitisTarihi = "", string aramaMetni = "")
        {
            var query = _context.MusteriAktiviteleri
                .Where(a => a.MusteriId == musteriId)
                .AsQueryable();

            // Aktivite türü filtresi
            if (!string.IsNullOrEmpty(tur) && tur != "Tumu")
            {
                if (Enum.TryParse(tur, out AktiviteTuru aktiviteTuru))
                {
                    query = query.Where(a => a.Tur == aktiviteTuru);
                }
            }

            // Tarih aralığı filtresi
            if (!string.IsNullOrEmpty(baslangicTarihi) && DateTime.TryParse(baslangicTarihi, out DateTime baslangic))
            {
                query = query.Where(a => a.OlusturulmaTarihi >= baslangic);
            }

            if (!string.IsNullOrEmpty(bitisTarihi) && DateTime.TryParse(bitisTarihi, out DateTime bitis))
            {
                // Bitiş tarihine saat 23:59:59 ekliyoruz
                bitis = bitis.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.OlusturulmaTarihi <= bitis);
            }

            // Arama metni filtresi (açıklama içinde arama)
            if (!string.IsNullOrEmpty(aramaMetni))
            {
                query = query.Where(a => a.Aciklama.Contains(aramaMetni));
            }

            var aktiviteler = await query
                .OrderByDescending(a => a.OlusturulmaTarihi)
                .ToListAsync();

            // Partial view döndür (Açıklama içinden kesitler)
            return PartialView("AktiviteListesi", aktiviteler);
        }

        // Genel aktivite istatistikleri 
            [HttpGet]
    public async Task<IActionResult> AktiviteIstatistikleri()
    {
        // Basit istatistikler
        var toplamAktivite = await _context.MusteriAktiviteleri.CountAsync();
        var toplamMusteri = await _context.Musteriler.CountAsync();
        
        // Tüm aktiviteleri ve müşterileri çekme
        var tumAktiviteler = await _context.MusteriAktiviteleri.ToListAsync();
        var tumMusteriler = await _context.Musteriler.Include(m => m.Aktiviteler).ToListAsync();
        
        var istatistikler = new
        {
            // En çok hangi türde aktivite var (Memory'de gruplama)
            aktiviteTurleri = tumAktiviteler
                .GroupBy(a => a.Tur)
                .Select(g => new { 
                    tur = g.Key.ToString(), 
                    sayi = g.Count() 
                })
                .OrderByDescending(x => x.sayi)
                .ToList(),

            // Son 6 ayın aktiviteleri (Memory'de filtreleme)
            aylikTrend = tumAktiviteler
                .Where(a => a.OlusturulmaTarihi >= DateTime.Now.AddMonths(-6))
                .GroupBy(a => new {
                    Yil = a.OlusturulmaTarihi.Year,
                    Ay = a.OlusturulmaTarihi.Month
                })
                .Select(g => new {
                    tarih = $"{g.Key.Yil}-{g.Key.Ay:00}",
                    sayi = g.Count()
                })
                .OrderBy(x => x.tarih)
                .ToList(),

                // Aktif/Pasif müşteri analizi (Son 30 gün) - Memory'de hesaplama
                musteriAnalizi = new
                {
                    aktifMusteri = tumMusteriler.Count(m => 
                        m.Aktiviteler.Any(a => a.OlusturulmaTarihi >= DateTime.Now.AddDays(-30))),
                    pasifMusteri = tumMusteriler.Count(m => 
                        !m.Aktiviteler.Any(a => a.OlusturulmaTarihi >= DateTime.Now.AddDays(-30))),
                    toplamAktivite = toplamAktivite,
                    toplamMusteri = toplamMusteri
                }
            };

            return Json(istatistikler);
        }

        // Belirli müşteri için aktivite istatistikleri (Müşteri detayı için)
        [HttpGet]
        public async Task<IActionResult> MusteriAktiviteIstatistikleri(int musteriId)
        {
            var musteri = await _context.Musteriler
                .Include(m => m.Aktiviteler)
                .FirstOrDefaultAsync(m => m.Id == musteriId);

            if (musteri == null)
            {
                return NotFound();
            }

            // Tarih değişkenini local olarak tanımla
            var altiAyOnce = DateTime.Now.AddMonths(-6);

            var istatistikler = new
            {
                // Bu müşterinin aktivite türleri
                aktiviteTurleri = musteri.Aktiviteler
                    .GroupBy(a => a.Tur)
                    .Select(g => new {
                        Tur = g.Key.ToString(),
                        Sayi = g.Count()
                    })
                    .OrderByDescending(x => x.Sayi)
                    .ToList(),

                // Bu müşterinin aylık aktivitesi (Son 6 ay)  
                aylikTrend = musteri.Aktiviteler
                    .Where(a => a.OlusturulmaTarihi >= altiAyOnce)
                    .GroupBy(a => new {
                        Yil = a.OlusturulmaTarihi.Year,
                        Ay = a.OlusturulmaTarihi.Month
                    })
                    .Select(g => new {
                        Tarih = $"{g.Key.Yil}-{g.Key.Ay:00}",
                        Sayi = g.Count()
                    })
                    .OrderBy(x => x.Tarih)
                    .ToList(),

                // Genel bilgiler
                toplamAktivite = musteri.Aktiviteler.Count,
                sonAktiviteTarihi = musteri.Aktiviteler
                    .OrderByDescending(a => a.OlusturulmaTarihi)
                    .FirstOrDefault()?.OlusturulmaTarihi,
                enCokKullanilan = musteri.Aktiviteler
                    .GroupBy(a => a.Tur)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key.ToString()
            };

            return Json(istatistikler);
        }

        // Etiket ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EtiketEkle(int musteriId, int etiketId, string notlar = "")
        {
            var musteri = await _context.Musteriler.FindAsync(musteriId);
            var etiket = await _context.MusteriEtiketleri.FindAsync(etiketId);
            
            if (musteri == null || etiket == null)
            {
                TempData["Error"] = "Müşteri veya etiket bulunamadı.";
                return RedirectToAction("Detay", new { id = musteriId });
            }

            // Aynı etiket zaten atanmış mı kontrol et
            var mevcutAtama = await _context.MusteriEtiketAtamalari
                .FirstOrDefaultAsync(ea => ea.MusteriId == musteriId && ea.MusteriEtiketiId == etiketId);
            
            if (mevcutAtama != null)
            {
                TempData["Error"] = "Bu etiket zaten atanmış.";
                return RedirectToAction("Detay", new { id = musteriId });
            }

            // Yeni etiket ataması oluştur
            var etiketAtama = new MusteriEtiketAtama
            {
                MusteriId = musteriId,
                MusteriEtiketiId = etiketId,
                Notlar = notlar,
                AtamaTarihi = DateTime.Now
            };

            _context.MusteriEtiketAtamalari.Add(etiketAtama);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Etiket başarıyla eklendi.";
            return RedirectToAction("Detay", new { id = musteriId });
        }

        // Etiket kaldırma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EtiketKaldir(int musteriId, int etiketId)
        {
            var etiketAtama = await _context.MusteriEtiketAtamalari
                .FirstOrDefaultAsync(ea => ea.MusteriId == musteriId && ea.MusteriEtiketiId == etiketId);
            
            if (etiketAtama == null)
            {
                TempData["Error"] = "Etiket ataması bulunamadı.";
                return RedirectToAction("Detay", new { id = musteriId });
            }

            _context.MusteriEtiketAtamalari.Remove(etiketAtama);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Etiket kaldırıldı.";
            return RedirectToAction("Detay", new { id = musteriId });
        }

        // Etiket yönetimi sayfası
        public async Task<IActionResult> EtiketYonetimi(int id)
        {
            var musteri = await _context.Musteriler
                .Include(m => m.EtiketAtamalari)
                    .ThenInclude(ea => ea.MusteriEtiketi)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (musteri == null)
            {
                TempData["Error"] = "Müşteri bulunamadı.";
                return RedirectToAction("Ara");
            }

            ViewBag.MevcutEtiketler = musteri.EtiketAtamalari.Select(ea => ea.MusteriEtiketi).ToList();
            
            // Kullanılabilir etiketleri ayrı sorgu ile al
            var mevcutEtiketIdleri = musteri.EtiketAtamalari.Select(ea => ea.MusteriEtiketiId).ToList();
            ViewBag.KullanilabilirEtiketler = await _context.MusteriEtiketleri
                .Where(e => e.Aktif && !mevcutEtiketIdleri.Contains(e.Id))
                .ToListAsync();

            return View(musteri);
        }

        // Etiket oluşturma sayfası
        public IActionResult EtiketOlustur()
        {
            return View();
        }

        // Etiket oluşturma (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EtiketOlustur(MusteriEtiketi etiket)
        {
            if (ModelState.IsValid)
            {
                // Aynı isimde etiket var mı kontrol et
                var mevcutEtiket = await _context.MusteriEtiketleri
                    .FirstOrDefaultAsync(e => e.Ad == etiket.Ad);
                
                if (mevcutEtiket != null)
                {
                    ModelState.AddModelError("Ad", "Bu isimde bir etiket zaten mevcut.");
                    return View(etiket);
                }

                etiket.OlusturulmaTarihi = DateTime.Now;
                _context.MusteriEtiketleri.Add(etiket);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Etiket başarıyla oluşturuldu.";
                return RedirectToAction("EtiketListesi");
            }

            return View(etiket);
        }

        // Etiket listesi
        public async Task<IActionResult> EtiketListesi()
        {
            var etiketler = await _context.MusteriEtiketleri
                .OrderBy(e => e.Ad)
                .ToListAsync();
            
            return View(etiketler);
        }

        // Etiket düzenleme sayfası
        public async Task<IActionResult> EtiketDuzenle(int id)
        {
            var etiket = await _context.MusteriEtiketleri.FindAsync(id);
            if (etiket == null)
            {
                TempData["Error"] = "Etiket bulunamadı.";
                return RedirectToAction("EtiketListesi");
            }

            return View(etiket);
        }

        // Etiket düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EtiketDuzenle(MusteriEtiketi etiket)
        {
            if (ModelState.IsValid)
            {
                var mevcutEtiket = await _context.MusteriEtiketleri.FindAsync(etiket.Id);
                if (mevcutEtiket == null)
                {
                    TempData["Error"] = "Etiket bulunamadı.";
                    return RedirectToAction("EtiketListesi");
                }

                // Aynı isimde başka etiket var mı kontrol et
                var ayniIsimdeEtiket = await _context.MusteriEtiketleri
                    .FirstOrDefaultAsync(e => e.Ad == etiket.Ad && e.Id != etiket.Id);
                
                if (ayniIsimdeEtiket != null)
                {
                    ModelState.AddModelError("Ad", "Bu isimde başka bir etiket zaten mevcut.");
                    return View(etiket);
                }

                mevcutEtiket.Ad = etiket.Ad;
                mevcutEtiket.Aciklama = etiket.Aciklama;
                mevcutEtiket.RenkKodu = etiket.RenkKodu;
                mevcutEtiket.Aktif = etiket.Aktif;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Etiket güncellendi.";
                return RedirectToAction("EtiketListesi");
            }

            return View(etiket);
        }

        // Etiket silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EtiketSil(int id)
        {
            var etiket = await _context.MusteriEtiketleri.FindAsync(id);
            if (etiket == null)
            {
                TempData["Error"] = "Etiket bulunamadı.";
                return RedirectToAction("EtiketListesi");
            }

            // Etiket kullanılıyor mu kontrol et
            var kullanilanEtiket = await _context.MusteriEtiketAtamalari
                .AnyAsync(ea => ea.MusteriEtiketiId == id);
            
            if (kullanilanEtiket)
            {
                TempData["Error"] = "Bu etiket kullanıldığı için silinemez. Önce etiket atamalarını kaldırın.";
                return RedirectToAction("EtiketListesi");
            }

            _context.MusteriEtiketleri.Remove(etiket);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Etiket silindi.";
            return RedirectToAction("EtiketListesi");
        }
    }
}

