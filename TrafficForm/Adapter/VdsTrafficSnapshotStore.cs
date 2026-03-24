using TrafficForm.Domain;

namespace TrafficForm.Adapter
{
    public sealed class VdsTrafficSnapshotStore
    {
        private VdsTrafficSnapshot _current;

        public VdsTrafficSnapshotStore()
        {
            _current = VdsTrafficSnapshot.Empty;
        }

        public VdsTrafficSnapshot GetCurrent()
        {
            return Volatile.Read(ref _current);
        }

        public VdsTrafficSnapshot Swap(VdsTrafficSnapshot next)
        {
            ArgumentNullException.ThrowIfNull(next);

            return Interlocked.Exchange(ref _current, next);
        }
    }
}
