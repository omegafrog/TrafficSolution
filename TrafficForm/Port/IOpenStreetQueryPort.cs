using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IOpenStreetQueryPort
    {
        public Task<Dictionary<int, HighWay>> GetAdjacentHighWays(Location location);
        
    }
}