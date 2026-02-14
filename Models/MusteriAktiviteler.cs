using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    public class MusteriAktiviteler
    {
        public int Id { get; set; }  //Her Aktivite için benzersiz id//
        [Required]
        public int MusteriId { get; set; }  //Aktivitenin ait olduğu müşteri id//
        
        public int? OperatorId { get; set; }  //Aktiviteyi yapan operatör id//
        
        [Required(ErrorMessage = "Aktivite türü seçiniz")]
        public AktiviteTuru Tur { get; set; }

        [Required(ErrorMessage = "Konu gereklidir")]
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir")]
        [Display(Name = "Konu")]
        public string Konu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aktivite açıklaması gereklidir")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Aciklama { get; set; } = string.Empty;
        
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;  //Aktivitenin oluşturulma tarihi//
        
        public DateTime? CozumTarihi { get; set; }  //Çözüm tarihi//
        
        [Display(Name = "Durum")]
        public AktiviteDurumu Durum { get; set; } = AktiviteDurumu.Yeni;
        
        [Display(Name = "Öncelik")]
        public AktiviteOncelik Oncelik { get; set; } = AktiviteOncelik.Normal;
        
        [Display(Name = "Çağrı Süresi (dk)")]
        public decimal? CagriSuresi { get; set; }
        
        [Display(Name = "Müşteri Memnuniyet")]
        [Range(1, 5, ErrorMessage = "Memnuniyet puanı 1-5 arasında olmalıdır")]
        public int? MusteriMemnuniyet { get; set; }
        
        public virtual Musteri Musteri { get; set; } = null!;  //Aktivitenin ait olduğu müşteri bilgisi//
        public virtual Operator? Operator { get; set; }  //Aktiviteyi yapan operatör bilgisi//

    }
    public enum AktiviteTuru
    {
        Sikayet,
        Talep,
        Bilgi,
        Oneri,
        Diger
    }

    public enum AktiviteDurumu
    {
        [Display(Name = "Yeni")]
        Yeni,
        
        [Display(Name = "İşlemde")]
        Islemde,
        
        [Display(Name = "Beklemede")]
        Beklemede,
        
        [Display(Name = "Çözümlendi")]
        Cozumlendi,
        
        [Display(Name = "İptal Edildi")]
        IptalEdildi
    }

    public enum AktiviteOncelik
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
