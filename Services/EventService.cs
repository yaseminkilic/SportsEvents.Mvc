using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SportsEvents.Mvc.Data;
using SportsEvents.Mvc.Entities;
using SportsEvents.Mvc.Models;

namespace SportsEvents.Mvc.Services;

public sealed class EventService(SportsEventsDbContext db) : IEventService
{
    public async Task<EventPage> GetPageAsync(DateOnly? from, DateOnly? to, int page,
        RegistrationInput input, CancellationToken ct)
    {
        const int pageSize = 20;
        page = Math.Clamp(page, 1, 10_000);
        var query = db.Events.AsNoTracking();

        if (from is { } firstDay)
        {
            var start = firstDay.ToDateTime(TimeOnly.MinValue);
            query = query.Where(e => e.StartAt >= start);
        }

        if (to is { } lastDay && lastDay < DateOnly.MaxValue)
        {
            var end = lastDay.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(e => e.StartAt < end);
        }

        var events = await query.OrderBy(e => e.StartAt).ThenBy(e => e.Id)
            .Skip((page - 1) * pageSize).Take(pageSize + 1)
            .Select(e => new EventRow(e.Id, e.Name, e.StartAt, e.Capacity, e.RegisteredCount))
            .ToListAsync(ct);

        var participants = await db.Participants.AsNoTracking()
            .OrderBy(p => p.FullName).ThenBy(p => p.Id)
            .Select(p => new ParticipantOption(p.Id, p.FullName))
            .ToListAsync(ct);

        var hasNext = events.Count > pageSize;
        if (hasNext)
            events.RemoveAt(pageSize);

        return new EventPage(from, to, page, hasNext && page < 10_000, events, participants, input);
    }

    public async Task<string?> RegisterAsync(RegistrationInput input, CancellationToken ct)
    {
        if (!await db.Participants.AnyAsync(p => p.Id == input.ParticipantId, ct))
            return "Katılımcı bulunamadı.";

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var affectedRows = await db.Events
            .Where(e => e.Id == input.EventId && e.RegisteredCount < e.Capacity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(
                e => e.RegisteredCount, e => e.RegisteredCount + 1), ct);

        if (affectedRows == 0)
        {
            var exists = await db.Events.AnyAsync(e => e.Id == input.EventId, ct);
            return exists ? "Kontenjan dolu." : "Etkinlik bulunamadı.";
        }

        var registration = new Registration
        {
            EventId = input.EventId,
            ParticipantId = input.ParticipantId
        };
        db.Registrations.Add(registration);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            db.Entry(registration).State = EntityState.Detached;
            return "Bu katılımcının etkinlik için zaten bir kaydı var.";
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            db.Entry(registration).State = EntityState.Detached;
            return "Etkinlik veya katılımcı artık mevcut değil.";
        }

        await transaction.CommitAsync(ct);
        return null;
    }
}
