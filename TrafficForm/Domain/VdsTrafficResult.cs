using System.Text.Json.Nodes;

namespace TrafficForm.Domain
{
    public class VdsTrafficResult
    {
        public required string VdsId { get; set; }
        public string CollectedDate { get;  set; }
        public double Speed { get;  set; }
        public int Volume { get;  set; }
        public double Occupancy { get;  set; }
        public Location Location { get; set; }
    }
}