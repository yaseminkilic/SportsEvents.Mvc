# SportsEvents.Mvc — Adım adım oluşturma ve çalıştırma

## 1. Hangi şablon kullanılıyor?

Bu projenin yapısına uygun şablon **ASP.NET Core Web App (Model-View-Controller)**, CLI kısa adıyla **`mvc`** şablonudur. Hedef framework **.NET 10 (`net10.0`)**, dil **C#**, arayüz **Razor Views**, veri erişimi **EF Core 10**, veritabanı **SQL Server** olarak seçilmiştir.

Depoda ilk oluşturma komutunun geçmişi bulunmadığından projenin başlangıçta kesin olarak `dotnet new mvc` komutuyla üretildiği doğrulanamıyor. Mevcut proje MVC yapısına göre sadeleştirilmiştir; aşağıdaki adımlar aynı yapıyı yeniden kurma rehberidir.

Visual Studio'da yeni proje oluştururken **ASP.NET Core Web App (Model-View-Controller)** şablonunu seç; proje adını `SportsEvents.Mvc`, framework'ü `.NET 10`, kimlik doğrulamayı `None` olarak ayarla. MVC şablonu görünmüyorsa ASP.NET ve web geliştirme iş yükünü kontrol et. Mevcut projeyi kullanacaksan yeni proje oluşturman gerekmez; **4. adımdan devam et**.

## 2. Yeni proje oluştur

Bu komutlar yalnız sıfırdan kurulum içindir. Mevcut proje klasörünün içinde tekrar çalıştırma; ayrı, boş bir çalışma dizini kullan.

```powershell
dotnet --list-sdks
dotnet new mvc --name SportsEvents.Mvc --framework net10.0 --auth None
Set-Location SportsEvents.Mvc
```

Standart MVC şablonu HomeController, ortak layout ve ek statik dosyalar üretir. Bizim taslakta ana ekran EventsController ve `Views/Events/Index.cshtml` üzerinden çalışır; ekran kendi HTML iskeletini ve yerel CSS/JavaScript dosyalarını kullanır. Bu nedenle standart şablon çıktısı tek başına mevcut uygulamanın tamamı değildir.

## 3. Paketleri ve uygulama yapısını hazırla

Yeni oluşturduğun projede SQL Server sağlayıcısını ve Database First araç desteğini ekle:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version "10.0.*"
dotnet add package Microsoft.EntityFrameworkCore.Design --version "10.0.*"
dotnet user-secrets init
```

Mevcut projede bunlar tanımlıdır; tekrar eklemene gerek yok. Design paketinin proje dosyasında `PrivateAssets="all"` olması sağlanmıştır. `10.0.*` paket sürüm aralığıdır; tekrarlanabilir bir ekip/üretim derlemesinde doğrulanmış tam sürüm ve paket kilidi tercih edilebilir.

Uygulama dosyalarını şu sorumluluklarla oluştur. Tam uygulama kodları bu depodaki ilgili dosyalardadır:

| Sıra | Dosya / klasör | Yapılacak işlem |
|---|---|---|
| 1 | `Database/01-Setup.sql` | SQL şeması, kısıtlar, indeks ve başlangıç verilerini tanımla. |
| 2 | `Entities/` | Event, Participant ve Registration varlıklarını tanımla. |
| 3 | `Data/SportsEventsDbContext.cs` | DbSet, tablo/kolon eşlemeleri, anahtarlar ve ilişkileri tanımla. |
| 4 | `Models/EventPage.cs` | Ekran modeli, liste DTO'ları ve doğrulamalı kayıt girişini oluştur. |
| 5 | `Services/IEventService.cs` | Listeleme ve kayıt işlemleri için servis sözleşmesini tanımla. |
| 6 | `Services/EventService.cs` | Filtre, sayfalama ve transaction içindeki kontenjan/kayıt işlemlerini uygula. |
| 7 | `Controllers/EventsController.cs` | MVC listeleme, form doğrulama, antiforgery ve yönlendirme akışını kur. |
| 8 | `Controllers/EventsApiController.cs` | Salt okunur JSON etkinlik listesini ekle. |
| 9 | `Views/_ViewImports.cshtml` | Model namespace'ini ve MVC Tag Helper'larını ekle. |
| 10 | `Views/Events/Index.cshtml` | Filtre, tablo, sayfalama, kayıt formu ve mesajları oluştur. |
| 11 | `wwwroot/site.css`, `wwwroot/site.js` | Stil ve istemci tarih doğrulamasını ekle. |
| 12 | `Program.cs` | MVC, DbContext, servis DI kaydı, middleware ve rotaları bağla. |

Program.cs içindeki temel bağlantılar:

```csharp
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SportsEventsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SportsEvents")
        ?? throw new InvalidOperationException("Bağlantı dizesini tanımlayın.")));
builder.Services.AddScoped<IEventService, EventService>();
```

Bu bölüm tam Program.cs yerine geçmez; namespace, güvenlik middleware'i ve endpoint tanımları için depodaki dosyayı kullan. Varsayılan MVC rotası `{controller=Events}/{action=Index}/{id?}`, API rotası `/api/events` şeklindedir. `app.MapControllers()` API attribute rotalarını da eşler.

Mimari akış: **Controller → IEventService → EventService → DbContext → SQL Server**. Mevcut taslak tek proje içinde mantıksal katmanlar kullanır.

## 4. Veritabanını oluştur

1. SQL Server servisinin çalıştığını doğrula.
2. SQL Server Management Studio ile kullanacağın sunucuya bağlan.
3. `Database/01-Setup.sql` dosyasını aç ve çalıştır.
4. Betik yoksa **SportsEvents** veritabanını oluşturur; `USE [SportsEvents]` ile bu veritabanına geçer.
5. Object Explorer'ı yenile; `dbo.Event`, `dbo.Participant`, `dbo.Registration` tablolarını kontrol et.

Betik başlangıç verisi de ekler. Tablolar zaten varsa yeniden kurulum yapmaz, hata vererek durur. Mevcut veritabanında önce [README.md içindeki şema tamamlama açıklamasını](README.md#mevcut-örnek-şemayı-tamamla) incele. `CleanAppDB` adlı eski bir veritabanı varsa betiğin adını değiştirmek onu yeniden adlandırmaz veya verisini taşımaz.

## 5. Bağlantı dizesini tanımla

Mevcut projenin kök klasöründe PowerShell aç:

```powershell
dotnet user-secrets set "ConnectionStrings:SportsEvents" "Server=localhost;Database=SportsEvents;Integrated Security=True;TrustServerCertificate=True"
```

`Server` değerini SSMS'de bağlandığın sunucuyla aynı yap. Örneğin SQL Express için `Server=.\SQLEXPRESS` kullanılabilir. `Integrated Security=True`, uygulamayı çalıştıran Windows hesabıyla bağlanır; bu hesabın SportsEvents veritabanına yetkisi olmalıdır. `TrustServerCertificate=True` yerel prova içindir.

`ConnectionStrings:SportsEvents` uygulamanın ayar anahtarıdır; `Database=SportsEvents` SQL Server'daki gerçek veritabanı adıdır. Bağlantı dizesini C# koduna veya Git'e yazma. Mevcut csproj'da UserSecretsId tanımlıdır; user-secrets bu nedenle proje klasöründen kullanılabilir.

## 6. Derle ve çalıştır

```powershell
dotnet restore SportsEvents.Mvc.csproj
dotnet build SportsEvents.Mvc.csproj --no-restore
dotnet run --project SportsEvents.Mvc.csproj --no-build
```

Mevcut `Properties/launchSettings.json`, ortamı `Development`, adresi `http://localhost:5076` olarak ayarlar. Tarayıcıdan bu adresi aç. Uygulamayı durdurmak için terminalde `Ctrl+C` kullan.

Yeni MVC şablonundan başladıysan aynı launch profilini depodaki dosyadan aktar; aksi halde terminalde yazan adresi kullan. Üretimde HTTPS yönlendirmesi ve HSTS etkinleşir; yerel HTTP profili Development içindir.

## 7. Ekran ve kayıt akışını dene

Aşağıdaki sıra temiz başlangıç verisini esas alır:

1. Başlangıç `16.11.2026`, bitiş `17.11.2026` seçip filtrele: Yüzme, Basketbol, Voleybol görünmeli.
2. Elif'i Yüzme'ye kaydet: zaten kaydı olduğu için hata görünmeli; sayaç 1 kalmalı.
3. Can'ı Yüzme'ye kaydet: başarı mesajı görünmeli; sayaç 2 olmalı.
4. Deniz'i Yüzme'ye kaydet: kontenjan dolu mesajı görünmeli; sayaç 2 kalmalı.
5. Ters tarih aralığını dene: tarayıcı kontrolü devrede olmalı; sunucu da geçersiz aralığı reddetmeli.

Kayıtta önce koşullu UPDATE ile boş kontenjan ayrılır, sonra Registration eklenir. İki işlem aynı transaction içindedir; INSERT başarısız olursa sayaç artışı da geri alınır.

## 8. REST ve SQL raporlarını dene

Uygulama çalışırken şu adresi aç:

```text
http://localhost:5076/api/events?from=2026-11-16&to=2026-11-17&page=1
```

Yanıtta sayfa, devam bilgisi ve etkinlikler bulunur; katılımcı listesi yayımlanmaz. `page=0` için 400 beklenir.

SSMS'de **SportsEvents** veritabanını seçerek:

1. `Database/02-OccupancyReport.sql` çalıştır: saklanan sayaç ile aktif kayıt sayısını karşılaştır.
2. `Database/03-AdvancedReport.sql` çalıştır: stored procedure oluşturulur ve örnek rapor yürütülür.
3. İleri raporda CTE, DENSE_RANK, pencere toplamı ve CounterDifference kolonunu incele. Normal durumda fark 0 olmalı.
4. Performans incelemesinde gerçek yürütme planını aç; STATISTICS IO/TIME çıktısını kaydet. Yorum satırındaki indeks adayını ölçmeden uygulama.

## 9. Otomatik kontrolleri çalıştır

Uygulamanın açık olması ve SQL bağlantısı bu kontroller için gerekli değildir:

```powershell
dotnet build tests/SportsEvents.Checks.csproj -c Release -warnaserror
dotnet run --project tests/SportsEvents.Checks.csproj -c Release --no-build
```

Beklenen: 10 adet PASS çıktısı. Bu bir console kontrol programıdır; controller ve doğrulamayı sahte servisle sınar. Gerçek SQL Server, HTTP middleware, antiforgery ve tarayıcı akışlarını test etmez. Bu senaryolar [test planında](docs/TestPlani.md) yer alır.

Test kodlarının web projesine derlenmesini önlemek için csproj'da `Compile Remove="tests/**/*.cs"` tanımı bulunur. Test/artifact içerikleri de web içeriğinden çıkarılmıştır.

## 10. İsteğe bağlı: Database First model üret

Mevcut projede şemaya uygun varlıklar ve EF eşlemeleri elle yazılmıştır; uygulamayı çalıştırmak için scaffold gerekmez. Gerçek veritabanından yeniden üretmek istersen önce mevcut dosyalarının kopyasını al; `--force` Entities ve DbContext dosyalarını üzerine yazar.

```powershell
dotnet tool install --global dotnet-ef --version "10.0.*"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef dbcontext scaffold 'Name=ConnectionStrings:SportsEvents' Microsoft.EntityFrameworkCore.SqlServer --context SportsEventsDbContext --context-dir Data --output-dir Entities --table dbo.Event --table dbo.Participant --table dbo.Registration --no-onconfiguring --force
dotnet build SportsEvents.Mvc.csproj
```

Araç zaten kuruluysa sürümünü `dotnet ef --version` ile kontrol et; gerekiyorsa install yerine update kullan. Bu projede şema SQL betikleriyle yönetilir; kurulum için `dotnet ef database update` kullanılmaz.

## 11. Yayın paketi üret

```powershell
dotnet publish SportsEvents.Mvc.csproj -c Release -o artifacts/web
```

Çıktı `artifacts/web` altında oluşur; IIS için web.config dosyasını kontrol et. Paket üretmek sunucuya yayın yapmak değildir. IIS hazırlığı, bağlantı ayarları ve geri alma adımları [yayınlama planında](docs/LegacyVeYayinlama.md) açıklanmıştır.

`azure-pipelines.yml`, Azure DevOps üzerinde SDK kurulumu, restore, derleme, kontroller ve artifact üretimini tanımlar. Pipeline'ın çalışması için depo ve pipeline Azure DevOps'ta ayrıca bağlanmalıdır; mevcut dosya canlı deployment yapmaz.

## 12. Kriterlere göre çalış

[Kriterler.md](docs/Kriterler.md) üzerinden c–s maddelerini sırayla incele. [Analiz.md](docs/Analiz.md) iş kuralları ve diyagramları içerir. EF 4/4.5, SOAP/WCF, Windows Service, jQuery ve eski IIS sürümleri için mevcut modern uygulamadan ayrı çalışma gerekir; bunlar uygulanmış özellikler olarak sunulmaz.

## Sık karşılaşılan sorunlar

| Belirti | Kontrol |
|---|---|
| ConnectionStrings:SportsEvents ayarı eksik | user-secrets komutunu doğru proje klasöründe çalıştır; Development profilini kontrol et. |
| SQL bağlantısı kurulamıyor | Sunucu/instance adı, SQL servisi, Windows hesabı yetkisi ve Database=SportsEvents değerini kontrol et. |
| Invalid object name / kolon bulunamadı | Uygulama ve SSMS aynı veritabanını mı kullanıyor, şema kurulmuş mu kontrol et. |
| Tablolar zaten var hatası | 01-Setup.sql'i tekrar kurulum için çalıştırma; mevcut şemayı incele. |
| Sayfa 5076 üzerinde açılmıyor | Terminaldeki adresi, port kullanımını ve launchSettings.json dosyasını kontrol et. |
| Örnek kayıt sonuçları farklı | Önceki denemeler veriyi değiştirmiş olabilir; raporla sayaç/kayıtları incele. |
