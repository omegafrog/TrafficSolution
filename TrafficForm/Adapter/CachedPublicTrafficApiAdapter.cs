using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public sealed class CachedPublicTrafficApiAdapter : IPublicTrafficApiPort
    {
        private static readonly TimeSpan ColdStartTimeout = TimeSpan.FromSeconds(8);

        private readonly VdsTrafficSnapshotStore _store;
        private readonly IVdsTrafficSnapshotRefresherPort _refresher;
        private readonly IVdsGeoRepositoryPort _geoRepository;

        public CachedPublicTrafficApiAdapter(
            VdsTrafficSnapshotStore store,
            IVdsTrafficSnapshotRefresherPort refresher,
            IVdsGeoRepositoryPort geoRepository)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _refresher = refresher ?? throw new ArgumentNullException(nameof(refresher));
            _geoRepository = geoRepository ?? throw new ArgumentNullException(nameof(geoRepository));
        }

        public async Task<List<VdsTrafficResult>> GetTrafficResult(
            int highwayNo,
            double minLongitude,
            double minLatitude,
            double maxLongitude,
            double maxLatitude)
        {
            VdsTrafficSnapshot snapshot = await GetSnapshotAsync().ConfigureAwait(false);

            Dictionary<string, Tuple<double, double>> rawVdsLoc = await _geoRepository
                .findVdsIdIn(highwayNo * 10, minLatitude, minLongitude, maxLatitude, maxLongitude)
                .ConfigureAwait(false);
            Dictionary<string, Tuple<double, double>> vdsLoc =
                new Dictionary<string, Tuple<double, double>>(rawVdsLoc, StringComparer.Ordinal);

            List<VdsTrafficResult> filteredResults = new List<VdsTrafficResult>();
            foreach (KeyValuePair<string, Tuple<double, double>> entry in vdsLoc)
            {
                if (!snapshot.ByVdsId.TryGetValue(entry.Key, out VdsTrafficObservation? observation))
                {
                    continue;
                }

                Tuple<double, double> coordinate = entry.Value;
                VdsTrafficResult result = new VdsTrafficResult
                {
                    VdsId = observation.VdsId,
                    CollectedDate = observation.CollectedDate,
                    Speed = observation.Speed,
                    Volume = observation.Volume,
                    Occupancy = observation.Occupancy,
                    Location = new Location
                    {
                        Latitude = coordinate.Item1,
                        Longitude = coordinate.Item2
                    }
                };

                result.TrafficLevel = TrafficLevelPolicy.CalculateTrafficLevel(result);
                filteredResults.Add(result);
            }

            Dictionary<string, List<Location>> rawSegments = await _geoRepository
                .findResponsibilitySegments(highwayNo * 10, filteredResults.Select(result => result.VdsId))
                .ConfigureAwait(false);
            Dictionary<string, List<Location>> segmentByVdsId =
                new Dictionary<string, List<Location>>(rawSegments, StringComparer.Ordinal);

            foreach (VdsTrafficResult trafficResult in filteredResults)
            {
                if (segmentByVdsId.TryGetValue(trafficResult.VdsId, out List<Location>? segmentPoints))
                {
                    trafficResult.ResponsibilitySegment = new List<Location>(segmentPoints);
                }
            }

            return filteredResults;
        }

        public async Task<List<Location>> findAllVdiLoc()
        {
            return await _geoRepository.findAllVdsLoc().ConfigureAwait(false);
        }

        private async Task<VdsTrafficSnapshot> GetSnapshotAsync()
        {
            VdsTrafficSnapshot snapshot = _store.GetCurrent();
            if (snapshot.HasData)
            {
                return snapshot;
            }

            using CancellationTokenSource cts = new CancellationTokenSource(ColdStartTimeout);
            try
            {
                await _refresher.RefreshOnceAsync(cts.Token).ConfigureAwait(false);
            }
            catch (TrafficResultRequestFailedException)
            {
                snapshot = _store.GetCurrent();
                if (!snapshot.HasData)
                {
                    throw;
                }

                return snapshot;
            }
            catch (Exception exception)
            {
                snapshot = _store.GetCurrent();
                if (!snapshot.HasData)
                {
                    throw new TrafficResultRequestFailedException("캐시된 교통량 스냅샷이 비어 있습니다.", exception);
                }

                return snapshot;
            }

            snapshot = _store.GetCurrent();
            if (!snapshot.HasData)
            {
                throw new TrafficResultRequestFailedException("캐시된 교통량 스냅샷이 비어 있습니다.");
            }

            return snapshot;
        }
    }
}
