using Microsoft.AspNetCore.Mvc;
using SportsEvents.Mvc.Models;
using SportsEvents.Mvc.Services;

namespace SportsEvents.Mvc.Controllers;

public sealed class EventsController(IEventService service) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to,
        CancellationToken ct, int page = 1)
    {
        if (!ModelState.IsValid || from > to)
            return BadRequest("Geçerli bir tarih aralığı giriniz.");

        return View(await service.GetPageAsync(from, to, page, new RegistrationInput(), ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind(Prefix = "Input")] RegistrationInput input,
        DateOnly? from, DateOnly? to, CancellationToken ct, int page = 1)
    {
        if (from > to)
            return BadRequest("Geçerli bir tarih aralığı giriniz.");

        if (ModelState.IsValid)
        {
            var error = await service.RegisterAsync(input, ct);
            if (error is null)
            {
                TempData["Success"] = "Katılımcı kaydedildi.";
                var dateFormat = System.Globalization.CultureInfo.InvariantCulture;
                return RedirectToAction(nameof(Index), new
                {
                    from = from?.ToString("yyyy-MM-dd", dateFormat),
                    to = to?.ToString("yyyy-MM-dd", dateFormat),
                    page
                });
            }

            ModelState.AddModelError(string.Empty, error);
        }

        return View(nameof(Index), await service.GetPageAsync(from, to, page, input, ct));
    }

    [Route("Events/Error")]
    public IActionResult Error() =>
        Problem(title: "İşlem tamamlanamadı.", statusCode: StatusCodes.Status500InternalServerError);
}
