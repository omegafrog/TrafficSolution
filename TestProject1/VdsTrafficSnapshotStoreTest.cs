using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrafficForm.Adapter;
using TrafficForm.Domain;

namespace TestProject1
{
    [TestClass]
    public sealed class VdsTrafficSnapshotStoreTest
    {
        [TestMethod]
        public async Task GetCurrent_ConcurrentReadsAndSwaps_ReturnsWholeSnapshots()
        {
            VdsTrafficSnapshotStore store = new VdsTrafficSnapshotStore();
            ConcurrentDictionary<long, Guid> knownSnapshots = new ConcurrentDictionary<long, Guid>();

            VdsTrafficSnapshot initial = CreateSnapshot(1, Guid.NewGuid());
            knownSnapshots.TryAdd(initial.Version, initial.SnapshotId);
            store.Swap(initial);

            const int iterations = 1000;
            int readerCount = Math.Max(2, Environment.ProcessorCount);
            int stop = 0;

            Task[] readers = new Task[readerCount];
            for (int i = 0; i < readerCount; i++)
            {
                readers[i] = Task.Run(() =>
                {
                    while (Volatile.Read(ref stop) == 0)
                    {
                        VdsTrafficSnapshot snapshot = store.GetCurrent();
                        AssertSnapshotKnown(snapshot, knownSnapshots);
                    }

                    VdsTrafficSnapshot finalSnapshot = store.GetCurrent();
                    AssertSnapshotKnown(finalSnapshot, knownSnapshots);
                });
            }

            Task writer = Task.Run(() =>
            {
                for (int version = 2; version <= iterations + 1; version++)
                {
                    Guid snapshotId = Guid.NewGuid();
                    VdsTrafficSnapshot next = CreateSnapshot(version, snapshotId);
                    knownSnapshots.TryAdd(next.Version, next.SnapshotId);
                    store.Swap(next);
                }

                Volatile.Write(ref stop, 1);
            });

            await writer;
            await Task.WhenAll(readers);
        }

        private static void AssertSnapshotKnown(VdsTrafficSnapshot snapshot, ConcurrentDictionary<long, Guid> knownSnapshots)
        {
            Assert.IsTrue(knownSnapshots.TryGetValue(snapshot.Version, out Guid snapshotId));
            Assert.AreEqual(snapshotId, snapshot.SnapshotId);
        }

        private static VdsTrafficSnapshot CreateSnapshot(long version, Guid snapshotId)
        {
            VdsTrafficObservation observation = new VdsTrafficObservation(
                vdsId: $"VDS-{version}",
                collectedDate: "2000-01-01 00:00:00",
                speed: 80,
                volume: 100,
                occupancy: 10);

            Dictionary<string, VdsTrafficObservation> byVdsId = new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal)
            {
                [observation.VdsId] = observation
            };

            return new VdsTrafficSnapshot(
                snapshotId,
                version,
                byVdsId,
                lastSuccessUtc: DateTimeOffset.UtcNow,
                lastAttemptUtc: DateTimeOffset.UtcNow,
                lastError: null);
        }
    }
}
