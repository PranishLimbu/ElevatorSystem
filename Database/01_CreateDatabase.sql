-- Run this on the "master" database (the program does this automatically on start-up).
-- It only creates the database if it does not exist yet, so it is safe to run many times.
IF DB_ID(N'ElevatorDB') IS NULL
BEGIN
    CREATE DATABASE ElevatorDB;
END
GO
