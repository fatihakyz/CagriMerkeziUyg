using CagriMerkeziUyg.Data;
using CagriMerkeziUyg.Models;
using Microsoft.EntityFrameworkCore;

namespace CagriMerkeziUyg.Services
{
    /// Operatör durum yönetimi servisi
    public class OperatorDurumService
    {
        private readonly CagriMerkeziDbContext _context;

        public OperatorDurumService(CagriMerkeziDbContext context)
        {
            _context = context;
        }
        /// Operatör durumunu değiştir ve geçmişe kaydet
        public async Task<(bool success, string message)> DurumDegistir(
            int operatorId, 
            OperatorDurumu yeniDurum, 
            string? not = null, 
            bool otomatikGecis = false,
            int? ilgiliAramaLogId = null)
        {
            try
            {
                var operatorEntity = await _context.Operatorler.FindAsync(operatorId);
                if (operatorEntity == null)
                {
                    return (false, "Operatör bulunamadı");
                }

                var oncekiDurum = operatorEntity.MevcutDurum;

                // Eğer durum aynıysa işlem yapma
                if (oncekiDurum == yeniDurum && !otomatikGecis)
                {
                    return (false, "Operatör zaten bu durumda");
                }

                // Önceki durum kaydının bitiş zamanını güncelle
                var oncekiKayit = await _context.OperatorDurumGecmisleri
                    .Where(d => d.OperatorId == operatorId && d.BitisZamani == null)
                    .OrderByDescending(d => d.GecisZamani)
                    .FirstOrDefaultAsync();

                if (oncekiKayit != null)
                {
                    oncekiKayit.BitisZamani = DateTime.Now;
                    _context.Update(oncekiKayit);
                }

                // Yeni durum kaydı oluştur
                var yeniKayit = new OperatorDurumGecmisi
                {
                    OperatorId = operatorId,
                    OncekiDurum = oncekiDurum,
                    YeniDurum = yeniDurum,
                    GecisZamani = DateTime.Now,
                    Not = not,
                    OtomatikGecis = otomatikGecis,
                    IlgiliAramaLogId = ilgiliAramaLogId
                };

                _context.OperatorDurumGecmisleri.Add(yeniKayit);

                // Operatör durumunu güncelle
                operatorEntity.MevcutDurum = yeniDurum;
                operatorEntity.SonDurumDegisikliği = DateTime.Now;
                operatorEntity.DurumNotu = not;

                // İlk giriş mi? (Offline'dan müsait'e geçiş)
                if (oncekiDurum == OperatorDurumu.Offline && 
                    (yeniDurum == OperatorDurumu.Musait || yeniDurum == OperatorDurumu.Cagirida))
                {
                    operatorEntity.GununBaslangicSaati = DateTime.Now;
                    operatorEntity.SonGiris = DateTime.Now;
                }

                // Çıkış mı? (Müsait veya başka durumdan Offline'a geçiş)
                if (yeniDurum == OperatorDurumu.Offline)
                {
                    operatorEntity.GununBitisSaati = DateTime.Now;
                }

                _context.Update(operatorEntity);
                await _context.SaveChangesAsync();

                return (true, $"Durum başarıyla değiştirildi: {yeniDurum}");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }
        /// Operatörün bugünkü durum geçmişini getir
        public async Task<List<OperatorDurumGecmisi>> BugunkuDurumGecmisiniGetir(int operatorId)
        {
            var bugun = DateTime.Today;
            return await _context.OperatorDurumGecmisleri
                .Where(d => d.OperatorId == operatorId && d.GecisZamani.Date == bugun)
                .OrderBy(d => d.GecisZamani)
                .ToListAsync();
        }

        /// Tüm operatörlerin anlık durumunu getir (Dashboard için)
        public async Task<List<OperatorDurumBilgisi>> TumOperatorDurumlariniGetir()
        {
            var operatorler = await _context.Operatorler
                .Where(o => o.Aktif)
                .OrderBy(o => o.MevcutDurum)
                .ThenBy(o => o.Ad)
                .ToListAsync();

            return operatorler.Select(o => new OperatorDurumBilgisi
            {
                OperatorId = o.Id,
                Ad = o.Ad,
                Soyad = o.Soyad,
                TamAd = o.TamAd,
                MevcutDurum = o.MevcutDurum,
                SonDurumDegisikliği = o.SonDurumDegisikliği,
                DurumSuresi = o.SonDurumDegisikliği.HasValue 
                    ? (DateTime.Now - o.SonDurumDegisikliği.Value).TotalMinutes 
                    : 0,
                DurumNotu = o.DurumNotu,
                DurumRengi = o.DurumRengi,
                DurumIkon = o.DurumIkon
            }).ToList();
        }

        /// Operatörün günlük özet raporunu hesapla ve kaydet
        public async Task<OperatorGunlukDurumOzeti> GunlukOzetHesapla(int operatorId, DateTime tarih)
        {
            // Mevcut özet var mı kontrol et
            var mevcutOzet = await _context.OperatorGunlukDurumOzetleri
                .FirstOrDefaultAsync(o => o.OperatorId == operatorId && o.Tarih.Date == tarih.Date);

            // O günün durum geçmişlerini al
            var durumGecmisleri = await _context.OperatorDurumGecmisleri
                .Where(d => d.OperatorId == operatorId && d.GecisZamani.Date == tarih.Date)
                .OrderBy(d => d.GecisZamani)
                .ToListAsync();

            if (!durumGecmisleri.Any())
            {
                return mevcutOzet ?? new OperatorGunlukDurumOzeti { OperatorId = operatorId, Tarih = tarih.Date };
            }

            // Süreleri hesapla
            decimal toplamCalismaSuresi = 0;
            decimal cagrıdaGecenSure = 0;
            decimal araCalismaSuresi = 0;
            decimal musaitSure = 0;
            decimal molaSuresi = 0;
            decimal ogleYemegiSuresi = 0;
            decimal toplantiSuresi = 0;
            decimal egitimSuresi = 0;

            foreach (var gecis in durumGecmisleri)
            {
                var sure = gecis.SureDakika ?? 0;
                
                switch (gecis.YeniDurum)
                {
                    case OperatorDurumu.Cagirida:
                        cagrıdaGecenSure += sure;
                        toplamCalismaSuresi += sure;
                        break;
                    case OperatorDurumu.AraCalısma:
                        araCalismaSuresi += sure;
                        toplamCalismaSuresi += sure;
                        break;
                    case OperatorDurumu.Musait:
                        musaitSure += sure;
                        toplamCalismaSuresi += sure;
                        break;
                    case OperatorDurumu.Mola:
                        molaSuresi += sure;
                        break;
                    case OperatorDurumu.OgleYemegi:
                        ogleYemegiSuresi += sure;
                        break;
                    case OperatorDurumu.Toplantida:
                        toplantiSuresi += sure;
                        break;
                    case OperatorDurumu.Egitimde:
                        egitimSuresi += sure;
                        break;
                }
            }

            // Çağrı istatistikleri
            var gunlukCagrilar = await _context.AramaLoglari
                .Where(a => a.OperatorId == operatorId && a.BaslangicZamani.Date == tarih.Date)
                .ToListAsync();

            var toplamCagriSayisi = gunlukCagrilar.Count;
            var ortalamaCagriSuresi = gunlukCagrilar.Any(c => c.Sure.HasValue) 
                ? (decimal)gunlukCagrilar.Where(c => c.Sure.HasValue).Average(c => c.Sure!.Value)
                : 0;

            // Ara çalışma süreleri
            var araCalismaSureleri = durumGecmisleri
                .Where(d => d.YeniDurum == OperatorDurumu.AraCalısma && d.SureDakika.HasValue)
                .Select(d => d.SureDakika!.Value)
                .ToList();
            
            var ortalamaAraCalismaSuresi = araCalismaSureleri.Any() 
                ? araCalismaSureleri.Average() 
                : 0;

            // Özet oluştur veya güncelle
            if (mevcutOzet == null)
            {
                mevcutOzet = new OperatorGunlukDurumOzeti
                {
                    OperatorId = operatorId,
                    Tarih = tarih.Date,
                    OlusturulmaTarihi = DateTime.Now
                };
                _context.OperatorGunlukDurumOzetleri.Add(mevcutOzet);
            }

            mevcutOzet.ToplamCalismaSuresi = toplamCalismaSuresi;
            mevcutOzet.CagrıdaGecenSure = cagrıdaGecenSure;
            mevcutOzet.AraCalismaSuresi = araCalismaSuresi;
            mevcutOzet.MusaitSure = musaitSure;
            mevcutOzet.MolaSuresi = molaSuresi;
            mevcutOzet.OgleYemegiSuresi = ogleYemegiSuresi;
            mevcutOzet.ToplantiSuresi = toplantiSuresi;
            mevcutOzet.EgitimSuresi = egitimSuresi;
            mevcutOzet.ToplamCagriSayisi = toplamCagriSayisi;
            mevcutOzet.OrtalamaCagriSuresi = ortalamaCagriSuresi;
            mevcutOzet.OrtalamaAraCalismaSuresi = ortalamaAraCalismaSuresi;
            mevcutOzet.SonGuncelleme = DateTime.Now;

            await _context.SaveChangesAsync();

            return mevcutOzet;
        }

        /// Uzun süreli durumda kalan operatörleri bul (Uyarı için)
        public async Task<List<OperatorUyariBilgisi>> UzunSureliDurumKontrol()
        {
            var uyarilar = new List<OperatorUyariBilgisi>();
            var operatorler = await _context.Operatorler
                .Where(o => o.Aktif && o.MevcutDurum != OperatorDurumu.Offline)
                .ToListAsync();

            foreach (var op in operatorler)
            {
                if (!op.SonDurumDegisikliği.HasValue) continue;

                var gecenSure = (DateTime.Now - op.SonDurumDegisikliği.Value).TotalMinutes;

                // Uyarı kriterleri
                string? uyariMesaji = op.MevcutDurum switch
                {
                    OperatorDurumu.Mola when gecenSure > 20 => $"Mola süresi çok uzun ({gecenSure:0} dk)",
                    OperatorDurumu.OgleYemegi when gecenSure > 60 => $"Öğle yemeği süresi çok uzun ({gecenSure:0} dk)",
                    OperatorDurumu.AraCalısma when gecenSure > 10 => $"Ara çalışma süresi çok uzun ({gecenSure:0} dk)",
                    OperatorDurumu.Musait when gecenSure > 30 => $"Uzun süredir müsait ({gecenSure:0} dk)",
                    OperatorDurumu.Cagirida when gecenSure > 45 => $"Çağrı süresi çok uzun ({gecenSure:0} dk)",
                    _ => null
                };

                if (uyariMesaji != null)
                {
                    uyarilar.Add(new OperatorUyariBilgisi
                    {
                        OperatorId = op.Id,
                        OperatorAdi = op.TamAd,
                        MevcutDurum = op.MevcutDurum,
                        GecenSure = gecenSure,
                        UyariMesaji = uyariMesaji,
                        OncelikSeviyesi = gecenSure > 60 ? "Yüksek" : "Normal"
                    });
                }
            }

            return uyarilar;
        }
    }

    // Yardımcı sınıflar
    public class OperatorDurumBilgisi
    {
        public int OperatorId { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string TamAd { get; set; } = string.Empty;
        public OperatorDurumu MevcutDurum { get; set; }
        public DateTime? SonDurumDegisikliği { get; set; }
        public double DurumSuresi { get; set; }  // Dakika
        public string? DurumNotu { get; set; }
        public string DurumRengi { get; set; } = string.Empty;
        public string DurumIkon { get; set; } = string.Empty;
    }

    public class OperatorUyariBilgisi
    {
        public int OperatorId { get; set; }
        public string OperatorAdi { get; set; } = string.Empty;
        public OperatorDurumu MevcutDurum { get; set; }
        public double GecenSure { get; set; }
        public string UyariMesaji { get; set; } = string.Empty;
        public string OncelikSeviyesi { get; set; } = string.Empty;
    }
}












