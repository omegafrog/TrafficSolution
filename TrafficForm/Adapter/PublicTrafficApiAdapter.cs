using System.Text.Json.Nodes;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public class PublicTrafficApiAdapter : IPublicTrafficApiPort
    {
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = "https://openapi.its.go.kr:9443/vdsInfo";
        private string? serviceKey = Environment.GetEnvironmentVariable("SERVICE_KEY");
        private readonly VdsRepository _vdsRepository;

        public PublicTrafficApiAdapter(HttpClient httpClient, VdsRepository vdsRepository)
        {
            _httpClient = httpClient;
            _vdsRepository = vdsRepository;
        }
        public async Task<List<VdsTrafficResult>> GetTrafficResult(int highwayNo, double minLongitude, double minLatitude, double maxLongitude, double maxLatitude)
        {
            List<VdsTrafficResult> result = new List<VdsTrafficResult>();
            HttpResponseMessage response = await _httpClient.GetAsync(baseUrl + "?apiKey=" + serviceKey + "&getType=json");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
                
                JsonNode? node = JsonNode.Parse(json)?["body"];
                JsonArray? items = node?["items"]?.AsArray();
                foreach(var item in items!)
                {
                    try
                    {
                    string vdsId = item?["vdsId"]?.GetValue<string>() ?? "";
                    string collectedDate = item?["colctedDate"]?.GetValue<string>() ?? "";

                    if (!double.TryParse(item?["speed"]?.GetValue<string>(), out double speed))
                    {
                        continue;
                    }

                    if (!int.TryParse(item?["volume"]?.GetValue<string>(), out int volume))
                    {
                        volume = -1;
                    }

                    if (!double.TryParse(item?["occupancy"]?.GetValue<string>(), out double occupacy))
                    {
                        occupacy = -1;
                    }

                    if (string.IsNullOrEmpty(vdsId) || string.IsNullOrEmpty(collectedDate) || speed < 0 || occupacy < 0 || volume < 0)
                    {
                        continue;
                    }

                    result.Add(new VdsTrafficResult()
                        {
                            VdsId = vdsId,
                            CollectedDate = collectedDate,
                            Speed = speed,
                            Volume = volume,
                            Occupancy = occupacy
                        });
                    }catch(Exception)
                {
                    Console.WriteLine(item);
                }
                    
                }


            Dictionary<string, Tuple<double, double>> vdsLoc = await _vdsRepository.findVdsIdIn(highwayNo*10, minLatitude, minLongitude, maxLatitude, maxLongitude);

            List<VdsTrafficResult> filteredResults = result.Where(r =>
            {
                if (vdsLoc.TryGetValue(r.VdsId, out Tuple<double, double>? coordinate))
                {
                    r.Location = new Location()
                    {
                        Latitude = coordinate.Item1,
                        Longitude = coordinate.Item2
                    };
                    return true;
                }
                return false;
            }).ToList();

            foreach (VdsTrafficResult trafficResult in filteredResults)
            {
                trafficResult.TrafficLevel = TrafficLevelPolicy.CalculateTrafficLevel(trafficResult);
            }

            Dictionary<string, List<Location>> segmentByVdsId = await _vdsRepository.findResponsibilitySegments(
                highwayNo * 10,
                filteredResults.Select(result => result.VdsId));

            foreach (VdsTrafficResult trafficResult in filteredResults)
            {
                if (segmentByVdsId.TryGetValue(trafficResult.VdsId, out List<Location>? segmentPoints))
                {
                    trafficResult.ResponsibilitySegment = segmentPoints;
                }
            }

            return filteredResults;
        }

        public async Task<List<Location>> findAllVdiLoc()
        {
            return await _vdsRepository.findAllVdsLoc();
        }
    }
}
