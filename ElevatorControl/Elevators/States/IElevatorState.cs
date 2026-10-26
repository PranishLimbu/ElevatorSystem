namespace ElevatorControl.Elevators.States
{
    // STATE PATTERN - the interface (abstraction) every state must follow.
    public interface IElevatorState
    {
        string Name { get; }
        void OnEnter(Elevator elevator);                  // runs once when we enter this state
        void HandleRequest(Elevator elevator, int floor); // a button was pressed
        void Tick(Elevator elevator);                     // the timer ticked
    }
}
