using ElevatorControl.Models;

namespace ElevatorControl.Elevators.Observers
{
    // OBSERVER PATTERN - the "subject" side.
    // It keeps a list of observers and tells all of them when something happens.
    // Elevator INHERITS from this class, so it gets Attach/Detach for free.
    public abstract class ElevatorSubject
    {
        private readonly List<IElevatorObserver> observers = new List<IElevatorObserver>();

        public void Attach(IElevatorObserver observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public void Detach(IElevatorObserver observer)
        {
            observers.Remove(observer);
        }

        protected void NotifyObservers(Elevator source, ElevatorEventType type, string message)
        {
            // ToArray() makes a copy, so it is safe if an observer detaches itself while we loop
            foreach (IElevatorObserver observer in observers.ToArray())
            {
                observer.OnElevatorEvent(source, type, message);
            }
        }
    }
}
