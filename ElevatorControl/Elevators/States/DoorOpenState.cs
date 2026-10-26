using ElevatorControl.Models;

namespace ElevatorControl.Elevators.States
{
    // The elevator has stopped at a floor. The door opens, stays open for a while, then closes.
    // The door stays CLOSED while the elevator travels (so nobody touches the shaft wall).
    public class DoorOpenState : ElevatorStateBase
    {
        private enum Phase
        {
            Opening,
            Holding,
            Closing
        }

        private const double DoorStep = 0.05;   // door speed per tick
        private const int HoldTicks = 50;       // about 1.5 seconds with a 30 ms timer
        private const int MinimumHoldTicks = 10;

        private Phase phase = Phase.Opening;
        private int heldFor = 0;

        public override string Name
        {
            get { return "Door open"; }
        }

        public override void OnEnter(Elevator elevator)
        {
            elevator.Direction = Direction.None;
            phase = Phase.Opening;
            heldFor = 0;
            elevator.Announce(ElevatorEventType.DoorOpened,
                "Door of Elevator " + elevator.Id + " opened at floor " + elevator.CurrentFloor);
        }

        public override void HandleRequest(Elevator elevator, int floor)
        {
            if (floor == elevator.CurrentFloor)
            {
                // Someone pressed the button for the floor we are on: keep the door open longer
                elevator.CompleteRequest(floor);
                heldFor = 0;
                if (phase == Phase.Closing)
                {
                    phase = Phase.Opening;
                }
            }
            else
            {
                elevator.AddDestination(floor);
            }
        }

        public override void Tick(Elevator elevator)
        {
            switch (phase)
            {
                case Phase.Opening:
                    elevator.ChangeDoor(DoorStep);
                    if (elevator.DoorOpenAmount >= 1)
                    {
                        phase = Phase.Holding;
                    }
                    break;

                case Phase.Holding:
                    heldFor++;
                    // Close sooner if a passenger already chose another floor
                    bool someoneWaiting = elevator.HasPendingDestinations && heldFor >= MinimumHoldTicks;
                    if (heldFor >= HoldTicks || someoneWaiting)
                    {
                        phase = Phase.Closing;
                    }
                    break;

                case Phase.Closing:
                    elevator.ChangeDoor(-DoorStep);
                    if (elevator.DoorOpenAmount <= 0)
                    {
                        elevator.Announce(ElevatorEventType.DoorClosed,
                            "Door of Elevator " + elevator.Id + " closed at floor " + elevator.CurrentFloor);
                        elevator.ChangeState(new IdleState());
                    }
                    break;
            }
        }
    }
}
