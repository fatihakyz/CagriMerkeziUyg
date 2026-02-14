using System.ComponentModel.DataAnnotations;

namespace CagriMerkeziUyg.Models
{
    /// Müşteri kaydının tam olup olmadığını belirten enum
    public enum MusteriKayitDurumu
    {
        /// Normal kayıt - Tüm bilgiler eksiksiz doldurulmuş
        [Display(Name = "Tam Kayıt")]
        Tam = 0,

        /// Hızlı kayıt - Sadece ad, soyad, telefon var
        /// Çağrı sırasında hızlıca oluşturulmuş, detaylar eksik
        [Display(Name = "Kısmi Kayıt")]
        Kismi = 1,
        /// Geçici kayıt - Müşteri bilgi vermek istemedi
        /// Anonim kayıt, sonradan gerçek müşteriye bağlanabilir
        [Display(Name = "Geçici Kayıt")]
        Gecici = 2
    }
}












