using System.Collections.Concurrent;
using System.ComponentModel;
using ElevatorControl.Data;
using ElevatorControl.Models;

namespace ElevatorControl.Elevators.Observers
{
    // An observer that saves elevator events to the database.
    // It is the ONLY place that decides when to log, so there is no database code
    // repeated in the button click handlers (Task 3 - "no duplication").
    //
    // BackgroundWorker (Task 5.1): the database work runs on another thread,
    // so the animation never freezes while SQL Server is busy.
    public class LoggingObserver : IElevatorObserver
    {
        private readonly ElevatorLogRepository repository;
        private readonly ConcurrentQueue<LogEntry> pending = new ConcurrentQueue<LogEntry>();
        private readonly BackgroundWorker worker = new BackgroundWorker();

        // A delegate-based event: tells the GUI when saving failed
        public event Action<string> LogError;

        public LoggingObserver(ElevatorLogRepository repository)
        {
            this.repository = repository;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        public void OnElevatorEvent(Elevator elevator, ElevatorEventType type, string message)
        {
            // StateChanged is only for the GUI, we don't need it in the log
            if (type == ElevatorEventType.StateChanged)
            {
                return;
            }

            pending.Enqueue(new LogEntry(elevator.Id, type.ToString(), elevator.CurrentFloor, message, DateTime.Now));
            StartWorkerIfNeeded();
        }

        private void StartWorkerIfNeeded()
        {
            // A BackgroundWorker can only do one job at a time
            if (!worker.IsBusy && !pending.IsEmpty)
            {
                worker.RunWorkerAsync();
            }
        }

        // Runs on a background thread
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (pending.TryDequeue(out LogEntry entry))
                {
                    repository.AddLog(entry);
                }
            }
            catch (Exception ex)
            {
                // The database is probably down: throw away the waiting entries
                // so we don't retry (and wait for a timeout) for every one of them
                while (pending.TryDequeue(out LogEntry _)) { }
                e.Result = ex.Message;
            }
        }

        // Runs back on the GUI thread
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is string errorMessage)
            {
                LogError?.Invoke(errorMessage);
            }

            // Events may have arrived while the worker was busy
            StartWorkerIfNeeded();
        }
    }
}
