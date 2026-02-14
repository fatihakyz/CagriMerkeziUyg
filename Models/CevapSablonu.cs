using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    /// Operatörlerin kullanacağı hızlı yanıt şablonları
    /// Her şablon bir kategoriye aittir
    public class CevapSablonu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori seçimi gereklidir")]
        [Display(Name = "Kategori")]
        public int KategoriId { get; set; }

        [Required(ErrorMessage = "Şablon başlığı gereklidir")]
        [Display(Name = "Başlık")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şablon içeriği gereklidir")]
        [Display(Name = "İçerik")]
        [StringLength(2000, ErrorMessage = "İçerik en fazla 2000 karakter olabilir")]
        public string Icerik { get; set; } = string.Empty;

        [Display(Name = "Operatör Notu")]
        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
        public string? Notlar { get; set; }  // Operatör için görünür not 

        [Display(Name = "Sıra")]
        public int Sira { get; set; } = 0;

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        // İstatistik alanları
        [Display(Name = "Kullanım Sayısı")]
        public int KullanimSayisi { get; set; } = 0;

        [Display(Name = "Son Kullanım Tarihi")]
        public DateTime? SonKullanimTarihi { get; set; }

        [Display(Name = "Değişken İçeriyor")]
        public bool DegiskenIceriyor { get; set; } = false;  

        [Display(Name = "Kısayol Tuşu")]
        [StringLength(20)]
        public string? KisaYol { get; set; }  // Örnek: "F1", "Ctrl+1"

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Oluşturan Operatör")]
        public int? OlusturanOperatorId { get; set; }

        // Navigation Properties
        public virtual CevapSablonKategori Kategori { get; set; } = null!;
        public virtual Operator? OlusturanOperator { get; set; }
        public virtual ICollection<CevapSablonKullanim> KullanimKayitlari { get; set; } = new List<CevapSablonKullanim>();
    }
}












