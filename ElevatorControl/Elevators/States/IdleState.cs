using ElevatorControl.Models;

namespace ElevatorControl.Elevators.States
{
    // The elevator is standing still with the door closed, waiting for work.
    public class IdleState : ElevatorStateBase
    {
        public override string Name
        {
            get { return "Idle"; }
        }

        public override void OnEnter(Elevator elevator)
        {
            elevator.Direction = Direction.None;
        }

        public override void Tick(Elevator elevator)
        {
            if (!elevator.HasPendingDestinations)
            {
                return;
            }

            int next = elevator.NextDestination;

            if (next == elevator.CurrentFloor)
            {
                // Already on that floor: just open the door
                elevator.CompleteRequest(next);
                elevator.ChangeState(new DoorOpenState());
            }
            else if (next > elevator.CurrentFloor)
            {
                elevator.ChangeState(new MovingUpState());
            }
            else
            {
                elevator.ChangeState(new MovingDownState());
            }
        }
    }
}
