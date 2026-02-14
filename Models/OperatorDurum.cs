using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    /// Operatör durum bilgisi
    public enum OperatorDurumu
    {
        [Display(Name = "Çevrimdışı")]
        Offline = 0,

        [Display(Name = "Müsait")]
        Musait = 1,

        [Display(Name = "Çağrıda")]
        Cagirida = 2,

        [Display(Name = "Ara Çalışma")]
        AraCalısma = 3,  // Wrap-up: Çağrı sonrası not alma, işlemleri tamamlama

        [Display(Name = "Mola")]
        Mola = 4,

        [Display(Name = "Öğle Yemeği")]
        OgleYemegi = 5,

        [Display(Name = "Eğitimde")]
        Egitimde = 6,

        [Display(Name = "Toplantıda")]
        Toplantida = 7,

        [Display(Name = "Uzakta")]
        Uzakta = 8,  // Away

        [Display(Name = "Meşgul")]
        Mesgul = 9
    }

    /// Operatör durum geçişi kaydı 
    public class OperatorDurumGecmisi
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Operatör")]
        public int OperatorId { get; set; }
        public virtual Operator? Operator { get; set; }

        [Required]
        [Display(Name = "Önceki Durum")]
        public OperatorDurumu OncekiDurum { get; set; }

        [Required]
        [Display(Name = "Yeni Durum")]
        public OperatorDurumu YeniDurum { get; set; }

        [Required]
        [Display(Name = "Geçiş Zamanı")]
        public DateTime GecisZamani { get; set; } = DateTime.Now;

        [Display(Name = "Bitiş Zamanı")]
        public DateTime? BitisZamani { get; set; }

        [Display(Name = "Süre (Dakika)")]
        public decimal? SureDakika
        {
            get
            {
                if (BitisZamani.HasValue)
                {
                    return (decimal)(BitisZamani.Value - GecisZamani).TotalMinutes;
                }
                return null;
            }
        }

        [Display(Name = "Not")]
        [StringLength(500)]
        public string? Not { get; set; }

        [Display(Name = "Otomatik Geçiş")]
        public bool OtomatikGecis { get; set; } = false;  // Sistem mi yaptı yoksa kullanıcı mı?

        // İlgili çağrı varsa
        [Display(Name = "İlgili Çağrı")]
        public int? IlgiliAramaLogId { get; set; }
        public virtual AramaLog? IlgiliAramaLog { get; set; }
    }

    /// Operatör günlük durum özeti - Dashboard ve raporlar için
    public class OperatorGunlukDurumOzeti
    {
        public int Id { get; set; }

        [Required]
        public int OperatorId { get; set; }
        public virtual Operator? Operator { get; set; }

        [Required]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; }

        [Display(Name = "Toplam Çalışma Süresi (dk)")]
        public decimal ToplamCalismaSuresi { get; set; }

        [Display(Name = "Çağrıda Geçen Süre (dk)")]
        public decimal CagrıdaGecenSure { get; set; }

        [Display(Name = "Ara Çalışma Süresi (dk)")]
        public decimal AraCalismaSuresi { get; set; }

        [Display(Name = "Müsait Süre (dk)")]
        public decimal MusaitSure { get; set; }

        [Display(Name = "Mola Süresi (dk)")]
        public decimal MolaSuresi { get; set; }

        [Display(Name = "Öğle Yemeği Süresi (dk)")]
        public decimal OgleYemegiSuresi { get; set; }

        [Display(Name = "Toplantı Süresi (dk)")]
        public decimal ToplantiSuresi { get; set; }

        [Display(Name = "Eğitim Süresi (dk)")]
        public decimal EgitimSuresi { get; set; }

        [Display(Name = "Toplam Çağrı Sayısı")]
        public int ToplamCagriSayisi { get; set; }

        [Display(Name = "Ortalama Çağrı Süresi (dk)")]
        public decimal? OrtalamaCagriSuresi { get; set; }

        [Display(Name = "Ortalama Ara Çalışma Süresi (dk)")]
        public decimal? OrtalamaAraCalismaSuresi { get; set; }

        [Display(Name = "Verimlilik Oranı %")]
        public decimal VerimlilıkOrani
        {
            get
            {
                if (ToplamCalismaSuresi > 0)
                {
                    var produktifSure = CagrıdaGecenSure + AraCalismaSuresi;
                    return Math.Round((produktifSure / ToplamCalismaSuresi) * 100, 2);
                }
                return 0;
            }
        }

        [Display(Name = "Kullanım Oranı %")]
        public decimal KullanimOrani
        {
            get
            {
                if (ToplamCalismaSuresi > 0)
                {
                    return Math.Round((CagrıdaGecenSure / ToplamCalismaSuresi) * 100, 2);
                }
                return 0;
            }
        }

        [Display(Name = "Oluşturulma Zamanı")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Son Güncelleme")]
        public DateTime SonGuncelleme { get; set; } = DateTime.Now;
    }
}












