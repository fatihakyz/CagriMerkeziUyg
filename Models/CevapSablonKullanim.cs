using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    /// Şablon kullanım geçmişi - istatistik ve analiz için
    /// Hangi operatör, hangi şablonu, ne zaman, hangi müşteri için kullandı
 
    public class CevapSablonKullanim
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Şablon")]
        public int SablonId { get; set; }

        [Required]
        [Display(Name = "Operatör")]
        public int OperatorId { get; set; }

        [Display(Name = "Müşteri")]
        public int? MusteriId { get; set; }

        [Display(Name = "Aktivite")]
        public int? AktiviteId { get; set; }

        [Display(Name = "Kullanım Tarihi")]
        public DateTime KullanimTarihi { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual CevapSablonu Sablon { get; set; } = null!;
        public virtual Operator Operator { get; set; } = null!;
        public virtual Musteri? Musteri { get; set; }
        public virtual MusteriAktiviteler? Aktivite { get; set; }
    }
}












