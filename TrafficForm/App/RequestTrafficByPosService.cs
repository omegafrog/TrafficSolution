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
        public async Task<Dictionary<int, List<VdsTrafficResult>>> GetAdjacentHighWays(UpdateSelectedPosTrafficInfoCommand command)
        {
            double adjacentDiff = 5.0;
            try
            {
                Dictionary<int, HighWay> adjacentHighWays = await openStreetQueryPort.GetAdjacentHighWays(mapper.Invoke(command));
                Dictionary<int, List<VdsTrafficResult>> result = new Dictionary<int, List<VdsTrafficResult>>();
                foreach(var e in adjacentHighWays)
                {
                    List<VdsTrafficResult> res = await GetTrafficResult(e.Key, command);
                    result.Add(e.Key, [.. res.Select(res =>
                    {
                        res.Location.Name = e.Value.Name;
                        return res;
                    })]);
                }
                return result;
            }
            catch (Exception ex)
            {
                //TODO : 공공 도로 데이터를 조회에 실패했을 때 롤백 작업이 필요함.
                throw new NotImplementedException("인접한 고속도로를 가져오는데 실패했습니다. 실패 콜백이 필요합니다.", ex);
            }
            
        }

        // 줌인한 박스 내부의 highwayNo에 해당하는 vds의 정보를 가져옴. 
        private async Task<List<VdsTrafficResult>> GetTrafficResult(int highwayNo, UpdateSelectedPosTrafficInfoCommand command)
        {
            try
            {
                List<VdsTrafficResult> result = await publicTrafficApiPort.GetTrafficResult(highwayNo, command.MinLongitude, command.MinLatitude, command.MaxLongitude, command.MaxLatitude);

                List<VdsTrafficResult> filtered = result.Where(e => validateTrafficResult(e)).ToList();
                return filtered;
            }catch (TrafficResultRequestFailedException ex) {
                // TODO : 공공 교통량 데이터를 조회에 실패했을 때 롤백 작업이 필요함.
                throw new NotImplementedException("교통정보를 가져오는데 실패했습니다. 실패 콜백이 필요합니다.", ex);

            }
        }
        static bool validateTrafficResult(VdsTrafficResult e)
        {
            return string.IsNullOrEmpty(e.Location.Name)
                                || DateTime.Parse(e.CollectedDate) >= DateTime.Now.AddDays(1)
                                || e.Volume < 0
                                || e.Occupancy < 0;
        }


        private Func<UpdateSelectedPosTrafficInfoCommand, Location> mapper = (command)=> new Location() { Latitude = command.Latitude, Longitude = command.Longitude };
    }
}
