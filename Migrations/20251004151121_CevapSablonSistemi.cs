using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class CevapSablonSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GeciciKayit",
                table: "Musteriler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KayitDurumu",
                table: "Musteriler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "KayitTamamlandi",
                table: "Musteriler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CagriSirasindaKayitOlusturuldu",
                table: "AramaLoglari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MusteriKayitliydi",
                table: "AramaLoglari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CevapSablonKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Kategori adı"),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Kategori açıklaması"),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Font Awesome icon sınıfı"),
                    RenkKodu = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false, defaultValue: "#007bff", comment: "Kategori renk kodu (HEX)"),
                    Sira = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Gösterim sırası"),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Kategori aktif mi"),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Kategori oluşturulma tarihi"),
                    AktiviteTuru = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Hangi aktivite türü için (null ise tümü)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CevapSablonKategorileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CevapSablonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KategoriId = table.Column<int>(type: "int", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Şablon başlığı"),
                    Icerik = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "Şablon içeriği"),
                    Notlar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Operatör için notlar"),
                    Sira = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Kategori içinde sıra"),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Şablon aktif mi"),
                    KullanimSayisi = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Kaç kez kullanıldı"),
                    SonKullanimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Son kullanım tarihi"),
                    DegiskenIceriyor = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Değişken placeholder içeriyor mu"),
                    KisaYol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "Klavye kısayolu"),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Şablon oluşturulma tarihi"),
                    OlusturanOperatorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CevapSablonlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CevapSablonlari_CevapSablonKategorileri_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "CevapSablonKategorileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CevapSablonlari_Operatorler_OlusturanOperatorId",
                        column: x => x.OlusturanOperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CevapSablonKullanimlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SablonId = table.Column<int>(type: "int", nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    MusteriId = table.Column<int>(type: "int", nullable: true),
                    AktiviteId = table.Column<int>(type: "int", nullable: true),
                    KullanimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Kullanım tarihi")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CevapSablonKullanimlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CevapSablonKullanimlar_CevapSablonlari_SablonId",
                        column: x => x.SablonId,
                        principalTable: "CevapSablonlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CevapSablonKullanimlar_MusteriAktiviteleri_AktiviteId",
                        column: x => x.AktiviteId,
                        principalTable: "MusteriAktiviteleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CevapSablonKullanimlar_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CevapSablonKullanimlar_Operatorler_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CevapSablonKategorileri",
                columns: new[] { "Id", "Aciklama", "Ad", "Aktif", "AktiviteTuru", "IconClass", "OlusturulmaTarihi", "RenkKodu", "Sira" },
                values: new object[,]
                {
                    { 1, "Çağrı başlangıcı ve bitiş mesajları", "Karşılama & Veda", true, null, "fa-hand-wave", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "#28a745", 1 },
                    { 2, "Ürün özellikleri, fiyat, stok bilgileri", "Ürün & Hizmet Bilgisi", true, null, "fa-box", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "#007bff", 2 },
                    { 3, "İade süreci, iptal talepleri", "İade & İptal", true, null, "fa-undo", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "#dc3545", 3 },
                    { 4, "Sipariş takibi, teslimat bilgileri", "Sipariş & Teslimat", true, null, "fa-truck", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "#fd7e14", 4 },
                    { 5, "Sorun giderme, kurulum yardımı", "Teknik Destek", true, null, "fa-tools", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "#6f42c1", 5 },
                    { 6, "Ödeme yöntemleri, fatura talepleri", "Ödeme & Fatura", true, null, "fa-credit-card", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "#20c997", 6 },
                    { 7, "Şikayet alma ve yönetme", "Şikayet Yönetimi", true, "Sikayet", "fa-exclamation-triangle", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "#ffc107", 7 }
                });

            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GeciciKayit", "KayitDurumu", "KayitTamamlandi" },
                values: new object[] { false, 0, true });

            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GeciciKayit", "KayitDurumu", "KayitTamamlandi" },
                values: new object[] { false, 0, true });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "DegiskenIceriyor", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[,]
                {
                    { 1, true, "Standart Karşılama", true, "Merhaba, [Şirket Adı] çağrı merkezine hoş geldiniz. Ben {OperatorAdi}, size nasıl yardımcı olabilirim?", 1, "F1", "Müşteriye güler yüzle hitap edin", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null },
                    { 2, true, "VIP Müşteri Karşılama", true, "Sayın {MusteriAdi}, hoş geldiniz! Sizi tanıdığımız için çok mutluyuz. Size nasıl yardımcı olabilirim?", 1, "F2", "VIP etiketli müşteriler için kullanın", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, null },
                    { 3, true, "Standart Veda", true, "Bize ulaştığınız için teşekkür ederim {MusteriAdi}. İyi günler dilerim!", 1, "F3", "Çağrı sonunda kullanın", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, null },
                    { 4, true, "Fiyat Bilgisi", true, "İlgilendiğiniz ürünün güncel fiyatını size aktarıyorum. Ürün fiyatı: {Fiyat} TL. Kargo ücreti dahildir. Başka bir ürün hakkında bilgi almak ister misiniz?", 2, null, "Fiyat bilgisini güncel sistemden kontrol edin", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null }
                });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[] { 5, true, "Stok Durumu - Var", "İyi haberlerim var! İlgilendiğiniz ürün stoklarımızda mevcut. Hemen sipariş verebilirsiniz. Sipariş vermek ister misiniz?", 2, null, null, null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, null });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "DegiskenIceriyor", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[] { 6, true, "Stok Durumu - Yok", true, "Üzgünüm, ilgilendiğiniz ürün şu anda stoklarımızda bulunmuyor. Ancak {TarihTahmini} tarihinde tekrar stoklarımıza gireceğini tahmin ediyoruz. Bilgilendirme için iletişim bilgilerinizi kaydedebilirim.", 2, null, "Tahmini tarihi lojistik ekibinden öğrenin", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, null });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[] { 7, true, "Standart İade Süreci", "Tabii efendim, iade işleminize yardımcı olabilirim. İade prosedürümüz şu şekildedir:\n\n1. Ürün 14 gün içinde iade edilmelidir\n2. Ürün kullanılmamış ve ambalajı açılmamış olmalıdır\n3. Fatura veya sipariş numarası gereklidir\n\nKargo ücreti cayma hakkı veya hasarlı ürün durumunda tarafımızca karşılanır. Size iade kodu gönderebilirim.", 3, "F4", "İade nedeni mutlaka sorun", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "DegiskenIceriyor", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[,]
                {
                    { 8, true, "Sipariş İptal", true, "Sipariş iptal talebinizi aldım. Sipariş numaranız {SiparisNo}. Eğer siparişiniz henüz kargoya verilmediyse iptal edebiliriz. Kargoya verildiyse ürünü teslim almadan iade edebilirsiniz. Kontrol edip size dönüş yapacağım.", 3, null, "Sipariş durumunu sisteme bakıp kontrol edin", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, null },
                    { 9, true, "Sipariş Takibi", true, "Sipariş numaranız: {SiparisNo}\nDurum: {Durum}\nKargo Takip No: {KargoNo}\n\nSiparişiniz {KargoFirmasi} ile gönderilmiştir. Takip numarası ile kargo firmasının web sitesinden detaylı takip yapabilirsiniz.", 4, "F5", "Bilgileri sistemden kontrol edin", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null }
                });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[,]
                {
                    { 10, true, "Teslimat Süresi", "Siparişiniz onaylandıktan sonra 1-3 iş günü içinde kargoya verilecektir. Kargo süresi bulunduğunuz ile bağlı olarak 2-5 iş günü arasında değişmektedir. Toplam teslimat süresi ortalama 3-8 iş günüdür.", 4, null, null, null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, null },
                    { 11, true, "Genel Sorun Giderme", "Yaşadığınız sorunu çözmek için birlikte adım adım ilerleyelim:\n\n1. Cihazı kapatıp 30 saniye bekleyin\n2. Tekrar açın\n3. Sorun devam ediyorsa bağlantı kablolarını kontrol edin\n4. Güncelleme olup olmadığını kontrol edin\n\nBu adımları denedikten sonra durum nasıl?", 5, null, "Sabırlı olun, müşteriye rehberlik edin", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null },
                    { 12, true, "Teknik Ekibe Yönlendirme", "Durumu değerlendirdim ve bu konuda teknik ekibimizin size yardımcı olması daha uygun olacaktır. Bilgilerinizi teknik destek ekibine ileteceğim ve en kısa sürede sizinle iletişime geçeceklerdir. İletişim bilgilerinizi teyit edebilir miyim?", 5, null, "Ticket oluşturun ve teknik ekibe atayın", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, null },
                    { 13, true, "Ödeme Yöntemleri", "Ödeme seçeneklerimiz şunlardır:\n\n• Kredi Kartı (Tek Çekim / Taksit)\n• Banka Havalesi / EFT\n• Kapıda Ödeme (Nakit / Kredi Kartı)\n\nHangi ödeme yöntemini tercih edersiniz?", 6, null, null, null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null },
                    { 14, true, "Fatura Talebi", "Faturanız sipariş tamamlandıktan sonra e-posta adresinize otomatik olarak gönderilecektir. Ayrıca hesabınızdan 'Siparişlerim' bölümünden de faturanızı indirebilirsiniz. Kurumsal fatura için fatura bilgilerinizi şimdi kaydedebilirim.", 6, null, "Kurumsal fatura için vergi no ve şirket unvanı alın", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, null }
                });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "DegiskenIceriyor", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[] { 15, true, "Şikayet Alma", true, "Yaşadığınız olumsuz deneyim için özür dilerim {MusteriAdi}. Şikayetinizi en üst düzeyde önemsiyoruz. Lütfen durumu detaylı anlatır mısınız? Sorununuzu çözmek için elimden geleni yapacağım.", 7, "F6", "Empatik olun, müşteriyi dinleyin, not alın", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null });

            migrationBuilder.InsertData(
                table: "CevapSablonlari",
                columns: new[] { "Id", "Aktif", "Baslik", "Icerik", "KategoriId", "KisaYol", "Notlar", "OlusturanOperatorId", "OlusturulmaTarihi", "Sira", "SonKullanimTarihi" },
                values: new object[] { 16, true, "Supervisor'e Yönlendirme", "Durumunuzu anlıyorum ve size en iyi şekilde yardımcı olmak istiyorum. Bu konuda yetkilim dahilinde yapabileceklerim sınırlı. Müdürümüzle görüştüreyim, size daha kapsamlı yardımcı olabilir. Müsait olana kadar kısa bir süre bekler misiniz?", 7, null, "Supervisor'e durumu özetleyin", null, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, null });

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKategorileri_Ad",
                table: "CevapSablonKategorileri",
                column: "Ad");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKategorileri_Aktif",
                table: "CevapSablonKategorileri",
                column: "Aktif");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKategorileri_Sira",
                table: "CevapSablonKategorileri",
                column: "Sira");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKullanimlar_AktiviteId",
                table: "CevapSablonKullanimlar",
                column: "AktiviteId");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKullanimlar_KullanimTarihi",
                table: "CevapSablonKullanimlar",
                column: "KullanimTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKullanimlar_MusteriId",
                table: "CevapSablonKullanimlar",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKullanimlar_OperatorId",
                table: "CevapSablonKullanimlar",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKullanimlar_SablonId",
                table: "CevapSablonKullanimlar",
                column: "SablonId");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonKullanimlar_SablonId_KullanimTarihi",
                table: "CevapSablonKullanimlar",
                columns: new[] { "SablonId", "KullanimTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonlari_Aktif",
                table: "CevapSablonlari",
                column: "Aktif");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonlari_KategoriId",
                table: "CevapSablonlari",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonlari_KategoriId_Sira",
                table: "CevapSablonlari",
                columns: new[] { "KategoriId", "Sira" });

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonlari_KullanimSayisi",
                table: "CevapSablonlari",
                column: "KullanimSayisi");

            migrationBuilder.CreateIndex(
                name: "IX_CevapSablonlari_OlusturanOperatorId",
                table: "CevapSablonlari",
                column: "OlusturanOperatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CevapSablonKullanimlar");

            migrationBuilder.DropTable(
                name: "CevapSablonlari");

            migrationBuilder.DropTable(
                name: "CevapSablonKategorileri");

            migrationBuilder.DropColumn(
                name: "GeciciKayit",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "KayitDurumu",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "KayitTamamlandi",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "CagriSirasindaKayitOlusturuldu",
                table: "AramaLoglari");

            migrationBuilder.DropColumn(
                name: "MusteriKayitliydi",
                table: "AramaLoglari");
        }
    }
}
