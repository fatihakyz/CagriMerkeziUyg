using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    /// Hızlı yanıt şablonlarını gruplamak için kategori modeli
    /// Örnek: "Ürün Bilgisi", "İade & İptal", "Teknik Destek"
    public class CevapSablonKategori
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir")]
        [Display(Name = "Kategori Adı")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Aciklama { get; set; }

        [Display(Name = "Icon Sınıfı")]
        [StringLength(50, ErrorMessage = "Icon sınıfı en fazla 50 karakter olabilir")]
        public string? IconClass { get; set; }  

        [Required]
        [Display(Name = "Renk Kodu")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "Renk kodu #RRGGBB formatında olmalıdır")]
        public string RenkKodu { get; set; } = "#007bff";  

        [Display(Name = "Sıra")]
        public int Sira { get; set; } = 0;  

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Aktivite Türü")]
        public AktiviteTuru? AktiviteTuru { get; set; }  // null ise tüm türler için geçerli

        // Navigation Property - Bu kategoriye ait şablonlar
        public virtual ICollection<CevapSablonu> Sablonlar { get; set; } = new List<CevapSablonu>();
    }
}












