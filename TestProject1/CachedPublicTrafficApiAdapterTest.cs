using Moq;
using TrafficForm.Adapter;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1
{
    [TestClass]
    public sealed class CachedPublicTrafficApiAdapterTest
    {
        [TestMethod]
        public async Task GetTrafficResult_WhenSnapshotHasData_DoesNotRefreshAndMaterializesResults()
        {
            const int highwayNo = 1;
            const double minLongitude = 126.8;
            const double minLatitude = 37.3;
            const double maxLongitude = 127.2;
            const double maxLatitude = 37.7;

            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            store.Swap(CreateSnapshot("VDS-1"));

            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            geoRepository
                .Setup(repo => repo.findVdsIdIn(
                    highwayNo * 10,
                    minLatitude,
                    minLongitude,
                    maxLatitude,
                    maxLongitude))
                .ReturnsAsync(new Dictionary<string, Tuple<double, double>>(StringComparer.Ordinal)
                {
                    ["VDS-1"] = new Tuple<double, double>(37.51, 127.01)
                });
            geoRepository
                .Setup(repo => repo.findResponsibilitySegments(
                    highwayNo * 10,
                    It.Is<IEnumerable<string>>(ids => ids.Contains("VDS-1"))))
                .ReturnsAsync(new Dictionary<string, List<Location>>(StringComparer.Ordinal)
                {
                    ["VDS-1"] = new List<Location>
                    {
                        new Location { Latitude = 37.5, Longitude = 127.0 }
                    }
                });

            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();

            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            List<VdsTrafficResult> results1 = await adapter.GetTrafficResult(
                highwayNo,
                minLongitude,
                minLatitude,
                maxLongitude,
                maxLatitude);
            List<VdsTrafficResult> results2 = await adapter.GetTrafficResult(
                highwayNo,
                minLongitude,
                minLatitude,
                maxLongitude,
                maxLatitude);

            Assert.HasCount(1, results1);
            Assert.AreEqual("VDS-1", results1[0].VdsId);
            Assert.AreEqual(37.51, results1[0].Location.Latitude);
            Assert.AreEqual(127.01, results1[0].Location.Longitude);
            Assert.AreEqual(TrafficLevel.Smooth, results1[0].TrafficLevel);
            Assert.HasCount(1, results1[0].ResponsibilitySegment);
            Assert.IsFalse(ReferenceEquals(results1[0], results2[0]));

            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetTrafficResult_WhenSnapshotEmptyAndStillEmpty_Throws()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();
            refresher
                .Setup(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            await Assert.ThrowsAsync<TrafficResultRequestFailedException>(() => adapter.GetTrafficResult(
                1,
                0,
                0,
                0,
                0));

            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrafficResult_WhenSnapshotEmpty_RefreshesAndUsesNewSnapshot()
        {
            const int highwayNo = 1;

            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();
            refresher
                .Setup(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()))
                .Callback(() => store.Swap(CreateSnapshot("VDS-9")))
                .Returns(Task.CompletedTask);

            geoRepository
                .Setup(repo => repo.findVdsIdIn(
                    highwayNo * 10,
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<double>()))
                .ReturnsAsync(new Dictionary<string, Tuple<double, double>>(StringComparer.Ordinal)
                {
                    ["VDS-9"] = new Tuple<double, double>(37.0, 127.0)
                });
            geoRepository
                .Setup(repo => repo.findResponsibilitySegments(
                    highwayNo * 10,
                    It.Is<IEnumerable<string>>(ids => ids.Contains("VDS-9"))))
                .ReturnsAsync(new Dictionary<string, List<Location>>(StringComparer.Ordinal)
                {
                    ["VDS-9"] = new List<Location>()
                });

            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            List<VdsTrafficResult> results = await adapter.GetTrafficResult(
                highwayNo,
                0,
                0,
                0,
                0);

            Assert.HasCount(1, results);
            Assert.AreEqual("VDS-9", results[0].VdsId);
            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrafficResult_WhenRefreshCanceledAndSnapshotStillEmpty_Throws()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();
            refresher
                .Setup(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            await Assert.ThrowsAsync<TrafficResultRequestFailedException>(() => adapter.GetTrafficResult(
                1,
                0,
                0,
                0,
                0));

            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrafficResult_WhenRefreshFailsAndSnapshotStillEmpty_Throws()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();
            refresher
                .Setup(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            await Assert.ThrowsAsync<TrafficResultRequestFailedException>(() => adapter.GetTrafficResult(
                1,
                0,
                0,
                0,
                0));

            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrafficResult_WhenRefreshFailsWithTrafficResultExceptionAndSnapshotEmpty_Rethrows()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();
            TrafficResultRequestFailedException expected =
                new TrafficResultRequestFailedException("원본 예외 메시지 유지");
            refresher
                .Setup(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            TrafficResultRequestFailedException exception =
                await Assert.ThrowsAsync<TrafficResultRequestFailedException>(() => adapter.GetTrafficResult(
                    1,
                    0,
                    0,
                    0,
                    0));

            Assert.AreSame(expected, exception);
            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrafficResult_WhenRefreshFailsButSnapshotPopulated_ReturnsResults()
        {
            const int highwayNo = 1;

            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();
            refresher
                .Setup(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()))
                .Callback(() => store.Swap(CreateSnapshot("VDS-9")))
                .ThrowsAsync(new InvalidOperationException("boom"));

            geoRepository
                .Setup(repo => repo.findVdsIdIn(
                    highwayNo * 10,
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<double>()))
                .ReturnsAsync(new Dictionary<string, Tuple<double, double>>(StringComparer.Ordinal)
                {
                    ["VDS-9"] = new Tuple<double, double>(37.0, 127.0)
                });
            geoRepository
                .Setup(repo => repo.findResponsibilitySegments(
                    highwayNo * 10,
                    It.Is<IEnumerable<string>>(ids => ids.Contains("VDS-9"))))
                .ReturnsAsync(new Dictionary<string, List<Location>>(StringComparer.Ordinal)
                {
                    ["VDS-9"] = new List<Location>()
                });

            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            List<VdsTrafficResult> results = await adapter.GetTrafficResult(
                highwayNo,
                0,
                0,
                0,
                0);

            Assert.HasCount(1, results);
            Assert.AreEqual("VDS-9", results[0].VdsId);
            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrafficResult_ConcurrentRequestsWithSnapshot_DoesNotThrow()
        {
            const int highwayNo = 1;
            const double minLongitude = 126.8;
            const double minLatitude = 37.3;
            const double maxLongitude = 127.2;
            const double maxLatitude = 37.7;

            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            store.Swap(CreateSnapshot("VDS-1"));

            Mock<IVdsGeoRepositoryPort> geoRepository = new Mock<IVdsGeoRepositoryPort>();
            geoRepository
                .Setup(repo => repo.findVdsIdIn(
                    highwayNo * 10,
                    minLatitude,
                    minLongitude,
                    maxLatitude,
                    maxLongitude))
                .ReturnsAsync(new Dictionary<string, Tuple<double, double>>(StringComparer.Ordinal)
                {
                    ["VDS-1"] = new Tuple<double, double>(37.51, 127.01)
                });
            geoRepository
                .Setup(repo => repo.findResponsibilitySegments(
                    highwayNo * 10,
                    It.Is<IEnumerable<string>>(ids => ids.Contains("VDS-1"))))
                .ReturnsAsync(new Dictionary<string, List<Location>>(StringComparer.Ordinal)
                {
                    ["VDS-1"] = new List<Location>
                    {
                        new Location { Latitude = 37.5, Longitude = 127.0 }
                    }
                });

            Mock<IVdsTrafficSnapshotRefresherPort> refresher = new Mock<IVdsTrafficSnapshotRefresherPort>();
            CachedPublicTrafficApiAdapter adapter = new CachedPublicTrafficApiAdapter(
                store,
                refresher.Object,
                geoRepository.Object);

            int requestCount = Math.Max(4, Environment.ProcessorCount * 2);
            Task<List<VdsTrafficResult>>[] tasks = new Task<List<VdsTrafficResult>>[requestCount];
            for (int i = 0; i < requestCount; i++)
            {
                tasks[i] = adapter.GetTrafficResult(
                    highwayNo,
                    minLongitude,
                    minLatitude,
                    maxLongitude,
                    maxLatitude);
            }

            List<VdsTrafficResult>[] results = await Task.WhenAll(tasks);

            Assert.HasCount(requestCount, results);
            foreach (List<VdsTrafficResult> result in results)
            {
                Assert.HasCount(1, result);
                Assert.AreEqual("VDS-1", result[0].VdsId);
            }

            refresher.Verify(r => r.RefreshOnceAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static VdsTrafficSnapshot CreateSnapshot(string vdsId)
        {
            Dictionary<string, VdsTrafficObservation> observations =
                new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal)
                {
                    [vdsId] = new VdsTrafficObservation(
                        vdsId,
                        collectedDate: "2000-01-01 00:00:00",
                        speed: 80,
                        volume: 100,
                        occupancy: 10)
                };

            DateTimeOffset now = DateTimeOffset.UtcNow;
            return new VdsTrafficSnapshot(
                Guid.NewGuid(),
                1,
                observations,
                lastSuccessUtc: now,
                lastAttemptUtc: now,
                lastError: null);
        }
    }
}
