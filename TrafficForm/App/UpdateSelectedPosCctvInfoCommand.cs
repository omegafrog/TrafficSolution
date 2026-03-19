namespace TrafficForm.App
{
    public class UpdateSelectedPosCctvInfoCommand
    {
        public static readonly double MIN_LATITUDE = 33.0;
        public static readonly double MAX_LATITUDE = 39.0;
        public static readonly double MIN_LONGITUDE = 125.0;
        public static readonly double MAX_LONGITUDE = 132.0;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double MinLongitude { get; set; }

        public double MinLatitude { get; set; }

        public double MaxLongitude { get; set; }

        public double MaxLatitude { get; set; }

        public UpdateSelectedPosCctvInfoCommand(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            MinLongitude = longitude;
            MinLatitude = latitude;
            MaxLongitude = longitude;
            MaxLatitude = latitude;
        }

        public void NormalizeBounds()
        {
            double normalizedMinLongitude = Math.Min(MinLongitude, MaxLongitude);
            double normalizedMaxLongitude = Math.Max(MinLongitude, MaxLongitude);
            double normalizedMinLatitude = Math.Min(MinLatitude, MaxLatitude);
            double normalizedMaxLatitude = Math.Max(MinLatitude, MaxLatitude);

            MinLongitude = Clamp(normalizedMinLongitude, MIN_LONGITUDE, MAX_LONGITUDE);
            MaxLongitude = Clamp(normalizedMaxLongitude, MIN_LONGITUDE, MAX_LONGITUDE);
            MinLatitude = Clamp(normalizedMinLatitude, MIN_LATITUDE, MAX_LATITUDE);
            MaxLatitude = Clamp(normalizedMaxLatitude, MIN_LATITUDE, MAX_LATITUDE);
        }

        private static double Clamp(double value, double minValue, double maxValue)
        {
            return Math.Min(maxValue, Math.Max(minValue, value));
        }
    }
}
