# Taslak doğrulama kaydı

15 Kasım 2026 tarihinde yerelde çalıştırıldı:

- `dotnet build tests/SportsEvents.Checks.csproj -c Release -warnaserror`: başarılı, 0 uyarı / 0 hata.
- `dotnet run --project tests/SportsEvents.Checks.csproj -c Release --no-build`: 10 kontrol geçti.
- `dotnet publish SportsEvents.Mvc.csproj -c Release --no-restore -o artifacts/web`: başarılı; yerel yayın paketi üretildi.

Gerçek SQL Server bağlantısı/rapor çalıştırma, eşzamanlı kayıt entegrasyonu, tarayıcı/HTTP testleri, yük testi, Azure Pipelines çalıştırması ve IIS deployment bu doğrulamaya dahil değildir. Yayın paketi üretimi canlı yayın yapıldığı anlamına gelmez.
