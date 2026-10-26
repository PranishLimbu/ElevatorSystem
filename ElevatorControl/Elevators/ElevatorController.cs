namespace ElevatorControl.Elevators
{
    // Looks after all the elevators and decides which one answers an OUTSIDE request.
    public class ElevatorController
    {
        private readonly List<Elevator> elevators = new List<Elevator>();

        public IReadOnlyList<Elevator> Elevators
        {
            get { return elevators; }
        }

        public ElevatorController(int numberOfElevators)
        {
            for (int i = 1; i <= numberOfElevators; i++)
            {
                elevators.Add(new Elevator(i));
            }
        }

        // Someone pressed the request button on a floor.
        // The elevator with the shortest estimated response time gets the job.
        public Elevator CallElevator(int floor)
        {
            // If an elevator is already coming for this floor, do nothing
            foreach (Elevator elevator in elevators)
            {
                if (elevator.HasCallFor(floor))
                {
                    return elevator;
                }
            }

            Elevator best = null;
            double bestTime = double.MaxValue;

            foreach (Elevator elevator in elevators)
            {
                double time = elevator.EstimateResponseTime(floor);
                if (time < bestTime)
                {
                    bestTime = time;
                    best = elevator;
                }
            }

            best.RequestFloor(floor, false);
            return best;
        }

        public bool IsFloorCalled(int floor)
        {
            foreach (Elevator elevator in elevators)
            {
                if (elevator.HasCallFor(floor))
                {
                    return true;
                }
            }
            return false;
        }

        // Called by the GUI timer
        public void Tick()
        {
            foreach (Elevator elevator in elevators)
            {
                elevator.Tick();
            }
        }
    }
}
