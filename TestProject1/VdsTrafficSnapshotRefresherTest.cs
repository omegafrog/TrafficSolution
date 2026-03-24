using Moq;
using TrafficForm.Adapter;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1
{
    [TestClass]
    public sealed class VdsTrafficSnapshotRefresherTest
    {
        [TestMethod]
        public async Task RefreshOnceAsync_Success_SwapsSnapshotAndUpdatesMetadata()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            VdsTrafficSnapshot initial = CreateSnapshot(7, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-5));
            store.Swap(initial);

            Dictionary<string, VdsTrafficObservation> observations = new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal)
            {
                ["VDS-9"] = CreateObservation("VDS-9")
            };

            Mock<IVdsTrafficSnapshotSourcePort> source = new Mock<IVdsTrafficSnapshotSourcePort>();
            source
                .Setup(s => s.FetchAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(observations);

            VdsTrafficSnapshotRefresher refresher = new VdsTrafficSnapshotRefresher(store, source.Object);

            await refresher.RefreshOnceAsync(CancellationToken.None);

            VdsTrafficSnapshot snapshot = store.GetCurrent();
            Assert.AreEqual(initial.Version + 1, snapshot.Version);
            Assert.AreNotEqual(initial.SnapshotId, snapshot.SnapshotId);
            Assert.IsNotNull(snapshot.LastAttemptUtc);
            Assert.IsNotNull(snapshot.LastSuccessUtc);
            Assert.IsNull(snapshot.LastError);
            Assert.HasCount(1, snapshot.ByVdsId);
            Assert.IsTrue(snapshot.ByVdsId.ContainsKey("VDS-9"));
        }

        [TestMethod]
        public async Task RefreshOnceAsync_Failure_RetainsDataAndSetsError()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            DateTimeOffset lastSuccess = DateTimeOffset.UtcNow.AddMinutes(-10);
            VdsTrafficSnapshot initial = CreateSnapshot(4, Guid.NewGuid(), lastSuccess);
            store.Swap(initial);

            Mock<IVdsTrafficSnapshotSourcePort> source = new Mock<IVdsTrafficSnapshotSourcePort>();
            source
                .Setup(s => s.FetchAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            VdsTrafficSnapshotRefresher refresher = new VdsTrafficSnapshotRefresher(store, source.Object);

            await refresher.RefreshOnceAsync(CancellationToken.None);

            VdsTrafficSnapshot snapshot = store.GetCurrent();
            Assert.AreEqual(initial.Version + 1, snapshot.Version);
            Assert.AreEqual(initial.SnapshotId, snapshot.SnapshotId);
            Assert.AreEqual(lastSuccess, snapshot.LastSuccessUtc);
            Assert.IsNotNull(snapshot.LastAttemptUtc);
            Assert.IsNotNull(snapshot.LastError);
            Assert.IsTrue(snapshot.LastError!.Contains("boom", StringComparison.Ordinal));
            Assert.HasCount(1, snapshot.ByVdsId);
            Assert.AreEqual(initial.ByVdsId["VDS-4"].Speed, snapshot.ByVdsId["VDS-4"].Speed);
        }

        [TestMethod]
        public async Task RefreshOnceAsync_WhenAlreadyRefreshing_ReturnsWithoutCallingSourceAgain()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            store.Swap(CreateSnapshot(1, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-1)));

            TaskCompletionSource fetchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource unblockFetch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            Mock<IVdsTrafficSnapshotSourcePort> source = new Mock<IVdsTrafficSnapshotSourcePort>();
            source
                .Setup(s => s.FetchAsync(It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    fetchStarted.SetResult();
                    await unblockFetch.Task.ConfigureAwait(false);
                    return new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal)
                    {
                        ["VDS-9"] = CreateObservation("VDS-9")
                    };
                });

            VdsTrafficSnapshotRefresher refresher = new VdsTrafficSnapshotRefresher(store, source.Object);

            Task firstRefresh = refresher.RefreshOnceAsync(CancellationToken.None);
            await fetchStarted.Task.ConfigureAwait(false);

            Task secondRefresh = refresher.RefreshOnceAsync(CancellationToken.None);

            Assert.IsTrue(secondRefresh.IsCompletedSuccessfully);
            source.Verify(s => s.FetchAsync(It.IsAny<CancellationToken>()), Times.Once);

            unblockFetch.SetResult();
            await firstRefresh.ConfigureAwait(false);
        }

        private static VdsTrafficSnapshot CreateSnapshot(long version, Guid snapshotId, DateTimeOffset lastSuccess)
        {
            Dictionary<string, VdsTrafficObservation> observations = new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal)
            {
                ["VDS-4"] = CreateObservation("VDS-4")
            };

            return new VdsTrafficSnapshot(
                snapshotId,
                version,
                observations,
                lastSuccessUtc: lastSuccess,
                lastAttemptUtc: lastSuccess,
                lastError: null);
        }

        private static VdsTrafficObservation CreateObservation(string vdsId)
        {
            return new VdsTrafficObservation(
                vdsId,
                collectedDate: "2000-01-01 00:00:00",
                speed: 80,
                volume: 100,
                occupancy: 10);
        }
    }
}
