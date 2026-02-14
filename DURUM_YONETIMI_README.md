# 📊 Operatör Durum Yönetimi Sistemi

## ✅ Eklenen Özellikler

### 🎯 Model ve Veritabanı
- ✅ **OperatorDurumu Enum**: 9 farklı durum tanımlandı
  - Offline, Müsait, Çağrıda, Ara Çalışma (Wrap-up)
  - Mola, Öğle Yemeği, Eğitimde, Toplantıda, Uzakta, Meşgul

- ✅ **OperatorDurumGecmisi**: Her durum değişikliği kaydediliyor
  - Önceki/yeni durum
  - Geçiş zamanı ve bitiş zamanı
  - Süre hesaplaması
  - Not ekleme
  - Otomatik/manuel geçiş kontrolü

- ✅ **OperatorGunlukDurumOzeti**: Günlük özet istatistikler
  - Toplam çalışma süresi
  - Çağrıda geçen süre
  - Ara çalışma, mola, öğle yemeği süreleri
  - Verimlilik oranı
  - Kullanım oranı

- ✅ **Operator Modeline Eklenenler**:
  - MevcutDurum
  - SonDurumDegisikliği
  - DurumNotu
  - GununBaslangicSaati / GununBitisSaati
  - DurumRengi (badge için)
  - DurumIkon (Font Awesome icon)

### 🔧 Servis Katmanı
- ✅ **OperatorDurumService** oluşturuldu
  - `DurumDegistir()`: Durum değiştirme ve kaydetme
  - `BugunkuDurumGecmisiniGetir()`: Günlük geçmiş
  - `TumOperatorDurumlariniGetir()`: Tüm operatörlerin anlık durumu
  - `GunlukOzetHesapla()`: Günlük özet hesaplama
  - `UzunSureliDurumKontrol()`: Uyarı sistemi

### 🎮 Controller Endpoint'leri

#### Operatör (OperatorController)
```csharp
POST /Operator/DurumDegistir
GET  /Operator/MevcutDurumGetir
GET  /Operator/BugunkuDurumGecmisi
GET  /Operator/BugunkuDurumOzeti
POST /Operator/HizliDurumDegistir
```

#### Admin/Süpervizör (AdminController)
```csharp
GET  /Admin/CanliDurum (View)
GET  /Admin/GetTumOperatorDurumlari
GET  /Admin/GetDurumUyarilari
GET  /Admin/OperatorDurumGecmisi?operatorId=1&tarih=2025-10-05
GET  /Admin/GunlukDurumOzetleri?tarih=2025-10-05
POST /Admin/OperatorDurumDegistir
```

---

## 🚀 Veritabanı Güncellemesi

Migration oluşturuldu ancak henüz uygulanmadı. Şimdi çalıştırın:

```bash
dotnet ef database update
```

---

## 📱 Kullanım Senaryoları

### Operatör Perspektifi
1. **Giriş Yapınca**: Durum "Offline"dan "Müsait"e geçer
2. **Çağrı Başlayınca**: "Çağrıda" durumuna geçer (otomatik)
3. **Çağrı Bitince**: "Ara Çalışma" durumuna geçer (notlar alır)
4. **Mola İsteyince**: "Mola" veya "Öğle Yemeği" seçer
5. **Toplantıya Gidince**: "Toplantıda" durumuna geçer
6. **Çıkış Yapınca**: "Offline" olur

### Süpervizör Perspektifi
1. **Canlı Dashboard**: Tüm operatörleri anlık görür
2. **Durum Renkleri**: Her durum farklı renk badge'i
3. **Uyarı Sistemi**: Uzun süreli molada kalanlar
4. **Geçmiş Görüntüleme**: Operatörün bugünkü tüm durum geçişleri
5. **Manuel Müdahale**: Gerekirse operatör durumu değiştirebilir

---

## 📊 Otomatik Hesaplanan Metrikler

### Verimlilik Oranı
```
(Çağrıda Geçen Süre + Ara Çalışma Süresi) / Toplam Çalışma Süresi * 100
```

### Kullanım Oranı
```
Çağrıda Geçen Süre / Toplam Çalışma Süresi * 100
```

---

## 🎨 UI Entegrasyonu (Yapılacaklar)

### 1. Operatör Dashboard'a Eklenecek Widgetlar
```html
<!-- Durum Seçici Dropdown -->
<div class="status-selector">
  <button class="btn btn-success">
    <i class="fas fa-check-circle"></i> Müsait
  </button>
  <div class="dropdown-menu">
    <a href="#" data-durum="Musait">✅ Müsait</a>
    <a href="#" data-durum="Mola">☕ Mola</a>
    <a href="#" data-durum="OgleYemegi">🍽️ Öğle Yemeği</a>
    <a href="#" data-durum="Toplantida">👥 Toplantıda</a>
    <a href="#" data-durum="Egitimde">🎓 Eğitimde</a>
  </div>
</div>

<!-- Günlük Özet Kartı -->
<div class="card">
  <div class="card-header">📊 Bugünkü Performansım</div>
  <div class="card-body">
    <p>Toplam Çağrı: <strong>45</strong></p>
    <p>Çağrıda Geçen Süre: <strong>320 dk</strong></p>
    <p>Verimlilik: <strong>85%</strong></p>
    <div class="progress">
      <div class="progress-bar bg-success" style="width: 85%"></div>
    </div>
  </div>
</div>
```

### 2. Süpervizör Canlı Dashboard
```html
<!-- Operatör Durum Kartları -->
<div class="operator-status-grid">
  <div class="operator-card status-musait">
    <div class="operator-avatar">👤</div>
    <h5>Mehmet Demir</h5>
    <span class="badge badge-success">
      <i class="fas fa-check-circle"></i> Müsait
    </span>
    <small>12 dakikadır</small>
  </div>
  
  <div class="operator-card status-cagirida">
    <div class="operator-avatar">👤</div>
    <h5>Ayşe Yılmaz</h5>
    <span class="badge badge-danger">
      <i class="fas fa-phone"></i> Çağrıda
    </span>
    <small>8 dakikadır</small>
  </div>
  
  <!-- Uyarı Badge'i -->
  <div class="operator-card status-mola warning">
    <div class="operator-avatar">👤</div>
    <h5>Ali Kaya</h5>
    <span class="badge badge-warning">
      <i class="fas fa-coffee"></i> Mola
    </span>
    <small class="text-danger">⚠️ 25 dakikadır!</small>
  </div>
</div>

<!-- Durum İstatistikleri -->
<div class="status-summary">
  <div class="stat-card green">
    <h3>5</h3>
    <p>Müsait</p>
  </div>
  <div class="stat-card red">
    <h3>3</h3>
    <p>Çağrıda</p>
  </div>
  <div class="stat-card blue">
    <h3>2</h3>
    <p>Mola</p>
  </div>
  <div class="stat-card gray">
    <h3>1</h3>
    <p>Toplantıda</p>
  </div>
</div>
```

### 3. JavaScript (AJAX) Kodları
```javascript
// Operatör durum değiştirme
function durumDegistir(yeniDurum) {
    $.ajax({
        url: '/Operator/HizliDurumDegistir',
        type: 'POST',
        data: { 
            durum: yeniDurum,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                toastr.success('Durum değiştirildi: ' + response.yeniDurum);
                durumGuncelle(); // UI'ı güncelle
            }
        }
    });
}

// Süpervizör - Canlı durum takibi (her 5 saniyede)
setInterval(function() {
    $.get('/Admin/GetTumOperatorDurumlari', function(data) {
        if (data.success) {
            operatorleriGuncelle(data.operatorler);
            uyarilariKontrolEt();
        }
    });
}, 5000);

// Uyarıları kontrol et
function uyarilariKontrolEt() {
    $.get('/Admin/GetDurumUyarilari', function(data) {
        if (data.success && data.uyariSayisi > 0) {
            // Bildirim göster
            data.uyarilar.forEach(function(uyari) {
                if (uyari.oncelikSeviyesi === 'Yüksek') {
                    toastr.warning(uyari.uyariMesaji);
                }
            });
        }
    });
}
```

---

## 🔄 Otomatik Durum Geçişleri

Şu işlemlerde otomatik durum değişikliği yapılabilir:

### Çağrı Başladığında
```csharp
// OperatorController -> CagriBaslat metodunda
await _durumService.DurumDegistir(
    operatorId, 
    OperatorDurumu.Cagirida, 
    otomatikGecis: true,
    ilgiliAramaLogId: aramaLog.Id
);
```

### Çağrı Bittiğinde
```csharp
// OperatorController -> CagriBitir metodunda
await _durumService.DurumDegistir(
    operatorId, 
    OperatorDurumu.AraCalısma, 
    otomatikGecis: true
);
```

### Login Olunca
```csharp
// AuthController -> Login metodunda
await _durumService.DurumDegistir(
    operatorId, 
    OperatorDurumu.Musait, 
    not: "Giriş yapıldı"
);
```

### Logout Olunca
```csharp
// AuthController -> Logout metodunda
await _durumService.DurumDegistir(
    operatorId, 
    OperatorDurumu.Offline, 
    not: "Çıkış yapıldı"
);
```

---

## 📈 Raporlama Önerileri

### Günlük Rapor
- Operatör bazında çalışma süresi analizi
- En verimli operatörler
- En uzun mola alanlar
- Çağrı yoğunluğu vs müsait operatör sayısı

### Haftalık Rapor
- Operatör verimlilik trendi
- Mola süresi ortalamaları
- Peak saatlerde operatör dağılımı

### Aylık Rapor
- Departman performans özeti
- Operatör karşılaştırmaları
- SLA uyumluluğu

---

## ⚙️ Ayarlar ve Özelleştirme

### Uyarı Süreleri (OperatorDurumService.cs)
```csharp
OperatorDurumu.Mola when gecenSure > 20 => // 20 dakikadan fazla mola
OperatorDurumu.OgleYemegi when gecenSure > 60 => // 60 dakikadan fazla öğle
OperatorDurumu.AraCalısma when gecenSure > 10 => // 10 dakikadan fazla wrap-up
OperatorDurumu.Cagirida when gecenSure > 45 => // 45 dakikadan uzun çağrı
```

Bu süreleri ihtiyacınıza göre değiştirebilirsiniz!

---

## 🎯 Sonraki Adımlar

1. ✅ **Migration Uygula**: `dotnet ef database update`
2. 🔲 **UI Sayfaları Oluştur**:
   - `/Views/Admin/CanliDurum.cshtml` (Canlı operatör takibi)
   - Operatör dashboard'ına durum değiştirme widget'ı ekle
3. 🔲 **JavaScript Entegrasyonu**:
   - AJAX çağrıları
   - SignalR (gerçek zamanlı güncelleme)
   - Toastr bildirimleri
4. 🔲 **Otomatik Geçişler Ekle**:
   - Login/Logout durumları
   - Çağrı başlat/bitir durumları
5. 🔲 **Raporlar Oluştur**:
   - Günlük durum raporu
   - Verimlilik raporu

---

## 💡 İpuçları

### Performans
- Günlük özetler her gün sonu otomatik hesaplanabilir (Background Job)
- Canlı dashboard için SignalR kullanılabilir
- Cache mekanizması eklenebilir

### Güvenlik
- Süpervizör sadece kendi ekibinin durumunu görebilir
- Operatör sadece kendi durumunu değiştirebilir
- Durum değişiklikleri loglanıyor

### UX İyileştirmeleri
- Klavye kısayolları (F1: Müsait, F2: Mola, vb.)
- Sesli uyarılar (uzun süreli mola)
- Mobil responsive tasarım
- Dark mode desteği

---

## 📞 Test Senaryoları

### Senaryo 1: Normal Bir İş Günü
1. 09:00 - Giriş (Offline → Müsait)
2. 09:15 - Çağrı gelir (Müsait → Çağrıda)
3. 09:25 - Çağrı biter (Çağrıda → Ara Çalışma)
4. 09:30 - Not alımı biter (Ara Çalışma → Müsait)
5. 12:00 - Öğle yemeği (Müsait → Öğle Yemeği)
6. 13:00 - Öğle bitir (Öğle Yemeği → Müsait)
7. 18:00 - Çıkış (Müsait → Offline)

**Beklenen Sonuç**: 
- Toplam çalışma: ~7 saat
- Çağrıda: 10 dk
- Ara çalışma: 5 dk
- Öğle yemeği: 60 dk

---

## 🎉 Tebrikler!

Artık operatör durum yönetimi sistemi hazır! Bu özellik:
- ✅ Gerçek zamanlı takip sağlar
- ✅ Verimlilik ölçümü yapar
- ✅ Süpervizörlere görünürlük kazandırır
- ✅ Raporlama için veri toplar
- ✅ Operatörlere kendi performanslarını görme imkanı verir

**Başarılar! 🚀**












