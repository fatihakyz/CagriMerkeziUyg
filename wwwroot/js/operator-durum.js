// Operatör Durum Yönetimi JavaScript

$(document).ready(function () {
    // Anti-forgery token'ı al
    const token = $('input[name="__RequestVerificationToken"]').val();

    // İlk yüklemede durumu getir
    mevcutDurumuGetir();

    // Her 30 saniyede bir durumu güncelle
    setInterval(mevcutDurumuGetir, 30000);

    // Durum değiştirme butonları
    $('.durum-item').on('click', function (e) {
        e.preventDefault();
        const yeniDurum = $(this).data('durum');
        durumDegistir(yeniDurum);
    });

    /**
     * Mevcut durumu sunucudan getir ve UI'ı güncelle
     */
    function mevcutDurumuGetir() {
        $.ajax({
            url: '/Operator/MevcutDurumGetir',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    durumUIGuncelle(response);
                }
            },
            error: function () {
                console.error('Durum getirilemedi');
            }
        });
    }

    /**
     * Durumu değiştir
     */
    function durumDegistir(yeniDurum) {
        // Loading göster
        $('#mevcutDurumBadge').html('<i class="bx bx-loader bx-spin me-1"></i><span>Güncelleniyor...</span>');

        $.ajax({
            url: '/Operator/HizliDurumDegistir',
            type: 'POST',
            data: {
                durum: yeniDurum,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    toastr.success('Durum başarıyla değiştirildi', 'Başarılı!');
                    mevcutDurumuGetir(); // Durumu yeniden getir
                } else {
                    toastr.error(response.message, 'Hata!');
                    mevcutDurumuGetir(); // Eski duruma dön
                }
            },
            error: function () {
                toastr.error('Durum değiştirilemedi', 'Hata!');
                mevcutDurumuGetir(); // Eski duruma dön
            }
        });
    }

    /**
     * UI'ı güncelle
     */
    function durumUIGuncelle(data) {
        const durumlar = {
            'Offline': { text: 'Çevrimdışı', icon: 'bx-power-off', class: 'bg-dark' },
            'Musait': { text: 'Müsait', icon: 'bx-check-circle', class: 'bg-success' },
            'Cagirida': { text: 'Çağrıda', icon: 'bx-phone', class: 'bg-danger' },
            'AraCalısma': { text: 'Ara Çalışma', icon: 'bx-pen', class: 'bg-warning' },
            'Mola': { text: 'Mola', icon: 'bx-coffee', class: 'bg-info' },
            'OgleYemegi': { text: 'Öğle Yemeği', icon: 'bx-food-menu', class: 'bg-info' },
            'Egitimde': { text: 'Eğitimde', icon: 'bx-book', class: 'bg-primary' },
            'Toplantida': { text: 'Toplantıda', icon: 'bx-group', class: 'bg-secondary' },
            'Uzakta': { text: 'Uzakta', icon: 'bx-time', class: 'bg-secondary' },
            'Mesgul': { text: 'Meşgul', icon: 'bx-minus-circle', class: 'bg-warning' }
        };

        const durum = durumlar[data.durum] || durumlar['Offline'];
        
        $('#mevcutDurumBadge')
            .removeClass()
            .addClass('durum-badge badge-lg badge ' + durum.class)
            .html(`<i class="bx ${durum.icon} me-1"></i><span>${durum.text}</span>`);

        // Süreyi güncelle
        if (data.durumSuresi) {
            const sureDk = Math.floor(data.durumSuresi);
            const sureSn = Math.floor((data.durumSuresi - sureDk) * 60);
            $('#durumSuresi').text(`${sureDk}:${sureSn.toString().padStart(2, '0')}`);
        }

        // Eğer çağrıda ise uyarı ver
        if (data.durum === 'Cagirida' && data.durumSuresi > 30) {
            if (!$('#cagriUyarisi').length) {
                toastr.warning('Çağrı 30 dakikayı aştı!', 'Dikkat', {
                    timeOut: 10000,
                    extendedTimeOut: 5000,
                    closeButton: true,
                    progressBar: true
                });
            }
        }

        // Mola uyarısı
        if ((data.durum === 'Mola' && data.durumSuresi > 20) || 
            (data.durum === 'OgleYemegi' && data.durumSuresi > 60)) {
            if (!$('#molaUyarisi').length) {
                toastr.info('Mola süreniz uzadı, lütfen sisteme geri dönün', 'Bilgilendirme', {
                    timeOut: 10000,
                    extendedTimeOut: 5000,
                    closeButton: true,
                    progressBar: true
                });
            }
        }
    }

    /**
     * Bugünkü durum özetini göster (modal)
     */
    window.bugunkuDurumOzeti = function() {
        $.ajax({
            url: '/Operator/BugunkuDurumOzeti',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const data = response.data;
                    
                    let html = `
                        <div class="modal fade" id="durumOzetiModal" tabindex="-1">
                            <div class="modal-dialog modal-lg">
                                <div class="modal-content">
                                    <div class="modal-header">
                                        <h5 class="modal-title">
                                            <i class="bx bx-bar-chart me-2"></i>Bugünkü Durum Özetim
                                        </h5>
                                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                    </div>
                                    <div class="modal-body">
                                        <div class="row">
                                            <div class="col-md-6 mb-3">
                                                <div class="card bg-light">
                                                    <div class="card-body">
                                                        <h6 class="card-title">⏱️ Toplam Çalışma</h6>
                                                        <h3>${data.toplamCalismaSuresi} dk</h3>
                                                        <small class="text-muted">${(data.toplamCalismaSuresi / 60).toFixed(1)} saat</small>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6 mb-3">
                                                <div class="card bg-light">
                                                    <div class="card-body">
                                                        <h6 class="card-title">📞 Çağrı Süresi</h6>
                                                        <h3>${data.cagrıdaGecenSure} dk</h3>
                                                        <small class="text-muted">${data.toplamCagriSayisi} çağrı</small>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-4 mb-3">
                                                <div class="card">
                                                    <div class="card-body text-center">
                                                        <p class="mb-1">☕ Mola</p>
                                                        <strong>${data.molaSuresi} dk</strong>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-4 mb-3">
                                                <div class="card">
                                                    <div class="card-body text-center">
                                                        <p class="mb-1">🍽️ Öğle</p>
                                                        <strong>${data.ogleYemegiSuresi} dk</strong>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-4 mb-3">
                                                <div class="card">
                                                    <div class="card-body text-center">
                                                        <p class="mb-1">✍️ Ara Çalışma</p>
                                                        <strong>${data.araCalismaSuresi} dk</strong>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        
                                        <hr>
                                        
                                        <div class="row">
                                            <div class="col-md-6">
                                                <h6>📊 Verimlilik Oranı</h6>
                                                <div class="progress mb-3" style="height: 25px;">
                                                    <div class="progress-bar ${data.verimlilıkOrani >= 80 ? 'bg-success' : data.verimlilıkOrani >= 60 ? 'bg-warning' : 'bg-danger'}" 
                                                         style="width: ${data.verimlilıkOrani}%">
                                                        ${data.verimlilıkOrani}%
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <h6>📈 Kullanım Oranı</h6>
                                                <div class="progress mb-3" style="height: 25px;">
                                                    <div class="progress-bar ${data.kullanimOrani >= 70 ? 'bg-success' : data.kullanimOrani >= 50 ? 'bg-info' : 'bg-warning'}" 
                                                         style="width: ${data.kullanimOrani}%">
                                                        ${data.kullanimOrani}%
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        
                                        <div class="alert alert-info">
                                            <i class="bx bx-info-circle me-1"></i>
                                            <strong>Ortalama Çağrı Süresi:</strong> ${data.ortalamaCagriSuresi} dakika
                                        </div>
                                    </div>
                                    <div class="modal-footer">
                                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    
                    // Var olan modalı kaldır
                    $('#durumOzetiModal').remove();
                    
                    // Yeni modalı ekle ve göster
                    $('body').append(html);
                    $('#durumOzetiModal').modal('show');
                    
                    // Modal kapanınca DOM'dan kaldır
                    $('#durumOzetiModal').on('hidden.bs.modal', function () {
                        $(this).remove();
                    });
                }
            },
            error: function () {
                toastr.error('Özet getirilemedi', 'Hata!');
            }
        });
    };

    /**
     * Bugünkü durum geçmişini göster (modal)
     */
    window.bugunkuDurumGecmisi = function() {
        $.ajax({
            url: '/Operator/BugunkuDurumGecmisi',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const gecmis = response.data;
                    
                    let html = `
                        <div class="modal fade" id="durumGecmisiModal" tabindex="-1">
                            <div class="modal-dialog modal-lg">
                                <div class="modal-content">
                                    <div class="modal-header">
                                        <h5 class="modal-title">
                                            <i class="bx bx-history me-2"></i>Bugünkü Durum Geçmişim
                                        </h5>
                                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                    </div>
                                    <div class="modal-body">
                                        <div class="table-responsive">
                                            <table class="table table-hover">
                                                <thead>
                                                    <tr>
                                                        <th>Saat</th>
                                                        <th>Önceki Durum</th>
                                                        <th>Yeni Durum</th>
                                                        <th>Süre</th>
                                                        <th>Not</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                    `;
                    
                    if (gecmis.length === 0) {
                        html += `<tr><td colspan="5" class="text-center text-muted">Henüz kayıt yok</td></tr>`;
                    } else {
                        gecmis.forEach(function(g) {
                            html += `
                                <tr>
                                    <td>${g.gecisZamani}</td>
                                    <td><span class="badge bg-secondary">${g.oncekiDurum}</span></td>
                                    <td><span class="badge bg-primary">${g.yeniDurum}</span></td>
                                    <td>${g.sureDakika ? g.sureDakika + ' dk' : '-'}</td>
                                    <td>${g.not || '-'}</td>
                                </tr>
                            `;
                        });
                    }
                    
                    html += `
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                    <div class="modal-footer">
                                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    
                    // Modal göster
                    $('#durumGecmisiModal').remove();
                    $('body').append(html);
                    $('#durumGecmisiModal').modal('show');
                    $('#durumGecmisiModal').on('hidden.bs.modal', function () {
                        $(this).remove();
                    });
                }
            },
            error: function () {
                toastr.error('Geçmiş getirilemedi', 'Hata!');
            }
        });
    };
});












