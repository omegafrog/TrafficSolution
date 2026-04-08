using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IRoadNameSearchPort
    {
        Task<RoadNameCandidate?> ResolveRoadNameAsync(string roadName, MapBounds bounds);
    }
}
