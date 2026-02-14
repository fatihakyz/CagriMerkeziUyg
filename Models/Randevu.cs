using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Randevu başlığı gereklidir")]
        [Display(Name = "Başlık")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Baslik { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Randevu tarihi ve saati gereklidir")]
        [Display(Name = "Randevu Tarihi ve Saati")]
        public DateTime RandevuZamani { get; set; }

        [Display(Name = "Bitiş Tarihi ve Saati")]
        public DateTime? BitisZamani { get; set; }

        [Required(ErrorMessage = "Randevu tipi seçiniz")]
        [Display(Name = "Randevu Tipi")]
        public RandevuTipi Tip { get; set; }

        [Required(ErrorMessage = "Randevu durumu gereklidir")]
        [Display(Name = "Durum")]
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;

        [Display(Name = "Öncelik")]
        public RandevuOncelik Oncelik { get; set; } = RandevuOncelik.Normal;

        // İlişkiler
        [Display(Name = "Müşteri")]
        public int? MusteriId { get; set; }
        public virtual Musteri? Musteri { get; set; }

        [Required(ErrorMessage = "Operatör seçimi gereklidir")]
        [Display(Name = "Atanan Operatör")]
        public int OperatorId { get; set; }
        public virtual Operator? Operator { get; set; }

        [Display(Name = "Oluşturan Operatör")]
        public int? OlusturanOperatorId { get; set; }
        public virtual Operator? OlusturanOperator { get; set; }

        // Hatırlatma
        [Display(Name = "Hatırlatma")]
        public bool HatirlatmaAktif { get; set; } = true;

        [Display(Name = "Hatırlatma Süresi (Dakika)")]
        [Range(5, 1440, ErrorMessage = "Hatırlatma süresi 5-1440 dakika arası olmalıdır")]
        public int HatirlatmaSuresi { get; set; } = 15; // 15 dakika önceden

        [Display(Name = "Hatırlatma Gönderildi")]
        public bool HatirlatmaGonderildi { get; set; } = false;

        // Notlar ve tamamlanma
        [Display(Name = "Tamamlanma Notu")]
        [StringLength(1000)]
        public string? TamamlanmaNotu { get; set; }

        [Display(Name = "Tamamlanma Tarihi")]
        public DateTime? TamamlanmaTarihi { get; set; }

        // Sistem alanları
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Son Güncelleme")]
        public DateTime SonGuncelleme { get; set; } = DateTime.Now;

        // Computed Properties
        [Display(Name = "Süre (Dakika)")]
        public int? Sure
        {
            get
            {
                if (BitisZamani.HasValue)
                {
                    return (int)(BitisZamani.Value - RandevuZamani).TotalMinutes;
                }
                return null;
            }
        }

        [Display(Name = "Randevu Bilgisi")]
        public string RandevuBilgisi
        {
            get
            {
                var sure = Sure.HasValue ? $" ({Sure} dk)" : "";
                return $"{Baslik} - {RandevuZamani:dd.MM.yyyy HH:mm}{sure}";
            }
        }

        [Display(Name = "Geçti mi?")]
        public bool Gecti => RandevuZamani < DateTime.Now;

        [Display(Name = "Bugün mü?")]
        public bool Bugun => RandevuZamani.Date == DateTime.Today;

        [Display(Name = "Yaklaşıyor mu?")]
        public bool Yaklasıyor
        {
            get
            {
                if (Durum == RandevuDurumu.Bekliyor)
                {
                    var fark = (RandevuZamani - DateTime.Now).TotalMinutes;
                    return fark > 0 && fark <= 60; // 1 saat içinde
                }
                return false;
            }
        }

        // Renk kodları (Takvim için)
        public string RenkKodu
        {
            get
            {
                return Durum switch
                {
                    RandevuDurumu.Bekliyor => Oncelik switch
                    {
                        RandevuOncelik.Dusuk => "#17a2b8",     // info (mavi)
                        RandevuOncelik.Normal => "#28a745",    // success (yeşil)
                        RandevuOncelik.Yuksek => "#ffc107",    // warning (sarı)
                        RandevuOncelik.Acil => "#dc3545",      // danger (kırmızı)
                        _ => "#6c757d"
                    },
                    RandevuDurumu.Tamamlandi => "#6c757d",     // secondary (gri)
                    RandevuDurumu.Iptal => "#343a40",          // dark (koyu gri)
                    RandevuDurumu.Ertelendi => "#fd7e14",      // orange
                    _ => "#007bff"                             // primary (mavi)
                };
            }
        }
    }

    public enum RandevuTipi
    {
        [Display(Name = "Geri Arama")]
        GeriArama,

        [Display(Name = "Teknik Destek")]
        TeknıkDestek,

        [Display(Name = "Satış Görüşmesi")]
        SatısGorusmesi,

        [Display(Name = "Şikayet Takibi")]
        SikayetTakibi,

        [Display(Name = "Ekip Toplantısı")]
        EkipToplantisi,

        [Display(Name = "Performans Görüşmesi")]
        PerformansGorusmesi,

        [Display(Name = "Eğitim")]
        Egitim,

        [Display(Name = "Diğer")]
        Diger
    }

    public enum RandevuDurumu
    {
        [Display(Name = "Bekliyor")]
        Bekliyor,

        [Display(Name = "Tamamlandı")]
        Tamamlandi,

        [Display(Name = "İptal Edildi")]
        Iptal,

        [Display(Name = "Ertelendi")]
        Ertelendi,

        [Display(Name = "Devam Ediyor")]
        DevamEdiyor
    }

    public enum RandevuOncelik
    {
        [Display(Name = "Düşük")]
        Dusuk = 1,

        [Display(Name = "Normal")]
        Normal = 2,

        [Display(Name = "Yüksek")]
        Yuksek = 3,

        [Display(Name = "Acil")]
        Acil = 4
    }
}















