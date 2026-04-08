namespace TrafficForm.App
{
    public class SearchRoadByNameCommand
    {
        public string Query { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double MinLongitude { get; set; }

        public double MinLatitude { get; set; }

        public double MaxLongitude { get; set; }

        public double MaxLatitude { get; set; }

        public void NormalizeBounds()
        {
            double normalizedMinLongitude = Math.Min(MinLongitude, MaxLongitude);
            double normalizedMaxLongitude = Math.Max(MinLongitude, MaxLongitude);
            double normalizedMinLatitude = Math.Min(MinLatitude, MaxLatitude);
            double normalizedMaxLatitude = Math.Max(MinLatitude, MaxLatitude);

            MinLongitude = Clamp(normalizedMinLongitude, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE, UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE);
            MaxLongitude = Clamp(normalizedMaxLongitude, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE, UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE);
            MinLatitude = Clamp(normalizedMinLatitude, UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE, UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE);
            MaxLatitude = Clamp(normalizedMaxLatitude, UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE, UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE);
        }

        private static double Clamp(double value, double minValue, double maxValue)
        {
            return Math.Min(maxValue, Math.Max(minValue, value));
        }
    }

    public enum RoadNameMatchKind
    {
        None,
        Exact,
        Partial
    }

    public class RoadNameSearchResult
    {
        public IReadOnlyList<TrafficForm.Domain.HighWay> Highways { get; init; } = Array.Empty<TrafficForm.Domain.HighWay>();

        public RoadNameMatchKind MatchKind { get; init; } = RoadNameMatchKind.None;
    }
}
