# Elevator Control - Assignment 1 (C# / Windows Forms / SQL Server)

A two-floor building with **two elevators**, controlled by a Windows Forms app.
Every elevator event (button pressed, moving, arrived, door opened/closed) is saved to SQL Server with a timestamp.

## What you need
- .NET 10 SDK (check with `dotnet --version`)
- Visual Studio 2026 (or VS Code / command line). Visual Studio 2022 cannot open .NET 10 projects.
- SQL Server Express running as `localhost\SQLEXPRESS` (SSMS 2022 to look at the data)

## How to run
1. Make sure the **SQL Server (SQLEXPRESS)** service is running (Windows "Services" app).
2. Open `ElevatorControl.sln` and press **F5**.
   Or in a terminal, from this folder: `dotnet run --project ElevatorControl`
3. On the first start the program creates the database `ElevatorDB` and its tables by itself
   (it runs the scripts in the `Database` folder - found with a relative path).
   If your instance has another name, edit the `Server=` part in `ElevatorControl/App.config`.

## How to use
- **Request Elevator - Floor 0 / Floor 1**: the outside request buttons. The nearest available elevator answers.
- **Panel buttons 1 / 0** (on each elevator's own panel): send *that* elevator to a floor.
- Lit (gold) buttons = request not served yet.
- **Log** button: opens the saved log from the database (newest first).

## Look at the database in SSMS
Connect to `localhost\SQLEXPRESS`, open `Database/03_TestQueries.sql` and run the queries.

## Folder guide
| Folder | Contents |
|---|---|
| `ElevatorControl/Models` | enums and the `LogEntry` data class |
| `ElevatorControl/Elevators` | `Elevator`, `ElevatorController` |
| `ElevatorControl/Elevators/States` | State pattern classes |
| `ElevatorControl/Elevators/Observers` | Observer pattern + `LoggingObserver` (BackgroundWorker) |
| `ElevatorControl/Data` | all database code (`DatabaseHelper`, `ElevatorLogRepository`) |
| `ElevatorControl/Forms` | `MainForm`, `LogForm`, `ElevatorPanelControl` |
| `Database` | SQL scripts |
| `Documentation` | testing report + OOP explanation |
