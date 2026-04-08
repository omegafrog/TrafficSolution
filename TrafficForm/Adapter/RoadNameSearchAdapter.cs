using System.Diagnostics;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    internal sealed class RoadNameSearchAdapter : IRoadNameSearchPort
    {
        private readonly OpenStreetDbRepository _repository;

        public RoadNameSearchAdapter(OpenStreetDbRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RoadNameCandidate?> ResolveRoadNameAsync(string roadName, MapBounds bounds)
        {
            List<RoadNameCandidate> candidates = await _repository.findRoadNameCandidates(
                roadName,
                bounds.MinLatitude,
                bounds.MinLongitude,
                bounds.MaxLatitude,
                bounds.MaxLongitude);

            Debug.WriteLine(
                $"[RoadNameSearchAdapter] roadName='{roadName}', bounds=({bounds.MinLongitude}, {bounds.MinLatitude})-({bounds.MaxLongitude}, {bounds.MaxLatitude}), candidateCount={candidates.Count}");

            foreach (RoadNameCandidate candidate in candidates.Take(10))
            {
                Debug.WriteLine(
                    $"[RoadNameSearchAdapter] candidate highwayNo={candidate.HighwayNo}, ref='{candidate.ReferenceNumber}', name='{candidate.HighwayName}', lat={candidate.Latitude}, lon={candidate.Longitude}, distanceM={candidate.DistanceMeters}");
            }

            return RoadNameResolutionSelector.SelectBestCandidate(candidates);
        }
    }
}
