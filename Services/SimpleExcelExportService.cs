using OfficeOpenXml;
using CagriMerkeziUyg.Models;

namespace CagriMerkeziUyg.Services
{
    public class SimpleExcelExportService
    {
        static SimpleExcelExportService()
        {
            SetLicense();
        }

        private static void SetLicense()
        {
            try
            {
                
                ExcelPackage.License.SetNonCommercialPersonal("Cagri Merkezi Uygulamasi");
            }
            catch 
            {
                
#pragma warning disable CS0618 
                try
                {
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                }
                catch { }
#pragma warning restore CS0618 
            }
        }

        public byte[] ExportPerformansRaporu(List<OperatorPerformansOzeti> performansListesi, string baslik = "Performans Raporu")
        {
            // Lisans ayarı
            SetLicense();
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(baslik);

            // Başlık
            worksheet.Cells[1, 1].Value = "Çağrı Merkezi Performans Raporu";
            worksheet.Cells[2, 1].Value = $"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Kolon başlıkları
            var headers = new string[]
            {
                "Operatör",
                "Başlangıç Tarihi",
                "Bitiş Tarihi", 
                "Toplam Çağrı",
                "Çözülen Çağrı",
                "Çözüm Oranı (%)",
                "Ort. Çağrı Süresi (dk)",
                "Müşteri Memnuniyet",
                "Performans Puanı"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[4, i + 1].Value = headers[i];
            }

            // Veri satırları
            int row = 5;
            foreach (var performans in performansListesi)
            {
                worksheet.Cells[row, 1].Value = performans.OperatorAdi;
                worksheet.Cells[row, 2].Value = performans.BaslangicTarihi.ToString("dd.MM.yyyy");
                worksheet.Cells[row, 3].Value = performans.BitisTarihi.ToString("dd.MM.yyyy");
                worksheet.Cells[row, 4].Value = performans.ToplamCagri;
                worksheet.Cells[row, 5].Value = performans.CozulenCagri;
                worksheet.Cells[row, 6].Value = Math.Round(performans.CozumOrani, 2);
                worksheet.Cells[row, 7].Value = Math.Round(performans.OrtalamaCagriSuresi, 2);
                worksheet.Cells[row, 8].Value = Math.Round(performans.OrtalamaMemnuniyet, 2);
                worksheet.Cells[row, 9].Value = Math.Round(performans.GenelPerformansPuani, 2);
                row++;
            }

            // Kolon genişlikleri
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 12;
            worksheet.Column(5).Width = 12;
            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 18;
            worksheet.Column(8).Width = 18;
            worksheet.Column(9).Width = 15;

            return package.GetAsByteArray();
        }

        public byte[] ExportGunlukRapor(List<MusteriAktiviteler> aktiviteler, DateTime tarih)
        {
            SetLicense();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Günlük Rapor");

            // Başlık
            worksheet.Cells[1, 1].Value = $"Günlük Aktivite Raporu - {tarih:dd.MM.yyyy}";

            // Kolon başlıkları
            var headers = new string[]
            {
                "Müşteri",
                "Telefon",
                "Operatör",
                "Aktivite Türü",
                "Durum",
                "Öncelik",
                "Oluşturulma",
                "Çözüm Tarihi",
                "Süre (dk)",
                "Memnuniyet",
                "Açıklama"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
            }

            // Veri satırları
            int row = 4;
            foreach (var aktivite in aktiviteler)
            {
                worksheet.Cells[row, 1].Value = aktivite.Musteri?.TamAd ?? "Bilinmiyor";
                worksheet.Cells[row, 2].Value = aktivite.Musteri?.TelefonNo ?? "";
                worksheet.Cells[row, 3].Value = aktivite.Operator?.TamAd ?? "Atanmamış";
                worksheet.Cells[row, 4].Value = aktivite.Tur.ToString();
                worksheet.Cells[row, 5].Value = aktivite.Durum.ToString();
                worksheet.Cells[row, 6].Value = aktivite.Oncelik.ToString();
                worksheet.Cells[row, 7].Value = aktivite.OlusturulmaTarihi.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cells[row, 8].Value = aktivite.CozumTarihi?.ToString("dd.MM.yyyy HH:mm") ?? "";
                worksheet.Cells[row, 9].Value = aktivite.CagriSuresi?.ToString() ?? "";
                worksheet.Cells[row, 10].Value = aktivite.MusteriMemnuniyet?.ToString() ?? "";
                worksheet.Cells[row, 11].Value = aktivite.Aciklama.Length > 100 
                    ? $"{aktivite.Aciklama[..100]}..."
                    : aktivite.Aciklama;
                row++;
            }

            // Kolon genişlikleri
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 20;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 12;
            worksheet.Column(6).Width = 10;
            worksheet.Column(7).Width = 18;
            worksheet.Column(8).Width = 18;
            worksheet.Column(9).Width = 10;
            worksheet.Column(10).Width = 12;
            worksheet.Column(11).Width = 40;

            return package.GetAsByteArray();
        }

        public byte[] ExportOperatorListesi(List<Operator> operatorler)
        {
            SetLicense();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Operatör Listesi");

            // Başlık
            worksheet.Cells[1, 1].Value = "Operatör Listesi";

            // Kolon başlıkları
            var headers = new string[]
            {
                "Ad Soyad",
                "Kullanıcı Adı",
                "Email",
                "Telefon",
                "Rol",
                "Durum",
                "Kayıt Tarihi",
                "Çalışma Saatleri"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
            }

            // Veri satırları
            int row = 4;
            foreach (var op in operatorler)
            {
                worksheet.Cells[row, 1].Value = op.TamAd;
                worksheet.Cells[row, 2].Value = op.KullaniciAdi;
                worksheet.Cells[row, 3].Value = op.Email;
                worksheet.Cells[row, 4].Value = op.Telefon;
                worksheet.Cells[row, 5].Value = op.Rol.ToString();
                worksheet.Cells[row, 6].Value = op.Aktif ? "Aktif" : "Pasif";
                worksheet.Cells[row, 7].Value = op.KayitTarihi.ToString("dd.MM.yyyy");
                
                var calismaText = "";
                if (op.CalismaSaatiBaslangic.HasValue && op.CalismaSaatiBitis.HasValue)
                {
                    calismaText = $"{op.CalismaSaatiBaslangic.Value:hh\\:mm} - {op.CalismaSaatiBitis.Value:hh\\:mm}";
                }
                worksheet.Cells[row, 8].Value = calismaText;
                row++;
            }

            // Kolon genişlikleri ayarla
            for (int col = 1; col <= 8; col++)
            {
                worksheet.Column(col).AutoFit();
            }

            return package.GetAsByteArray();
        }

        public byte[] ExportAylikRapor(List<MusteriAktiviteler> aktiviteler, string ayAdi)
        {
            SetLicense();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Aylık Rapor");

            // Başlık
            worksheet.Cells[1, 1].Value = $"Aylık Aktivite Raporu - {ayAdi}";
            worksheet.Cells[2, 1].Value = $"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Kolon başlıkları
            var headers = new[]
            {
                "Müşteri",
                "Telefon",
                "Operatör",
                "Aktivite Türü",
                "Açıklama",
                "Durum",
                "Öncelik",
                "Oluşturulma Tarihi",
                "Çözüm Tarihi",
                "Çağrı Süresi (dk)",
                "Memnuniyet",
                "Açıklama"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[4, i + 1].Value = headers[i];
            }

            // Veri satırları
            int row = 5;
            foreach (var aktivite in aktiviteler)
            {
                worksheet.Cells[row, 1].Value = aktivite.Musteri?.TamAd ?? "Bilinmiyor";
                worksheet.Cells[row, 2].Value = aktivite.Musteri?.Telefon ?? "-";
                worksheet.Cells[row, 3].Value = aktivite.Operator?.TamAd ?? "Atanmamış";
                worksheet.Cells[row, 4].Value = aktivite.Tur.ToString();
                worksheet.Cells[row, 5].Value = aktivite.Konu;
                worksheet.Cells[row, 6].Value = aktivite.Durum.ToString();
                worksheet.Cells[row, 7].Value = aktivite.Oncelik.ToString();
                worksheet.Cells[row, 8].Value = aktivite.OlusturulmaTarihi.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cells[row, 9].Value = aktivite.CozumTarihi?.ToString("dd.MM.yyyy HH:mm") ?? "-";
                worksheet.Cells[row, 10].Value = aktivite.CagriSuresi?.ToString("F1") ?? "-";
                worksheet.Cells[row, 11].Value = aktivite.MusteriMemnuniyet?.ToString() ?? "-";
                worksheet.Cells[row, 12].Value = aktivite.Aciklama ?? "-";
                row++;
            }

            // Kolon genişlikleri
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 20;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 30;
            worksheet.Column(6).Width = 12;
            worksheet.Column(7).Width = 12;
            worksheet.Column(8).Width = 18;
            worksheet.Column(9).Width = 18;
            worksheet.Column(10).Width = 15;
            worksheet.Column(11).Width = 12;
            worksheet.Column(12).Width = 40;

            return package.GetAsByteArray();
        }
    }
}
