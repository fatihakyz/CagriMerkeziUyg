// Admin Canlı Durum Dashboard JavaScript

$(document).ready(function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    // İlk yüklemede verileri getir
    tumDurumlariGetir();
    uyarilariGetir();
    
    // Her 5 saniyede otomatik yenile
    setInterval(function() {
        tumDurumlariGetir();
        uyarilariGetir();
    }, 5000);
    
    // Manuel yenile butonu
    $('#btnYenile').on('click', function() {
        tumDurumlariGetir();
        uyarilariGetir();
    });
    
    /**
     * Tüm operatör durumlarını getir
     */
    function tumDurumlariGetir() {
        $.ajax({
            url: '/Admin/GetTumOperatorDurumlari',
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    durumOzetiniGuncelle(response);
                    operatorKartlariniOlustur(response.operatorler);
                    $('#sonGuncelleme').text(new Date().toLocaleTimeString('tr-TR'));
                }
            },
            error: function() {
                console.error('Durumlar getirilemedi');
            }
        });
    }
    
    /**
     * Uyarıları getir
     */
    function uyarilariGetir() {
        $.ajax({
            url: '/Admin/GetDurumUyarilari',
            type: 'GET',
            success: function(response) {
                if (response.success && response.uyariSayisi > 0) {
                    uyarilariGoster(response.uyarilar);
                } else {
                    $('#uyarilarContainer').hide();
                }
            }
        });
    }
    
    /**
     * Durum özetini güncelle
     */
    function durumOzetiniGuncelle(data) {
        $('#toplamOperator').text(data.toplamOperator);
        
        let musait = 0, cagirida = 0, araCalısma = 0, mola = 0, diger = 0;
        
        data.operatorler.forEach(function(op) {
            switch(op.mevcutDurum) {
                case 'Musait':
                    musait++;
                    break;
                case 'Cagirida':
                    cagirida++;
                    break;
                case 'AraCalısma':
                    araCalısma++;
                    break;
                case 'Mola':
                case 'OgleYemegi':
                    mola++;
                    break;
                default:
                    diger++;
            }
        });
        
        $('#musaitSayisi').text(musait);
        $('#cagrıdaSayisi').text(cagirida);
        $('#araCalismaSayisi').text(araCalısma);
        $('#molaSayisi').text(mola);
        $('#digerSayisi').text(diger);
    }
    
    /**
     * Operatör kartlarını oluştur
     */
    function operatorKartlariniOlustur(operatorler) {
        let html = '';
        
        if (operatorler.length === 0) {
            html = '<div class="col-12 text-center py-5 text-muted">Aktif operatör bulunamadı</div>';
        } else {
            operatorler.forEach(function(op) {
                const durumBilgisi = getDurumBilgisi(op.mevcutDurum);
                const uyariClass = op.durumSuresi > 30 && (op.mevcutDurum === 'Mola' || op.mevcutDurum === 'OgleYemegi') ? 'warning' : '';
                
                html += `
                    <div class="col-xl-3 col-lg-4 col-md-6 mb-4">
                        <div class="card operator-card ${uyariClass}" data-operator-id="${op.id}" style="cursor: pointer;">
                            <div class="card-body text-center">
                                <div class="operator-avatar ${durumBilgisi.bgClass}">
                                    <i class="bx ${durumBilgisi.icon} text-white"></i>
                                </div>
                                <h5 class="card-title mb-1">${op.tamAd}</h5>
                                <span class="badge ${durumBilgisi.badgeClass} status-badge mb-2">
                                    <i class="bx ${durumBilgisi.icon} me-1"></i>${durumBilgisi.text}
                                </span>
                                <p class="text-muted mb-0">
                                    <i class="bx bx-time me-1"></i>${Math.floor(op.durumSuresi)} dakikadır
                                </p>
                                ${op.durumNotu ? `<small class="text-muted"><i class="bx bx-note"></i> ${op.durumNotu}</small>` : ''}
                            </div>
                        </div>
                    </div>
                `;
            });
        }
        
        $('#operatorKartlari').html(html);
        
        // Kart click eventi
        $('.operator-card').on('click', function() {
            const operatorId = $(this).data('operator-id');
            operatorDetayGoster(operatorId);
        });
    }
    
    /**
     * Uyarıları göster
     */
    function uyarilariGoster(uyarilar) {
        let html = '<ul class="mb-0">';
        uyarilar.forEach(function(uyari) {
            html += `<li><strong>${uyari.operatorAdi}:</strong> ${uyari.uyariMesaji}</li>`;
        });
        html += '</ul>';
        
        $('#uyarilarListesi').html(html);
        $('#uyarilarContainer').show();
    }
    
    /**
     * Operatör detayını göster
     */
    function operatorDetayGoster(operatorId) {
        // Modal içeriğini yükle
        $.ajax({
            url: '/Admin/OperatorDurumGecmisi',
            type: 'GET',
            data: { operatorId: operatorId },
            success: function(response) {
                if (response.success) {
                    $('#operatorDetayBaslik').html(`<i class="bx bx-user me-2"></i>${response.operatorAdi}`);
                    $('#detayOperatorId').val(operatorId);
                    
                    // Geçmişi göster
                    let html = '';
                    if (response.gecmis.length === 0) {
                        html = '<tr><td colspan="4" class="text-center text-muted">Bugün henüz durum değişikliği yok</td></tr>';
                    } else {
                        response.gecmis.forEach(function(g) {
                            html += `
                                <tr>
                                    <td>${g.gecisZamani}</td>
                                    <td><span class="badge bg-secondary">${g.oncekiDurum}</span></td>
                                    <td><span class="badge bg-primary">${g.yeniDurum}</span></td>
                                    <td>${g.sureDakika ? g.sureDakika + ' dk' : '-'}</td>
                                </tr>
                            `;
                        });
                    }
                    $('#detayDurumGecmisi').html(html);
                    
                    $('#operatorDetayModal').modal('show');
                }
            }
        });
    }
    
    /**
     * Süpervizör operatör durumunu değiştir
     */
    $('#btnDurumDegistir').on('click', function() {
        const operatorId = $('#detayOperatorId').val();
        const yeniDurum = $('#yeniDurumSecim').val();
        
        if (!operatorId || !yeniDurum) {
            toastr.error('Lütfen durum seçin', 'Hata');
            return;
        }
        
        if (!confirm(`${$('#operatorDetayBaslik').text().replace(/.*\s/, '')} operatörünün durumunu değiştirmek istediğinize emin misiniz?`)) {
            return;
        }
        
        $.ajax({
            url: '/Admin/OperatorDurumDegistir',
            type: 'POST',
            data: {
                operatorId: operatorId,
                yeniDurum: yeniDurum,
                not: `Süpervizör tarafından değiştirildi`,
                __RequestVerificationToken: token
            },
            success: function(response) {
                if (response.success) {
                    toastr.success('Durum başarıyla değiştirildi', 'Başarılı');
                    $('#operatorDetayModal').modal('hide');
                    tumDurumlariGetir();
                } else {
                    toastr.error(response.message, 'Hata');
                }
            },
            error: function() {
                toastr.error('Durum değiştirilemedi', 'Hata');
            }
        });
    });
    
    /**
     * Durum bilgisini döndür
     */
    function getDurumBilgisi(durum) {
        const durumlar = {
            'Offline': { text: 'Çevrimdışı', icon: 'bx-power-off', badgeClass: 'bg-dark', bgClass: 'bg-dark' },
            'Musait': { text: 'Müsait', icon: 'bx-check-circle', badgeClass: 'bg-success', bgClass: 'bg-success' },
            'Cagirida': { text: 'Çağrıda', icon: 'bx-phone', badgeClass: 'bg-danger', bgClass: 'bg-danger' },
            'AraCalısma': { text: 'Ara Çalışma', icon: 'bx-pen', badgeClass: 'bg-warning', bgClass: 'bg-warning' },
            'Mola': { text: 'Mola', icon: 'bx-coffee', badgeClass: 'bg-info', bgClass: 'bg-info' },
            'OgleYemegi': { text: 'Öğle Yemeği', icon: 'bx-food-menu', badgeClass: 'bg-info', bgClass: 'bg-info' },
            'Egitimde': { text: 'Eğitimde', icon: 'bx-book', badgeClass: 'bg-primary', bgClass: 'bg-primary' },
            'Toplantida': { text: 'Toplantıda', icon: 'bx-group', badgeClass: 'bg-secondary', bgClass: 'bg-secondary' },
            'Uzakta': { text: 'Uzakta', icon: 'bx-time', badgeClass: 'bg-secondary', bgClass: 'bg-secondary' },
            'Mesgul': { text: 'Meşgul', icon: 'bx-minus-circle', badgeClass: 'bg-warning', bgClass: 'bg-warning' }
        };
        
        return durumlar[durum] || durumlar['Offline'];
    }
});












