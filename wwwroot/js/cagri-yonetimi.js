/*  ÇAĞRI YÖNETİMİ SİSTEMİ - JAVASCRIPT */

// Global değişkenler
let aktifMusteri = null;
let aktifAramaLogId = null;
let cagriBaslangicZamani = null;
let cagriTimerInterval = null;

// Sayfa yüklendiğinde
$(document).ready(function () {
    initializeCagriYonetimi();
    initializeSablonlar();
});

// ÇAĞRI YÖNETİMİ İNİTİALİZE

function initializeCagriYonetimi() {
    // Panelleri başlangıçta kesinlikle kapat
    const cagriPanel = document.getElementById('cagriYonetimiPanel');
    const sablonPanel = document.getElementById('sablonlarPanel');
    
    if (cagriPanel) {
        cagriPanel.classList.remove('active');
        cagriPanel.classList.remove('dragging');
        cagriPanel.style.transform = 'translate3d(0, 0, 0)';
    }
    
    if (sablonPanel) {
        sablonPanel.classList.remove('active');
        sablonPanel.classList.remove('dragging');
        sablonPanel.style.transform = 'translate3d(0, 0, 0)';
    }
    
    // Floating button click
    $('#btnCagriYonetimi').on('click', function () {
        togglePanel('cagriYonetimiPanel');
    });

    // Panel kapatma butonu - GÜÇLÜ KORUMA
    const btnPanelKapatEl = document.getElementById('btnPanelKapat');
    if (btnPanelKapatEl) {
        // Mousedown'ı tamamen engelle
        btnPanelKapatEl.addEventListener('mousedown', function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            return false;
        }, true);

        // Click - Paneli kapat
        btnPanelKapatEl.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            console.log('Panel kapatılıyor...');
            
            // Paneli kapat
            const panel = document.getElementById('cagriYonetimiPanel');
            if (panel) {
                // Pozisyonu sıfırla
                panel.style.transform = 'translate3d(0, 0, 0)';
                
                // Active class'ı kaldır
                panel.classList.remove('active');
                panel.classList.remove('dragging');
                
                // LocalStorage'ı temizle (isteğe bağlı)
                // localStorage.removeItem('cagriYonetimiPanel_position');
                
                // Panel pozisyonlarını güncelle (şablon paneli açıksa sağa kaydır)
                updatePanelPositions();
            }
            
            return false;
        }, true);
    }

    // Panel sürüklenebilir yap (Drag Handle ile)
    makePanelDraggable('cagriYonetimiPanel');

    // Müşteri kontrol
    $('#btnMusteriKontrol').on('click', musteriKontrol);
    
    // Enter tuşu ile müşteri kontrol
    $('#telefonNoInput').on('keypress', function (e) {
        if (e.which === 13) { // Enter
            e.preventDefault();
            musteriKontrol();
        }
    });

    // Telefon numarası formatla (isteğe bağlı)
    $('#telefonNoInput').on('input', function () {
        let val = $(this).val().replace(/\D/g, ''); // Sadece rakamlar
        $(this).val(val);
    });

    // Hızlı kayıt modal
    $('#btnHizliKayit').on('click', function () {
        const telefon = $('#telefonNoInput').val();
        $('#hizliKayitTelefon').val(telefon);
        $('#hizliKayitModal').modal('show');
    });

    // Geçici kayıt
    $('#btnGeciciKayit').on('click', geciciMusteriOlustur);

    // Hızlı kayıt kaydet
    $('#btnHizliKayitKaydet').on('click', hizliMusteriOlustur);

    // Çağrı bitir butonu
    $('#btnCagriBitir').on('click', function () {
        $('#cagriBitirModal').modal('show');
    });

    // Çağrı bitir kaydet
    $('#btnCagriBitirKaydet').on('click', cagriBitir);
}

// PANEL TOGGLE

function togglePanel(panelId) {
    const panel = $('#' + panelId);
    const isActive = panel.hasClass('active');

    // Şablonlar paneli özel durum: çağrı yönetimi panelini kapatma
    if (panelId === 'sablonlarPanel') {
        // Sadece şablonlar panelini toggle et, diğer panellere dokunma
        if (isActive) {
            panel.removeClass('active');
        } else {
            panel.addClass('active');
            // Çağrı yönetimi paneli açıksa, şablonlar panelini onun yanına yerleştir
            updatePanelPositions();
        }
    } else {
        // Çağrı yönetimi paneli için: diğer çağrı panellerini kapat ama şablon panelini bırak
        $('.side-panel').not('#sablonlarPanel').removeClass('active');
        
        // Eğer panel kapalıysa aç
        if (!isActive) {
            panel.addClass('active');
        }
        
        // Panel pozisyonlarını güncelle
        updatePanelPositions();
    }
}

// Panel pozisyonlarını güncelle
function updatePanelPositions() {
    const cagriPanelActive = $('#cagriYonetimiPanel').hasClass('active');
    const sablonPanelActive = $('#sablonlarPanel').hasClass('active');
    
    if (cagriPanelActive && sablonPanelActive) {
        // İki panel de açıksa, şablonlar panelini çağrı panelinin yanına koy
        $('#sablonlarPanel').css('right', '400px');
    } else {
        // Sadece şablonlar paneli açıksa, sağ tarafa koy
        if (sablonPanelActive) {
            $('#sablonlarPanel').css('right', '0');
        }
    }
}

// PANEL SÜRÜKLENEBILIR YAP

function makePanelDraggable(panelId) {
    const panel = document.getElementById(panelId);
    if (!panel) return;

    const header = panel.querySelector('.side-panel-header');
    if (!header) return;

    let isDragging = false;
    let currentX;
    let currentY;
    let initialX;
    let initialY;
    let xOffset = 0;
    let yOffset = 0;

    // Başlangıç pozisyonunu kaydet
    const savedPosition = localStorage.getItem(panelId + '_position');
    if (savedPosition) {
        const pos = JSON.parse(savedPosition);
        xOffset = pos.x;
        yOffset = pos.y;
        setTranslate(pos.x, pos.y, panel);
    }

    // SADECE DRAG HANDLE'A EVENT BAĞLA
    const dragHandle = header.querySelector('.drag-handle');
    if (dragHandle) {
        // Event'leri SADECE drag handle'a bağla
        dragHandle.addEventListener('mousedown', dragStart);
        dragHandle.addEventListener('touchstart', dragStart);
    }

    function dragStart(e) {
        // Close button kontrolü - Eğer close button'daysa, drag başlatma
        const target = e.target;
        if (target.classList.contains('side-panel-close-btn') || 
            target.closest('.side-panel-close-btn') ||
            target.id === 'btnPanelKapat' ||
            target.id === 'btnSablonPanelKapat') {
            console.log('Close button algılandı, drag başlatılmıyor');
            return;
        }

        if (e.type === 'touchstart') {
            initialX = e.touches[0].clientX - xOffset;
            initialY = e.touches[0].clientY - yOffset;
        } else {
            initialX = e.clientX - xOffset;
            initialY = e.clientY - yOffset;
        }

        isDragging = true;
        panel.classList.add('dragging');
        
        console.log('Drag başlatıldı');

        document.addEventListener('mousemove', drag);
        document.addEventListener('mouseup', dragEnd);
        document.addEventListener('touchmove', drag);
        document.addEventListener('touchend', dragEnd);
    }

    function drag(e) {
        if (!isDragging) return;

        e.preventDefault();

        if (e.type === 'touchmove') {
            currentX = e.touches[0].clientX - initialX;
            currentY = e.touches[0].clientY - initialY;
        } else {
            currentX = e.clientX - initialX;
            currentY = e.clientY - initialY;
        }

        xOffset = currentX;
        yOffset = currentY;

        setTranslate(currentX, currentY, panel);
    }

    function dragEnd() {
        if (!isDragging) return;

        isDragging = false;
        panel.classList.remove('dragging');

        // Pozisyonu kaydet
        localStorage.setItem(panelId + '_position', JSON.stringify({
            x: xOffset,
            y: yOffset
        }));

        document.removeEventListener('mousemove', drag);
        document.removeEventListener('mouseup', dragEnd);
        document.removeEventListener('touchmove', drag);
        document.removeEventListener('touchend', dragEnd);
    }

    function setTranslate(xPos, yPos, el) {
        el.style.transform = `translate3d(${xPos}px, ${yPos}px, 0)`;
    }

    // Pozisyonu sıfırlama için çift tıklama (SADECE DRAG HANDLE'DA)
    if (dragHandle) {
        dragHandle.addEventListener('dblclick', function(e) {
            e.preventDefault();
            e.stopPropagation();
            xOffset = 0;
            yOffset = 0;
            setTranslate(0, 0, panel);
            localStorage.removeItem(panelId + '_position');
        });
    }
}

// MÜŞTERİ KONTROL

function musteriKontrol() {
    const telefonNo = $('#telefonNoInput').val().trim();

    if (!telefonNo) {
        showAlert('Lütfen telefon numarası girin', 'warning');
        return;
    }

    // Loading durumu
    $('#btnMusteriKontrol').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Kontrol ediliyor...');

    $.ajax({
        url: '/Operator/MusteriKontrol',
        type: 'POST',
        data: { telefonNo: telefonNo },
        success: function (response) {
            if (response.success) {
                if (response.musteriVar) {
                    // Müşteri kayıtlı
                    aktifMusteri = response.musteri;
                    gosterMusteriBilgisi(response.musteri, true);
                    $('#hizliIslemlerCard').hide();
                    
                    // Çağrıyı başlat
                    cagriBaslat(telefonNo, response.musteri.id, true, false);
                } else {
                    // Müşteri kayıtlı değil
                    aktifMusteri = null;
                    gosterMusteriBilgisi({ telefonNo: telefonNo }, false);
                    $('#hizliIslemlerCard').show();
                }
            } else {
                showAlert(response.message || 'Bir hata oluştu', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        },
        complete: function () {
            $('#btnMusteriKontrol').prop('disabled', false).html('<i class="bx bx-search me-1"></i>Müşteri Kontrol Et');
        }
    });
}

// MÜŞTERİ BİLGİSİ GÖSTER

function gosterMusteriBilgisi(musteri, kayitli) {
    let html = '';

    if (kayitli) {
        // Kayıtlı müşteri
        let etiketlerHtml = '';
        if (musteri.etiketler && musteri.etiketler.length > 0) {
            etiketlerHtml = musteri.etiketler.map(e => 
                `<span class="badge me-1" style="background-color: ${e.renkKodu}">${e.ad}</span>`
            ).join('');
        }

        html = `
            <div class="card mb-3 musteri-bilgi-card ${musteri.geciciKayit ? 'gecici' : 'kayitli'}">
                <div class="card-body">
                    <h6 class="card-title mb-3">
                        <i class="bx bx-user-check me-1 text-success"></i>Müşteri Bilgileri
                    </h6>
                    
                    <div class="mb-2">
                        <strong>${musteri.tamAd}</strong>
                        ${musteri.geciciKayit ? '<span class="badge badge-gecici ms-2">Geçici</span>' : ''}
                    </div>
                    
                    <div class="mb-2">
                        <i class="bx bx-phone me-1"></i>${musteri.telefonNo}
                    </div>
                    
                    ${musteri.email ? `<div class="mb-2"><i class="bx bx-envelope me-1"></i>${musteri.email}</div>` : ''}
                    
                    ${etiketlerHtml ? `<div class="mt-2">${etiketlerHtml}</div>` : ''}
                    
                    <div class="mt-3">
                        <a href="/Musteri/Detay/${musteri.id}" target="_blank" class="btn btn-sm btn-outline-primary w-100">
                            <i class="bx bx-detail me-1"></i>Detaylı Bilgi
                        </a>
                    </div>
                </div>
            </div>
        `;
    } else {
        // Kayıtsız müşteri
        html = `
            <div class="card mb-3 musteri-bilgi-card yeni">
                <div class="card-body">
                    <h6 class="card-title mb-3">
                        <i class="bx bx-user-x me-1 text-warning"></i>Müşteri Kayıtlı Değil
                    </h6>
                    
                    <div class="alert alert-warning mb-0">
                        <i class="bx bx-info-circle me-1"></i>
                        <small><strong>${musteri.telefonNo}</strong> numarası sistemde kayıtlı değil. Hızlı kayıt oluşturabilir veya geçici kayıt ile devam edebilirsiniz.</small>
                    </div>
                </div>
            </div>
        `;
    }

    $('#musteriBilgiAlani').html(html);
}

// HIZLI MÜŞTERİ OLUŞTUR

function hizliMusteriOlustur() {
    const telefon = $('#hizliKayitTelefon').val();
    const ad = $('#hizliKayitAd').val().trim();
    const soyad = $('#hizliKayitSoyad').val().trim();

    if (!ad || !soyad) {
        showAlert('Ad ve soyad alanları zorunludur', 'warning');
        return;
    }

    $('#btnHizliKayitKaydet').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Kaydediliyor...');

    $.ajax({
        url: '/Operator/HizliMusteriOlustur',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            ad: ad,
            soyad: soyad,
            telefonNo: telefon
        },
        success: function (response) {
            if (response.success) {
                aktifMusteri = response.musteri;
                $('#hizliKayitModal').modal('hide');
                showAlert('Müşteri başarıyla kaydedildi!', 'success');
                
                // Müşteri bilgisini göster
                gosterMusteriBilgisi(response.musteri, true);
                $('#hizliIslemlerCard').hide();
                
                // Çağrıyı başlat
                cagriBaslat(telefon, response.musteri.id, false, true);
                
                // Formu temizle
                $('#hizliKayitForm')[0].reset();
            } else {
                showAlert(response.message || 'Kayıt oluşturulamadı', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        },
        complete: function () {
            $('#btnHizliKayitKaydet').prop('disabled', false).html('<i class="bx bx-save me-1"></i>Kaydet ve Devam Et');
        }
    });
}

// GEÇİCİ MÜŞTERİ OLUŞTUR

function geciciMusteriOlustur() {
    const telefon = $('#telefonNoInput').val().trim();

    if (!telefon) {
        showAlert('Telefon numarası gerekli', 'warning');
        return;
    }

    if (!confirm('Anonim müşteri kaydı oluşturulacak. Devam etmek istiyor musunuz?')) {
        return;
    }

    $('#btnGeciciKayit').prop('disabled', true);

    $.ajax({
        url: '/Operator/GeciciMusteriOlustur',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            telefonNo: telefon
        },
        success: function (response) {
            if (response.success) {
                aktifMusteri = response.musteri;
                showAlert('Geçici müşteri kaydı oluşturuldu', 'success');
                
                // Müşteri bilgisini göster
                gosterMusteriBilgisi(response.musteri, true);
                $('#hizliIslemlerCard').hide();
                
                // Çağrıyı başlat
                cagriBaslat(telefon, response.musteri.id, false, true);
            } else {
                showAlert(response.message || 'Geçici kayıt oluşturulamadı', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        },
        complete: function () {
            $('#btnGeciciKayit').prop('disabled', false);
        }
    });
}

// ÇAĞRI BAŞLAT

function cagriBaslat(telefonNo, musteriId, musteriKayitliydi, cagriSirasindaKayitOlusturuldu) {
    $.ajax({
        url: '/Operator/CagriBaslat',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            telefonNo: telefonNo,
            musteriId: musteriId,
            musteriKayitliydi: musteriKayitliydi,
            cagriSirasindaKayitOlusturuldu: cagriSirasindaKayitOlusturuldu
        },
        success: function (response) {
            if (response.success) {
                aktifAramaLogId = response.aramaLogId;
                cagriBaslangicZamani = new Date(response.baslangicZamani);
                
                // Timer'ı başlat
                startCagriTimer();
                
                // Çağrı kontrol kartını göster
                $('#cagriKontrolCard').show();
                
                showAlert('Çağrı kaydı başlatıldı', 'info');
            } else {
                showAlert(response.message || 'Çağrı başlatılamadı', 'danger');
            }
        },
        error: function () {
            showAlert('Çağrı başlatılırken hata oluştu', 'danger');
        }
    });
}

// ÇAĞRI TIMER

function startCagriTimer() {
    if (cagriTimerInterval) {
        clearInterval(cagriTimerInterval);
    }

    cagriTimerInterval = setInterval(function () {
        if (!cagriBaslangicZamani) return;

        const now = new Date();
        const diff = Math.floor((now - cagriBaslangicZamani) / 1000); // saniye

        const minutes = Math.floor(diff / 60);
        const seconds = diff % 60;

        $('#cagriSuresi').text(
            String(minutes).padStart(2, '0') + ':' + String(seconds).padStart(2, '0')
        );
    }, 1000);
}

function stopCagriTimer() {
    if (cagriTimerInterval) {
        clearInterval(cagriTimerInterval);
        cagriTimerInterval = null;
    }
}

// ÇAĞRI BİTİR

function cagriBitir() {
    const durum = $('#cagriBitirDurum').val();
    const memnuniyet = $('#cagriBitirMemnuniyet').val();
    const notlar = $('#cagriBitirNotlar').val();

    if (!aktifAramaLogId) {
        showAlert('Aktif çağrı bulunamadı', 'warning');
        return;
    }

    $('#btnCagriBitirKaydet').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Kaydediliyor...');

    $.ajax({
        url: '/Operator/CagriBitir',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            aramaLogId: aktifAramaLogId,
            durum: durum,
            notlar: notlar,
            memnuniyet: memnuniyet ? parseInt(memnuniyet) : null
        },
        success: function (response) {
            if (response.success) {
                stopCagriTimer();
                $('#cagriBitirModal').modal('hide');
                showAlert(`Çağrı sonlandırıldı (${response.sure.toFixed(1)} dk)`, 'success');
                
                // Formu sıfırla
                resetCagriForm();
            } else {
                showAlert(response.message || 'Çağrı sonlandırılamadı', 'danger');
            }
        },
        error: function () {
            showAlert('Çağrı sonlandırılırken hata oluştu', 'danger');
        },
        complete: function () {
            $('#btnCagriBitirKaydet').prop('disabled', false).html('<i class="bx bx-phone-off me-1"></i>Çağrıyı Sonlandır');
        }
    });
}

// FORM RESET

function resetCagriForm() {
    aktifMusteri = null;
    aktifAramaLogId = null;
    cagriBaslangicZamani = null;
    
    $('#telefonNoInput').val('');
    $('#musteriBilgiAlani').html('');
    $('#hizliIslemlerCard').hide();
    $('#cagriKontrolCard').hide();
    $('#cagriSuresi').text('00:00');
    $('#cagriBitirForm')[0].reset();
}

// HIZLI YANIT ŞABLONLARI
function initializeSablonlar() {
    // Şablon panelini başlangıçta kesinlikle kapat
    const sablonPanel = document.getElementById('sablonlarPanel');
    if (sablonPanel) {
        sablonPanel.classList.remove('active');
        sablonPanel.classList.remove('dragging');
        sablonPanel.style.transform = 'translate3d(0, 0, 0)';
    }
    
    // Şablonlar paneli aç
    $('#btnSablonlariAc').on('click', function () {
        togglePanel('sablonlarPanel');
        loadPopulerSablonlar();
        loadKategoriler();
    });

    // Şablonlar paneli kapatma butonu - GÜÇLÜ KORUMA
    const btnSablonPanelKapatEl = document.getElementById('btnSablonPanelKapat');
    if (btnSablonPanelKapatEl) {
        // Mousedown'ı tamamen engelle
        btnSablonPanelKapatEl.addEventListener('mousedown', function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            return false;
        }, true);

        // Click - Paneli kapat
        btnSablonPanelKapatEl.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            console.log('Şablon paneli kapatılıyor...');
            
            // Paneli kapat
            const panel = document.getElementById('sablonlarPanel');
            if (panel) {
                // Pozisyonu sıfırla
                panel.style.transform = 'translate3d(0, 0, 0)';
                panel.style.right = ''; // CSS right değerini temizle
                
                // Active class'ı kaldır
                panel.classList.remove('active');
                panel.classList.remove('dragging');
                
                // LocalStorage'ı temizle (isteğe bağlı)
                // localStorage.removeItem('sablonlarPanel_position');
                
                // Panel pozisyonlarını güncelle
                updatePanelPositions();
            }
            
            return false;
        }, true);
    }

    // Şablonlar panelini sürüklenebilir yap (Drag Handle ile)
    makePanelDraggable('sablonlarPanel');

    // Kategoriye dön
    $('#btnKategoriyeDon').on('click', function () {
        $('#sablonlarListesi').hide();
        loadKategoriler();
    });
}

// Popüler şablonları yükle
function loadPopulerSablonlar() {
    $.ajax({
        url: '/CevapSablon/GetPopulerSablonlar',
        type: 'GET',
        data: { adet: 3 },
        success: function (response) {
            if (response.success && response.data.length > 0) {
                let html = '';
                response.data.forEach(function (sablon) {
                    html += createSablonCard(sablon, true);
                });
                $('#populerSablonlarAlani').html(html);
            } else {
                $('#populerSablonlarAlani').html('<p class="text-muted small">Henüz popüler şablon yok</p>');
            }
        },
        error: function () {
            $('#populerSablonlarAlani').html('<p class="text-danger small">Yüklenemedi</p>');
        }
    });
}

// Kategorileri yükle
function loadKategoriler() {
    $('#kategorilerAlani').html('<div class="text-center py-3"><div class="spinner-border spinner-border-sm"></div></div>');

    $.ajax({
        url: '/CevapSablon/GetKategoriler',
        type: 'GET',
        success: function (response) {
            if (response.success && response.data.length > 0) {
                let html = '';
                response.data.forEach(function (kategori) {
                    html += `
                        <button class="kategori-btn" data-kategori-id="${kategori.id}">
                            <div class="d-flex align-items-center flex-fill">
                                <div class="kategori-btn-icon" style="background-color: ${kategori.renkKodu}">
                                    <i class="${kategori.iconClass || 'bx bx-folder'}"></i>
                                </div>
                                <div class="kategori-btn-content">
                                    <div class="kategori-btn-title">${kategori.ad}</div>
                                    ${kategori.aciklama ? `<p class="kategori-btn-desc">${kategori.aciklama}</p>` : ''}
                                </div>
                            </div>
                            <span class="kategori-btn-badge">${kategori.sablonSayisi}</span>
                        </button>
                    `;
                });
                $('#kategorilerAlani').html(html);

                // Kategori click event
                $('.kategori-btn').on('click', function () {
                    const kategoriId = $(this).data('kategori-id');
                    const kategoriAd = $(this).find('.kategori-btn-title').text();
                    loadSablonlar(kategoriId, kategoriAd);
                });
            } else {
                $('#kategorilerAlani').html('<p class="text-muted">Kategori bulunamadı</p>');
            }
        },
        error: function () {
            $('#kategorilerAlani').html('<p class="text-danger">Kategoriler yüklenemedi</p>');
        }
    });
}

// Şablonları yükle
function loadSablonlar(kategoriId, kategoriAd) {
    $('#secilenKategoriBaslik').text(kategoriAd);
    $('#sablonlarAlani').html('<div class="text-center py-3"><div class="spinner-border spinner-border-sm"></div></div>');
    $('#sablonlarListesi').show();

    $.ajax({
        url: '/CevapSablon/GetSablonlar',
        type: 'GET',
        data: { kategoriId: kategoriId },
        success: function (response) {
            if (response.success && response.data.length > 0) {
                let html = '';
                response.data.forEach(function (sablon) {
                    html += createSablonCard(sablon, false);
                });
                $('#sablonlarAlani').html(html);
            } else {
                $('#sablonlarAlani').html('<p class="text-muted">Bu kategoride şablon yok</p>');
            }
        },
        error: function () {
            $('#sablonlarAlani').html('<p class="text-danger">Şablonlar yüklenemedi</p>');
        }
    });
}

// Şablon kartı oluştur
function createSablonCard(sablon, isPopuler) {
    return `
        <div class="sablon-card" data-sablon-id="${sablon.id}">
            <div class="sablon-card-header">
                <h6 class="sablon-card-title">${sablon.baslik}</h6>
                ${sablon.kisaYol ? `<span class="sablon-card-shortcut">${sablon.kisaYol}</span>` : ''}
            </div>
            <div class="sablon-card-content">
                ${sablon.icerik}
            </div>
            <div class="sablon-card-footer">
                ${isPopuler ? `<span class="badge bg-warning text-dark"><i class="bx bx-star"></i> Popüler</span>` : ''}
                <span class="sablon-card-usage">
                    <i class="bx bx-check"></i>
                    ${sablon.kullanimSayisi} kez kullanıldı
                </span>
            </div>
        </div>
    `;
}

// Şablon click event (dinamik) - Detay modalı aç
$(document).on('click', '.sablon-card', function () {
    const sablonId = $(this).data('sablon-id');
    
    // Şablon detayını getir ve modal aç
    getSablonDetay(sablonId);
});

// Şablon detayını getir ve modal göster
function getSablonDetay(sablonId) {
    $.ajax({
        url: '/CevapSablon/GetSablon',
        type: 'GET',
        data: { id: sablonId },
        success: function (response) {
            if (response.success && response.data) {
                gosterSablonDetay(response.data);
            } else {
                showAlert(response.message || 'Şablon yüklenemedi', 'danger');
            }
        },
        error: function () {
            showAlert('Şablon detayı alınırken hata oluştu', 'danger');
        }
    });
}

// Şablon detay modalını göster
function gosterSablonDetay(sablon) {
    // Modal başlık
    $('#sablonDetayBaslik').html(`<i class="bx bx-message-square-detail me-2"></i>${sablon.baslik}`);
    
    // Kategori
    $('#sablonDetayKategori').text(sablon.kategoriAdi).css('background-color', sablon.kategoriRenk || '#007bff');
    
    // İçerik (tam metin)
    $('#sablonDetayIcerik').text(sablon.icerik);
    
    // Notlar (varsa göster)
    if (sablon.notlar) {
        $('#sablonDetayNotlar').text(sablon.notlar);
        $('#sablonDetayNotlarDiv').show();
    } else {
        $('#sablonDetayNotlarDiv').hide();
    }
    
    // Kısayol (varsa göster)
    if (sablon.kisaYol) {
        $('#sablonDetayKisaYol').text(sablon.kisaYol);
        $('#sablonDetayKisaYolDiv').show();
    } else {
        $('#sablonDetayKisaYolDiv').hide();
    }
    
    // Kullanım sayısı
    $('#sablonDetayKullanim').text(sablon.kullanimSayisi + ' kez kullanıldı');
    
    // Modalı aç
    $('#sablonDetayModal').modal('show');
}


function showAlert(message, type) {
    // Basit toast benzeri alert
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible alert-animate" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    $('body').append(alertHtml);
    
    setTimeout(function () {
        $('.alert-animate').fadeOut(function () {
            $(this).remove();
        });
    }, 4000);
}

// CSRF Token (Her AJAX isteğinde otomatik ekle)
$.ajaxSetup({
    beforeSend: function (xhr, settings) {
        if (settings.type !== 'GET') {
            const token = $('input[name="__RequestVerificationToken"]').val();
            if (token) {
                xhr.setRequestHeader('RequestVerificationToken', token);
            }
        }
    }
});

