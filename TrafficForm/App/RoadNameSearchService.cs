using System.Diagnostics;
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
            Debug.WriteLine(
                $"[RoadNameSearch] request roadName='{roadName}', bounds=({bounds.MinLongitude}, {bounds.MinLatitude})-({bounds.MaxLongitude}, {bounds.MaxLatitude}), mode={command.Mode}");

            RoadNameCandidate? candidate = await _roadNameSearchPort.ResolveRoadNameAsync(roadName, bounds);

            if (candidate == null)
            {
                Debug.WriteLine($"[RoadNameSearch] no candidate found for roadName='{roadName}'");
                throw new InvalidOperationException($"'{roadName}'에 해당하는 도로를 찾지 못했습니다.");
            }

            Debug.WriteLine(
                $"[RoadNameSearch] selected highwayNo={candidate.HighwayNo}, ref='{candidate.ReferenceNumber}', name='{candidate.HighwayName}', lat={candidate.Latitude}, lon={candidate.Longitude}, distanceM={candidate.DistanceMeters}");

            return new RoadSearchDispatchResult
            {
                Mode = command.Mode,
                Candidate = candidate,
                Bounds = bounds
            };
        }
    }
}
