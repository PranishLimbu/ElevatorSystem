namespace ElevatorControl.Elevators.States
{
    // An abstract base class with the behaviour most states share.
    // Child classes only override what is different (INHERITANCE + POLYMORPHISM).
    public abstract class ElevatorStateBase : IElevatorState
    {
        public abstract string Name { get; }

        public virtual void OnEnter(Elevator elevator)
        {
        }

        // Default: remember the floor, the elevator will get there later
        public virtual void HandleRequest(Elevator elevator, int floor)
        {
            elevator.AddDestination(floor);
        }

        public abstract void Tick(Elevator elevator);
    }
}
