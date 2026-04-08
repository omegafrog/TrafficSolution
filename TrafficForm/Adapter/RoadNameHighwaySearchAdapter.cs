using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public class RoadNameHighwaySearchAdapter : IRoadNameHighwaySearchPort
    {
        private static readonly string[] RoadNameNoiseKeywords =
        {
            "고속도로",
            "고속",
            "도로",
            "본선",
            "지선"
        };

        private readonly VdsRepository _vdsRepository;

        public RoadNameHighwaySearchAdapter(VdsRepository vdsRepository)
        {
            _vdsRepository = vdsRepository ?? throw new ArgumentNullException(nameof(vdsRepository));
        }

        public async Task<IReadOnlyList<HighWay>> SearchExactAsync(
            IReadOnlyList<string> normalizedQueries,
            double minLongitude,
            double minLatitude,
            double maxLongitude,
            double maxLatitude)
        {
            IReadOnlyList<HighWay> candidates = await _vdsRepository.FindDistinctHighwaysInBoundsAsync(
                minLongitude,
                minLatitude,
                maxLongitude,
                maxLatitude);

            HashSet<string> querySet = normalizedQueries
                .Where(query => !string.IsNullOrWhiteSpace(query))
                .Select(DefaultRoadNameQueryExpanderAdapter.Normalize)
                .ToHashSet(StringComparer.Ordinal);

            return candidates
                .Where(candidate => querySet.Contains(DefaultRoadNameQueryExpanderAdapter.Normalize(candidate.Name)))
                .ToArray();
        }

        public async Task<IReadOnlyList<HighWay>> SearchPartialAsync(
            IReadOnlyList<string> normalizedQueries,
            double minLongitude,
            double minLatitude,
            double maxLongitude,
            double maxLatitude)
        {
            IReadOnlyList<HighWay> candidates = await _vdsRepository.FindDistinctHighwaysInBoundsAsync(
                minLongitude,
                minLatitude,
                maxLongitude,
                maxLatitude);

            string[] normalizedQueryValues = normalizedQueries
                .Where(query => !string.IsNullOrWhiteSpace(query))
                .Select(DefaultRoadNameQueryExpanderAdapter.Normalize)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            return candidates
                .Where(candidate => MatchesPartial(normalizedQueryValues, candidate.Name))
                .ToArray();
        }

        private static bool MatchesPartial(IEnumerable<string> queries, string roadName)
        {
            string normalizedRoadName = DefaultRoadNameQueryExpanderAdapter.Normalize(roadName);
            string compactRoadName = normalizedRoadName.Replace(" ", string.Empty, StringComparison.Ordinal);
            string coreRoadName = NormalizeCoreRoadName(roadName);

            foreach (string query in queries)
            {
                string compactQuery = query.Replace(" ", string.Empty, StringComparison.Ordinal);
                if (normalizedRoadName.Contains(query, StringComparison.Ordinal)
                    || compactRoadName.Contains(compactQuery, StringComparison.Ordinal)
                    || coreRoadName.Contains(compactQuery, StringComparison.Ordinal)
                    || compactQuery.Contains(coreRoadName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeCoreRoadName(string roadName)
        {
            string normalized = DefaultRoadNameQueryExpanderAdapter.Normalize(roadName)
                .Replace(" ", string.Empty, StringComparison.Ordinal);

            foreach (string keyword in RoadNameNoiseKeywords)
            {
                normalized = normalized.Replace(keyword, string.Empty, StringComparison.Ordinal);
            }

            return normalized;
        }
    }
}
