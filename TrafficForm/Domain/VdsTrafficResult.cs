namespace TrafficForm.Domain
{
    public class VdsTrafficResult
    {
        public required string VdsId { get; set; }
        public string CollectedDate { get;  set; } = string.Empty;
        public double Speed { get;  set; }
        public int Volume { get;  set; }
        public double Occupancy { get;  set; }
        public Location Location { get; set; } = new Location();
        public TrafficLevel TrafficLevel { get; set; } = TrafficLevel.Unknown;
        public List<Location> ResponsibilitySegment { get; set; } = new List<Location>();
    }
}
