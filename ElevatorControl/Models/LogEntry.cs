namespace ElevatorControl.Models
{
    // A simple data class (model) that holds one row of the log
    public class LogEntry
    {
        public int ElevatorId { get; set; }
        public string EventType { get; set; }
        public int FloorNumber { get; set; }
        public string Details { get; set; }
        public DateTime LogTime { get; set; }

        public LogEntry(int elevatorId, string eventType, int floorNumber, string details, DateTime logTime)
        {
            ElevatorId = elevatorId;
            EventType = eventType;
            FloorNumber = floorNumber;
            Details = details;
            LogTime = logTime;
        }
    }
}
