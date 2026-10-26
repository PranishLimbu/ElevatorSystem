namespace ElevatorControl.Models
{
    // The kinds of things an elevator tells its observers about
    public enum ElevatorEventType
    {
        ButtonPressed,
        StartedMoving,
        Arrived,
        DoorOpened,
        DoorClosed,
        StateChanged
    }
}
