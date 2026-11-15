# Kriter değerlendirmesi ve proje taslağı

Amaç: Spor etkinliklerine katılımcı kaydeden bir uygulama üzerinden teknik kriterleri somut örneklerle çalışmak ve sunmak. Bir taslak, kişinin geçmiş iş tecrübesini kanıtlamaz. Aşağıdaki cevaplar projedeki yaklaşımı anlatır; kişisel deneyim beyanı olarak kullanılmamalıdır.

## Kriterler ve açıklama örnekleri

| Madde | Açıklama / görüşmede cevaplanacak konu | Projedeki karşılık ve kapsam |
|---|---|---|
| c | C# async/await ve dependency injection kullanırım; MVC'de controller HTTP akışını, service iş akışını, Razor görünümü yönetir. | Controllers, Services, Views; .NET 10 / ASP.NET Core MVC. |
| d | Gereksinim, analiz, tasarım, kodlama, inceleme, test, yayın ve izleme adımlarını izlerim. Girdi doğrulama, CSRF, XSS ve sır yönetimini tasarımda ele alırım. | Antiforgery, DTO doğrulama, Razor encoding, güvenlik başlıkları; üretim kimlik/yetki yönetimi henüz yok. |
| e | SQL Server'da PK, FK, unique ve check kısıtlarıyla tutarlılığı korurum; EF eşlemelerini şemayla uyumlu tutarım. | Database/01-Setup.sql, Data/SportsEventsDbContext.cs. Oracle/PostgreSQL uygulaması yok; “ve/veya” şartı için SQL Server seçildi. |
| f | Sunum, uygulama servisi, veri erişimi ve varlık sorumluluklarını ayırırım; controller arayüze bağımlıdır. | IEventService ile mantıksal katmanlar. Ayrı assembly sınırları henüz yok. |
| g | Sorguyu sayfalar, yalnız gerekli kolonları seçer, güvenlik kontrollerini ve performans ölçümünü ayrı doğrularım. | AsNoTracking, Take, projection, CSP; otomatik controller kontrolleri ve TestPlani.md. Yük testi sonucu yok. |
| h | İndeksi sorgunun filtre/sıralamasına göre seçer; gerçek yürütme planı, mantıksal okuma ve süreyi önce/sonra karşılaştırırım. | StartAt/Id indeksi, yarı açık tarih aralığı; SQL raporunda STATISTICS IO/TIME. |
| i | JOIN, GROUP BY, CTE, pencere fonksiyonları, parametreli stored procedure ve transaction kullanabilirim. | 02-OccupancyReport.sql, 03-AdvancedReport.sql. PL/SQL örneği yok. |
| j | Semantik HTML, CSS ve JavaScript ile form davranışını kurarım; istemci doğrulamasına güvenmem. | Razor, site.css, site.js. jQuery ve XHTML uygulaması yok; ek çalışma konusu. |
| k | EF ile veri erişimi, Windows Service yaşam döngüsü ve TFS iş akışlarını birbirinden ayırırım. | EF Core mevcut. EF 4/4.5, gerçek Windows Service ve TFS çalışması uygulanmadı; LegacyVeYayinlama.md içindeki laboratuvar planı. |
| l | JavaScript olaylarını, veri akışını ve DOM kullanımını kaynak koddan takip ederim; güvensiz veri yerleştirmelerini ararım. | site.js üzerinde inceleme alıştırması: input → setCustomValidity; innerHTML/eval kullanılmıyor. Compiler uyarıları CI'da hata sayılır. Kapsamlı SAST henüz yok. |
| m | REST kaynak ve HTTP durum kodlarını; SOAP zarf, WSDL ve sözleşme kavramlarını açıklayabilirim. | GET /api/events JSON örneği var. XML/SOAP/WCF sözleşme taslağı belgede; çalışan SOAP sunucusu yok. |
| n | IIS yayınında runtime/Hosting Bundle, uygulama havuzu, sertifika, erişim hakları, loglar ve rollback'i kontrol ederim. | Modern IIS için yayın runbook'u. IIS 6/7/8 üzerinde doğrulanmış deployment yok. |
| o–p | Kod inceleme ve CI ile derleme/test/paket üretir, CD'de ortama özgü sırlar ve kontrollü terfi kullanırım. | azure-pipelines.yml restore/build/check/publish/artifact içerir. Canlı CD, Git geçmişi, TFS/SVN entegrasyonu yok. p satırı o maddesindeki CI-CD açılımının devamı olarak yorumlandı. |
| q | Aktörleri, kapsamı ve istisnaları belirleyip kullanıcı hikâyesini ölçülebilir kabul koşullarına çeviririm. | Analiz.md: hikâyeler, kabul koşulları ve açık kararlar. |
| r | İş kurallarını numaralandırır, sınıf ve ER diyagramını veri kısıtlarıyla ilişkilendiririm. | Analiz.md: Mermaid sınıf ve ER diyagramları. |
| s | Birim, SQL entegrasyon, HTTP/güvenlik, uçtan uca ve yük testlerini risklere göre seçerim. | tests/SportsEvents.Checks.csproj çalıştırılabilir kontrol programı; SQL/HTTP/yük testleri için plan var. |

## Sunum sırası

1. Analiz.md üzerinden problemi ve kabul koşullarını anlat.
2. MVC → IEventService → EventService → EF → SQL akışını göster.
3. İki eşzamanlı kayıt isteğinde koşullu UPDATE ve transaction'ın kontenjanı nasıl koruduğunu açıkla.
4. SQL raporunda CTE ve DENSE_RANK göster; ölçüm yapmadan hız artışı iddia etme.
5. Otomatik kontrolleri çalıştır; bunların gerçek SQL ve HTTP testleri olmadığını belirt.
6. CI paketini ve IIS yayın/geri alma planını anlat; eski teknolojilerde uygulamadığın konuları açıkça söyle.

## Sonraki geliştirme adımları

1. Kimlik doğrulama ve rol/nesne düzeyinde yetkilendirme; katılımcı bilgilerini yalnız yetkili görevliye açma.
2. Domain/Application/Infrastructure/Web projelerine ayırma; giriş modellerini web, uygulama DTO'larını Application içinde tutma.
3. Gerçek SQL Server entegrasyon testleri, HTTP antiforgery kontrolleri, Playwright ve k6 otomasyonu.
4. Katılımcı aramasını sayfalama; API listesini katılımcı sorgusundan ayırma (taslakta ortak GetPageAsync kullanır).
5. Windows Service hatırlatma worker'ı, kalıcı iş kuyruğu, retry ve idempotency.
6. Ayrı legacy laboratuvarında EF 4/4.5, jQuery, SOAP/WCF ve kurumun TFS/IIS akışlarını çalışma.
