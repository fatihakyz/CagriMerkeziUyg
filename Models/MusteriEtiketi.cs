using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    public class MusteriEtiketi
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Etiket adı gereklidir")]
        [Display(Name = "Etiket Adı")]
        [StringLength(50, ErrorMessage = "Etiket adı en fazla 50 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;
        
        [Display(Name = "Açıklama")]
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir")]
        public string? Aciklama { get; set; }
        
        [Required(ErrorMessage = "Renk kodu gereklidir")]
        [Display(Name = "Renk Kodu")]
        [StringLength(7, ErrorMessage = "Geçerli bir renk kodu giriniz (örn: #FF0000)")]
        public string RenkKodu { get; set; } = "#007bff";
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<MusteriEtiketAtama> MusteriEtiketAtamalari { get; set; } = [];
    }
    
    public class MusteriEtiketAtama
    {
        public int Id { get; set; }
        
        [Required]
        public int MusteriId { get; set; }
        
        [Required]
        public int MusteriEtiketiId { get; set; }
        
        [Display(Name = "Atama Tarihi")]
        public DateTime AtamaTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Atayan Operatör")]
        public int? AtayanOperatorId { get; set; }
        
        [Display(Name = "Notlar")]
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        public string? Notlar { get; set; }
        
        // Navigation properties
        public virtual Musteri Musteri { get; set; } = null!;
        public virtual MusteriEtiketi MusteriEtiketi { get; set; } = null!;
        public virtual Operator? AtayanOperator { get; set; }
    }
    
    public enum MusteriTipi
    {
        [Display(Name = "VIP Müşteri")]
        VIP = 1,
        
        [Display(Name = "Düzenli Müşteri")]
        Düzenli = 2,
        
        [Display(Name = "Yeni Müşteri")]
        Yeni = 3,
        
        [Display(Name = "Potansiyel Müşteri")]
        Potansiyel = 4,
        
        [Display(Name = "Riskli Müşteri")]
        Riskli = 5,
        
        [Display(Name = "Şikayetçi Müşteri")]
        Sikayetci = 6,
        
        [Display(Name = "Memnun Müşteri")]
        Memnun = 7,
        
        [Display(Name = "Kurumsal Müşteri")]
        Kurumsal = 8,
        
        [Display(Name = "Bireysel Müşteri")]
        Bireysel = 9,
        
        [Display(Name = "Özel Durum")]
        OzelDurum = 10
    }
}

















