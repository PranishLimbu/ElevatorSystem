using ElevatorControl.Models;

namespace ElevatorControl.Elevators.States
{
    public class MovingDownState : MovingState
    {
        public override string Name { get { return "Moving down"; } }
        protected override int Step { get { return -1; } }
        protected override Direction MoveDirection { get { return Direction.Down; } }
        protected override string Word { get { return "down"; } }
    }
}
