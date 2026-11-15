using SportsEvents.Mvc.Models;

namespace SportsEvents.Mvc.Services;

public interface IEventService
{
    Task<EventPage> GetPageAsync(DateOnly? from, DateOnly? to, int page,
        RegistrationInput input, CancellationToken ct);
    Task<string?> RegisterAsync(RegistrationInput input, CancellationToken ct);
}
