using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IRoadNameHighwaySearchPort
    {
        Task<IReadOnlyList<HighWay>> SearchExactAsync(
            IReadOnlyList<string> normalizedQueries,
            double minLongitude,
            double minLatitude,
            double maxLongitude,
            double maxLatitude);

        Task<IReadOnlyList<HighWay>> SearchPartialAsync(
            IReadOnlyList<string> normalizedQueries,
            double minLongitude,
            double minLatitude,
            double maxLongitude,
            double maxLatitude);
    }
}
