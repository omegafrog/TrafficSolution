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

            return RoadNameResolutionSelector.SelectBestCandidate(candidates);
        }
    }
}
