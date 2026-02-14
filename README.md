# 📞 Çağrı Merkezi Uygulaması

Modern ve kapsamlı bir çağrı merkezi yönetim sistemi. ASP.NET Core MVC ile geliştirilmiş, operatör yönetimi, müşteri takibi, randevu sistemi ve performans raporlama özellikleri içerir.

## ✨ Özellikler

### 👥 Kullanıcı Yönetimi
- **Rol Tabanlı Yetkilendirme**: Admin, Süpervizör ve Operatör rolleri
- **Operatör Durum Yönetimi**: Müsait, Çağrıda, Mola, Toplantıda vb. durumlar
- **Çalışma Saati Takibi**: Günlük giriş/çıkış saatleri ve süre hesaplamaları
- **Canlı Operatör Durumu**: Tüm operatörlerin anlık durumlarını görüntüleme

### 👤 Müşteri Yönetimi
- **Detaylı Müşteri Profilleri**: Ad, soyad, telefon, email, adres ve notlar
- **Hızlı Kayıt Sistemi**: Arama sırasında hızlı müşteri kaydı
- **Müşteri Etiketleme**: VIP, Potansiyel, Şikayetçi gibi özel etiketler
- **Aktivite Geçmişi**: Tüm müşteri etkileşimlerinin detaylı kaydı
- **Arama Geçmişi**: Gelen/giden aramalar ve arama detayları

### 📞 Arama Yönetimi
- **Çağrı Kaydı**: Detaylı çağrı notları ve süre takibi
- **Arama Tipi**: Gelen/Giden arama ayrımı
- **Telefon Entegrasyonu**: Otomatik müşteri tanıma
- **Cevap Şablonları**: Sık kullanılan yanıtlar için hazır şablonlar
- **Şablon Kategorileri**: Organize edilmiş şablon sistemi

### 📅 Randevu Sistemi
- **Randevu Oluşturma**: Müşteriler için randevu planlama
- **Randevu Takibi**: Bekleyen, tamamlanan, iptal edilen randevular
- **Günlük Randevu Listesi**: Bugünün randevularını görüntüleme
- **Randevu Bildirimleri**: Yaklaşan randevular için hatırlatıcılar

### 📊 Raporlama ve Analiz
- **Operatör Performansı**: Günlük, haftalık, aylık performans raporları
- **Arama İstatistikleri**: Toplam arama sayısı, ortalama süre, başarı oranı
- **Günlük Raporlar**: Detaylı günlük aktivite raporları
- **Aylık Raporlar**: Kapsamlı aylık performans analizleri
- **Excel Export**: Raporları Excel formatında dışa aktarma

### 🎨 Kullanıcı Arayüzü
- Modern ve responsive tasarım
- Bootstrap 5 tabanlı arayüz
- Font Awesome ikonları
- Kullanıcı dostu dashboard
- Mobil uyumlu tasarım

## 🛠️ Teknolojiler

- **Framework**: ASP.NET Core MVC (.NET 9.0)
- **ORM**: Entity Framework Core 9.0
- **Veritabanı**: Microsoft SQL Server
- **Authentication**: Cookie-based Authentication
- **Excel Export**: EPPlus 8.1.0
- **UI Framework**: Bootstrap 5
- **Icons**: Font Awesome

## 📋 Gereksinimler

- .NET 9.0 SDK
- SQL Server (LocalDB veya Express)
- Visual Studio 2022 veya Visual Studio Code
- Windows 10/11 veya Linux/macOS (.NET Core desteği ile)

## 🚀 Kurulum

### 1. Projeyi Klonlayın

```bash
git clone https://github.com/[kullanici-adi]/CagriMerkeziUyg.git
cd CagriMerkeziUyg
```

### 2. Veritabanı Bağlantısını Yapılandırın

`appsettings.json` dosyasını oluşturun (örnek dosyadan kopyalayın):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CagriMerkeziDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Veritabanını Oluşturun

```bash
dotnet ef database update
```

### 4. Uygulamayı Çalıştırın

```bash
dotnet run
```

Uygulama `https://localhost:5001` adresinde çalışmaya başlayacaktır.

## 👤 Varsayılan Kullanıcılar

Uygulama ilk çalıştırmada otomatik olarak demo kullanıcılar oluşturur:

### Admin Hesabı
- **Kullanıcı Adı**: `admin`
- **Şifre**: `admin123`
- **Rol**: Admin

### Süpervizör Hesabı
- **Kullanıcı Adı**: `supervisor`
- **Şifre**: `123456`
- **Rol**: Supervisor

### Operatör Hesabı
- **Kullanıcı Adı**: `operator1`
- **Şifre**: `123456`
- **Rol**: Operator

## 📁 Proje Yapısı

```
CagriMerkeziUyg/
├── Controllers/          # MVC Controller'ları
│   ├── AuthController.cs
│   ├── OperatorController.cs
│   ├── MusteriController.cs
│   ├── AdminController.cs
│   └── ...
├── Models/              # Veri modelleri
│   ├── Operator.cs
│   ├── Musteri.cs
│   ├── Randevu.cs
│   └── ...
├── Views/               # Razor view'ları
│   ├── Operator/
│   ├── Musteri/
│   ├── Admin/
│   └── ...
├── Data/                # DbContext ve veritabanı yapılandırması
│   └── CagriMerkeziDbContext.cs
├── Services/            # İş mantığı servisleri
│   ├── SimpleExcelExportService.cs
│   └── OperatorDurumService.cs
├── Migrations/          # EF Core migration dosyaları
└── wwwroot/            # Statik dosyalar (CSS, JS, images)
```

## 🔐 Güvenlik

- Şifrelerin güvenli bir şekilde hash'lenmesi önerilir (BCrypt, PBKDF2)
- Production ortamında güçlü şifreler kullanın
- `appsettings.json` dosyasını asla Git'e commit etmeyin
- HTTPS kullanımı önerilir
- Cookie ayarları güvenlik için optimize edilmiştir

## 🤝 Katkıda Bulunma

1. Bu repo'yu fork edin
2. Feature branch'i oluşturun (`git checkout -b feature/YeniOzellik`)
3. Değişikliklerinizi commit edin (`git commit -m 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/YeniOzellik`)
5. Pull Request oluşturun

## 📝 Yapılacaklar

- [ ] Gerçek zamanlı bildirim sistemi (SignalR)
- [ ] SMS/Email entegrasyonu
- [ ] Müşteri memnuniyet anketi
- [ ] Dark mode desteği
- [ ] Gelişmiş filtreleme ve arama
- [ ] PDF rapor oluşturma
- [ ] API endpoint'leri
- [ ] Mobile uygulama desteği

## 📄 Lisans

Bu proje açık kaynak kodludur ve [MIT lisansı](LICENSE) altında dağıtılmaktadır.

## 📧 İletişim

Sorularınız için issue açabilir veya pull request gönderebilirsiniz.

## 🙏 Teşekkürler

Bu projeyi kullandığınız için teşekkür ederiz! Yıldız ⭐ vermeyi unutmayın!
