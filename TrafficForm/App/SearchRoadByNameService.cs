using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.App
{
    public class SearchRoadByNameService
    {
        private readonly IRoadNameHighwaySearchPort _roadNameHighwaySearchPort;
        private readonly IRoadNameQueryExpanderPort _roadNameQueryExpanderPort;

        public SearchRoadByNameService(
            IRoadNameHighwaySearchPort roadNameHighwaySearchPort,
            IRoadNameQueryExpanderPort roadNameQueryExpanderPort)
        {
            _roadNameHighwaySearchPort = roadNameHighwaySearchPort ?? throw new ArgumentNullException(nameof(roadNameHighwaySearchPort));
            _roadNameQueryExpanderPort = roadNameQueryExpanderPort ?? throw new ArgumentNullException(nameof(roadNameQueryExpanderPort));
        }

        public async Task<RoadNameSearchResult> SearchAsync(SearchRoadByNameCommand command)
        {
            if (command == null)
            {
                throw new RequiredCommandNotFoundException("도로명 검색 조건이 필요합니다.");
            }

            command.NormalizeBounds();

            if (string.IsNullOrWhiteSpace(command.Query))
            {
                return CreateEmptyResult();
            }

            IReadOnlyList<string> expandedQueries = _roadNameQueryExpanderPort
                .Expand(command.Query)
                .Where(query => !string.IsNullOrWhiteSpace(query))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (expandedQueries.Count == 0)
            {
                return CreateEmptyResult();
            }

            IReadOnlyList<HighWay> exactMatches = await _roadNameHighwaySearchPort.SearchExactAsync(
                expandedQueries,
                command.MinLongitude,
                command.MinLatitude,
                command.MaxLongitude,
                command.MaxLatitude);

            if (exactMatches.Count > 0)
            {
                return new RoadNameSearchResult
                {
                    Highways = DistinctHighways(exactMatches),
                    MatchKind = RoadNameMatchKind.Exact
                };
            }

            IReadOnlyList<HighWay> partialMatches = await _roadNameHighwaySearchPort.SearchPartialAsync(
                expandedQueries,
                command.MinLongitude,
                command.MinLatitude,
                command.MaxLongitude,
                command.MaxLatitude);

            if (partialMatches.Count == 0)
            {
                return CreateEmptyResult();
            }

            return new RoadNameSearchResult
            {
                Highways = DistinctHighways(partialMatches),
                MatchKind = RoadNameMatchKind.Partial
            };
        }

        private static RoadNameSearchResult CreateEmptyResult()
        {
            return new RoadNameSearchResult();
        }

        private static IReadOnlyList<HighWay> DistinctHighways(IReadOnlyList<HighWay> highways)
        {
            return highways
                .GroupBy(highway => highway.ReferenceNumber, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
        }
    }
}
