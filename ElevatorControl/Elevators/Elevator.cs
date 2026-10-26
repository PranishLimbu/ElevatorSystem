using ElevatorControl.Elevators.Observers;
using ElevatorControl.Elevators.States;
using ElevatorControl.Models;

namespace ElevatorControl.Elevators
{
    // A DELEGATE type. The animation uses it: every timer tick the elevator calls
    // all methods that subscribed to the Animated event (the GUI panel redraws itself).
    public delegate void ElevatorAnimationHandler(Elevator sender);

    // The Elevator class.
    //  - INHERITANCE: it inherits from ElevatorSubject (Observer pattern)
    //  - ENCAPSULATION: the fields are private, other classes use properties and methods
    //  - STATE PATTERN: this is the "context". It holds the current state object and
    //    hands the work (requests and timer ticks) over to it.
    public class Elevator : ElevatorSubject
    {
        // Two floors: 0 and 1. Position is a number from 0.0 to 1.0 so the car can move smoothly.
        public const int TopFloor = 1;
        public const double Speed = 0.02;         // how far the car moves per timer tick
        public const int TicksPerFloor = 50;      // 1.0 / 0.02
        public const int DoorCycleTicks = 90;     // roughly: door opens + stays open + door closes

        private IElevatorState state;
        private readonly List<int> destinations = new List<int>();
        private readonly HashSet<int> panelLights = new HashSet<int>();   // lit buttons INSIDE the car
        private readonly HashSet<int> callLights = new HashSet<int>();    // lit request buttons OUTSIDE

        public event ElevatorAnimationHandler Animated;

        public int Id { get; }
        public double Position { get; private set; }
        public double DoorOpenAmount { get; private set; }   // 0 = closed, 1 = fully open
        public Direction Direction { get; set; }

        public int CurrentFloor
        {
            get { return (int)Math.Round(Position); }
        }

        public DoorState DoorState
        {
            get
            {
                if (DoorOpenAmount <= 0) return DoorState.Closed;
                if (DoorOpenAmount >= 1) return DoorState.Open;
                return DoorState.InTransit;
            }
        }

        public string StateName
        {
            get { return state.Name; }
        }

        public bool HasPendingDestinations
        {
            get { return destinations.Count > 0; }
        }

        public int NextDestination
        {
            get { return destinations[0]; }
        }

        public Elevator(int id)
        {
            Id = id;
            state = new IdleState();
            state.OnEnter(this);
        }

        // ---------- called by the buttons ----------

        public void RequestFloor(int floor, bool fromInside)
        {
            if (floor < 0 || floor > TopFloor)
            {
                throw new ArgumentOutOfRangeException(nameof(floor), "There is no such floor.");
            }

            if (fromInside)
            {
                panelLights.Add(floor);
            }
            else
            {
                callLights.Add(floor);
            }

            string text = fromInside
                ? "Floor button " + floor + " pressed inside Elevator " + Id
                : "Request button on floor " + floor + " answered by Elevator " + Id;
            Announce(ElevatorEventType.ButtonPressed, text);

            // POLYMORPHISM: each state decides in its own way what to do with the request
            state.HandleRequest(this, floor);
        }

        // ---------- called by the timer ----------

        public void Tick()
        {
            state.Tick(this);

            // Only redraw every tick while something is moving
            if (!(state is IdleState))
            {
                Animated?.Invoke(this);
            }
        }

        // ---------- methods used by the state classes ----------

        public void ChangeState(IElevatorState newState)
        {
            state = newState;
            state.OnEnter(this);
            Announce(ElevatorEventType.StateChanged, "Elevator " + Id + " is now: " + state.Name);
        }

        public void Announce(ElevatorEventType type, string message)
        {
            NotifyObservers(this, type, message);
        }

        public void AddDestination(int floor)
        {
            if (!destinations.Contains(floor))
            {
                destinations.Add(floor);
            }
        }

        // The request for this floor is finished: remove it and switch its lights off
        public void CompleteRequest(int floor)
        {
            destinations.Remove(floor);
            panelLights.Remove(floor);
            callLights.Remove(floor);
        }

        public void MoveBy(double amount)
        {
            Position = Math.Clamp(Position + amount, 0, TopFloor);
        }

        public void SnapToFloor(int floor)
        {
            Position = floor;
        }

        public void ChangeDoor(double amount)
        {
            DoorOpenAmount = Math.Clamp(DoorOpenAmount + amount, 0, 1);
        }

        // ---------- information for the GUI and the controller ----------

        public bool PanelLightOn(int floor)
        {
            return panelLights.Contains(floor);
        }

        public bool HasCallFor(int floor)
        {
            return callLights.Contains(floor);
        }

        // Estimated time (in timer ticks) before this elevator could be at 'floor'.
        // It adds up all the jobs the elevator already has, so ongoing tasks are not disturbed.
        public double EstimateResponseTime(int floor)
        {
            double time = 0;
            double position = Position;

            if (state is DoorOpenState)
            {
                time += DoorCycleTicks / 2;   // the door is part way through its cycle
            }

            foreach (int destination in destinations)
            {
                time += Math.Abs(position - destination) * TicksPerFloor + DoorCycleTicks;
                position = destination;
            }

            time += Math.Abs(position - floor) * TicksPerFloor;
            return time;
        }
    }
}
