using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Models;
using CagriMerkeziUyg.Controllers;

namespace CagriMerkeziUyg.Data
{
    public class CagriMerkeziDbContext : DbContext
    {
        public CagriMerkeziDbContext(DbContextOptions<CagriMerkeziDbContext> options) : base(options)
        {
        }

        public DbSet<Musteri> Musteriler { get; set; }
        public DbSet<MusteriAktiviteler> MusteriAktiviteleri { get; set; }
        public DbSet<Operator> Operatorler { get; set; }
        public DbSet<OperatorPerformans> OperatorPerformansKayitlari { get; set; }
        public DbSet<AramaLog> AramaLoglari { get; set; }
        public DbSet<MusteriEtiketi> MusteriEtiketleri { get; set; }
        public DbSet<MusteriEtiketAtama> MusteriEtiketAtamalari { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<CevapSablonKategori> CevapSablonKategorileri { get; set; }
        public DbSet<CevapSablonu> CevapSablonlari { get; set; }
        public DbSet<CevapSablonKullanim> CevapSablonKullanimlar { get; set; }
        public DbSet<OperatorDurumGecmisi> OperatorDurumGecmisleri { get; set; }
        public DbSet<OperatorGunlukDurumOzeti> OperatorGunlukDurumOzetleri { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Musteri tablosu konfigürasyonu
            modelBuilder.Entity<Musteri>(entity =>
            {
                // Primary Key
                entity.HasKey(m => m.Id);

                // Tablo adı
                entity.ToTable("Musteriler");

                // Alan konfigürasyonları
                entity.Property(m => m.TelefonNo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasComment("Müşteri telefon numarası");

                entity.Property(m => m.Ad)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Müşteri adı");

                entity.Property(m => m.Soyad)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Müşteri soyadı");

                entity.Property(m => m.Email)
                    .HasMaxLength(100)
                    .HasComment("Müşteri e-posta adresi");

                entity.Property(m => m.Adres)
                    .HasMaxLength(500)
                    .HasComment("Müşteri adresi");

                entity.Property(m => m.Notlar)
                    .HasMaxLength(1000)
                    .HasComment("Müşteri hakkında notlar");

                entity.Property(m => m.KayitTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Kayıt oluşturulma tarihi");

                entity.Property(m => m.SonGuncelleme)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Son güncelleme tarihi");

                entity.Property(m => m.DogumTarihi)
                    .HasComment("Müşteri doğum tarihi");

                // Unique constraint - Telefon numarası unique olmalı
                entity.HasIndex(m => m.TelefonNo)
                    .IsUnique()
                    .HasDatabaseName("IX_Musteriler_TelefonNo");

                // Email index (optional)
                entity.HasIndex(m => m.Email)
                    .HasDatabaseName("IX_Musteriler_Email");
            });
            modelBuilder.Entity<MusteriAktiviteler>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("MusteriAktiviteleri");

                entity.Property(a => a.Aciklama)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(a => a.Tur)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(a => a.OlusturulmaTarihi)
                    .IsRequired();

                entity.HasOne(a => a.Musteri)
                    .WithMany(m => m.Aktiviteler)
                    .HasForeignKey(a => a.MusteriId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.MusteriId);
                entity.HasIndex(a => a.OperatorId);
                
                // Operator relation
                entity.HasOne(a => a.Operator)
                    .WithMany(o => o.Aktiviteler)
                    .HasForeignKey(a => a.OperatorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Operator tablosu konfigürasyonu
            modelBuilder.Entity<Operator>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("Operatorler");

                entity.Property(o => o.Ad)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(o => o.Soyad)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(o => o.KullaniciAdi)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(o => o.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(o => o.Telefon)
                    .HasMaxLength(20);

                entity.Property(o => o.Rol)
                    .IsRequired()
                    .HasConversion<string>();

                // Unique constraints
                entity.HasIndex(o => o.KullaniciAdi)
                    .IsUnique()
                    .HasDatabaseName("IX_Operatorler_KullaniciAdi");

                entity.HasIndex(o => o.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Operatorler_Email");
            });

            // OperatorPerformans tablosu konfigürasyonu
            modelBuilder.Entity<OperatorPerformans>(entity =>
            {
                entity.HasKey(op => op.Id);
                entity.ToTable("OperatorPerformansKayitlari");

                entity.Property(op => op.OrtalamaCagriSuresi)
                    .HasPrecision(10, 2);

                entity.Property(op => op.MusteriMemnuniyetPuani)
                    .HasPrecision(3, 2);

                entity.Property(op => op.AktifCalismaSuresi)
                    .HasPrecision(10, 2);

                entity.Property(op => op.ToplamMolaSuresi)
                    .HasPrecision(10, 2);

                entity.Property(op => op.PerformansPuani)
                    .HasPrecision(5, 2);

                entity.Property(op => op.Notlar)
                    .HasMaxLength(500);

                // Operator relation
                entity.HasOne(op => op.Operator)
                    .WithMany(o => o.PerformansKayitlari)
                    .HasForeignKey(op => op.OperatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(op => op.OperatorId);
                entity.HasIndex(op => op.Tarih);
            });

            // Seed data - Başlangıç verileri
            modelBuilder.Entity<Musteri>().HasData(
                new Musteri
                {
                    Id = 1,
                    TelefonNo = "05551234567",
                    Ad = "Ahmet",
                    Soyad = "Yılmaz",
                    Email = "ahmet@email.com",
                    Adres = "İstanbul, Türkiye",
                    DogumTarihi = new DateTime(1980, 5, 15),
                    Notlar = "Düzenli müşteri",
                    KayitTarihi = new DateTime(2025, 8, 5, 10, 0, 0),
                    SonGuncelleme = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new Musteri
                {
                    Id = 2,
                    TelefonNo = "05559876543",
                    Ad = "Ayşe",
                    Soyad = "Kaya",
                    Email = "ayse@email.com",
                    Adres = "Ankara, Türkiye",
                    DogumTarihi = new DateTime(1990, 8, 22),
                    Notlar = "Yeni müşteri",
                    KayitTarihi = new DateTime(2025, 8, 5, 10, 0, 0),
                    SonGuncelleme = new DateTime(2025, 8, 5, 10, 0, 0)
                }
            );

            // Operatör seed data
            modelBuilder.Entity<Operator>().HasData(
                new Operator
                {
                    Id = 1,
                    Ad = "Admin",
                    Soyad = "User",
                    KullaniciAdi = "admin",
                    Email = "admin@cagrimerkezi.com",
                    Telefon = "05551111111",
                    Sifre = "admin123",
                    Rol = OperatorRolu.Admin,
                    Aktif = true,
                    KayitTarihi = new DateTime(2025, 8, 5, 10, 0, 0),
                    CalismaSaatiBaslangic = new TimeSpan(8, 0, 0),
                    CalismaSaatiBitis = new TimeSpan(18, 0, 0)
                },
                new Operator
                {
                    Id = 2,
                    Ad = "Mehmet",
                    Soyad = "Demir",
                    KullaniciAdi = "mehmet.demir",
                    Email = "mehmet@cagrimerkezi.com",
                    Telefon = "05552222222",
                    Sifre = "op123",
                    Rol = OperatorRolu.Operator,
                    Aktif = true,
                    KayitTarihi = new DateTime(2025, 8, 5, 10, 0, 0),
                    CalismaSaatiBaslangic = new TimeSpan(9, 0, 0),
                    CalismaSaatiBitis = new TimeSpan(17, 0, 0)
                },
                new Operator
                {
                    Id = 3,
                    Ad = "Fatma",
                    Soyad = "Özkan",
                    KullaniciAdi = "supervisor",
                    Email = "fatma@cagrimerkezi.com",
                    Telefon = "05553333333",
                    Sifre = "super123",
                    Rol = OperatorRolu.Supervisor,
                    Aktif = true,
                    KayitTarihi = new DateTime(2025, 8, 5, 10, 0, 0),
                    CalismaSaatiBaslangic = new TimeSpan(8, 30, 0),
                    CalismaSaatiBitis = new TimeSpan(17, 30, 0)
                },
                new Operator
                {
                    Id = 4,
                    Ad = "Ali",
                    Soyad = "Kaya",
                    KullaniciAdi = "operator1",
                    Email = "ali@cagrimerkezi.com",
                    Telefon = "05554444444",
                    Sifre = "op123",
                    Rol = OperatorRolu.Operator,
                    Aktif = true,
                    KayitTarihi = new DateTime(2025, 8, 5, 10, 0, 0),
                    CalismaSaatiBaslangic = new TimeSpan(9, 0, 0),
                    CalismaSaatiBitis = new TimeSpan(18, 0, 0)
                },
                new Operator
                {
                    Id = 5,
                    Ad = "Zeynep",
                    Soyad = "Çelik",
                    KullaniciAdi = "operator2",
                    Email = "zeynep@cagrimerkezi.com",
                    Telefon = "05555555555",
                    Sifre = "op123",
                    Rol = OperatorRolu.Operator,
                    Aktif = true,
                    KayitTarihi = new DateTime(2025, 8, 5, 10, 0, 0),
                    CalismaSaatiBaslangic = new TimeSpan(8, 30, 0),
                    CalismaSaatiBitis = new TimeSpan(17, 30, 0)
                }
            );

            // AramaLog tablosu konfigürasyonu
            modelBuilder.Entity<AramaLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("AramaLoglari");

                entity.Property(a => a.TelefonNo)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(a => a.Notlar)
                    .HasMaxLength(1000);

                entity.Property(a => a.Durum)
                    .HasConversion<int>();

                // Müşteri ile ilişki (opsiyonel)
                entity.HasOne(a => a.Musteri)
                    .WithMany()
                    .HasForeignKey(a => a.MusteriId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Operatör ile ilişki (zorunlu)
                entity.HasOne(a => a.Operator)
                    .WithMany()
                    .HasForeignKey(a => a.OperatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => a.TelefonNo);
                entity.HasIndex(a => a.BaslangicZamani);
                entity.HasIndex(a => a.MusteriId);
                entity.HasIndex(a => a.OperatorId);
            });

            // MusteriEtiketi seed data
            modelBuilder.Entity<MusteriEtiketi>().HasData(
                new MusteriEtiketi
                {
                    Id = 1,
                    Ad = "VIP Müşteri",
                    Aciklama = "Yüksek değerli müşteriler",
                    RenkKodu = "#FFD700",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new MusteriEtiketi
                {
                    Id = 2,
                    Ad = "Düzenli Müşteri",
                    Aciklama = "Düzenli olarak hizmet alan müşteriler",
                    RenkKodu = "#28a745",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new MusteriEtiketi
                {
                    Id = 3,
                    Ad = "Yeni Müşteri",
                    Aciklama = "Yeni kayıt olan müşteriler",
                    RenkKodu = "#17a2b8",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new MusteriEtiketi
                {
                    Id = 4,
                    Ad = "Riskli Müşteri",
                    Aciklama = "Dikkatli yaklaşılması gereken müşteriler",
                    RenkKodu = "#dc3545",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new MusteriEtiketi
                {
                    Id = 5,
                    Ad = "Şikayetçi Müşteri",
                    Aciklama = "Sık sık şikayet eden müşteriler",
                    RenkKodu = "#fd7e14",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new MusteriEtiketi
                {
                    Id = 6,
                    Ad = "Memnun Müşteri",
                    Aciklama = "Hizmetlerden memnun olan müşteriler",
                    RenkKodu = "#6f42c1",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new MusteriEtiketi
                {
                    Id = 7,
                    Ad = "Kurumsal Müşteri",
                    Aciklama = "Kurumsal müşteriler",
                    RenkKodu = "#20c997",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                },
                new MusteriEtiketi
                {
                    Id = 8,
                    Ad = "Bireysel Müşteri",
                    Aciklama = "Bireysel müşteriler",
                    RenkKodu = "#6c757d",
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 8, 5, 10, 0, 0)
                }
            );

            // MusteriEtiketi tablosu konfigürasyonu
            modelBuilder.Entity<MusteriEtiketi>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("MusteriEtiketleri");

                entity.Property(e => e.Ad)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Etiket adı");

                entity.Property(e => e.Aciklama)
                    .HasMaxLength(200)
                    .HasComment("Etiket açıklaması");

                entity.Property(e => e.RenkKodu)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasDefaultValue("#007bff")
                    .HasComment("Etiket renk kodu");

                entity.Property(e => e.Aktif)
                    .IsRequired()
                    .HasDefaultValue(true)
                    .HasComment("Etiket aktif durumu");

                entity.Property(e => e.OlusturulmaTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Etiket oluşturulma tarihi");

                // Unique constraint - Etiket adı unique olmalı
                entity.HasIndex(e => e.Ad)
                    .IsUnique()
                    .HasDatabaseName("IX_MusteriEtiketleri_Ad");
            });

            // MusteriEtiketAtama tablosu konfigürasyonu
            modelBuilder.Entity<MusteriEtiketAtama>(entity =>
            {
                entity.HasKey(ea => ea.Id);
                entity.ToTable("MusteriEtiketAtamalari");

                entity.Property(ea => ea.AtamaTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Etiket atama tarihi");

                entity.Property(ea => ea.Notlar)
                    .HasMaxLength(500)
                    .HasComment("Atama notları");

                // Müşteri ile ilişki
                entity.HasOne(ea => ea.Musteri)
                    .WithMany(m => m.EtiketAtamalari)
                    .HasForeignKey(ea => ea.MusteriId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Etiket ile ilişki
                entity.HasOne(ea => ea.MusteriEtiketi)
                    .WithMany(e => e.MusteriEtiketAtamalari)
                    .HasForeignKey(ea => ea.MusteriEtiketiId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Atayan operatör ile ilişki
                entity.HasOne(ea => ea.AtayanOperator)
                    .WithMany()
                    .HasForeignKey(ea => ea.AtayanOperatorId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexler
                entity.HasIndex(ea => ea.MusteriId);
                entity.HasIndex(ea => ea.MusteriEtiketiId);
                entity.HasIndex(ea => ea.AtamaTarihi);

                // Unique constraint - Aynı müşteriye aynı etiket birden fazla atanamaz
                entity.HasIndex(ea => new { ea.MusteriId, ea.MusteriEtiketiId })
                    .IsUnique()
                    .HasDatabaseName("IX_MusteriEtiketAtamalari_MusteriId_MusteriEtiketiId");
            });

            // Randevu tablosu konfigürasyonu
            modelBuilder.Entity<Randevu>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("Randevular");

                entity.Property(r => r.Baslik)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasComment("Randevu başlığı");

                entity.Property(r => r.Aciklama)
                    .HasMaxLength(1000)
                    .HasComment("Randevu açıklaması");

                entity.Property(r => r.RandevuZamani)
                    .IsRequired()
                    .HasComment("Randevu tarihi ve saati");

                entity.Property(r => r.BitisZamani)
                    .HasComment("Randevu bitiş zamanı");

                entity.Property(r => r.Tip)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasComment("Randevu tipi");

                entity.Property(r => r.Durum)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(RandevuDurumu.Bekliyor)
                    .HasComment("Randevu durumu");

                entity.Property(r => r.Oncelik)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(RandevuOncelik.Normal)
                    .HasComment("Randevu önceliği");

                entity.Property(r => r.HatirlatmaAktif)
                    .IsRequired()
                    .HasDefaultValue(true)
                    .HasComment("Hatırlatma aktif mi");

                entity.Property(r => r.HatirlatmaSuresi)
                    .IsRequired()
                    .HasDefaultValue(15)
                    .HasComment("Hatırlatma süresi (dakika)");

                entity.Property(r => r.HatirlatmaGonderildi)
                    .IsRequired()
                    .HasDefaultValue(false)
                    .HasComment("Hatırlatma gönderildi mi");

                entity.Property(r => r.TamamlanmaNotu)
                    .HasMaxLength(1000)
                    .HasComment("Tamamlanma notu");

                entity.Property(r => r.OlusturulmaTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Randevu oluşturulma tarihi");

                entity.Property(r => r.SonGuncelleme)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Son güncelleme tarihi");

                // Müşteri ile ilişki (opsiyonel)
                entity.HasOne(r => r.Musteri)
                    .WithMany()
                    .HasForeignKey(r => r.MusteriId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Atanan operatör ile ilişki (zorunlu)
                entity.HasOne(r => r.Operator)
                    .WithMany()
                    .HasForeignKey(r => r.OperatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Oluşturan operatör ile ilişki (opsiyonel)
                entity.HasOne(r => r.OlusturanOperator)
                    .WithMany()
                    .HasForeignKey(r => r.OlusturanOperatorId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexler
                entity.HasIndex(r => r.RandevuZamani)
                    .HasDatabaseName("IX_Randevular_RandevuZamani");

                entity.HasIndex(r => r.OperatorId)
                    .HasDatabaseName("IX_Randevular_OperatorId");

                entity.HasIndex(r => r.MusteriId)
                    .HasDatabaseName("IX_Randevular_MusteriId");

                entity.HasIndex(r => r.Durum)
                    .HasDatabaseName("IX_Randevular_Durum");

                entity.HasIndex(r => new { r.RandevuZamani, r.OperatorId })
                    .HasDatabaseName("IX_Randevular_RandevuZamani_OperatorId");
            });

            // CevapSablonKategori tablosu konfigürasyonu
            modelBuilder.Entity<CevapSablonKategori>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.ToTable("CevapSablonKategorileri");

                entity.Property(k => k.Ad)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasComment("Kategori adı");

                entity.Property(k => k.Aciklama)
                    .HasMaxLength(500)
                    .HasComment("Kategori açıklaması");

                entity.Property(k => k.IconClass)
                    .HasMaxLength(50)
                    .HasComment("Font Awesome icon sınıfı");

                entity.Property(k => k.RenkKodu)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasDefaultValue("#007bff")
                    .HasComment("Kategori renk kodu (HEX)");

                entity.Property(k => k.Sira)
                    .HasDefaultValue(0)
                    .HasComment("Gösterim sırası");

                entity.Property(k => k.Aktif)
                    .IsRequired()
                    .HasDefaultValue(true)
                    .HasComment("Kategori aktif mi");

                entity.Property(k => k.OlusturulmaTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Kategori oluşturulma tarihi");

                entity.Property(k => k.AktiviteTuru)
                    .HasConversion<string>()
                    .HasComment("Hangi aktivite türü için (null ise tümü)");

                // Index'ler
                entity.HasIndex(k => k.Ad)
                    .HasDatabaseName("IX_CevapSablonKategorileri_Ad");

                entity.HasIndex(k => k.Sira)
                    .HasDatabaseName("IX_CevapSablonKategorileri_Sira");

                entity.HasIndex(k => k.Aktif)
                    .HasDatabaseName("IX_CevapSablonKategorileri_Aktif");
            });

            // CevapSablonu tablosu konfigürasyonu
            modelBuilder.Entity<CevapSablonu>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.ToTable("CevapSablonlari");

                entity.Property(s => s.Baslik)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasComment("Şablon başlığı");

                entity.Property(s => s.Icerik)
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasComment("Şablon içeriği");

                entity.Property(s => s.Notlar)
                    .HasMaxLength(500)
                    .HasComment("Operatör için notlar");

                entity.Property(s => s.Sira)
                    .HasDefaultValue(0)
                    .HasComment("Kategori içinde sıra");

                entity.Property(s => s.Aktif)
                    .IsRequired()
                    .HasDefaultValue(true)
                    .HasComment("Şablon aktif mi");

                entity.Property(s => s.KullanimSayisi)
                    .HasDefaultValue(0)
                    .HasComment("Kaç kez kullanıldı");

                entity.Property(s => s.SonKullanimTarihi)
                    .HasComment("Son kullanım tarihi");

                entity.Property(s => s.DegiskenIceriyor)
                    .HasDefaultValue(false)
                    .HasComment("Değişken placeholder içeriyor mu");

                entity.Property(s => s.KisaYol)
                    .HasMaxLength(20)
                    .HasComment("Klavye kısayolu");

                entity.Property(s => s.OlusturulmaTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Şablon oluşturulma tarihi");

                // İlişkiler
                entity.HasOne(s => s.Kategori)
                    .WithMany(k => k.Sablonlar)
                    .HasForeignKey(s => s.KategoriId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.OlusturanOperator)
                    .WithMany()
                    .HasForeignKey(s => s.OlusturanOperatorId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Index'ler
                entity.HasIndex(s => s.KategoriId)
                    .HasDatabaseName("IX_CevapSablonlari_KategoriId");

                entity.HasIndex(s => s.Aktif)
                    .HasDatabaseName("IX_CevapSablonlari_Aktif");

                entity.HasIndex(s => s.KullanimSayisi)
                    .HasDatabaseName("IX_CevapSablonlari_KullanimSayisi");

                entity.HasIndex(s => new { s.KategoriId, s.Sira })
                    .HasDatabaseName("IX_CevapSablonlari_KategoriId_Sira");
            });

            // CevapSablonKullanim tablosu konfigürasyonu
            modelBuilder.Entity<CevapSablonKullanim>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.ToTable("CevapSablonKullanimlar");

                entity.Property(k => k.KullanimTarihi)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Kullanım tarihi");

                // İlişkiler
                entity.HasOne(k => k.Sablon)
                    .WithMany(s => s.KullanimKayitlari)
                    .HasForeignKey(k => k.SablonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(k => k.Operator)
                    .WithMany()
                    .HasForeignKey(k => k.OperatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(k => k.Musteri)
                    .WithMany()
                    .HasForeignKey(k => k.MusteriId)
                    .OnDelete(DeleteBehavior.NoAction); // SetNull yerine NoAction - SQL Server cascade path hatası önleme

                entity.HasOne(k => k.Aktivite)
                    .WithMany()
                    .HasForeignKey(k => k.AktiviteId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Index'ler
                entity.HasIndex(k => k.SablonId)
                    .HasDatabaseName("IX_CevapSablonKullanimlar_SablonId");

                entity.HasIndex(k => k.OperatorId)
                    .HasDatabaseName("IX_CevapSablonKullanimlar_OperatorId");

                entity.HasIndex(k => k.KullanimTarihi)
                    .HasDatabaseName("IX_CevapSablonKullanimlar_KullanimTarihi");

                entity.HasIndex(k => new { k.SablonId, k.KullanimTarihi })
                    .HasDatabaseName("IX_CevapSablonKullanimlar_SablonId_KullanimTarihi");
            });

            // OperatorDurumGecmisi tablosu konfigürasyonu
            modelBuilder.Entity<OperatorDurumGecmisi>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.ToTable("OperatorDurumGecmisleri");

                entity.Property(d => d.OncekiDurum)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasComment("Önceki durum");

                entity.Property(d => d.YeniDurum)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasComment("Yeni durum");

                entity.Property(d => d.GecisZamani)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()")
                    .HasComment("Durum geçiş zamanı");

                entity.Property(d => d.BitisZamani)
                    .HasComment("Durum bitiş zamanı");

                entity.Property(d => d.Not)
                    .HasMaxLength(500)
                    .HasComment("Durum değişikliği notu");

                entity.Property(d => d.OtomatikGecis)
                    .HasDefaultValue(false)
                    .HasComment("Otomatik geçiş mi?");

                // Operator ilişkisi
                entity.HasOne(d => d.Operator)
                    .WithMany(o => o.DurumGecmisleri)
                    .HasForeignKey(d => d.OperatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                // AramaLog ilişkisi (opsiyonel)
                entity.HasOne(d => d.IlgiliAramaLog)
                    .WithMany()
                    .HasForeignKey(d => d.IlgiliAramaLogId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Index'ler
                entity.HasIndex(d => d.OperatorId)
                    .HasDatabaseName("IX_OperatorDurumGecmisleri_OperatorId");

                entity.HasIndex(d => d.GecisZamani)
                    .HasDatabaseName("IX_OperatorDurumGecmisleri_GecisZamani");

                entity.HasIndex(d => new { d.OperatorId, d.GecisZamani })
                    .HasDatabaseName("IX_OperatorDurumGecmisleri_OperatorId_GecisZamani");
            });

            // OperatorGunlukDurumOzeti tablosu konfigürasyonu
            modelBuilder.Entity<OperatorGunlukDurumOzeti>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("OperatorGunlukDurumOzetleri");

                entity.Property(o => o.Tarih)
                    .IsRequired()
                    .HasComment("Özet tarihi");

                entity.Property(o => o.ToplamCalismaSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Toplam çalışma süresi (dakika)");

                entity.Property(o => o.CagrıdaGecenSure)
                    .HasPrecision(10, 2)
                    .HasComment("Çağrıda geçen süre (dakika)");

                entity.Property(o => o.AraCalismaSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Ara çalışma süresi (dakika)");

                entity.Property(o => o.MusaitSure)
                    .HasPrecision(10, 2)
                    .HasComment("Müsait bekleme süresi (dakika)");

                entity.Property(o => o.MolaSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Mola süresi (dakika)");

                entity.Property(o => o.OgleYemegiSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Öğle yemeği süresi (dakika)");

                entity.Property(o => o.ToplantiSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Toplantı süresi (dakika)");

                entity.Property(o => o.EgitimSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Eğitim süresi (dakika)");

                entity.Property(o => o.OrtalamaCagriSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Ortalama çağrı süresi (dakika)");

                entity.Property(o => o.OrtalamaAraCalismaSuresi)
                    .HasPrecision(10, 2)
                    .HasComment("Ortalama ara çalışma süresi (dakika)");

                // Operator ilişkisi
                entity.HasOne(o => o.Operator)
                    .WithMany()
                    .HasForeignKey(o => o.OperatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index'ler
                entity.HasIndex(o => o.OperatorId)
                    .HasDatabaseName("IX_OperatorGunlukDurumOzetleri_OperatorId");

                entity.HasIndex(o => o.Tarih)
                    .HasDatabaseName("IX_OperatorGunlukDurumOzetleri_Tarih");

                // Unique constraint - Operatör ve tarih kombinasyonu
                entity.HasIndex(o => new { o.OperatorId, o.Tarih })
                    .IsUnique()
                    .HasDatabaseName("IX_OperatorGunlukDurumOzetleri_OperatorId_Tarih");
            });

            // ============================================
            // SEED DATA - Örnek Kategoriler ve Şablonlar
            // ============================================

            // Kategori
            modelBuilder.Entity<CevapSablonKategori>().HasData(
                new CevapSablonKategori
                {
                    Id = 1,
                    Ad = "Karşılama & Veda",
                    Aciklama = "Çağrı başlangıcı ve bitiş mesajları",
                    IconClass = "fa-hand-wave",
                    RenkKodu = "#28a745",
                    Sira = 1,
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonKategori
                {
                    Id = 2,
                    Ad = "Ürün & Hizmet Bilgisi",
                    Aciklama = "Ürün özellikleri, fiyat, stok bilgileri",
                    IconClass = "fa-box",
                    RenkKodu = "#007bff",
                    Sira = 2,
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonKategori
                {
                    Id = 3,
                    Ad = "İade & İptal",
                    Aciklama = "İade süreci, iptal talepleri",
                    IconClass = "fa-undo",
                    RenkKodu = "#dc3545",
                    Sira = 3,
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonKategori
                {
                    Id = 4,
                    Ad = "Sipariş & Teslimat",
                    Aciklama = "Sipariş takibi, teslimat bilgileri",
                    IconClass = "fa-truck",
                    RenkKodu = "#fd7e14",
                    Sira = 4,
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonKategori
                {
                    Id = 5,
                    Ad = "Teknik Destek",
                    Aciklama = "Sorun giderme, kurulum yardımı",
                    IconClass = "fa-tools",
                    RenkKodu = "#6f42c1",
                    Sira = 5,
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonKategori
                {
                    Id = 6,
                    Ad = "Ödeme & Fatura",
                    Aciklama = "Ödeme yöntemleri, fatura talepleri",
                    IconClass = "fa-credit-card",
                    RenkKodu = "#20c997",
                    Sira = 6,
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonKategori
                {
                    Id = 7,
                    Ad = "Şikayet Yönetimi",
                    Aciklama = "Şikayet alma ve yönetme",
                    IconClass = "fa-exclamation-triangle",
                    RenkKodu = "#ffc107",
                    Sira = 7,
                    Aktif = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0),
                    AktiviteTuru = AktiviteTuru.Sikayet
                }
            );

            // Şablon Seed Data
            modelBuilder.Entity<CevapSablonu>().HasData(
                // Karşılama & Veda Şablonları
                new CevapSablonu
                {
                    Id = 1,
                    KategoriId = 1,
                    Baslik = "Standart Karşılama",
                    Icerik = "Merhaba, [Şirket Adı] çağrı merkezine hoş geldiniz. Ben {OperatorAdi}, size nasıl yardımcı olabilirim?",
                    Notlar = "Müşteriye güler yüzle hitap edin",
                    Sira = 1,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    KisaYol = "F1",
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 2,
                    KategoriId = 1,
                    Baslik = "VIP Müşteri Karşılama",
                    Icerik = "Sayın {MusteriAdi}, hoş geldiniz! Sizi tanıdığımız için çok mutluyuz. Size nasıl yardımcı olabilirim?",
                    Notlar = "VIP etiketli müşteriler için kullanın",
                    Sira = 2,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    KisaYol = "F2",
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 3,
                    KategoriId = 1,
                    Baslik = "Standart Veda",
                    Icerik = "Bize ulaştığınız için teşekkür ederim {MusteriAdi}. İyi günler dilerim!",
                    Notlar = "Çağrı sonunda kullanın",
                    Sira = 3,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    KisaYol = "F3",
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                // Ürün & Hizmet Bilgisi Şablonları
                new CevapSablonu
                {
                    Id = 4,
                    KategoriId = 2,
                    Baslik = "Fiyat Bilgisi",
                    Icerik = "İlgilendiğiniz ürünün güncel fiyatını size aktarıyorum. Ürün fiyatı: {Fiyat} TL. Kargo ücreti dahildir. Başka bir ürün hakkında bilgi almak ister misiniz?",
                    Notlar = "Fiyat bilgisini güncel sistemden kontrol edin",
                    Sira = 1,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 5,
                    KategoriId = 2,
                    Baslik = "Stok Durumu - Var",
                    Icerik = "İyi haberlerim var! İlgilendiğiniz ürün stoklarımızda mevcut. Hemen sipariş verebilirsiniz. Sipariş vermek ister misiniz?",
                    Sira = 2,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 6,
                    KategoriId = 2,
                    Baslik = "Stok Durumu - Yok",
                    Icerik = "Üzgünüm, ilgilendiğiniz ürün şu anda stoklarımızda bulunmuyor. Ancak {TarihTahmini} tarihinde tekrar stoklarımıza gireceğini tahmin ediyoruz. Bilgilendirme için iletişim bilgilerinizi kaydedebilirim.",
                    Notlar = "Tahmini tarihi lojistik ekibinden öğrenin",
                    Sira = 3,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                // İade & İptal Şablonları
                new CevapSablonu
                {
                    Id = 7,
                    KategoriId = 3,
                    Baslik = "Standart İade Süreci",
                    Icerik = "Tabii efendim, iade işleminize yardımcı olabilirim. İade prosedürümüz şu şekildedir:\n\n1. Ürün 14 gün içinde iade edilmelidir\n2. Ürün kullanılmamış ve ambalajı açılmamış olmalıdır\n3. Fatura veya sipariş numarası gereklidir\n\nKargo ücreti cayma hakkı veya hasarlı ürün durumunda tarafımızca karşılanır. Size iade kodu gönderebilirim.",
                    Notlar = "İade nedeni mutlaka sorun",
                    Sira = 1,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    KisaYol = "F4",
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 8,
                    KategoriId = 3,
                    Baslik = "Sipariş İptal",
                    Icerik = "Sipariş iptal talebinizi aldım. Sipariş numaranız {SiparisNo}. Eğer siparişiniz henüz kargoya verilmediyse iptal edebiliriz. Kargoya verildiyse ürünü teslim almadan iade edebilirsiniz. Kontrol edip size dönüş yapacağım.",
                    Notlar = "Sipariş durumunu sisteme bakıp kontrol edin",
                    Sira = 2,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                // Sipariş & Teslimat Şablonları
                new CevapSablonu
                {
                    Id = 9,
                    KategoriId = 4,
                    Baslik = "Sipariş Takibi",
                    Icerik = "Sipariş numaranız: {SiparisNo}\nDurum: {Durum}\nKargo Takip No: {KargoNo}\n\nSiparişiniz {KargoFirmasi} ile gönderilmiştir. Takip numarası ile kargo firmasının web sitesinden detaylı takip yapabilirsiniz.",
                    Notlar = "Bilgileri sistemden kontrol edin",
                    Sira = 1,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    KisaYol = "F5",
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 10,
                    KategoriId = 4,
                    Baslik = "Teslimat Süresi",
                    Icerik = "Siparişiniz onaylandıktan sonra 1-3 iş günü içinde kargoya verilecektir. Kargo süresi bulunduğunuz ile bağlı olarak 2-5 iş günü arasında değişmektedir. Toplam teslimat süresi ortalama 3-8 iş günüdür.",
                    Sira = 2,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                // Teknik Destek Şablonları
                new CevapSablonu
                {
                    Id = 11,
                    KategoriId = 5,
                    Baslik = "Genel Sorun Giderme",
                    Icerik = "Yaşadığınız sorunu çözmek için birlikte adım adım ilerleyelim:\n\n1. Cihazı kapatıp 30 saniye bekleyin\n2. Tekrar açın\n3. Sorun devam ediyorsa bağlantı kablolarını kontrol edin\n4. Güncelleme olup olmadığını kontrol edin\n\nBu adımları denedikten sonra durum nasıl?",
                    Notlar = "Sabırlı olun, müşteriye rehberlik edin",
                    Sira = 1,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 12,
                    KategoriId = 5,
                    Baslik = "Teknik Ekibe Yönlendirme",
                    Icerik = "Durumu değerlendirdim ve bu konuda teknik ekibimizin size yardımcı olması daha uygun olacaktır. Bilgilerinizi teknik destek ekibine ileteceğim ve en kısa sürede sizinle iletişime geçeceklerdir. İletişim bilgilerinizi teyit edebilir miyim?",
                    Notlar = "Ticket oluşturun ve teknik ekibe atayın",
                    Sira = 2,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                // Ödeme & Fatura Şablonları
                new CevapSablonu
                {
                    Id = 13,
                    KategoriId = 6,
                    Baslik = "Ödeme Yöntemleri",
                    Icerik = "Ödeme seçeneklerimiz şunlardır:\n\n• Kredi Kartı (Tek Çekim / Taksit)\n• Banka Havalesi / EFT\n• Kapıda Ödeme (Nakit / Kredi Kartı)\n\nHangi ödeme yöntemini tercih edersiniz?",
                    Sira = 1,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 14,
                    KategoriId = 6,
                    Baslik = "Fatura Talebi",
                    Icerik = "Faturanız sipariş tamamlandıktan sonra e-posta adresinize otomatik olarak gönderilecektir. Ayrıca hesabınızdan 'Siparişlerim' bölümünden de faturanızı indirebilirsiniz. Kurumsal fatura için fatura bilgilerinizi şimdi kaydedebilirim.",
                    Notlar = "Kurumsal fatura için vergi no ve şirket unvanı alın",
                    Sira = 2,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                // Şikayet Yönetimi Şablonları
                new CevapSablonu
                {
                    Id = 15,
                    KategoriId = 7,
                    Baslik = "Şikayet Alma",
                    Icerik = "Yaşadığınız olumsuz deneyim için özür dilerim {MusteriAdi}. Şikayetinizi en üst düzeyde önemsiyoruz. Lütfen durumu detaylı anlatır mısınız? Sorununuzu çözmek için elimden geleni yapacağım.",
                    Notlar = "Empatik olun, müşteriyi dinleyin, not alın",
                    Sira = 1,
                    Aktif = true,
                    DegiskenIceriyor = true,
                    KisaYol = "F6",
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                },
                new CevapSablonu
                {
                    Id = 16,
                    KategoriId = 7,
                    Baslik = "Supervisor'e Yönlendirme",
                    Icerik = "Durumunuzu anlıyorum ve size en iyi şekilde yardımcı olmak istiyorum. Bu konuda yetkilim dahilinde yapabileceklerim sınırlı. Müdürümüzle görüştüreyim, size daha kapsamlı yardımcı olabilir. Müsait olana kadar kısa bir süre bekler misiniz?",
                    Notlar = "Supervisor'e durumu özetleyin",
                    Sira = 2,
                    Aktif = true,
                    DegiskenIceriyor = false,
                    OlusturulmaTarihi = new DateTime(2025, 1, 1, 10, 0, 0)
                }
            );
        }

        // SaveChanges override - SonGuncelleme alanını otomatik güncelle
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var musteriEntries = ChangeTracker.Entries<Musteri>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in musteriEntries)
            {
                entry.Entity.SonGuncelleme = DateTime.Now;
            }

            var randevuEntries = ChangeTracker.Entries<Randevu>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in randevuEntries)
            {
                entry.Entity.SonGuncelleme = DateTime.Now;
            }
        }
    }
}