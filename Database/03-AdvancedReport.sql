-- Uygulamanın kullandığı veritabanında, 01-Setup.sql sonrasında çalıştırın.
CREATE OR ALTER PROCEDURE dbo.GetEventOccupancy
    @From datetime2(0),
    @ToExclusive datetime2(0)
AS
BEGIN
    SET NOCOUNT ON;
    IF @From >= @ToExclusive
        THROW 50001, N'Başlangıç bitişten önce olmalıdır.', 1;

    ;WITH Occupancy AS (
        SELECT e.Id, e.Name, e.StartAt, e.Capacity, e.RegisteredCount,
               COUNT(r.Id) AS ActiveCount
        FROM dbo.[Event] e
        LEFT JOIN dbo.Registration r ON r.EventId = e.Id AND r.IsCancelled = 0
        WHERE e.StartAt >= @From AND e.StartAt < @ToExclusive
        GROUP BY e.Id, e.Name, e.StartAt, e.Capacity, e.RegisteredCount
    ), Rates AS (
        SELECT *, CAST(100.0 * ActiveCount / NULLIF(Capacity, 0) AS decimal(9,2)) AS OccupancyPercent
        FROM Occupancy
    )
    SELECT *, DENSE_RANK() OVER (ORDER BY OccupancyPercent DESC) AS OccupancyRank,
           SUM(ActiveCount) OVER () AS TotalActiveCount,
           ActiveCount - RegisteredCount AS CounterDifference
    FROM Rates
    ORDER BY OccupancyRank, Id;
END;
GO
-- Ölçüm: gerçek yürütme planıyla birlikte çalıştırın; istatistikleri kaydedin.
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
EXEC dbo.GetEventOccupancy @From = '2026-11-16', @ToExclusive = '2026-11-19';
SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
-- İndeks adayı: aktif kayıtlar çok küçük bir alt kümeyse ölçerek değerlendirin.
-- CREATE INDEX IX_Registration_Active_Event ON dbo.Registration(EventId)
--     WHERE IsCancelled = 0;
