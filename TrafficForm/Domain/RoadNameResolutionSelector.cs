namespace TrafficForm.Domain
{
    public static class RoadNameResolutionSelector
    {
        public static RoadNameCandidate? SelectBestCandidate(IEnumerable<RoadNameCandidate> candidates)
        {
            return candidates
                .OrderBy(candidate => candidate.DistanceMeters)
                .ThenBy(candidate => candidate.HighwayNo)
                .ThenBy(candidate => candidate.HighwayName, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.ReferenceNumber, StringComparer.Ordinal)
                .FirstOrDefault();
        }
    }
}
