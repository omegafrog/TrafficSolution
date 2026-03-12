using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IOpenStreetQueryPort
    {
        public Task<List<HighWay>> GetAdjacentHighWays(Location location);
        
    }
}