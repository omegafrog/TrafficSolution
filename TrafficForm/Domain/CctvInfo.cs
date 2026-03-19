namespace TrafficForm.Domain
{
    public class CctvInfo
    {
        public required string CctvId { get; set; }

        public string RoadSectionId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string StreamUrl { get; set; } = string.Empty;

        public string StreamType { get; set; } = string.Empty;

        public string Format { get; set; } = string.Empty;

        public string Resolution { get; set; } = string.Empty;

        public string CapturedAtRaw { get; set; } = string.Empty;

        public int HighwayNo { get; set; }

        public string HighwayName { get; set; } = string.Empty;

        public Location Location { get; set; } = new Location();
    }
}
