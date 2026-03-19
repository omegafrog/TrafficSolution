using Moq;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1
{
    [TestClass]
    public sealed class RequestTrafficServiceTest
    {
        [TestMethod]
        public void CheckSelectedPoint_OutOfRangePoint_ThrowsPointOutOfRangeException()
        {
            Mock<IOpenStreetQueryPort> mockPort = new Mock<IOpenStreetQueryPort>();
            Mock<IPublicTrafficApiPort> mockTrafficInfoPort = new Mock<IPublicTrafficApiPort>();
            RequestTrafficByPosService trafficService = new RequestTrafficByPosService(mockPort.Object, mockTrafficInfoPort.Object);

            UpdateSelectedPosTrafficInfoCommand command1 = new UpdateSelectedPosTrafficInfoCommand(
                UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE - 1,
                UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE);
            UpdateSelectedPosTrafficInfoCommand command2 = new UpdateSelectedPosTrafficInfoCommand(
                UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE,
                UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE - 1);
            UpdateSelectedPosTrafficInfoCommand command3 = new UpdateSelectedPosTrafficInfoCommand(
                UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE + 1,
                UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE);
            UpdateSelectedPosTrafficInfoCommand command4 = new UpdateSelectedPosTrafficInfoCommand(
                UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE,
                UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE + 1);

            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(command1));
            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(command2));
            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(command3));
            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(command4));
        }

        [TestMethod]
        public async Task GetAdjacentHighways_Success()
        {
            Mock<IOpenStreetQueryPort> mockPort = new Mock<IOpenStreetQueryPort>();
            Mock<IPublicTrafficApiPort> mockTrafficInfoPort = new Mock<IPublicTrafficApiPort>();
            RequestTrafficByPosService trafficService = new RequestTrafficByPosService(mockPort.Object, mockTrafficInfoPort.Object);

            UpdateSelectedPosTrafficInfoCommand command = new UpdateSelectedPosTrafficInfoCommand(37.5, 127.0)
            {
                MinLongitude = 126.8,
                MinLatitude = 37.3,
                MaxLongitude = 127.2,
                MaxLatitude = 37.7
            };

            const int highwayNo = 1;
            const string highwayName = "경부고속도로";

            mockPort
                .Setup(p => p.GetAdjacentHighWays(It.Is<Location>(loc =>
                    loc.Latitude == command.Latitude && loc.Longitude == command.Longitude)))
                .ReturnsAsync(new Dictionary<int, HighWay>
                {
                    [highwayNo] = new HighWay
                    {
                        ReferenceNumber = highwayNo.ToString(),
                        Name = highwayName
                    }
                });

            mockTrafficInfoPort
                .Setup(p => p.GetTrafficResult(
                    highwayNo,
                    command.MinLongitude,
                    command.MinLatitude,
                    command.MaxLongitude,
                    command.MaxLatitude))
                .ReturnsAsync(new List<VdsTrafficResult>
                {
                    new VdsTrafficResult
                    {
                        VdsId = "0010VDS00001",
                        CollectedDate = "2000-01-01 00:00:00",
                        Speed = 80,
                        Volume = 100,
                        Occupancy = 20,
                        Location = new Location
                        {
                            Latitude = 37.51,
                            Longitude = 127.01,
                            Name = string.Empty
                        }
                    }
                });

            Dictionary<int, List<VdsTrafficResult>> result = await trafficService.GetAdjacentHighWays(command);

            Assert.IsTrue(result.ContainsKey(highwayNo));
            Assert.HasCount(1, result[highwayNo]);
            Assert.AreEqual(highwayName, result[highwayNo][0].Location.Name);

            mockTrafficInfoPort.Verify(p => p.GetTrafficResult(
                highwayNo,
                command.MinLongitude,
                command.MinLatitude,
                command.MaxLongitude,
                command.MaxLatitude), Times.Once);
        }

        [TestMethod]
        public async Task GetAdjacentHighways_WhenTrafficApiFails_ThrowsNotImplementedException()
        {
            Mock<IOpenStreetQueryPort> mockPort = new Mock<IOpenStreetQueryPort>();
            Mock<IPublicTrafficApiPort> mockTrafficInfoPort = new Mock<IPublicTrafficApiPort>();
            RequestTrafficByPosService trafficService = new RequestTrafficByPosService(mockPort.Object, mockTrafficInfoPort.Object);

            UpdateSelectedPosTrafficInfoCommand command = new UpdateSelectedPosTrafficInfoCommand(37.5, 127.0)
            {
                MinLongitude = 126.8,
                MinLatitude = 37.3,
                MaxLongitude = 127.2,
                MaxLatitude = 37.7
            };

            mockPort
                .Setup(p => p.GetAdjacentHighWays(It.IsAny<Location>()))
                .ReturnsAsync(new Dictionary<int, HighWay>
                {
                    [1] = new HighWay
                    {
                        ReferenceNumber = "1",
                        Name = "경부고속도로"
                    }
                });

            mockTrafficInfoPort
                .Setup(p => p.GetTrafficResult(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                .ThrowsAsync(new TrafficResultRequestFailedException("공공 교통량 데이터 조회 실패"));

            await Assert.ThrowsAsync<NotImplementedException>(() => trafficService.GetAdjacentHighWays(command));
        }
    }
}
