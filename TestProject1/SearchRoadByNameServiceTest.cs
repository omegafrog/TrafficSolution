using Moq;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1
{
    [TestClass]
    public sealed class SearchRoadByNameServiceTest
    {
        [TestMethod]
        public async Task SearchAsync_WhenExactMatchesExist_PrefersExactAndSkipsPartial()
        {
            Mock<IRoadNameHighwaySearchPort> searchPort = new Mock<IRoadNameHighwaySearchPort>();
            Mock<IRoadNameQueryExpanderPort> expanderPort = new Mock<IRoadNameQueryExpanderPort>();
            SearchRoadByNameService service = new SearchRoadByNameService(searchPort.Object, expanderPort.Object);

            SearchRoadByNameCommand command = new SearchRoadByNameCommand
            {
                Query = "경부고속도로",
                MinLongitude = 126.8,
                MinLatitude = 37.3,
                MaxLongitude = 127.2,
                MaxLatitude = 37.7
            };

            expanderPort
                .Setup(port => port.Expand(command.Query))
                .Returns(new[] { "경부고속도로" });

            searchPort
                .Setup(port => port.SearchExactAsync(
                    It.Is<IReadOnlyList<string>>(queries => queries.SequenceEqual(new[] { "경부고속도로" })),
                    command.MinLongitude,
                    command.MinLatitude,
                    command.MaxLongitude,
                    command.MaxLatitude))
                .ReturnsAsync(new[]
                {
                    new HighWay { ReferenceNumber = "1", Name = "경부고속도로" }
                });

            RoadNameSearchResult result = await service.SearchAsync(command);

            Assert.AreEqual(RoadNameMatchKind.Exact, result.MatchKind);
            Assert.HasCount(1, result.Highways);
            Assert.AreEqual("1", result.Highways[0].ReferenceNumber);
            searchPort.Verify(port => port.SearchPartialAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        }

        [TestMethod]
        public async Task SearchAsync_WhenExactMisses_UsesPartialMatches()
        {
            Mock<IRoadNameHighwaySearchPort> searchPort = new Mock<IRoadNameHighwaySearchPort>();
            Mock<IRoadNameQueryExpanderPort> expanderPort = new Mock<IRoadNameQueryExpanderPort>();
            SearchRoadByNameService service = new SearchRoadByNameService(searchPort.Object, expanderPort.Object);

            SearchRoadByNameCommand command = new SearchRoadByNameCommand
            {
                Query = "경부",
                MinLongitude = 126.8,
                MinLatitude = 37.3,
                MaxLongitude = 127.2,
                MaxLatitude = 37.7
            };

            expanderPort
                .Setup(port => port.Expand(command.Query))
                .Returns(new[] { "경부" });

            searchPort
                .Setup(port => port.SearchExactAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(Array.Empty<HighWay>());

            searchPort
                .Setup(port => port.SearchPartialAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new[]
                {
                    new HighWay { ReferenceNumber = "1", Name = "경부고속도로" }
                });

            RoadNameSearchResult result = await service.SearchAsync(command);

            Assert.AreEqual(RoadNameMatchKind.Partial, result.MatchKind);
            Assert.HasCount(1, result.Highways);
            expanderPort.Verify(port => port.Expand(command.Query), Times.Once);
        }

        [TestMethod]
        public async Task SearchAsync_NormalizesBoundsBeforeSearch()
        {
            Mock<IRoadNameHighwaySearchPort> searchPort = new Mock<IRoadNameHighwaySearchPort>();
            Mock<IRoadNameQueryExpanderPort> expanderPort = new Mock<IRoadNameQueryExpanderPort>();
            SearchRoadByNameService service = new SearchRoadByNameService(searchPort.Object, expanderPort.Object);

            SearchRoadByNameCommand command = new SearchRoadByNameCommand
            {
                Query = "경부고속도로",
                MinLongitude = 132.9,
                MinLatitude = 39.4,
                MaxLongitude = 124.1,
                MaxLatitude = 32.5
            };

            expanderPort
                .Setup(port => port.Expand(command.Query))
                .Returns(new[] { "경부고속도로" });

            searchPort
                .Setup(port => port.SearchExactAsync(
                    It.IsAny<IReadOnlyList<string>>(),
                    UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE,
                    UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE,
                    UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE,
                    UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE))
                .ReturnsAsync(Array.Empty<HighWay>());

            searchPort
                .Setup(port => port.SearchPartialAsync(
                    It.IsAny<IReadOnlyList<string>>(),
                    UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE,
                    UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE,
                    UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE,
                    UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE))
                .ReturnsAsync(Array.Empty<HighWay>());

            RoadNameSearchResult result = await service.SearchAsync(command);

            Assert.AreEqual(RoadNameMatchKind.None, result.MatchKind);
            searchPort.Verify(port => port.SearchExactAsync(
                It.IsAny<IReadOnlyList<string>>(),
                UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE,
                UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE,
                UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE,
                UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE), Times.Once);
        }

        [TestMethod]
        public async Task SearchAsync_WhenQueryIsBlank_ReturnsNoneWithoutCallingPorts()
        {
            Mock<IRoadNameHighwaySearchPort> searchPort = new Mock<IRoadNameHighwaySearchPort>();
            Mock<IRoadNameQueryExpanderPort> expanderPort = new Mock<IRoadNameQueryExpanderPort>();
            SearchRoadByNameService service = new SearchRoadByNameService(searchPort.Object, expanderPort.Object);

            RoadNameSearchResult result = await service.SearchAsync(new SearchRoadByNameCommand
            {
                Query = "   "
            });

            Assert.AreEqual(RoadNameMatchKind.None, result.MatchKind);
            Assert.AreEqual(0, result.Highways.Count);
            expanderPort.Verify(port => port.Expand(It.IsAny<string>()), Times.Never);
            searchPort.VerifyNoOtherCalls();
        }
    }
}
