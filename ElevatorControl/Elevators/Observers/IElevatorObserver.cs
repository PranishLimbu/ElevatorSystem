using ElevatorControl.Models;

namespace ElevatorControl.Elevators.Observers
{
    // OBSERVER PATTERN - the "observer" side.
    // Anything that wants to be told about elevator events implements this interface
    // (the GUI panels, the main form and the database logger all do).
    public interface IElevatorObserver
    {
        void OnElevatorEvent(Elevator elevator, ElevatorEventType type, string message);
    }
}
