using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IVdsTrafficSnapshotSourcePort
    {
        Task<IReadOnlyDictionary<string, VdsTrafficObservation>> FetchAsync(CancellationToken cancellationToken);
    }
}
