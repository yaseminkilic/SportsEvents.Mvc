-- Önce SSMS'de uygulamanın kullandığı veritabanını seçin.
DECLARE @From date = '2026-11-16';
DECLARE @ToExclusive date = '2026-11-18';

SELECT
    e.Id,
    e.Name,
    e.Capacity,
    e.RegisteredCount AS StoredCount,
    COUNT(r.Id) AS ActiveCount,
    CAST(100.0 * COUNT(r.Id) / e.Capacity AS decimal(9, 2)) AS OccupancyPercent
FROM dbo.[Event] AS e
LEFT JOIN dbo.Registration AS r ON r.EventId = e.Id AND r.IsCancelled = 0
WHERE e.StartAt >= @From AND e.StartAt < @ToExclusive
GROUP BY e.Id, e.Name, e.Capacity, e.RegisteredCount
ORDER BY OccupancyPercent DESC, e.Id;
