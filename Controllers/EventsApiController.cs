using Microsoft.AspNetCore.Mvc;
using SportsEvents.Mvc.Models;
using SportsEvents.Mvc.Services;

namespace SportsEvents.Mvc.Controllers;

// Yerel prova için salt okunur endpoint. Katılımcı listesini dışarı vermez.
[ApiController]
[Route("api/events")]
public sealed class EventsApiController(IEventService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(DateOnly? from, DateOnly? to,
        CancellationToken ct, int page = 1)
    {
        if (from > to || page is < 1 or > 10_000)
            return BadRequest(new ProblemDetails { Title = "Tarih aralığı veya sayfa geçersiz.", Status = 400 });

        var result = await service.GetPageAsync(from, to, page, new RegistrationInput(), ct);
        return Ok(new { result.Page, result.HasNext, result.Events });
    }
}
