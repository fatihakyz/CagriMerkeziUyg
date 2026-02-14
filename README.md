# 📞 Çağrı Merkezi Uygulaması

Modern ve kapsamlı bir çağrı merkezi yönetim sistemi. ASP.NET Core MVC ile geliştirilmiş, operatör yönetimi, müşteri takibi, randevu sistemi ve performans raporlama özellikleri içerir.

## ✨ Temel Özellikler

- **👥 Kullanıcı Yönetimi**: Rol tabanlı yetkilendirme (Admin, Süpervizör, Operatör), operatör durum yönetimi ve canlı durum takibi
- **👤 Müşteri Yönetimi**: Detaylı müşteri profilleri, etiketleme sistemi, aktivite ve arama geçmişi
- **📞 Arama Yönetimi**: Çağrı kaydı, gelen/giden arama takibi, cevap şablonları
- **📅 Randevu Sistemi**: Randevu planlama, takip ve hatırlatıcılar
- **📊 Raporlama**: Operatör performans raporları, arama istatistikleri, Excel export
- **🎨 Modern Arayüz**: Responsive tasarım, Bootstrap 5, mobil uyumlu

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

```bash
# 1. Projeyi klonlayın
git clone https://github.com/fatihakyz/CagriMerkeziUyg.git
cd CagriMerkeziUyg

# 2. appsettings.json dosyasını oluşturun (appsettings.example.json'dan)
# Connection string'inizi yapılandırın

# 3. Veritabanını oluşturun
dotnet ef database update

# 4. Uygulamayı çalıştırın
dotnet run
```

Uygulama `https://localhost:5001` adresinde çalışacaktır.

## 👤 Demo Kullanıcılar

| Rol | Kullanıcı Adı | Şifre |
|-----|---------------|-------|
| Admin | `admin` | `admin123` |
| Süpervizör | `supervisor` | `123456` |
| Operatör | `operator1` | `123456` |

## 📁 Proje Yapısı

```
CagriMerkeziUyg/
├── Controllers/     # MVC Controller'ları
├── Models/         # Veri modelleri
├── Views/          # Razor view'ları
├── Data/           # DbContext
├── Services/       # İş mantığı servisleri
├── Migrations/     # EF Core migrations
└── wwwroot/        # Statik dosyalar
```

## 📝 Geliştirme Fikirleri

- Gerçek zamanlı bildirim sistemi (SignalR)
- SMS/Email entegrasyonu
- Dark mode desteği
- PDF rapor oluşturma
- API endpoint'leri

## 📄 Lisans

Bu proje [MIT lisansı](LICENSE) altında dağıtılmaktadır.
