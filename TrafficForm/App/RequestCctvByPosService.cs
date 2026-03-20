using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.App
{
    public class RequestCctvByPosService
    {
        private const double CctvHighwayProximityThresholdKm = 2.0;
        private const int FallbackCctvCount = 40;

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

            List<CctvInfo> filteredCctvInfos = FilterByHighwayProximity(selectedHighwayVds, cctvCandidates);
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

        private static List<CctvInfo> FilterByHighwayProximity(List<VdsTrafficResult> selectedHighwayVds, List<CctvInfo> cctvCandidates)
        {
            if (cctvCandidates.Count == 0)
            {
                return new List<CctvInfo>();
            }

            if (selectedHighwayVds.Count == 0)
            {
                return cctvCandidates;
            }

            List<(CctvInfo CctvInfo, double DistanceKm)> projected = cctvCandidates
                .Select(cctvInfo => (cctvInfo, FindNearestDistanceKm(cctvInfo, selectedHighwayVds)))
                .OrderBy(item => item.Item2)
                .ToList();

            List<CctvInfo> withinThreshold = projected
                .Where(item => item.DistanceKm <= CctvHighwayProximityThresholdKm)
                .Select(item => item.CctvInfo)
                .ToList();

            if (withinThreshold.Count > 0)
            {
                return withinThreshold;
            }

            int takeCount = Math.Min(FallbackCctvCount, projected.Count);
            return projected.Take(takeCount).Select(item => item.CctvInfo).ToList();
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
