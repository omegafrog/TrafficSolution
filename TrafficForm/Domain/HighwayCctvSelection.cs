namespace TrafficForm.Domain
{
    public class HighwayCctvSelection
    {
        public required int HighwayNo { get; set; }

        public required string HighwayName { get; set; }

        public List<CctvInfo> CctvInfos { get; set; } = new List<CctvInfo>();
    }
}
