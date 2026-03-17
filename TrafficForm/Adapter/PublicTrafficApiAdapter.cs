using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using TrafficForm.Domain;
using TrafficForm.Port;
using static System.Net.WebRequestMethods;

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
                    double speed = double.Parse(item?["speed"]?.GetValue<string>());
                    int volume = int.Parse(item?["volume"]?.GetValue<string>() ?? "-1");
                    double occupacy = double.Parse(item?["occupancy"]?.GetValue<string>() ?? "-1");

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
                    }catch(Exception e)
                {
                    Console.WriteLine(item);
                }
                    
                }


            Dictionary<string, Tuple<double, double>> vdsLoc = await _vdsRepository.findVdsIdIn(highwayNo*10, minLatitude, minLongitude, maxLatitude, maxLongitude);

            return result.Where(r =>
            {
                if (vdsLoc.Keys.Contains(r.VdsId))
                {
                    r.Location = new Location()
                    {
                        Latitude = vdsLoc[r.VdsId].Item1,
                        Longitude = vdsLoc[r.VdsId].Item2
                    };
                    return true;
                }
                return false;
            }).ToList();
        }

        public async Task<List<Location>> findAllVdiLoc()
        {
            return await _vdsRepository.findAllVdsLoc();
        }
    }
}
