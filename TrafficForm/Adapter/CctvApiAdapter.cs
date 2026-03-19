using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public class CctvApiAdapter : ICctvApiPort
    {
        private const string BaseUrl = "https://openapi.its.go.kr:9443/cctvInfo";
        private const string CctvServiceKeyEnvironmentVariable = "CCTV_SERVICE_KEY";

        private readonly HttpClient _httpClient;

        public CctvApiAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CctvInfo>> GetCctvInfo(double minLongitude, double minLatitude, double maxLongitude, double maxLatitude)
        {
            string? serviceKey = Environment.GetEnvironmentVariable(CctvServiceKeyEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(serviceKey))
            {
                throw new CctvResultRequestFailedException($"환경변수 '{CctvServiceKeyEnvironmentVariable}'가 설정되지 않았습니다.");
            }

            string requestUri =
                $"{BaseUrl}?apiKey={Uri.EscapeDataString(serviceKey)}"
                + "&type=ex"
                + "&cctvType=1"
                + $"&minX={minLongitude.ToString(CultureInfo.InvariantCulture)}"
                + $"&maxX={maxLongitude.ToString(CultureInfo.InvariantCulture)}"
                + $"&minY={minLatitude.ToString(CultureInfo.InvariantCulture)}"
                + $"&maxY={maxLatitude.ToString(CultureInfo.InvariantCulture)}"
                + "&getType=json";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(requestUri);
            }
            catch (Exception exception)
            {
                throw new CctvResultRequestFailedException("ITS CCTV API 호출에 실패했습니다.", exception);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new CctvResultRequestFailedException($"ITS CCTV API 응답이 비정상입니다. status={(int)response.StatusCode}");
            }

            string content = await response.Content.ReadAsStringAsync();

            try
            {
                JsonNode? root = JsonNode.Parse(content);
                //ValidateResultCode(root);

                JsonNode? body = root?["response"]?["data"];
                JsonArray? items = body?.AsArray();

                if (items == null)
                {
                    return new List<CctvInfo>();
                }

                List<CctvInfo> results = new List<CctvInfo>();
                int sequence = 0;

                foreach (JsonNode? item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    JsonNode parsedItem = item;

                    if (!TryParseCoordinate(parsedItem["coordy"], out double latitude)
                        || !TryParseCoordinate(parsedItem["coordx"], out double longitude))
                    {
                        continue;
                    }

                    string streamUrl = parsedItem["cctvurl"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(streamUrl))
                    {
                        continue;
                    }

                    string roadSectionId = parsedItem["roadsectionid"]?.ToString() ?? string.Empty;
                    string name = parsedItem["cctvname"]?.ToString() ?? roadSectionId;
                    string cctvId = BuildCctvId(parsedItem, sequence++);

                    results.Add(new CctvInfo
                    {
                        CctvId = cctvId,
                        RoadSectionId = roadSectionId,
                        Name = name,
                        StreamUrl = streamUrl,
                        StreamType = parsedItem["cctvtype"]?.ToString() ?? string.Empty,
                        Format = parsedItem["cctvformat"]?.ToString() ?? string.Empty,
                        Resolution = parsedItem["cctvresolution"]?.ToString() ?? string.Empty,
                        CapturedAtRaw = parsedItem["filecreatetime"]?.ToString() ?? string.Empty,
                        Location = new Location
                        {
                            Latitude = latitude,
                            Longitude = longitude,
                            Name = name
                        }
                    });
                }

                return results
                    .GroupBy(item => item.CctvId)
                    .Select(group => group.First())
                    .ToList();
            }
            catch (JsonException exception)
            {
                throw new CctvResultRequestFailedException("ITS CCTV API 응답 파싱에 실패했습니다.", exception);
            }
        }

        private static bool TryParseCoordinate(JsonNode? node, out double value)
        {
            string text = node?.ToString() ?? string.Empty;
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static string BuildCctvId(JsonNode item, int sequence)
        {
            string roadSectionId = item["roadsectionid"]?.ToString() ?? string.Empty;
            string name = item["cctvname"]?.ToString() ?? string.Empty;
            string longitude = item["coordx"]?.ToString() ?? string.Empty;
            string latitude = item["coordy"]?.ToString() ?? string.Empty;

            string composite = $"{roadSectionId}|{name}|{longitude}|{latitude}";
            bool hasContent = composite.Replace("|", string.Empty, StringComparison.Ordinal).Length > 0;
            return hasContent ? composite : $"cctv-{sequence}";
        }

        private static void ValidateResultCode(JsonNode? root)
        {
            string? resultCode = root?["response"]?["header"]?["resultCode"]?.ToString();
            if (string.IsNullOrWhiteSpace(resultCode))
            {
                return;
            }

            if (string.Equals(resultCode, "0", StringComparison.Ordinal))
            {
                return;
            }

            string resultMessage = root?["response"]?["header"]?["resultMsg"]?.ToString() ?? "Unknown";
            throw new CctvResultRequestFailedException($"ITS CCTV API 오류 코드: {resultCode}, 메시지: {resultMessage}");
        }
    }
}
