using System;
using System.Collections.Generic;
using System.Text;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.App
{
    public class RequestTrafficByPosService
    {
        private readonly IOpenStreetQueryPort openStreetQueryPort;
        private readonly IPublicTrafficApiPort publicTrafficApiPort;
        
        public RequestTrafficByPosService(IOpenStreetQueryPort updateTrafficInfoByPosCommandPort, IPublicTrafficApiPort publicTrafficApiPort)
        {
            this.publicTrafficApiPort = publicTrafficApiPort;
            this.openStreetQueryPort = updateTrafficInfoByPosCommandPort ?? throw new ArgumentNullException(nameof(updateTrafficInfoByPosCommandPort));
        }
        public void CheckSelectedPoint(UpdateSelectedPosTrafficInfoCommand updateSelectedPosTrafficInfoCommand)
        {
            if(updateSelectedPosTrafficInfoCommand == null)
            {
                throw new RequiredCommandNotFoundException("좌표를 선택해야 합니다.");
            }
            if(updateSelectedPosTrafficInfoCommand.Latitude <33.0 || updateSelectedPosTrafficInfoCommand.Latitude>38.6 || updateSelectedPosTrafficInfoCommand.Longitude<125.0 || updateSelectedPosTrafficInfoCommand.Longitude>129.8)
            {
                throw new PointOutOfRangeException("좌표가 유효범위를 넘었습니다.");
            }
        }
        public async Task<List<HighWay>> GetAdjacentHighWays(UpdateSelectedPosTrafficInfoCommand command)
        {
            double adjacentDiff = 5.0;
            try
            {
                List<HighWay> adjacentHighWays = await openStreetQueryPort.GetAdjacentHighWays(mapper.Invoke(command));
                adjacentHighWays
                    .Where(h => Util.DistanceKm(
                        h.Location.Latitude,
                        h.Location.Longitude,
                        command.Latitude,
                        command.Longitude) <= adjacentDiff)
                    .ToList();

                if (adjacentHighWays.Count > 5)
                {
                    adjacentHighWays = adjacentHighWays[..5];
                }
                return adjacentHighWays;
            }
            catch (Exception ex)
            {
                //TODO : 공공 도로 데이터를 조회에 실패했을 때 롤백 작업이 필요함.
                throw new NotImplementedException("인접한 고속도로를 가져오는데 실패했습니다. 실패 콜백이 필요합니다.", ex);
            }
            
        }

        public async Task<TrafficResult> GetTrafficResult(HighWay highWay)
        {
            try
            {
                TrafficResult result = await publicTrafficApiPort.GetTrafficResult(highWay);
                TrafficLevelPolicy.CalculateTrafficLevel(result);
                return result;
            }catch (TrafficResultRequestFailedException ex) {
                // TODO : 공공 교통량 데이터를 조회에 실패했을 때 롤백 작업이 필요함.
                throw new NotImplementedException("교통정보를 가져오는데 실패했습니다. 실패 콜백이 필요합니다.", ex);

            }
        }

        
        private Func<UpdateSelectedPosTrafficInfoCommand, Location> mapper = (command)=> new Location(command.Latitude, command.Longitude);
    }
}
