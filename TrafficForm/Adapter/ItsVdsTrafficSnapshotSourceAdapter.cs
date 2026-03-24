using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public sealed class ItsVdsTrafficSnapshotSourceAdapter : IVdsTrafficSnapshotSourcePort
    {
        private const string BaseUrl = "https://openapi.its.go.kr:9443/vdsInfo";
        private const string ServiceKeyEnvironmentVariable = "SERVICE_KEY";
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

        private readonly HttpClient _httpClient;

        public ItsVdsTrafficSnapshotSourceAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyDictionary<string, VdsTrafficObservation>> FetchAsync(CancellationToken cancellationToken)
        {
            string? serviceKey = Environment.GetEnvironmentVariable(ServiceKeyEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(serviceKey))
            {
                throw new TrafficResultRequestFailedException("환경변수 'SERVICE_KEY'가 설정되지 않았습니다.");
            }

            string requestUri = $"{BaseUrl}?apiKey={Uri.EscapeDataString(serviceKey)}&getType=json";

            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(RequestTimeout);

            using HttpResponseMessage response = await SendRequestAsync(requestUri, cancellationToken, linkedCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new TrafficResultRequestFailedException(
                    $"ITS VDS API 응답이 비정상입니다. status={(int)response.StatusCode}");
            }

            string content = await response.Content.ReadAsStringAsync();

            try
            {
                JsonNode? root = JsonNode.Parse(content);
                JsonNode? body = root?["body"];
                body ??= root?["response"]?["body"];
                JsonArray? items = body?["items"]?.AsArray();

                if (items == null)
                {
                    return new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal);
                }

                Dictionary<string, VdsTrafficObservation> byVdsId =
                    new Dictionary<string, VdsTrafficObservation>(StringComparer.Ordinal);
                Dictionary<string, DateTime> parsedCollectedDates =
                    new Dictionary<string, DateTime>(StringComparer.Ordinal);

                foreach (JsonNode? item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    string vdsId = item["vdsId"]?.ToString() ?? string.Empty;
                    string collectedDate = item["colctedDate"]?.ToString() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(vdsId) || string.IsNullOrWhiteSpace(collectedDate))
                    {
                        continue;
                    }

                    if (!TryParseDouble(item["speed"], out double speed)
                        || !TryParseInt(item["volume"], out int volume)
                        || !TryParseDouble(item["occupancy"], out double occupancy))
                    {
                        continue;
                    }

                    if (speed < 0 || volume < 0 || occupancy < 0)
                    {
                        continue;
                    }

                    VdsTrafficObservation observation = new VdsTrafficObservation(
                        vdsId: vdsId,
                        collectedDate: collectedDate,
                        speed: speed,
                        volume: volume,
                        occupancy: occupancy);

                    bool parsedDate = TryParseCollectedDate(collectedDate, out DateTime collectedAt);

                    if (byVdsId.TryGetValue(vdsId, out VdsTrafficObservation? existing))
                    {
                        if (parsedDate
                            && parsedCollectedDates.TryGetValue(vdsId, out DateTime existingCollectedAt)
                            && collectedAt > existingCollectedAt)
                        {
                            byVdsId[vdsId] = observation;
                            parsedCollectedDates[vdsId] = collectedAt;
                        }

                        continue;
                    }

                    byVdsId[vdsId] = observation;
                    if (parsedDate)
                    {
                        parsedCollectedDates[vdsId] = collectedAt;
                    }
                }

                return byVdsId;
            }
            catch (JsonException exception)
            {
                throw new TrafficResultRequestFailedException("ITS VDS API 응답 파싱에 실패했습니다.", exception);
            }
        }

        private static bool TryParseCollectedDate(string value, out DateTime parsed)
        {
            string[] formats =
            {
                "yyyyMMddHHmmss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss"
            };

            if (DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed))
            {
                return true;
            }

            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed);
        }

        private static bool TryParseDouble(JsonNode? node, out double value)
        {
            string text = node?.ToString() ?? string.Empty;
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseInt(JsonNode? node, out int value)
        {
            string text = node?.ToString() ?? string.Empty;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(
            string requestUri,
            CancellationToken callerToken,
            CancellationToken timeoutToken)
        {
            try
            {
                return await _httpClient.GetAsync(requestUri, timeoutToken);
            }
            catch (OperationCanceledException) when (callerToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException exception)
            {
                throw new TrafficResultRequestFailedException(
                    "ITS VDS API 호출이 시간 초과되었습니다.",
                    exception);
            }
            catch (Exception exception)
            {
                throw new TrafficResultRequestFailedException("ITS VDS API 호출에 실패했습니다.", exception);
            }
        }
    }
}
