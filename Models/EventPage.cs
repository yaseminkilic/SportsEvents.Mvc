using System.ComponentModel.DataAnnotations;

namespace SportsEvents.Mvc.Models;

public sealed record EventRow(int Id, string Name, DateTime StartAt, int Capacity, int RegisteredCount);
public sealed record ParticipantOption(int Id, string FullName);

public sealed class RegistrationInput
{
    [Range(1, int.MaxValue, ErrorMessage = "Etkinlik seçiniz.")]
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Katılımcı seçiniz.")]
    public int ParticipantId { get; set; }
}

public sealed record EventPage(
    DateOnly? From, DateOnly? To, int Page, bool HasNext,
    IReadOnlyList<EventRow> Events, IReadOnlyList<ParticipantOption> Participants,
    RegistrationInput Input)
{
    public string? FromValue => From?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    public string? ToValue => To?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}
