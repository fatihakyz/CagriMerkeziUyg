// Telefon Entegrasyon Sistemi
class TelefonYoneticisi {
    constructor() {
        this.aktifArama = null;
        this.aramaBaslangic = null;
        this.mikrofonAktivatoru = null;
        this.webRTCDestek = this.webRTCKontrol();
        this.gelenAramaTimer = null;
        this.aramaSimulasyonInterval = null;
        
        this.init();
    }

    init() {
        console.log('📞 Telefon Yöneticisi başlatılıyor...');
        this.setupEventListeners();
        this.checkPermissions();
        this.startGelenAramaSimulasyonu();
    }

    // WebRTC desteği kontrol et
    webRTCKontrol() {
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            console.log('✅ WebRTC destekleniyor');
            return true;
        } else {
            console.log('❌ WebRTC desteklenmiyor');
            return false;
        }
    }

    // Mikrofon izinlerini kontrol et
    async checkPermissions() {
        if (!this.webRTCDestek) return false;

        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            console.log('✅ Mikrofon izni alındı');
            stream.getTracks().forEach(track => track.stop()); // İzin test sonrası kapat
            return true;
        } catch (error) {
            console.log('❌ Mikrofon izni alınamadı:', error);
            this.showMicrofonIzinUyarisi();
            return false;
        }
    }

    // Event listener'lar
    setupEventListeners() {
        // Click to call butonları
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('telefon-ara-btn')) {
                e.preventDefault();
                const telefonNo = e.target.dataset.telefon;
                const musteriId = e.target.dataset.musteriId;
                this.aramaBaslat(telefonNo, musteriId);
            }

            if (e.target.classList.contains('arama-sonlandir-btn')) {
                this.aramaSonlandir();
            }

            if (e.target.classList.contains('gelen-arama-cevapla-btn')) {
                const aramaId = e.target.dataset.aramaId;
                this.gelenAramayiCevapla(aramaId);
            }

            if (e.target.classList.contains('gelen-arama-reddet-btn')) {
                const aramaId = e.target.dataset.aramaId;
                this.gelenAramayiReddet(aramaId);
            }

            if (e.target.classList.contains('gelen-arama-simulasyon-btn')) {
                this.manuelGelenAramaSimulasyonu();
            }
        });

        // Klavye kısayolları
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.key === 'p') { // Ctrl+P
                e.preventDefault();
                this.showTelefonPaneli();
            }
        });
    }

    // Arama başlat
    async aramaBaslat(telefonNo, musteriId = null) {
        try {
            console.log(`📞 Arama başlatılıyor: ${telefonNo}`);

            if (!telefonNo) {
                this.showNotification('error', 'Telefon numarası gereklidir.');
                return;
            }

            // UI güncelle
            this.showAramaUI(telefonNo);

            // Mevcut kullanıcı bilgilerini al
            const userResponse = await fetch('/Auth/CurrentUser');
            const userData = await userResponse.json();
            
            if (!userData.authenticated) {
                this.showNotification('error', 'Giriş yapmanız gerekiyor.');
                return;
            }

            // CSRF token'ı al
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || 
                         document.querySelector('meta[name="__RequestVerificationToken"]')?.getAttribute('content') || 
                         $('input[name="__RequestVerificationToken"]').val();
            
            console.log('CSRF Token:', token ? 'bulundu' : 'bulunamadı');

            // Sunucuya arama kaydı gönder
            const headers = {
                'Content-Type': 'application/json'
            };
            
            // Token varsa header'a ekle
            if (token) {
                headers['RequestVerificationToken'] = token;
                headers['X-CSRF-TOKEN'] = token;
            }

            const response = await fetch('/Telefon/AramaBaslat', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify({
                    telefonNo: telefonNo,
                    musteriId: musteriId,
                    operatorId: parseInt(userData.id)
                })
            });

            const result = await response.json();

            if (result.success) {
                this.aktifArama = {
                    id: result.aramaId,
                    telefonNo: telefonNo,
                    musteriAdi: result.musteriAdi,
                    baslangic: new Date()
                };

                this.aramaBaslangic = new Date();
                this.startTimer();

                // WebRTC arama başlat (opsiyonel)
                if (this.webRTCDestek) {
                    await this.webRTCAramaBaslat(telefonNo);
                }

                this.showNotification('success', `${result.musteriAdi} aranıyor...`);
                
                // Click to call için telefon uygulamasını aç
                window.open(`tel:${telefonNo}`, '_self');
                
            } else {
                this.showNotification('error', result.message);
            }

        } catch (error) {
            console.error('Arama başlatılırken hata:', error);
            this.showNotification('error', 'Arama başlatılırken hata oluştu');
        }
    }

    // WebRTC arama başlat
    async webRTCAramaBaslat(telefonNo) {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this.mikrofonAktivatoru = stream;
            
            // Gerçek WebRTC implementasyonu burada olacak
            // SIP.js, Twilio, veya başka bir VoIP provider kullanılabilir
            
            console.log('🎤 Mikrofon aktif');
        } catch (error) {
            console.error('WebRTC arama başlatılamadı:', error);
        }
    }

    // Arama sonlandır
    async aramaSonlandir() {
        if (!this.aktifArama) return;

        try {
            const sure = Math.floor((new Date() - this.aramaBaslangic) / 1000 / 60); // dakika

            const response = await fetch('/Telefon/AramaSonlandir', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify({
                    aramaId: this.aktifArama.id,
                    cagriSuresi: sure,
                    notlar: document.getElementById('arama-notlari')?.value || ''
                })
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification('success', `Arama sonlandırıldı (${result.sure} dk)`);
            }

        } catch (error) {
            console.error('Arama sonlandırılırken hata:', error);
        } finally {
            this.aramaTemizle();
        }
    }

    // Arama temizle
    aramaTemizle() {
        this.aktifArama = null;
        this.aramaBaslangic = null;
        
        // Mikrofonu kapat
        if (this.mikrofonAktivatoru) {
            this.mikrofonAktivatoru.getTracks().forEach(track => track.stop());
            this.mikrofonAktivatoru = null;
        }

        this.hideAramaUI();
        this.stopTimer();
    }

    // Arama UI göster
    showAramaUI(telefonNo) {
        const aramaPanel = `
            <div id="aktif-arama-panel" class="position-fixed bottom-0 end-0 m-3 p-3 bg-primary text-white rounded shadow" style="z-index: 1050;">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <strong>📞 Aktif Arama</strong>
                    <button class="btn-close btn-close-white" onclick="telefonYoneticisi.aramaSonlandir()"></button>
                </div>
                <div class="mb-2">
                    <div><strong>${telefonNo}</strong></div>
                    <div class="small" id="arama-suresi">00:00</div>
                </div>
                <div class="mb-2">
                    <textarea id="arama-notlari" class="form-control form-control-sm" placeholder="Arama notları..." rows="2"></textarea>
                </div>
                <button class="btn btn-danger btn-sm arama-sonlandir-btn">
                    <i class="fas fa-phone-slash"></i> Aramayı Sonlandır
                </button>
            </div>
        `;

        // Varsa eski paneli kaldır
        const eskiPanel = document.getElementById('aktif-arama-panel');
        if (eskiPanel) eskiPanel.remove();

        document.body.insertAdjacentHTML('beforeend', aramaPanel);
    }

    // Arama UI gizle
    hideAramaUI() {
        const panel = document.getElementById('aktif-arama-panel');
        if (panel) panel.remove();
    }

    // Timer başlat
    startTimer() {
        this.timer = setInterval(() => {
            if (this.aramaBaslangic) {
                const gecenSure = new Date() - this.aramaBaslangic;
                const dakika = Math.floor(gecenSure / 60000);
                const saniye = Math.floor((gecenSure % 60000) / 1000);
                
                const sureetiket = document.getElementById('arama-suresi');
                if (sureetiket) {
                    sureetiket.textContent = `${dakika.toString().padStart(2, '0')}:${saniye.toString().padStart(2, '0')}`;
                }
            }
        }, 1000);
    }

    // Timer durdur
    stopTimer() {
        if (this.timer) {
            clearInterval(this.timer);
            this.timer = null;
        }
    }

    // Telefon paneli göster
    showTelefonPaneli() {
        const modal = `
            <div class="modal fade" id="telefonModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">📞 Telefon Paneli</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label class="form-label">Telefon Numarası</label>
                                <input type="tel" class="form-control" id="manuelTelefonNo" placeholder="05xx xxx xx xx">
                            </div>
                            <div class="d-grid gap-2">
                                <button class="btn btn-primary" onclick="telefonYoneticisi.manuelArama()">
                                    <i class="fas fa-phone"></i> Ara
                                </button>
                                <button class="btn btn-secondary" onclick="telefonYoneticisi.showAramaGecmisi()">
                                    <i class="fas fa-history"></i> Arama Geçmişi
                                </button>
                                <button class="btn btn-success gelen-arama-simulasyon-btn">
                                    <i class="fas fa-phone-plus"></i> Gelen Arama Simülasyonu
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Modal ekle ve göster
        document.body.insertAdjacentHTML('beforeend', modal);
        const modalEl = new bootstrap.Modal(document.getElementById('telefonModal'));
        modalEl.show();

        // Modal kapandığında temizle
        document.getElementById('telefonModal').addEventListener('hidden.bs.modal', function () {
            this.remove();
        });
    }

    // Manuel arama
    manuelArama() {
        const telefonNo = document.getElementById('manuelTelefonNo').value.trim();
        if (telefonNo) {
            bootstrap.Modal.getInstance(document.getElementById('telefonModal')).hide();
            this.aramaBaslat(telefonNo);
        }
    }

    // Arama geçmişi göster
    showAramaGecmisi() {
        window.open('/Telefon/AramaGecmisi', '_blank');
    }

    // Notification göster
    showNotification(type, message) {
        const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
        const icon = type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-triangle';
        
        const notification = `
            <div class="alert ${alertClass} alert-dismissible fade show position-fixed top-0 end-0 m-3" style="z-index: 1060;" role="alert">
                <i class="${icon}"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', notification);

        // 5 saniye sonra otomatik kapat
        setTimeout(() => {
            const alert = document.querySelector('.alert');
            if (alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    }

    // Mikrofon izin uyarısı
    showMicrofonIzinUyarisi() {
        this.showNotification('error', 'Mikrofon izni gerekiyor. Tarayıcı ayarlarından izin verin.');
    }

    // Gelen arama simülasyonu başlat (otomatik)
    startGelenAramaSimulasyonu() {
        // Her 2-5 dakika arası rastgele gelen arama simülasyonu
        this.aramaSimulasyonInterval = setInterval(() => {
            const rastgeleSure = Math.random() * (5 - 2) + 2; // 2-5 dakika arası
            if (Math.random() < 0.3) { // %30 şans ile gelen arama
                this.otomatikGelenAramaSimulasyonu();
            }
        }, 120000); // 2 dakikada bir kontrol
    }

    // Otomatik gelen arama simülasyonu
    async otomatikGelenAramaSimulasyonu() {
        if (this.aktifArama) {
            console.log('⚠️ Zaten aktif arama var, gelen arama simülasyonu atlanıyor');
            return;
        }

        try {
            console.log('📞 Gelen arama simülasyonu başlatılıyor...');
            
            const response = await fetch('/Telefon/GelenAramaSimulasyonu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            const result = await response.json();

            if (result.success) {
                this.showGelenAramaUI(result);
                this.gelenAramaSesiCal();
                
                // 30 saniye sonra otomatik cevapsız olarak işaretle
                this.gelenAramaTimer = setTimeout(() => {
                    this.gelenAramaCevapsiz(result.aramaId);
                }, 30000);
            }

        } catch (error) {
            console.error('Gelen arama simülasyonu hatası:', error);
        }
    }

    // Manuel gelen arama simülasyonu
    async manuelGelenAramaSimulasyonu() {
        if (this.aktifArama) {
            this.showNotification('warning', 'Zaten aktif bir arama var!');
            return;
        }

        await this.otomatikGelenAramaSimulasyonu();
    }

    // Gelen arama UI göster
    showGelenAramaUI(aramaData) {
        const gelenAramaPanel = `
            <div id="gelen-arama-panel" class="position-fixed top-50 start-50 translate-middle p-4 bg-success text-white rounded-3 shadow-lg animate__animated animate__bounceIn" style="z-index: 1060; min-width: 300px;">
                <div class="text-center">
                    <div class="mb-3">
                        <i class="fas fa-phone-alt fa-3x animate__animated animate__pulse animate__infinite"></i>
                    </div>
                    <h4 class="mb-2">📞 GELEN ARAMA</h4>
                    <div class="mb-2">
                        <strong class="fs-5">${aramaData.telefonNo}</strong>
                    </div>
                    <div class="mb-3">
                        <span class="badge bg-light text-dark">${aramaData.musteriAdi}</span>
                    </div>
                    <div class="mb-3">
                        <small>Operatör: ${aramaData.operatorAdi}</small>
                    </div>
                    <div class="d-grid gap-2">
                        <button class="btn btn-light btn-lg gelen-arama-cevapla-btn" data-arama-id="${aramaData.aramaId}">
                            <i class="fas fa-phone text-success"></i> CEVAPLA
                        </button>
                        <button class="btn btn-outline-light gelen-arama-reddet-btn" data-arama-id="${aramaData.aramaId}">
                            <i class="fas fa-phone-slash text-danger"></i> REDDET
                        </button>
                    </div>
                    <div class="mt-2">
                        <small id="gelen-arama-zamanlayici">Zaman aşımı: 30s</small>
                    </div>
                </div>
            </div>
        `;

        // Varsa eski paneli kaldır
        const eskiPanel = document.getElementById('gelen-arama-panel');
        if (eskiPanel) eskiPanel.remove();

        document.body.insertAdjacentHTML('beforeend', gelenAramaPanel);

        // Geri sayım başlat
        this.startGelenAramaGeriSayim();
    }

    // Gelen arama geri sayım
    startGelenAramaGeriSayim() {
        let kalanSure = 30;
        const geriSayimInterval = setInterval(() => {
            kalanSure--;
            const zamanlayici = document.getElementById('gelen-arama-zamanlayici');
            if (zamanlayici) {
                zamanlayici.textContent = `Zaman aşımı: ${kalanSure}s`;
            }

            if (kalanSure <= 0) {
                clearInterval(geriSayimInterval);
            }
        }, 1000);
    }

    // Gelen aramayı cevapla
    async gelenAramayiCevapla(aramaId) {
        try {
            this.hideGelenAramaUI();
            this.gelenAramaSesiDurdur();
            
            if (this.gelenAramaTimer) {
                clearTimeout(this.gelenAramaTimer);
                this.gelenAramaTimer = null;
            }

            // Arama durumunu "devam" olarak işaretle
            this.aktifArama = {
                id: aramaId,
                tip: 'Gelen',
                baslangic: new Date()
            };

            this.aramaBaslangic = new Date();
            this.startTimer();

            // Aktif arama UI göster (gelen arama için)
            this.showAktifGelenAramaUI(aramaId);

            this.showNotification('success', '📞 Gelen arama cevaplandı!');

        } catch (error) {
            console.error('Gelen arama cevaplarken hata:', error);
            this.showNotification('error', 'Arama cevaplarken hata oluştu');
        }
    }

    // Gelen aramayı reddet
    async gelenAramayiReddet(aramaId) {
        try {
            const response = await fetch('/Telefon/AramaReddet', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify({
                    aramaId: aramaId,
                    sebep: 'Operatör tarafından reddedildi'
                })
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification('info', '📞 Gelen arama reddedildi');
            }

        } catch (error) {
            console.error('Gelen arama reddederken hata:', error);
        } finally {
            this.hideGelenAramaUI();
            this.gelenAramaSesiDurdur();
            
            if (this.gelenAramaTimer) {
                clearTimeout(this.gelenAramaTimer);
                this.gelenAramaTimer = null;
            }
        }
    }

    // Gelen arama cevapsız
    async gelenAramaCevapsiz(aramaId) {
        try {
            const response = await fetch('/Telefon/AramaCevapsiz', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify({
                    aramaId: aramaId
                })
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification('warning', '📞 Gelen arama cevapsız kaldı');
            }

        } catch (error) {
            console.error('Gelen arama cevapsız işaretlerken hata:', error);
            this.showNotification('error', 'Arama cevapsız işaretlenirken hata oluştu');
        } finally {
            this.hideGelenAramaUI();
            this.gelenAramaSesiDurdur();
        }
    }

    // Aktif gelen arama UI göster
    showAktifGelenAramaUI(aramaId) {
        const aramaPanel = `
            <div id="aktif-arama-panel" class="position-fixed bottom-0 end-0 m-3 p-3 bg-success text-white rounded shadow" style="z-index: 1050;">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <strong>📞 Gelen Arama</strong>
                    <button class="btn-close btn-close-white" onclick="telefonYoneticisi.aramaSonlandir()"></button>
                </div>
                <div class="mb-2">
                    <div class="small" id="arama-suresi">00:00</div>
                </div>
                <div class="mb-2">
                    <textarea id="arama-notlari" class="form-control form-control-sm" placeholder="Arama notları..." rows="2"></textarea>
                </div>
                <button class="btn btn-danger btn-sm arama-sonlandir-btn">
                    <i class="fas fa-phone-slash"></i> Aramayı Sonlandır
                </button>
            </div>
        `;

        // Varsa eski paneli kaldır
        const eskiPanel = document.getElementById('aktif-arama-panel');
        if (eskiPanel) eskiPanel.remove();

        document.body.insertAdjacentHTML('beforeend', aramaPanel);
    }

    // Gelen arama UI gizle
    hideGelenAramaUI() {
        const panel = document.getElementById('gelen-arama-panel');
        if (panel) panel.remove();
    }

    // Gelen arama sesi çal (simülasyon)
    gelenAramaSesiCal() {
        // Gerçek uygulamada burada telefon zil sesi çalacak
        console.log('🔔 Telefon çalıyor...');
        
        this.zilSesiInterval = setInterval(() => {
            console.log('🔔 Ring... Ring...');
        }, 2000);
    }

    // Gelen arama sesi durdur
    gelenAramaSesiDurdur() {
        if (this.zilSesiInterval) {
            clearInterval(this.zilSesiInterval);
            this.zilSesiInterval = null;
        }
        console.log('🔇 Telefon sesi durduruldu');
    }
}

// Global telefon yöneticisi başlat
let telefonYoneticisi;

document.addEventListener('DOMContentLoaded', function() {
    telefonYoneticisi = new TelefonYoneticisi();
    
    // Global helper fonksiyonlar
    window.telefonAra = function(telefonNo, musteriId = null) {
        telefonYoneticisi.aramaBaslat(telefonNo, musteriId);
    };
});

// Helper fonksiyon: Telefon numarasını formatla
function telefonFormula(telefon) {
    if (!telefon) return '';
    const temiz = telefon.replace(/\D/g, '');
    if (temiz.length === 11 && temiz.startsWith('0')) {
        return `${temiz.slice(0,4)} ${temiz.slice(4,7)} ${temiz.slice(7,9)} ${temiz.slice(9)}`;
    }
    return telefon;
}

