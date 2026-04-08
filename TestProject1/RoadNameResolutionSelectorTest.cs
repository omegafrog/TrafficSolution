using TrafficForm.Domain;

namespace TestProject1
{
    [TestClass]
    public sealed class RoadNameResolutionSelectorTest
    {
        [TestMethod]
        public void SelectBestCandidate_PrefersShortestDistanceAndStableTieBreak()
        {
            RoadNameCandidate nearest = new RoadNameCandidate
            {
                HighwayNo = 1,
                ReferenceNumber = "1",
                HighwayName = "경부고속도로",
                Latitude = 37.5,
                Longitude = 127.0,
                DistanceMeters = 10
            };

            RoadNameCandidate sameDistanceButHigherHighwayNo = new RoadNameCandidate
            {
                HighwayNo = 50,
                ReferenceNumber = "50",
                HighwayName = "영동고속도로",
                Latitude = 37.6,
                Longitude = 127.1,
                DistanceMeters = 10
            };

            RoadNameCandidate farther = new RoadNameCandidate
            {
                HighwayNo = 2,
                ReferenceNumber = "2",
                HighwayName = "서해안고속도로",
                Latitude = 37.7,
                Longitude = 127.2,
                DistanceMeters = 25
            };

            RoadNameCandidate? best = RoadNameResolutionSelector.SelectBestCandidate(new[]
            {
                farther,
                sameDistanceButHigherHighwayNo,
                nearest
            });

            Assert.IsNotNull(best);
            Assert.AreEqual(1, best.HighwayNo);
            Assert.AreEqual("경부고속도로", best.HighwayName);
            Assert.AreEqual(10, best.DistanceMeters);
        }
    }
}
