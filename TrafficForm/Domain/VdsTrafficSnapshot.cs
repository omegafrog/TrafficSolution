using System.Collections.ObjectModel;

namespace TrafficForm.Domain
{
    public sealed class VdsTrafficSnapshot
    {
        private static readonly IReadOnlyDictionary<string, VdsTrafficObservation> EmptyDictionary =
            new ReadOnlyDictionary<string, VdsTrafficObservation>(
                new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal));

        public static VdsTrafficSnapshot Empty { get; } = new VdsTrafficSnapshot(
            Guid.Empty,
            0,
            EmptyDictionary,
            lastSuccessUtc: null,
            lastAttemptUtc: null,
            lastError: null);

        public Guid SnapshotId { get; }
        public long Version { get; }
        public IReadOnlyDictionary<string, VdsTrafficObservation> ByVdsId { get; }
        public DateTimeOffset? LastSuccessUtc { get; }
        public DateTimeOffset? LastAttemptUtc { get; }
        public string? LastError { get; }
        public bool HasData => ByVdsId.Count > 0;

        public VdsTrafficSnapshot(
            Guid snapshotId,
            long version,
            IReadOnlyDictionary<string, VdsTrafficObservation> byVdsId,
            DateTimeOffset? lastSuccessUtc,
            DateTimeOffset? lastAttemptUtc,
            string? lastError)
        {
            ArgumentNullException.ThrowIfNull(byVdsId);

            SnapshotId = snapshotId;
            Version = version;
            ByVdsId = CreateReadOnly(byVdsId);
            LastSuccessUtc = lastSuccessUtc;
            LastAttemptUtc = lastAttemptUtc;
            LastError = lastError;
        }

        private static IReadOnlyDictionary<string, VdsTrafficObservation> CreateReadOnly(
            IReadOnlyDictionary<string, VdsTrafficObservation> source)
        {
            Dictionary<string, VdsTrafficObservation> copy =
                new Dictionary<string, VdsTrafficObservation>(source.Count, StringComparer.Ordinal);

            foreach (KeyValuePair<string, VdsTrafficObservation> entry in source)
            {
                copy[entry.Key] = entry.Value;
            }

            return new ReadOnlyDictionary<string, VdsTrafficObservation>(copy);
        }
    }
}
