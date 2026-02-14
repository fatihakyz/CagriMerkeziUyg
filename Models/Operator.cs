using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    public class Operator
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Operatör adı gereklidir")]
        [Display(Name = "Ad")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Operatör soyadı gereklidir")]
        [Display(Name = "Soyad")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [Display(Name = "Kullanıcı Adı")]
        [StringLength(30, ErrorMessage = "Kullanıcı adı en fazla 30 karakter olabilir")]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email gereklidir")]
        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Telefon { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [Display(Name = "Şifre")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 50 karakter olmalıdır")]
        public string Sifre { get; set; } = "123456"; // Varsayılan şifre

        [Required(ErrorMessage = "Rol seçimi gereklidir")]
        [Display(Name = "Rol")]
        public OperatorRolu Rol { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Kayıt Tarihi")]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Son Giriş")]
        public DateTime? SonGiris { get; set; }

        [Display(Name = "Çalışma Saati Başlangıç")]
        public TimeSpan? CalismaSaatiBaslangic { get; set; }

        [Display(Name = "Çalışma Saati Bitiş")]
        public TimeSpan? CalismaSaatiBitis { get; set; }

        // DURUM YÖNETİMİ
        [Display(Name = "Mevcut Durum")]
        public OperatorDurumu MevcutDurum { get; set; } = OperatorDurumu.Offline;

        [Display(Name = "Son Durum Değişikliği")]
        public DateTime? SonDurumDegisikliği { get; set; }

        [Display(Name = "Durum Notu")]
        [StringLength(200)]
        public string? DurumNotu { get; set; }

        [Display(Name = "Günün Başlangıç Saati")]
        public DateTime? GununBaslangicSaati { get; set; }  // Bugün ilk giriş saati

        [Display(Name = "Günün Bitiş Saati")]
        public DateTime? GununBitisSaati { get; set; }  // Bugün çıkış saati

        // Tam ad property'si
        [Display(Name = "Tam Ad")]
        public string TamAd => $"{Ad} {Soyad}";

        // Durum badge rengi
        public string DurumRengi
        {
            get
            {
                return MevcutDurum switch
                {
                    OperatorDurumu.Musait => "success",      // Yeşil
                    OperatorDurumu.Cagirida => "danger",     // Kırmızı
                    OperatorDurumu.AraCalısma => "warning",  // Sarı
                    OperatorDurumu.Mola => "info",           // Mavi
                    OperatorDurumu.OgleYemegi => "info",     // Mavi
                    OperatorDurumu.Toplantida => "secondary",// Gri
                    OperatorDurumu.Egitimde => "primary",    // Lacivert
                    OperatorDurumu.Offline => "dark",        // Siyah
                    OperatorDurumu.Mesgul => "warning",      // Sarı
                    OperatorDurumu.Uzakta => "secondary",    // Gri
                    _ => "secondary"
                };
            }
        }

        // Durum ikonu
        public string DurumIkon
        {
            get
            {
                return MevcutDurum switch
                {
                    OperatorDurumu.Musait => "fa-check-circle",
                    OperatorDurumu.Cagirida => "fa-phone",
                    OperatorDurumu.AraCalısma => "fa-pen",
                    OperatorDurumu.Mola => "fa-coffee",
                    OperatorDurumu.OgleYemegi => "fa-utensils",
                    OperatorDurumu.Toplantida => "fa-users",
                    OperatorDurumu.Egitimde => "fa-graduation-cap",
                    OperatorDurumu.Offline => "fa-power-off",
                    OperatorDurumu.Mesgul => "fa-minus-circle",
                    OperatorDurumu.Uzakta => "fa-clock",
                    _ => "fa-question-circle"
                };
            }
        }

        // Navigation properties
        public virtual ICollection<MusteriAktiviteler> Aktiviteler { get; set; } = [];
        public virtual ICollection<OperatorPerformans> PerformansKayitlari { get; set; } = [];
        public virtual ICollection<OperatorDurumGecmisi> DurumGecmisleri { get; set; } = [];
    }

    public enum OperatorRolu
    {
        [Display(Name = "Operatör")]
        Operator,
        
        [Display(Name = "Süpervizör")]
        Supervisor,
        
        [Display(Name = "Admin")]
        Admin
    }
}


