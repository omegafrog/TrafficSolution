using TrafficForm.Domain;
using TrafficForm.Port;
using System.Text;

namespace TrafficForm.App
{
    public class RequestCctvByPosService
    {
        private const double CctvHighwayProximityThresholdKm = 1.0;
        private const double HighwayNameSimilarityThreshold = 0.45;
        private static readonly string[] HighwayNameNoiseKeywords =
        {
            "고속도로",
            "고속",
            "도로",
            "본선",
            "지선",
            "순환",
            "방향",
            "상행",
            "하행",
            "양방향",
            "구간",
            "선",
            "cctv",
            "camera",
            "cam",
            "tg",
            "ic",
            "jc",
            "jct",
            "휴게소",
            "입구",
            "출구",
            "부근"
        };

        private readonly IOpenStreetQueryPort _openStreetQueryPort;
        private readonly IPublicTrafficApiPort _publicTrafficApiPort;
        private readonly ICctvApiPort _cctvApiPort;

        public RequestCctvByPosService(
            IOpenStreetQueryPort openStreetQueryPort,
            IPublicTrafficApiPort publicTrafficApiPort,
            ICctvApiPort cctvApiPort)
        {
            _openStreetQueryPort = openStreetQueryPort ?? throw new ArgumentNullException(nameof(openStreetQueryPort));
            _publicTrafficApiPort = publicTrafficApiPort ?? throw new ArgumentNullException(nameof(publicTrafficApiPort));
            _cctvApiPort = cctvApiPort ?? throw new ArgumentNullException(nameof(cctvApiPort));
        }

        public async Task<HighwayCctvSelection> GetNearbyHighwayCctv(UpdateSelectedPosCctvInfoCommand command)
        {
            ValidateSelectedPoint(command);
            command.NormalizeBounds();

            Dictionary<int, HighWay> adjacentHighways = await _openStreetQueryPort.GetAdjacentHighWays(new Location
            {
                Latitude = command.Latitude,
                Longitude = command.Longitude
            });

            if (adjacentHighways.Count == 0)
            {
                throw new NoAdjacentHighWayException("선택 좌표 인근에서 고속도로를 찾지 못했습니다.");
            }

            Dictionary<int, List<VdsTrafficResult>> trafficByHighway = await LoadTrafficByHighway(adjacentHighways.Keys, command);
            int fallbackHighwayNo = adjacentHighways.Keys.First();
            int selectedHighwayNo = SelectClosestHighwayNo(command, trafficByHighway, fallbackHighwayNo);
            string selectedHighwayName = adjacentHighways[selectedHighwayNo].Name;

            List<VdsTrafficResult> selectedHighwayVds = trafficByHighway.TryGetValue(selectedHighwayNo, out List<VdsTrafficResult>? selectedTraffic)
                ? selectedTraffic
                : new List<VdsTrafficResult>();

            List<CctvInfo> cctvCandidates = await _cctvApiPort.GetCctvInfo(
                command.MinLongitude,
                command.MinLatitude,
                command.MaxLongitude,
                command.MaxLatitude);

            List<CctvInfo> filteredCctvInfos = FilterByHighwayProximityAndNameSimilarity(
                selectedHighwayVds,
                cctvCandidates,
                selectedHighwayName);
            foreach (CctvInfo cctvInfo in filteredCctvInfos)
            {
                cctvInfo.HighwayNo = selectedHighwayNo;
                cctvInfo.HighwayName = selectedHighwayName;
            }

            return new HighwayCctvSelection
            {
                HighwayNo = selectedHighwayNo,
                HighwayName = selectedHighwayName,
                CctvInfos = filteredCctvInfos
            };
        }

        private static void ValidateSelectedPoint(UpdateSelectedPosCctvInfoCommand command)
        {
            if (command == null)
            {
                throw new RequiredCommandNotFoundException("좌표를 선택해야 합니다.");
            }

            if (command.Latitude < UpdateSelectedPosCctvInfoCommand.MIN_LATITUDE
                || command.Latitude > UpdateSelectedPosCctvInfoCommand.MAX_LATITUDE
                || command.Longitude < UpdateSelectedPosCctvInfoCommand.MIN_LONGITUDE
                || command.Longitude > UpdateSelectedPosCctvInfoCommand.MAX_LONGITUDE)
            {
                throw new PointOutOfRangeException("좌표가 유효범위를 넘었습니다.");
            }
        }

        private async Task<Dictionary<int, List<VdsTrafficResult>>> LoadTrafficByHighway(
            IEnumerable<int> highwayNumbers,
            UpdateSelectedPosCctvInfoCommand command)
        {
            Dictionary<int, List<VdsTrafficResult>> result = new Dictionary<int, List<VdsTrafficResult>>();

            foreach (int highwayNo in highwayNumbers)
            {
                List<VdsTrafficResult> trafficResults = await _publicTrafficApiPort.GetTrafficResult(
                    highwayNo,
                    command.MinLongitude,
                    command.MinLatitude,
                    command.MaxLongitude,
                    command.MaxLatitude);

                List<VdsTrafficResult> withLocations = trafficResults
                    .Where(item => item.Location != null)
                    .ToList();

                result[highwayNo] = withLocations;
            }

            return result;
        }

        private static int SelectClosestHighwayNo(
            UpdateSelectedPosCctvInfoCommand command,
            Dictionary<int, List<VdsTrafficResult>> trafficByHighway,
            int fallbackHighwayNo)
        {
            int selectedHighwayNo = fallbackHighwayNo;
            double nearestDistance = double.MaxValue;

            foreach ((int highwayNo, List<VdsTrafficResult> trafficResults) in trafficByHighway)
            {
                foreach (VdsTrafficResult trafficResult in trafficResults)
                {
                    double distance = Util.DistanceKm(
                        command.Latitude,
                        command.Longitude,
                        trafficResult.Location.Latitude,
                        trafficResult.Location.Longitude);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        selectedHighwayNo = highwayNo;
                    }
                }
            }

            return selectedHighwayNo;
        }

        private static List<CctvInfo> FilterByHighwayProximityAndNameSimilarity(
            List<VdsTrafficResult> selectedHighwayVds,
            List<CctvInfo> cctvCandidates,
            string selectedHighwayName)
        {
            if (cctvCandidates.Count == 0 || selectedHighwayVds.Count == 0)
            {
                return new List<CctvInfo>();
            }

            string normalizedHighwayName = NormalizeRoadName(selectedHighwayName);
            bool shouldApplyNameSimilarity =
                normalizedHighwayName.Length >= 2
                && !string.Equals(normalizedHighwayName, "이름없음", StringComparison.Ordinal);

            return cctvCandidates
                .Select(cctvInfo =>
                {
                    double distanceKm = FindNearestDistanceKm(cctvInfo, selectedHighwayVds);
                    bool isNameSimilar = !shouldApplyNameSimilarity
                        || IsHighwayNameSimilar(normalizedHighwayName, cctvInfo);

                    return (CctvInfo: cctvInfo, DistanceKm: distanceKm, IsNameSimilar: isNameSimilar);
                })
                .Where(item => item.DistanceKm <= CctvHighwayProximityThresholdKm && item.IsNameSimilar)
                .OrderBy(item => item.DistanceKm)
                .Select(item => item.CctvInfo)
                .ToList();
        }

        private static bool IsHighwayNameSimilar(string normalizedHighwayName, CctvInfo cctvInfo)
        {
            string normalizedCctvName = NormalizeRoadName($"{cctvInfo.Name} {cctvInfo.RoadSectionId}");
            if (normalizedCctvName.Length < 2)
            {
                return false;
            }

            if (normalizedCctvName.Contains(normalizedHighwayName, StringComparison.Ordinal)
                || normalizedHighwayName.Contains(normalizedCctvName, StringComparison.Ordinal))
            {
                return true;
            }

            double similarity = CalculateDiceCoefficient(normalizedHighwayName, normalizedCctvName);
            return similarity >= HighwayNameSimilarityThreshold;
        }

        private static string NormalizeRoadName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                    continue;
                }

                if (character >= '\uAC00' && character <= '\uD7A3')
                {
                    builder.Append(character);
                }
            }

            string normalized = builder.ToString();
            foreach (string noiseKeyword in HighwayNameNoiseKeywords)
            {
                normalized = normalized.Replace(noiseKeyword, string.Empty, StringComparison.Ordinal);
            }

            return normalized;
        }

        private static double CalculateDiceCoefficient(string left, string right)
        {
            if (left.Length == 0 || right.Length == 0)
            {
                return 0;
            }

            if (string.Equals(left, right, StringComparison.Ordinal))
            {
                return 1;
            }

            if (left.Length < 2 || right.Length < 2)
            {
                return 0;
            }

            Dictionary<string, int> leftBigrams = BuildBigrams(left);
            Dictionary<string, int> rightBigrams = BuildBigrams(right);

            int overlapCount = 0;
            foreach ((string bigram, int count) in leftBigrams)
            {
                if (rightBigrams.TryGetValue(bigram, out int rightCount))
                {
                    overlapCount += Math.Min(count, rightCount);
                }
            }

            int leftBigramCount = left.Length - 1;
            int rightBigramCount = right.Length - 1;
            return (2.0 * overlapCount) / (leftBigramCount + rightBigramCount);
        }

        private static Dictionary<string, int> BuildBigrams(string value)
        {
            Dictionary<string, int> bigrams = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int index = 0; index < value.Length - 1; index++)
            {
                string bigram = value.Substring(index, 2);
                if (bigrams.TryGetValue(bigram, out int count))
                {
                    bigrams[bigram] = count + 1;
                    continue;
                }

                bigrams[bigram] = 1;
            }

            return bigrams;
        }

        private static double FindNearestDistanceKm(CctvInfo cctvInfo, List<VdsTrafficResult> selectedHighwayVds)
        {
            double nearestDistance = double.MaxValue;

            foreach (VdsTrafficResult trafficResult in selectedHighwayVds)
            {
                double distance = Util.DistanceKm(
                    cctvInfo.Location.Latitude,
                    cctvInfo.Location.Longitude,
                    trafficResult.Location.Latitude,
                    trafficResult.Location.Longitude);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                }
            }

            return nearestDistance;
        }
    }
}
