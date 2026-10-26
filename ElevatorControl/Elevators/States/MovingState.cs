using ElevatorControl.Models;

namespace ElevatorControl.Elevators.States
{
    // Abstract parent of MovingUpState and MovingDownState.
    // The moving code is written once here; the children only say which way to go.
    public abstract class MovingState : ElevatorStateBase
    {
        protected abstract int Step { get; }            // +1 = up, -1 = down
        protected abstract Direction MoveDirection { get; }
        protected abstract string Word { get; }

        public override void OnEnter(Elevator elevator)
        {
            elevator.Direction = MoveDirection;
            elevator.Announce(ElevatorEventType.StartedMoving,
                "Elevator " + elevator.Id + " moving " + Word + " from floor " + elevator.CurrentFloor);
        }

        public override void Tick(Elevator elevator)
        {
            if (!elevator.HasPendingDestinations)
            {
                elevator.ChangeState(new IdleState());
                return;
            }

            int target = elevator.NextDestination;
            elevator.MoveBy(Step * Elevator.Speed);

            bool reached = Step > 0
                ? elevator.Position >= target - 0.0001
                : elevator.Position <= target + 0.0001;

            if (reached)
            {
                elevator.SnapToFloor(target);
                elevator.CompleteRequest(target);
                elevator.Announce(ElevatorEventType.Arrived,
                    "Elevator " + elevator.Id + " arrived at floor " + target);
                elevator.ChangeState(new DoorOpenState());
            }
        }
    }
}
