/* ADMIN ŞABLON YÖNETİMİ - JAVASCRIPT */

// Sayfa yüklendiğinde
$(document).ready(function () {
    kategorileriYukle();
    sablonlariYukle();
    
    // Tarih filtreleri için varsayılan değerler
    var bugün = new Date().toISOString().split('T')[0];
    var birAyOnce = new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().split('T')[0];
    $('#istatistikBaslangic').val(birAyOnce);
    $('#istatistikBitis').val(bugün);
});
// KATEGORİ İŞLEMLERİ

function kategorileriYukle() {
    $.ajax({
        url: '/Admin/GetKategoriler',
        type: 'GET',
        success: function (response) {
            if (response.success) {
                kategorileriGoster(response.data);
                kategoriFilterDoldur(response.data);
                sablonKategoriDoldur(response.data);
            } else {
                showAlert(response.message || 'Kategoriler yüklenemedi', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
            $('#kategorilerBody').html('<tr><td colspan="7" class="text-center text-danger">Yükleme hatası!</td></tr>');
        }
    });
}

function kategorileriGoster(kategoriler) {
    if (kategoriler.length === 0) {
        $('#kategorilerBody').html('<tr><td colspan="7" class="text-center text-muted">Henüz kategori eklenmemiş</td></tr>');
        return;
    }

    let html = '';
    kategoriler.forEach(function (kategori) {
        const aktiviteTuruText = getAktiviteTuruText(kategori.aktiviteTuru);
        const durumBadge = kategori.aktif 
            ? '<span class="badge bg-success">Aktif</span>' 
            : '<span class="badge bg-secondary">Pasif</span>';

        html += `
            <tr>
                <td>${kategori.sira}</td>
                <td>
                    <div class="d-flex align-items-center">
                        <div class="avatar avatar-sm me-2">
                            <span class="avatar-initial rounded" style="background-color: ${kategori.renkKodu}">
                                <i class="${kategori.iconClass || 'bx bx-folder'} text-white"></i>
                            </span>
                        </div>
                        <strong>${kategori.ad}</strong>
                    </div>
                </td>
                <td><small class="text-muted">${kategori.aciklama || '-'}</small></td>
                <td>${aktiviteTuruText}</td>
                <td><span class="badge bg-label-primary">${kategori.sablonSayisi}</span></td>
                <td>${durumBadge}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-icon btn-outline-primary" onclick='kategoriDuzenle(${JSON.stringify(kategori)})' title="Düzenle">
                        <i class="bx bx-edit"></i>
                    </button>
                    <button type="button" class="btn btn-sm btn-icon btn-outline-danger" onclick="kategoriSil(${kategori.id}, '${kategori.ad}')" title="Sil">
                        <i class="bx bx-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    $('#kategorilerBody').html(html);
}

function kategoriFilterDoldur(kategoriler) {
    let html = '<option value="">Tüm Kategoriler</option>';
    kategoriler.forEach(function (kategori) {
        if (kategori.aktif) {
            html += `<option value="${kategori.id}">${kategori.ad}</option>`;
        }
    });
    $('#kategoriFiltre').html(html);
}

function sablonKategoriDoldur(kategoriler) {
    let html = '<option value="">Seçiniz...</option>';
    kategoriler.forEach(function (kategori) {
        if (kategori.aktif) {
            html += `<option value="${kategori.id}">${kategori.ad}</option>`;
        }
    });
    $('#sablonKategoriId').html(html);
}

function kategoriModalAc(islem) {
    $('#kategoriIslem').val(islem);
    
    if (islem === 'ekle') {
        $('#kategoriModalTitle').text('Yeni Kategori');
        $('#kategoriForm')[0].reset();
        $('#kategoriId').val('');
        $('#kategoriAktif').prop('checked', true);
        $('#kategoriRenkKodu').val('#007bff');
    }
}

function kategoriDuzenle(kategori) {
    $('#kategoriIslem').val('duzenle');
    $('#kategoriModalTitle').text('Kategori Düzenle');
    
    $('#kategoriId').val(kategori.id);
    $('#kategoriAd').val(kategori.ad);
    $('#kategoriAciklama').val(kategori.aciklama || '');
    $('#kategoriIconClass').val(kategori.iconClass || '');
    $('#kategoriRenkKodu').val(kategori.renkKodu);
    $('#kategoriSira').val(kategori.sira);
    $('#kategoriAktiviteTuru').val(kategori.aktiviteTuru || '');
    $('#kategoriAktif').prop('checked', kategori.aktif);
    
    $('#kategoriModal').modal('show');
}

function kategoriKaydet() {
    const islem = $('#kategoriIslem').val();
    const data = {
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
        Id: $('#kategoriId').val() || 0,
        Ad: $('#kategoriAd').val(),
        Aciklama: $('#kategoriAciklama').val(),
        IconClass: $('#kategoriIconClass').val(),
        RenkKodu: $('#kategoriRenkKodu').val(),
        Sira: parseInt($('#kategoriSira').val()),
        AktiviteTuru: $('#kategoriAktiviteTuru').val() || null,
        Aktif: $('#kategoriAktif').is(':checked')
    };

    if (!data.Ad) {
        showAlert('Kategori adı zorunludur', 'warning');
        return;
    }

    const url = islem === 'ekle' ? '/Admin/KategoriEkle' : '/Admin/KategoriGuncelle';

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function (response) {
            if (response.success) {
                showAlert(response.message, 'success');
                $('#kategoriModal').modal('hide');
                kategorileriYukle();
            } else {
                showAlert(response.message || 'İşlem başarısız', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        }
    });
}

function kategoriSil(id, ad) {
    if (!confirm(`"${ad}" kategorisini silmek istediğinizden emin misiniz?`)) {
        return;
    }

    $.ajax({
        url: '/Admin/KategoriSil',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            id: id
        },
        success: function (response) {
            if (response.success) {
                showAlert(response.message, 'success');
                kategorileriYukle();
            } else {
                showAlert(response.message || 'Silme işlemi başarısız', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        }
    });
}

// ŞABLON İŞLEMLERİ

function sablonlariYukle() {
    const kategoriId = $('#kategoriFiltre').val();

    $.ajax({
        url: '/Admin/GetSablonlar',
        type: 'GET',
        data: { kategoriId: kategoriId || null },
        success: function (response) {
            if (response.success) {
                sablonlariGoster(response.data);
            } else {
                showAlert(response.message || 'Şablonlar yüklenemedi', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
            $('#sablonlarBody').html('<tr><td colspan="8" class="text-center text-danger">Yükleme hatası!</td></tr>');
        }
    });
}

function sablonlariGoster(sablonlar) {
    if (sablonlar.length === 0) {
        $('#sablonlarBody').html('<tr><td colspan="8" class="text-center text-muted">Henüz şablon eklenmemiş</td></tr>');
        return;
    }

    let html = '';
    sablonlar.forEach(function (sablon) {
        const icerikKisa = sablon.icerik.length > 50 
            ? sablon.icerik.substring(0, 50) + '...' 
            : sablon.icerik;
        
        const durumBadge = sablon.aktif 
            ? '<span class="badge bg-success">Aktif</span>' 
            : '<span class="badge bg-secondary">Pasif</span>';

        const kisaYolBadge = sablon.kisaYol 
            ? `<span class="badge bg-info">${sablon.kisaYol}</span>` 
            : '-';

        html += `
            <tr>
                <td>${sablon.sira}</td>
                <td>
                    <span class="badge" style="background-color: ${sablon.kategoriRenk}">${sablon.kategoriAd}</span>
                </td>
                <td><strong>${sablon.baslik}</strong></td>
                <td>
                    <small class="text-muted">${icerikKisa}</small>
                    ${sablon.degiskenIceriyor ? '<i class="bx bx-code-alt text-info ms-1" title="Değişken içeriyor"></i>' : ''}
                </td>
                <td>${kisaYolBadge}</td>
                <td>
                    <span class="badge bg-label-primary">${sablon.kullanimSayisi}</span>
                </td>
                <td>${durumBadge}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-icon btn-outline-info" onclick='sablonDetay(${JSON.stringify(sablon)})' title="Detay">
                        <i class="bx bx-show"></i>
                    </button>
                    <button type="button" class="btn btn-sm btn-icon btn-outline-primary" onclick='sablonDuzenle(${JSON.stringify(sablon)})' title="Düzenle">
                        <i class="bx bx-edit"></i>
                    </button>
                    <button type="button" class="btn btn-sm btn-icon btn-outline-danger" onclick="sablonSil(${sablon.id}, '${sablon.baslik}')" title="Sil">
                        <i class="bx bx-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    $('#sablonlarBody').html(html);
}

function sablonModalAc(islem) {
    $('#sablonIslem').val(islem);
    
    if (islem === 'ekle') {
        $('#sablonModalTitle').text('Yeni Şablon');
        $('#sablonForm')[0].reset();
        $('#sablonId').val('');
        $('#sablonAktif').prop('checked', true);
    }
}

function sablonDuzenle(sablon) {
    $('#sablonIslem').val('duzenle');
    $('#sablonModalTitle').text('Şablon Düzenle');
    
    $('#sablonId').val(sablon.id);
    $('#sablonBaslik').val(sablon.baslik);
    $('#sablonKategoriId').val(sablon.kategoriId);
    $('#sablonIcerik').val(sablon.icerik);
    $('#sablonNotlar').val(sablon.notlar || '');
    $('#sablonSira').val(sablon.sira);
    $('#sablonKisaYol').val(sablon.kisaYol || '');
    $('#sablonAktif').prop('checked', sablon.aktif);
    
    $('#sablonModal').modal('show');
}

function sablonDetay(sablon) {
    alert(`Başlık: ${sablon.baslik}\n\nKategori: ${sablon.kategoriAd}\n\nİçerik:\n${sablon.icerik}\n\nKullanım: ${sablon.kullanimSayisi} kez\n\nOluşturan: ${sablon.olusturanAd}`);
}

function sablonKaydet() {
    const islem = $('#sablonIslem').val();
    const data = {
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
        Id: $('#sablonId').val() || 0,
        KategoriId: parseInt($('#sablonKategoriId').val()),
        Baslik: $('#sablonBaslik').val(),
        Icerik: $('#sablonIcerik').val(),
        Notlar: $('#sablonNotlar').val(),
        Sira: parseInt($('#sablonSira').val()),
        KisaYol: $('#sablonKisaYol').val(),
        Aktif: $('#sablonAktif').is(':checked')
    };

    if (!data.Baslik || !data.Icerik || !data.KategoriId) {
        showAlert('Başlık, içerik ve kategori zorunludur', 'warning');
        return;
    }

    const url = islem === 'ekle' ? '/Admin/SablonEkle' : '/Admin/SablonGuncelle';

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function (response) {
            if (response.success) {
                showAlert(response.message, 'success');
                $('#sablonModal').modal('hide');
                sablonlariYukle();
            } else {
                showAlert(response.message || 'İşlem başarısız', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        }
    });
}

function sablonSil(id, baslik) {
    if (!confirm(`"${baslik}" şablonunu silmek istediğinizden emin misiniz?`)) {
        return;
    }

    $.ajax({
        url: '/Admin/SablonSil',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            id: id
        },
        success: function (response) {
            if (response.success) {
                showAlert(response.message, 'success');
                sablonlariYukle();
            } else {
                showAlert(response.message || 'Silme işlemi başarısız', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        }
    });
}

// İSTATİSTİKLER

function istatistikleriYukle() {
    const baslangic = $('#istatistikBaslangic').val();
    const bitis = $('#istatistikBitis').val();

    if (!baslangic || !bitis) {
        showAlert('Lütfen tarih aralığı seçin', 'warning');
        return;
    }

    $.ajax({
        url: '/Admin/GetSablonIstatistikleri',
        type: 'GET',
        data: {
            baslangic: baslangic,
            bitis: bitis
        },
        success: function (response) {
            if (response.success) {
                istatistikleriGoster(response);
            } else {
                showAlert(response.message || 'İstatistikler yüklenemedi', 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası oluştu', 'danger');
        }
    });
}

function istatistikleriGoster(data) {
    // Şablon istatistikleri
    let sablonHtml = '';
    if (data.sablonIstatistik && data.sablonIstatistik.length > 0) {
        data.sablonIstatistik.slice(0, 10).forEach(function (item) {
            sablonHtml += `
                <tr>
                    <td>${item.sablonBaslik}</td>
                    <td><span class="badge bg-label-primary">${item.kategoriAd}</span></td>
                    <td class="text-end"><strong>${item.kullanimSayisi}</strong></td>
                </tr>
            `;
        });
    } else {
        sablonHtml = '<tr><td colspan="3" class="text-center text-muted">Veri yok</td></tr>';
    }
    $('#sablonIstatistikBody').html(sablonHtml);

    // Kategori istatistikleri
    let kategoriHtml = '';
    if (data.kategoriIstatistik && data.kategoriIstatistik.length > 0) {
        data.kategoriIstatistik.forEach(function (item) {
            kategoriHtml += `
                <tr>
                    <td>${item.kategoriAd}</td>
                    <td class="text-end"><strong>${item.kullanimSayisi}</strong></td>
                </tr>
            `;
        });
    } else {
        kategoriHtml = '<tr><td colspan="2" class="text-center text-muted">Veri yok</td></tr>';
    }
    $('#kategoriIstatistikBody').html(kategoriHtml);

    // Operatör istatistikleri
    let operatorHtml = '';
    if (data.operatorIstatistik && data.operatorIstatistik.length > 0) {
        data.operatorIstatistik.slice(0, 10).forEach(function (item) {
            operatorHtml += `
                <tr>
                    <td>${item.operatorAd}</td>
                    <td class="text-end"><strong>${item.kullanimSayisi}</strong></td>
                    <td class="text-end"><span class="badge bg-label-info">${item.farkliSablonSayisi}</span></td>
                </tr>
            `;
        });
    } else {
        operatorHtml = '<tr><td colspan="3" class="text-center text-muted">Veri yok</td></tr>';
    }
    $('#operatorIstatistikBody').html(operatorHtml);
}

// HELPER FUNCTIONS

function getAktiviteTuruText(tur) {
    if (tur === null || tur === undefined || tur === '') {
        return '<span class="badge bg-label-secondary">Tümü</span>';
    }
    
    const turler = {
        '0': '<span class="badge bg-label-primary">Telefon</span>',
        '1': '<span class="badge bg-label-info">Email</span>',
        '2': '<span class="badge bg-label-danger">Şikayet</span>',
        '3': '<span class="badge bg-label-warning">Talep</span>',
        '4': '<span class="badge bg-label-secondary">Diğer</span>'
    };
    
    return turler[tur] || '<span class="badge bg-label-secondary">Tümü</span>';
}

function showAlert(message, type) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    $('body').append(alertHtml);
    
    setTimeout(function () {
        $('.alert').fadeOut(function () {
            $(this).remove();
        });
    }, 4000);
}

// İstatistikler tab'ı açıldığında otomatik yükle
$('button[data-bs-target="#istatistikler-tab"]').on('shown.bs.tab', function () {
    istatistikleriYukle();
});












