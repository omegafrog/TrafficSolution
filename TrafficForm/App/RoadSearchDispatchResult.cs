using System.Text.Json;
using TrafficForm.Domain;

namespace TrafficForm.App
{
    public sealed class RoadSearchDispatchResult
    {
        public required CurrentMode Mode { get; init; }

        public required RoadNameCandidate Candidate { get; init; }

        public required MapBounds Bounds { get; init; }

        public bool IsTrafficMode => Mode == CurrentMode.Traffic;

        public bool IsCctvMode => Mode == CurrentMode.Cctv;

        public string CreateSelectionMessage()
        {
            return JsonSerializer.Serialize(new
            {
                type = "pos-selected",
                lat = Candidate.Latitude,
                lon = Candidate.Longitude,
                minLon = Bounds.MinLongitude,
                minLat = Bounds.MinLatitude,
                maxLon = Bounds.MaxLongitude,
                maxLat = Bounds.MaxLatitude
            });
        }
    }
}
