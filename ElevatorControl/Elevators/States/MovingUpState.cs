using ElevatorControl.Models;

namespace ElevatorControl.Elevators.States
{
    public class MovingUpState : MovingState
    {
        public override string Name { get { return "Moving up"; } }
        protected override int Step { get { return 1; } }
        protected override Direction MoveDirection { get { return Direction.Up; } }
        protected override string Word { get { return "up"; } }
    }
}
