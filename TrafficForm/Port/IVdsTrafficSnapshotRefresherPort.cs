using System.Threading.Tasks;

namespace TrafficForm.Port
{
    public interface IVdsTrafficSnapshotRefresherPort
    {
        Task RefreshOnceAsync(CancellationToken ct);
    }
}
