using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    public class OperatorPerformans
    {
        public int Id { get; set; }

        [Required]
        public int OperatorId { get; set; }

        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; }

        [Display(Name = "Çözülen Çağrı Sayısı")]
        public int CozulenCagriSayisi { get; set; }

        [Display(Name = "Toplam Çağrı Sayısı")]
        public int ToplamCagriSayisi { get; set; }

        [Display(Name = "Ortalama Çağrı Süresi (dk)")]
        public decimal OrtalamaCagriSuresi { get; set; }

        [Display(Name = "Müşteri Memnuniyet Puanı")]
        public decimal? MusteriMemnuniyetPuani { get; set; }

        [Display(Name = "Aktif Çalışma Süresi (dk)")]
        public decimal AktifCalismaSuresi { get; set; }

        [Display(Name = "Toplam Mola Süresi (dk)")]
        public decimal ToplamMolaSuresi { get; set; }

        [Display(Name = "Performans Puanı")]
        public decimal PerformansPuani { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notlar { get; set; }

        // Navigation property
        public virtual Operator Operator { get; set; } = null!;
    }

    public class OperatorPerformansOzeti
    {
        public int OperatorId { get; set; }
        public string OperatorAdi { get; set; } = string.Empty;
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public int ToplamCagri { get; set; }
        public int CozulenCagri { get; set; }
        public decimal CozumOrani { get; set; }
        public decimal OrtalamaCagriSuresi { get; set; }
        public decimal ToplamCalismaSaati { get; set; }
        public decimal OrtalamaMemnuniyet { get; set; }
        public decimal GenelPerformansPuani { get; set; }
    }

    public class AylikRaporOperatorPerformans
    {
        public string OperatorAdi { get; set; } = string.Empty;
        public string OperatorEmail { get; set; } = string.Empty;
        public int ToplamAktivite { get; set; }
        public int CozulenAktivite { get; set; }
        public int BekleyenAktivite { get; set; }
        public int IptalEdilenAktivite { get; set; }
        public decimal OrtalamaCagriSuresi { get; set; }
        public decimal MusteriMemnuniyetPuani { get; set; }
    }
}
















