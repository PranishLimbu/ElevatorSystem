-- Handy queries to run in SQL Server Management Studio (SSMS) while testing.
USE ElevatorDB;
GO

-- 1. Everything that has been logged, newest first
SELECT l.LogId, e.Name AS Elevator, l.LogTime, l.EventType, l.FloorNumber, l.Details
FROM dbo.ElevatorLog AS l
JOIN dbo.Elevators   AS e ON e.ElevatorId = l.ElevatorId
ORDER BY l.LogId DESC;

-- 2. How many events of each type?
SELECT EventType, COUNT(*) AS Total
FROM dbo.ElevatorLog
GROUP BY EventType
ORDER BY Total DESC;

-- 3. Events of one elevator
SELECT * FROM dbo.ElevatorLog WHERE ElevatorId = 1 ORDER BY LogTime;

-- 4. Start again with an empty log (careful!)
-- DELETE FROM dbo.ElevatorLog;

-- 5. Delete the whole database to test that the program recreates it (close the program first)
-- USE master; DROP DATABASE ElevatorDB;
