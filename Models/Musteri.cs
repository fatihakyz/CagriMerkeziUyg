using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    public class Musteri
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Display(Name = "Telefon Numarası")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string TelefonNo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Ad gereklidir")]
        [Display(Name = "Ad")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Soyad gereklidir")]
        [Display(Name = "Soyad")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string Soyad { get; set; } = string.Empty;
        
        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? Email { get; set; }
        
        [Display(Name = "Adres")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string? Adres { get; set; }
        
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }
        
        [Display(Name = "Kayıt Tarihi")]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Son Güncelleme")]
        public DateTime SonGuncelleme { get; set; } = DateTime.Now;
        
        [Display(Name = "Notlar")]
        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notlar { get; set; }
        
    [Display(Name = "Müşteri Tipi")]
    public MusteriTipi? MusteriTipi { get; set; }
    
    [Display(Name = "Özel Notlar")]
    [StringLength(1000, ErrorMessage = "Özel notlar en fazla 1000 karakter olabilir")]
    public string? OzelNotlar { get; set; }
    
    // Yeni alanlar - Hızlı kayıt sistemi için
    [Display(Name = "Kayıt Durumu")]
    public MusteriKayitDurumu KayitDurumu { get; set; } = MusteriKayitDurumu.Tam;
    
    [Display(Name = "Geçici Kayıt")]
    public bool GeciciKayit { get; set; } = false;  // Anonim/geçici müşteri mi?
    
    [Display(Name = "Kayıt Tamamlandı")]
    public bool KayitTamamlandi { get; set; } = true;  // Tüm bilgiler dolduruldu mu?
        
        // Tam ad property'si
        [Display(Name = "Tam Ad")]
        public string TamAd => $"{Ad} {Soyad}";
        
        // Telefon property'si (TelefonNo için alias)
        [Display(Name = "Telefon")]
        public string Telefon => TelefonNo;
        
        // Yaş hesaplama
        [Display(Name = "Yaş")]
        public int? Yas 
        { 
            get 
            {
                if (DogumTarihi.HasValue)
                {
                    var today = DateTime.Today;
                    var age = today.Year - DogumTarihi.Value.Year;
                    if (DogumTarihi.Value.Date > today.AddYears(-age)) age--;
                    return age;
                }
                return null;
            }
        }
        public virtual ICollection<MusteriAktiviteler> Aktiviteler { get; set; } = [];
        public virtual ICollection<MusteriEtiketAtama> EtiketAtamalari { get; set; } = [];
    }
}