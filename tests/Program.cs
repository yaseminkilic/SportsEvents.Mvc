using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SportsEvents.Mvc.Controllers;
using SportsEvents.Mvc.Models;
using SportsEvents.Mvc.Services;

static void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine($"PASS: {name}");
}

var input = new RegistrationInput();
Check(!Validator.TryValidateObject(input, new ValidationContext(input), [], true), "Sıfır kimlikler reddedilir");
var service = new FakeEvents();
var controller = new EventsController(service);
Check(await controller.Index(new(2026, 11, 18), new(2026, 11, 16), default) is BadRequestObjectResult,
    "Ters tarih aralığı servise gitmeden reddedilir");
Check(service.Reads == 0, "Geçersiz filtre veritabanına gitmez");
controller.ModelState.AddModelError("Input.EventId", "Geçersiz");
Check(await controller.Register(input, null, null, default) is ViewResult, "Geçersiz form yeniden gösterilir");
Check(service.Writes == 0, "Geçersiz form kayıt oluşturmaz");
controller.ModelState.Clear();
service.Error = "Kontenjan dolu.";
Check(await controller.Register(new() { EventId = 1, ParticipantId = 2 }, null, null, default) is ViewResult,
    "İş kuralı hatası formda gösterilir");
Check(!controller.ModelState.IsValid, "İş kuralı hatası ModelState içine yazılır");
controller.ModelState.Clear();
service.Error = null;
controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new MemoryTempData());
Check(await controller.Register(new() { EventId = 1, ParticipantId = 2 }, null, null, default) is RedirectToActionResult,
    "Başarılı kayıt POST/Redirect/GET uygular");
var api = new EventsApiController(service);
Check(await api.Get(null, null, default, 0) is BadRequestObjectResult, "API geçersiz sayfayı reddeder");
Check(await api.Get(null, null, default) is OkObjectResult, "API geçerli sorguya 200 sonucu üretir");

sealed class FakeEvents : IEventService
{
    public int Reads, Writes;
    public string? Error;
    public Task<EventPage> GetPageAsync(DateOnly? from, DateOnly? to, int page, RegistrationInput input, CancellationToken ct)
    {
        Reads++;
        return Task.FromResult(new EventPage(from, to, page, false, [], [], input));
    }
    public Task<string?> RegisterAsync(RegistrationInput input, CancellationToken ct)
    {
        Writes++;
        return Task.FromResult(Error);
    }
}

sealed class MemoryTempData : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
