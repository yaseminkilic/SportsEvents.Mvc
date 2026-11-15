# Legacy konular ve yayınlama taslağı

## Teknoloji ayrımı

Mevcut uygulama ASP.NET Core MVC ve EF Core kullanır. İlandaki klasik ASP.NET MVC, Entity Framework 4/4.5, WCF, Windows Service ve IIS 6/7/8 tecrübesi bu uygulamadan çıkarılamaz. Eski ortama doğrudan bu paketi kopyalamayı uyumluluk çözümü olarak kabul etme.

Microsoft'un [ASP.NET Core IIS rehberi](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0) ile hedef işletim sistemi, Hosting Bundle ve ASP.NET Core Module koşullarını doğrula. SOAP/WCF sunucusu için ayrı uyarlama gerekir; [CoreWCF destek politikası](https://dotnet.microsoft.com/en-us/platform/support/policy/corewcf) seçilecek sürüm için kontrol edilmelidir. CoreWCF burada kurulmadı.

## Ayrı laboratuvar görevleri

| Konu | Yapılacak prova | Kabul ölçütü |
|---|---|---|
| EF 4/4.5 | Uyumlu .NET Framework ortamında EDMX/Database First, LINQ ve transaction örneği | Oluşan SQL'i ve lazy/eager loading farkını anlat; Core eşlemesiyle karşılaştır. |
| Windows Service | Ayrı worker: yaklaşan etkinlik hatırlatma işi; servis hesabı, cancellation, Event Log, recovery | Start/stop testi; aynı bildirimin ikinci kez gönderilmemesi. Taslakta ileti gönderimi yok. |
| TFS/Azure DevOps | Branch, work item, review, build policy ve artifact akışı | Bir değişikliğin gereksinimden pakete izlenebilirliği. |
| jQuery/XHTML | Laboratuvar sayfasında seçici, event, AJAX; XHTML well-formed belge farkları | Sunucu doğrulamasını koru; CSP ile uyumlu harici script kullan. |
| SOAP/WCF/XML | Ayrı adapter projesinde WSDL/XSD, fault, timeout ve XML serializer | REST ve SOAP aynı uygulama servisini kullanır; entity doğrudan yayımlanmaz. |
| Eski IIS | Kuruma ait izole uyumlu legacy VM'de klasik uygulama yayın akışını çalış | App pool, web.config, binding, izin, log ve rollback'i göster. |

Örnek SOAP sözleşme taslağı: GetEventOccupancy(EventId:int) → EventId, Capacity, RegisteredCount; bulunamadığında EventNotFound fault. Request XML örneği aşağıdadır; endpoint/WSDL implementasyonu değildir.

```xml
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <GetEventOccupancy xmlns="urn:sportsevents:v1">
      <EventId>1</EventId>
    </GetEventOccupancy>
  </s:Body>
</s:Envelope>
```

XML alımında DTD/external entity çözümlemeyi kapatma, boyut sınırı ve şema doğrulama adapter tasarımına dahil edilmelidir.

## Modern IIS yayın runbook'u

1. Test ve kimlik/yetki işlerini tamamla. Hedef Windows/IIS/.NET uyumunu Microsoft rehberinden doğrula; uygun Hosting Bundle kur.
2. `dotnet publish SportsEvents.Mvc.csproj -c Release -o artifacts/web` çalıştır. Web SDK'nın ürettiği web.config dosyasını pakette doğrula.
3. Test ortamında ayrı site/app pool oluştur; .NET CLR ayarını No Managed Code seç, mimariyi paketle eşleştir. Uygulama havuzu hesabına yalnız gereken dizin/SQL izinlerini ver. Şema değiştirme yetkisini runtime hesabına verme.
4. HTTPS binding ve sertifikayı kur. `ASPNETCORE_ENVIRONMENT=Production` ve `ConnectionStrings__SportsEvents` ayarlarını sunucuda korumalı biçimde tanımla; sertifika doğrulamasını kapatma. Sırları YAML/pakete ekleme.
5. Veritabanı yedeğini al; versiyonlu ve incelenmiş SQL değişikliklerini ayrı dağıtım kimliğiyle uygula. 01-Setup.sql yeni prova içindir; mevcut üretim şemasını yükseltme betiği değildir.
6. Bakım penceresinde trafiği durdur, paketi yeni sürüm dizinine aç, IIS fiziksel yolu değiştir. Liste/API ve yetkili kayıt smoke testi yap; IIS/Event Log ve uygulama hata oranını izle.
7. Sorunda önceki paket dizinine dön. Veritabanını geri alma ayrı plandır: uygulama sürümüyle geriye uyumlu şema değişiklikleri tercih et; yedekten dönüşte veri kaybı ve kesinti değerlendirilir.

azure-pipelines.yml yalnız CI ve paket üretimini uygular. CD devamı için test ortamı deployment job'u, smoke testleri, ortam yetkileri ve üretim terfisi tanımlanmalıdır. Bu oturumda sunucuya yayın yapılmadı ve pipeline çalıştırılmadı.
