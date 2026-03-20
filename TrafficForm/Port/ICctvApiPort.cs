using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface ICctvApiPort
    {
        Task<List<CctvInfo>> GetCctvInfo(double minLongitude, double minLatitude, double maxLongitude, double maxLatitude);
    }
}
