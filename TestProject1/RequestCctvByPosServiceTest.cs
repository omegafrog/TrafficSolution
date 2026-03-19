using Moq;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1
{
    [TestClass]
    public sealed class RequestCctvByPosServiceTest
    {
        [TestMethod]
        public async Task GetNearbyHighwayCctv_OutOfRangePoint_ThrowsPointOutOfRangeException()
        {
            RequestCctvByPosService service = CreateService();
            UpdateSelectedPosCctvInfoCommand command = new UpdateSelectedPosCctvInfoCommand(
                UpdateSelectedPosCctvInfoCommand.MIN_LATITUDE - 0.1,
                UpdateSelectedPosCctvInfoCommand.MIN_LONGITUDE);

            await Assert.ThrowsAsync<PointOutOfRangeException>(() => service.GetNearbyHighwayCctv(command));
        }

        [TestMethod]
        public async Task GetNearbyHighwayCctv_SelectsClosestHighwayAndReturnsNearbyCctv()
        {
            Mock<IOpenStreetQueryPort> openStreetPort = new Mock<IOpenStreetQueryPort>();
            Mock<IPublicTrafficApiPort> trafficApiPort = new Mock<IPublicTrafficApiPort>();
            Mock<ICctvApiPort> cctvApiPort = new Mock<ICctvApiPort>();

            RequestCctvByPosService service = new RequestCctvByPosService(openStreetPort.Object, trafficApiPort.Object, cctvApiPort.Object);

            UpdateSelectedPosCctvInfoCommand command = new UpdateSelectedPosCctvInfoCommand(37.50, 127.00)
            {
                MinLongitude = 126.80,
                MinLatitude = 37.30,
                MaxLongitude = 127.20,
                MaxLatitude = 37.70
            };

            openStreetPort
                .Setup(port => port.GetAdjacentHighWays(It.IsAny<Location>()))
                .ReturnsAsync(new Dictionary<int, HighWay>
                {
                    [1] = new HighWay { ReferenceNumber = "1", Name = "경부고속도로" },
                    [50] = new HighWay { ReferenceNumber = "50", Name = "영동고속도로" }
                });

            trafficApiPort
                .Setup(port => port.GetTrafficResult(1, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new List<VdsTrafficResult>
                {
                    new VdsTrafficResult
                    {
                        VdsId = "0010VDS00001",
                        CollectedDate = "2000-01-01 00:00:00",
                        Speed = 85,
                        Volume = 100,
                        Occupancy = 15,
                        Location = new Location { Latitude = 37.5005, Longitude = 127.0004, Name = "" }
                    }
                });

            trafficApiPort
                .Setup(port => port.GetTrafficResult(50, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new List<VdsTrafficResult>
                {
                    new VdsTrafficResult
                    {
                        VdsId = "0500VDS00001",
                        CollectedDate = "2000-01-01 00:00:00",
                        Speed = 85,
                        Volume = 100,
                        Occupancy = 15,
                        Location = new Location { Latitude = 37.7000, Longitude = 127.3000, Name = "" }
                    }
                });

            cctvApiPort
                .Setup(port => port.GetCctvInfo(command.MinLongitude, command.MinLatitude, command.MaxLongitude, command.MaxLatitude))
                .ReturnsAsync(new List<CctvInfo>
                {
                    new CctvInfo
                    {
                        CctvId = "A",
                        Name = "경부선 CCTV",
                        StreamUrl = "https://example.com/a.m3u8",
                        Location = new Location { Latitude = 37.5007, Longitude = 127.0002, Name = "A" }
                    },
                    new CctvInfo
                    {
                        CctvId = "B",
                        Name = "영동선 CCTV",
                        StreamUrl = "https://example.com/b.m3u8",
                        Location = new Location { Latitude = 37.7001, Longitude = 127.3003, Name = "B" }
                    }
                });

            HighwayCctvSelection result = await service.GetNearbyHighwayCctv(command);

            Assert.AreEqual(1, result.HighwayNo);
            Assert.AreEqual("경부고속도로", result.HighwayName);
            Assert.HasCount(1, result.CctvInfos);
            Assert.AreEqual("A", result.CctvInfos[0].CctvId);
            Assert.AreEqual(1, result.CctvInfos[0].HighwayNo);
            Assert.AreEqual("경부고속도로", result.CctvInfos[0].HighwayName);
        }

        [TestMethod]
        public async Task GetNearbyHighwayCctv_NormalizesBoundsBeforeCctvQuery()
        {
            Mock<IOpenStreetQueryPort> openStreetPort = new Mock<IOpenStreetQueryPort>();
            Mock<IPublicTrafficApiPort> trafficApiPort = new Mock<IPublicTrafficApiPort>();
            Mock<ICctvApiPort> cctvApiPort = new Mock<ICctvApiPort>();

            RequestCctvByPosService service = new RequestCctvByPosService(openStreetPort.Object, trafficApiPort.Object, cctvApiPort.Object);

            UpdateSelectedPosCctvInfoCommand command = new UpdateSelectedPosCctvInfoCommand(37.5, 127.0)
            {
                MinLongitude = 132.9,
                MaxLongitude = 124.1,
                MinLatitude = 39.4,
                MaxLatitude = 32.5
            };

            openStreetPort
                .Setup(port => port.GetAdjacentHighWays(It.IsAny<Location>()))
                .ReturnsAsync(new Dictionary<int, HighWay>
                {
                    [1] = new HighWay { ReferenceNumber = "1", Name = "경부고속도로" }
                });

            trafficApiPort
                .Setup(port => port.GetTrafficResult(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new List<VdsTrafficResult>
                {
                    new VdsTrafficResult
                    {
                        VdsId = "0010VDS00001",
                        CollectedDate = "2000-01-01 00:00:00",
                        Speed = 60,
                        Volume = 100,
                        Occupancy = 20,
                        Location = new Location { Latitude = 37.5, Longitude = 127.0, Name = "" }
                    }
                });

            cctvApiPort
                .Setup(port => port.GetCctvInfo(
                    UpdateSelectedPosCctvInfoCommand.MIN_LONGITUDE,
                    UpdateSelectedPosCctvInfoCommand.MIN_LATITUDE,
                    UpdateSelectedPosCctvInfoCommand.MAX_LONGITUDE,
                    UpdateSelectedPosCctvInfoCommand.MAX_LATITUDE))
                .ReturnsAsync(new List<CctvInfo>());

            HighwayCctvSelection _ = await service.GetNearbyHighwayCctv(command);

            cctvApiPort.Verify(
                port => port.GetCctvInfo(
                    UpdateSelectedPosCctvInfoCommand.MIN_LONGITUDE,
                    UpdateSelectedPosCctvInfoCommand.MIN_LATITUDE,
                    UpdateSelectedPosCctvInfoCommand.MAX_LONGITUDE,
                    UpdateSelectedPosCctvInfoCommand.MAX_LATITUDE),
                Times.Once);
        }

        private static RequestCctvByPosService CreateService()
        {
            return new RequestCctvByPosService(
                new Mock<IOpenStreetQueryPort>().Object,
                new Mock<IPublicTrafficApiPort>().Object,
                new Mock<ICctvApiPort>().Object);
        }
    }
}
