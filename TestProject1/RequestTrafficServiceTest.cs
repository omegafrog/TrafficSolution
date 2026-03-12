using Moq;
using System.Text.RegularExpressions;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1
{

    [TestClass]
    public sealed class RequestTrafficServiceTest
    {
        [TestMethod]
        public void CheckSelectedPoint_OutOfRangePoint()
        {
            // Arrange
            Mock<IOpenStreetQueryPort> mockPort = new Mock<IOpenStreetQueryPort>();
            Mock<IPublicTrafficApiPort> mockTrafficInfoPort = new Mock<IPublicTrafficApiPort>();
            TrafficForm.App.RequestTrafficByPosService trafficService = new TrafficForm.App.RequestTrafficByPosService(mockPort.Object, mockTrafficInfoPort.Object);
            // Act & Assert
            UpdateSelectedPosTrafficInfoCommand userSelectedPointCommand1 = new UpdateSelectedPosTrafficInfoCommand(UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE - 1, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE);
            UpdateSelectedPosTrafficInfoCommand userSelectedPointCommand2 = new UpdateSelectedPosTrafficInfoCommand(UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE - 1);
            UpdateSelectedPosTrafficInfoCommand userSelectedPointCommand3 = new UpdateSelectedPosTrafficInfoCommand(UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE + 1, UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE);
            UpdateSelectedPosTrafficInfoCommand userSelectedPointCommand4 = new UpdateSelectedPosTrafficInfoCommand(UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE, UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE + 1);
            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(userSelectedPointCommand1));
            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(userSelectedPointCommand2));
            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(userSelectedPointCommand3));
            Assert.Throws<PointOutOfRangeException>(() => trafficService.CheckSelectedPoint(userSelectedPointCommand4));

        }
        [TestMethod]
        public async Task GetAdjacentHighways_Success()
        {
            // Arrange
            Mock<IOpenStreetQueryPort> mockPort = new Mock<IOpenStreetQueryPort>();
            Mock<IPublicTrafficApiPort> mockTrafficInfoPort = new Mock<IPublicTrafficApiPort>();
            TrafficForm.App.RequestTrafficByPosService trafficService = new TrafficForm.App.RequestTrafficByPosService(mockPort.Object, mockTrafficInfoPort.Object);

            Location location1 = new Location(UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE);
            HighWay highway1 = new HighWay
            {
                Id = "1",
                Location = new Location(UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE + 0.01, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE + 0.01)
            };
            HighWay highway6 = new HighWay
            {
                Id = "6",
                Location = new Location(UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE + 1, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE + 1)
            };

            Location captured = null;
            mockPort
                //.Setup(p => p.GetAdjacentHighWays(It.IsAny<Location>()))
                .Setup(p => p.GetAdjacentHighWays(It.Is<Location>(
                    loc=>loc.Latitude==location1.Latitude&&loc.Longitude==location1.Longitude)))
                .Callback<Location>(loc => captured = loc)
                .ReturnsAsync(new List<HighWay> { highway1, highway6 });
            
            // Act

            List<HighWay> result = await trafficService.GetAdjacentHighWays(new UpdateSelectedPosTrafficInfoCommand(location1.Latitude, location1.Longitude));
            // 거리가 5km보다 먼 highway6는 결과에 포함되지 않음.
            Assert.Contains(highway1, result);
            Assert.DoesNotContain(highway6, result);

        }
    [TestMethod]
    public void GetTrafficResult_Failed()
    {
        // Arrange
        Mock<IOpenStreetQueryPort> mockPort = new Mock<IOpenStreetQueryPort>();
        Mock<IPublicTrafficApiPort> mockTrafficInfoPort = new Mock<IPublicTrafficApiPort>();
        TrafficForm.App.RequestTrafficByPosService trafficService = new TrafficForm.App.RequestTrafficByPosService(mockPort.Object, mockTrafficInfoPort.Object);
        HighWay highway1 = new HighWay
        {
            Id = "1",
            Location = new Location(UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE + 0.01, UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE + 0.01)
        };
        mockTrafficInfoPort
            .Setup(p => p.GetTrafficResult(It.IsAny<HighWay>()))
            .ThrowsAsync(new TrafficResultRequestFailedException("공공 교통량 데이터 조회 실패"));
        
        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(async () => await trafficService.GetTrafficResult(highway1));
        }
    }
}
