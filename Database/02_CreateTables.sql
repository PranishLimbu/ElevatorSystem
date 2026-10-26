USE ElevatorDB;
GO

-- Table 1: the elevators in the building
IF OBJECT_ID(N'dbo.Elevators', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Elevators
    (
        ElevatorId INT           NOT NULL CONSTRAINT PK_Elevators PRIMARY KEY,
        Name       NVARCHAR(50)  NOT NULL
    );
END
GO

-- Table 2: the log of everything the elevators do
IF OBJECT_ID(N'dbo.ElevatorLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ElevatorLog
    (
        LogId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ElevatorLog PRIMARY KEY,
        ElevatorId  INT           NOT NULL
                    CONSTRAINT FK_ElevatorLog_Elevators REFERENCES dbo.Elevators(ElevatorId),
        EventType   NVARCHAR(50)  NOT NULL,
        FloorNumber INT           NOT NULL CONSTRAINT CK_ElevatorLog_Floor CHECK (FloorNumber IN (0, 1)),
        Details     NVARCHAR(200) NULL,
        LogTime     DATETIME2(0)  NOT NULL CONSTRAINT DF_ElevatorLog_LogTime DEFAULT SYSDATETIME()
    );

    CREATE INDEX IX_ElevatorLog_LogTime ON dbo.ElevatorLog(LogTime);
END
GO

-- The two elevators must exist before we can log anything for them
IF NOT EXISTS (SELECT 1 FROM dbo.Elevators)
BEGIN
    INSERT INTO dbo.Elevators (ElevatorId, Name) VALUES (1, N'Elevator 1'), (2, N'Elevator 2');
END
GO
