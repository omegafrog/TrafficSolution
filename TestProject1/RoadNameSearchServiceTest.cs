using Moq;
using System.Text.Json.Nodes;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1
{
    [TestClass]
    public sealed class RoadNameSearchServiceTest
    {
        [TestMethod]
        public async Task SearchRoadByNameAsync_NormalizesBoundsAndPreservesMode()
        {
            Mock<IRoadNameSearchPort> roadNameSearchPort = new Mock<IRoadNameSearchPort>();
            RoadNameSearchService service = new RoadNameSearchService(roadNameSearchPort.Object);

            MapBounds rawBounds = new MapBounds
            {
                MinLongitude = 131.4,
                MinLatitude = 38.8,
                MaxLongitude = 125.6,
                MaxLatitude = 34.2
            };

            RoadNameCandidate candidate = new RoadNameCandidate
            {
                HighwayNo = 1,
                ReferenceNumber = "1",
                HighwayName = "경부고속도로",
                Latitude = 37.55,
                Longitude = 127.05,
                DistanceMeters = 14.5
            };

            roadNameSearchPort
                .Setup(port => port.ResolveRoadNameAsync(
                    "경부고속도로",
                    It.Is<MapBounds>(bounds =>
                        bounds.MinLongitude == 125.6
                        && bounds.MinLatitude == 34.2
                        && bounds.MaxLongitude == 131.4
                        && bounds.MaxLatitude == 38.8)))
                .ReturnsAsync(candidate);

            RoadSearchDispatchResult result = await service.SearchRoadByNameAsync(
                new RoadNameSearchCommand("  경부고속도로  ", rawBounds, CurrentMode.Cctv));

            Assert.AreEqual(CurrentMode.Cctv, result.Mode);
            Assert.AreEqual(candidate.HighwayNo, result.Candidate.HighwayNo);
            Assert.AreEqual(candidate.HighwayName, result.Candidate.HighwayName);

            JsonNode? payload = JsonNode.Parse(result.CreateSelectionMessage());
            Assert.AreEqual("pos-selected", payload?["type"]?.GetValue<string>());
            Assert.AreEqual(candidate.Latitude, payload?["lat"]?.GetValue<double>());
            Assert.AreEqual(candidate.Longitude, payload?["lon"]?.GetValue<double>());
            Assert.AreEqual(125.6, payload?["minLon"]?.GetValue<double>());
            Assert.AreEqual(34.2, payload?["minLat"]?.GetValue<double>());
            Assert.AreEqual(131.4, payload?["maxLon"]?.GetValue<double>());
            Assert.AreEqual(38.8, payload?["maxLat"]?.GetValue<double>());

            roadNameSearchPort.VerifyAll();
        }

        [TestMethod]
        public async Task SearchRoadByNameAsync_RejectsEmptyRoadName()
        {
            RoadNameSearchService service = new RoadNameSearchService(new Mock<IRoadNameSearchPort>().Object);

            await Assert.ThrowsAsync<ArgumentException>(() => service.SearchRoadByNameAsync(
                new RoadNameSearchCommand("   ", new MapBounds(), CurrentMode.Traffic)));
        }
    }
}
