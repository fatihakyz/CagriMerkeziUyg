using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    public enum AramaDurumu
    {
        Baslatildi = 0,
        Devam = 1,
        Tamamlandi = 2,
        Iptal = 3,
        Cevapsiz = 4,
        Mesgul = 5
    }

    public enum AramaTipi
    {
        Giden = 0,
        Gelen = 1
    }

    public class AramaLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Telefon No")]
        public string TelefonNo { get; set; } = string.Empty;

        [Display(Name = "Müşteri")]
        public int? MusteriId { get; set; }
        public virtual Musteri? Musteri { get; set; }

        [Required]
        [Display(Name = "Operatör")]
        public int OperatorId { get; set; }
        public virtual Operator? Operator { get; set; }

        [Display(Name = "Arama Tipi")]
        public AramaTipi Tip { get; set; } = AramaTipi.Giden;

        [Display(Name = "Başlangıç Zamanı")]
        public DateTime BaslangicZamani { get; set; }

        [Display(Name = "Bitiş Zamanı")]
        public DateTime? BitisZamani { get; set; }

        [Display(Name = "Süre (dakika)")]
        public double? Sure { get; set; }

        [Display(Name = "Durum")]
        public AramaDurumu Durum { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notlar")]
        public string? Notlar { get; set; }

    [Display(Name = "Müşteri Memnuniyet")]
    [Range(1, 5)]
    public int? MusteriMemnuniyet { get; set; }
    
    // Müşteri kayıt durumu takibi
    [Display(Name = "Müşteri Kayıtlı mıydı")]
    public bool MusteriKayitliydi { get; set; } = true;  // Çağrı geldiğinde kayıtlı mıydı?
    
    [Display(Name = "Çağrı Sırasında Kayıt Oluşturuldu")]
    public bool CagriSirasindaKayitOlusturuldu { get; set; } = false;  // Hızlı kayıt yapıldı mı?

        [Display(Name = "Tam Süre")]
        public string TamSure
        {
            get
            {
                if (Sure.HasValue)
                {
                    var dakika = (int)Sure.Value;
                    var saniye = (int)((Sure.Value - dakika) * 60);
                    return $"{dakika:00}:{saniye:00}";
                }
                return "-";
            }
        }

        [Display(Name = "Durum Metni")]
        public string DurumMetni
        {
            get
            {
                return Durum switch
                {
                    AramaDurumu.Baslatildi => "Başlatıldı",
                    AramaDurumu.Devam => "Devam",
                    AramaDurumu.Tamamlandi => "Tamamlandı",
                    AramaDurumu.Iptal => "İptal",
                    AramaDurumu.Cevapsiz => "Cevapsız",
                    AramaDurumu.Mesgul => "Meşgul",
                    _ => "Bilinmiyor"
                };
            }
        }

        [Display(Name = "Tip Metni")]
        public string TipMetni
        {
            get
            {
                return Tip switch
                {
                    AramaTipi.Giden => "Giden",
                    AramaTipi.Gelen => "Gelen",
                    _ => "Bilinmiyor"
                };
            }
        }
    }
}















