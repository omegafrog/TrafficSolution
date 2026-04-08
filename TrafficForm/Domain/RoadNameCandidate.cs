namespace TrafficForm.Domain
{
    public sealed class RoadNameCandidate
    {
        public required int HighwayNo { get; init; }

        public required string ReferenceNumber { get; init; }

        public required string HighwayName { get; init; }

        public required double Latitude { get; init; }

        public required double Longitude { get; init; }

        public required double DistanceMeters { get; init; }
    }
}
