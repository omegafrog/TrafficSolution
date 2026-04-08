using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.App
{
    public sealed class RoadNameSearchService
    {
        private readonly IRoadNameSearchPort _roadNameSearchPort;

        public RoadNameSearchService(IRoadNameSearchPort roadNameSearchPort)
        {
            _roadNameSearchPort = roadNameSearchPort ?? throw new ArgumentNullException(nameof(roadNameSearchPort));
        }

        public async Task<RoadSearchDispatchResult> SearchRoadByNameAsync(RoadNameSearchCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            string roadName = command.RoadName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(roadName))
            {
                throw new ArgumentException("도로명을 입력해야 합니다.", nameof(command));
            }

            if (command.Bounds == null)
            {
                throw new ArgumentNullException(nameof(command.Bounds));
            }

            MapBounds bounds = command.Bounds.Normalize();
            RoadNameCandidate? candidate = await _roadNameSearchPort.ResolveRoadNameAsync(roadName, bounds);

            if (candidate == null)
            {
                throw new InvalidOperationException($"'{roadName}'에 해당하는 도로를 찾지 못했습니다.");
            }

            return new RoadSearchDispatchResult
            {
                Mode = command.Mode,
                Candidate = candidate,
                Bounds = bounds
            };
        }
    }
}
