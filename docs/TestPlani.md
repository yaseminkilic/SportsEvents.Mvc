# Doğrulama planı

## Çalıştırılabilir hızlı kontroller

```powershell
dotnet build tests/SportsEvents.Checks.csproj -c Release -warnaserror
dotnet run --project tests/SportsEvents.Checks.csproj -c Release --no-build
```

Bu ek test paketi gerektirmeyen bir console kontrol programıdır; hata durumunda sıfırdan farklı çıkış kodu üretir. Kimlik doğrulaması (Range), geçersiz filtrede servisin çağrılmaması, geçersiz formda yazma yapılmaması, iş kuralı hata aktarımı, başarılı yönlendirme ve API sayfa doğrulamasını kontrol eder. Gerçek veritabanını, middleware'i veya HTTP model binding'i çalıştırmaz; dotnet test keşfi/TRX çıktısı yoktur.

## SQL Server entegrasyon testleri — henüz otomatikleştirilmedi

Yalnız ayrı ve silinebilir test veritabanında 01-Setup.sql ile temiz veri kur. Her senaryo öncesi temiz duruma dön; üretim veritabanını kullanma.

| Kural | İşlem | Beklenen |
|---|---|---|
| BR-02 | Olmayan katılımcıyla kayıt | Kayıt yok, sayaç değişmez. |
| BR-03 | Elif'i tekrar Yüzme'ye kaydet | Duplicate hata, sayaç 1. |
| BR-04 | Can'ı Yüzme'ye kaydet | Tek kayıt, sayaç 2. |
| BR-04 | Sonraki katılımcıyı Yüzme'ye kaydet | Dolu hata, sayaç 2. |
| BR-04 | Temiz veride Can ve Deniz'i ayrı DbContext/bağlantılarla aynı anda kaydet | Tek başarı, aktif kayıt ve sayaç 2. |
| BR-04 | Boş kontenjanda aynı katılımcıyı iki kez ekle | Başarısız INSERT sayaç artışını geri alır. |
| BR-01 | SQL ile negatif/aşırı sayaç veya sıfır kapasite yaz | CHECK hatası. |
| US-01 | 21 etkinlikli veriyle filtre ve ikinci sayfa | İlk sayfa 20; devam doğru; aynı tarihli kayıtlar Id ile kararlı sıralanır. |

EF InMemory/SQLite ile SQL Server'ın kilitlenme ve constraint davranışları kanıtlanamaz. 02 ve 03 raporlarında CounterDifference sıfır olmalı.

## HTTP, güvenlik ve arayüz — plan

- Playwright: filtre → kayıt → başarı/hata mesajı; klavye erişimi, etiketler ve sayfalama.
- Antiforgery token olmadan gerçek HTTP POST → 400; token ile geçerli form → 302 ve ardından GET.
- Bozuk tarih/sayısal alan → 400 veya form hatası; kayıt oluşmaz.
- Veritabanına HTML içeren etkinlik adı koy → metin olarak gösterilir; script çalışmaz.
- API yanıtında katılımcı bilgileri bulunmaz; yanlış sayfa 400; geçerli liste 200.
- Güvenlik başlıklarını gerçek yanıt üzerinden kontrol et; üretimde hata detaylarının gizlendiğini doğrula.
- JavaScript kapalıyken sunucu doğrulaması çalışmalı. Kimlik/yetki testleri ilgili özellik eklendiğinde yazılmalı.

## Performans — plan, ölçüm sonucu yok

Sabit donanım ve SQL veri büyüklüğünü kaydet. Önerilen kabul hedefini ekip onaylasın: 10 bin etkinlik/100 bin kayıt, 20 eşzamanlı okuma, 5 dakika, p95 < 500 ms, beklenmeyen 5xx < %1. Bunlar garanti veya ölçülmüş sonuç değildir. k6 ile okuma/yazmayı ayrı ölç; yazma testlerini temiz test verisiyle çalıştır. SQL logical reads/CPU/süre, uygulama bellek ve bağlantı havuzunu kaydet. İndeks öncesi/sonrası aynı veriyle karşılaştır; yüksek sayfada OFFSET maliyetine karşı keyset pagination değerlendir.
