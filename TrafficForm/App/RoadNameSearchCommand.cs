using TrafficForm.Domain;

namespace TrafficForm.App
{
    public sealed class RoadNameSearchCommand
    {
        public RoadNameSearchCommand(string roadName, MapBounds bounds, CurrentMode mode)
        {
            RoadName = roadName;
            Bounds = bounds;
            Mode = mode;
        }

        public string RoadName { get; set; }

        public MapBounds Bounds { get; set; }

        public CurrentMode Mode { get; set; }
    }
}
