# SportsEvents.Mvc

Şablon seçimi ve sıfırdan kurulum/çalıştırma adımları: [readme-step.md](readme-step.md).

## Kriterlere göre hazırlanan taslak

[Kriter değerlendirmesi ve cevap örnekleri](docs/Kriterler.md), c–s maddelerini
mevcut uygulama, eklenen örnekler ve henüz uygulanmayan çalışmalarla eşleştirir.
[İş analizi ve diyagramlar](docs/Analiz.md), [test planı](docs/TestPlani.md) ve
[legacy / IIS yayınlama planı](docs/LegacyVeYayinlama.md) sunum taslağını tamamlar.
Bu proje kişisel iş tecrübesi veya tüm kriterlerin tamamlandığı iddiası değildir.

Eklenen örnekler: IEventService ile bağımlılık ayrımı, `GET /api/events`,
güvenlik başlıkları, JavaScript tarih kontrolü, ileri SQL raporu ve Azure Pipelines CI.
API örneği: `/api/events?from=2026-11-16&to=2026-11-17&page=1`.
İleri rapor için uygulama veritabanında `Database/03-AdvancedReport.sql` çalıştırılır.

```powershell
dotnet run --project tests/SportsEvents.Checks.csproj
```

Kontroller controller ve doğrulama seviyesindedir; SQL Server entegrasyonu ayrı test gerektirir.
Üretim HTTPS yönlendirmesi/HSTS içerir. Kimlik doğrulama ve yetkilendirme eklenmeden
bu yerel prova uygulamasını dış erişime açmayın.

.NET 10, ASP.NET Core MVC, EF Core 10 ve SQL Server ile etkinlik kaydı provası.
Razor ekranı tarih filtresi, sayfalama ve katılımcı kayıt formu içerir.

## Çalıştır

1. .NET 10 SDK'yı `dotnet --list-sdks` ile kontrol et.
2. Boş bir prova veritabanı için SSMS'de `Database/01-Setup.sql` dosyasını çalıştır.
   Önceki SportsEvents veritabanını kullanıyorsan bu dosyayı atla. Üç tablonun
   yanı sıra Event.RegisteredCount, Registration.IsCancelled ve
   UQ_Registration_Event_Participant kısıtını kontrol et. Sayaç, aktif kayıt
   sayısıyla eşleşmeli. Gerekirse uygulama kapalıyken aşağıdaki SQL ile doldur.
3. Proje klasöründe PowerShell'den çalıştır. Server ve Database'i SSMS'deki
   değerlerle değiştir. TrustServerCertificate ayarı yerel prova içindir.

```powershell
dotnet user-secrets set "ConnectionStrings:SportsEvents" "Server=localhost;Database=SportsEvents;Integrated Security=True;TrustServerCertificate=True"
dotnet restore
dotnet build --no-restore
dotnet run --no-build
```

Tarayıcıda http://localhost:5076 adresini aç. Yerel örnek kimlik doğrulaması içermediğinden
launch profili yalnızca localhost'a bağlanır. Visual Studio'da csproj dosyasını açarak
da çalıştırabilirsin.

### Mevcut örnek şemayı tamamla

SSMS'de mevcut prova veritabanını seç. Bu betiği uygulama kapalıyken çalıştır.

```sql
IF COL_LENGTH('dbo.Registration', 'IsCancelled') IS NULL
    ALTER TABLE dbo.Registration ADD IsCancelled bit NOT NULL DEFAULT (0);
IF COL_LENGTH('dbo.Event', 'RegisteredCount') IS NULL
    ALTER TABLE dbo.[Event] ADD RegisteredCount int NOT NULL DEFAULT (0);
GO
UPDATE e
SET RegisteredCount = (
    SELECT COUNT(*) FROM dbo.Registration AS r
    WHERE r.EventId = e.Id AND r.IsCancelled = 0
)
FROM dbo.[Event] AS e;
```

## Database First

Projede SQL şemasına göre elle yazılmış, doğrudan kullanılabilir EF eşlemeleri var.
Sınavda hazır veritabanından model üretmen istenirse aşağıdaki komutlarla mevcut
eşlemelerin yerine scaffold çıktısı alabilirsin. `--force` eşleme dosyalarını
üzerine yazar. Önce değişikliklerini kaydet.

```powershell
dotnet tool install --global dotnet-ef --version "10.0.*"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef dbcontext scaffold 'Name=ConnectionStrings:SportsEvents' Microsoft.EntityFrameworkCore.SqlServer --context SportsEventsDbContext --context-dir Data --output-dir Entities --table dbo.Event --table dbo.Participant --table dbo.Registration --no-onconfiguring --force
dotnet build
```

Araç zaten kuruluysa `dotnet ef --version` çalıştır. Eski global sürümü güncellemek
için ilk komutta `install` yerine `update` kullan. SQL şeması betiklerle yönetilir.

## Manuel kontrol

| İşlem | Temiz örnek veride beklenen |
|---|---|
| 16–17 Kasım 2026 aralığını filtrele | Yüzme, Basketbol ve Voleybol görünür. |
| Elif'i Yüzme'ye kaydet | Mükerrer kayıt hatası. Aktif kayıt ve sayaç 1. |
| Can'ı Yüzme'ye kaydet | Başarı mesajı. Aktif kayıt ve sayaç 2. |
| Deniz'i Yüzme'ye kaydet | Kontenjan dolu mesajı. Sayaç 2 kalır. |
| SSMS'de Database/02-OccupancyReport.sql çalıştır | Kayıtsız etkinlikler de görünür. StoredCount ve ActiveCount eşleşir. |

Eşzamanlılık provası için temiz veride Can ve Deniz'i farklı tarayıcı sekmelerinden
Yüzme'ye kaydet. Tek yer kaldığı için yalnızca biri başarılı olur. Koşullu UPDATE ve
INSERT aynı transaction'dadır. INSERT hatası sayacı da geri alır. Mevcut benzersizlik
kısıtı iptal edilmiş kayıtları kapsar.
