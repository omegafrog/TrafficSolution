namespace TrafficForm.Domain
{
    public sealed class MapBounds
    {
        public const double MinLatitudeLimit = 33.0;
        public const double MaxLatitudeLimit = 39.0;
        public const double MinLongitudeLimit = 125.0;
        public const double MaxLongitudeLimit = 132.0;

        public double MinLongitude { get; set; }
        public double MinLatitude { get; set; }
        public double MaxLongitude { get; set; }
        public double MaxLatitude { get; set; }

        public double CenterLongitude => (MinLongitude + MaxLongitude) / 2.0;

        public double CenterLatitude => (MinLatitude + MaxLatitude) / 2.0;

        public MapBounds Normalize()
        {
            double normalizedMinLongitude = Math.Min(MinLongitude, MaxLongitude);
            double normalizedMaxLongitude = Math.Max(MinLongitude, MaxLongitude);
            double normalizedMinLatitude = Math.Min(MinLatitude, MaxLatitude);
            double normalizedMaxLatitude = Math.Max(MinLatitude, MaxLatitude);

            return new MapBounds
            {
                MinLongitude = Clamp(normalizedMinLongitude, MinLongitudeLimit, MaxLongitudeLimit),
                MinLatitude = Clamp(normalizedMinLatitude, MinLatitudeLimit, MaxLatitudeLimit),
                MaxLongitude = Clamp(normalizedMaxLongitude, MinLongitudeLimit, MaxLongitudeLimit),
                MaxLatitude = Clamp(normalizedMaxLatitude, MinLatitudeLimit, MaxLatitudeLimit)
            };
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Min(maximum, Math.Max(minimum, value));
        }
    }
}
