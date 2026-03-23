namespace TrafficForm.Domain
{
    public sealed class MapViewSnapshot
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int ZoomLevel { get; set; }

        public double MinLongitude { get; set; }

        public double MinLatitude { get; set; }

        public double MaxLongitude { get; set; }

        public double MaxLatitude { get; set; }
    }
}
