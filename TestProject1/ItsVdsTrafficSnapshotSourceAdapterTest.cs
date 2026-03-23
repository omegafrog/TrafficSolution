using System.Net;
using System.Text;
using TrafficForm.Adapter;
using TrafficForm.App;
using TrafficForm.Domain;

namespace TestProject1
{
    [TestClass]
    public sealed class ItsVdsTrafficSnapshotSourceAdapterTest
    {
        [TestMethod]
        public async Task FetchAsync_MissingServiceKey_Throws()
        {
            string? original = Environment.GetEnvironmentVariable("SERVICE_KEY");
            Environment.SetEnvironmentVariable("SERVICE_KEY", null);

            try
            {
                ItsVdsTrafficSnapshotSourceAdapter adapter = CreateAdapter("{}");

                TrafficResultRequestFailedException exception = await Assert.ThrowsAsync<TrafficResultRequestFailedException>(
                    () => adapter.FetchAsync(CancellationToken.None));

                Assert.AreEqual("환경변수 'SERVICE_KEY'가 설정되지 않았습니다.", exception.Message);
            }
            finally
            {
                Environment.SetEnvironmentVariable("SERVICE_KEY", original);
            }
        }

        [TestMethod]
        public async Task FetchAsync_ParsesItemsIntoDictionary()
        {
            string? original = Environment.GetEnvironmentVariable("SERVICE_KEY");
            Environment.SetEnvironmentVariable("SERVICE_KEY", "test-key");

            try
            {
                string json = CreateResponseJson(
                    """
                    {"vdsId":"VDS-1","colctedDate":"20240101000000","speed":"80","volume":"10","occupancy":"5"},
                    {"vdsId":"VDS-2","colctedDate":"20240101050000","speed":"70","volume":"20","occupancy":"6"}
                    """);

                HttpRequestMessage? sentRequest = null;
                ItsVdsTrafficSnapshotSourceAdapter adapter = CreateAdapter(json, request => sentRequest = request);

                IReadOnlyDictionary<string, VdsTrafficObservation> result =
                    await adapter.FetchAsync(CancellationToken.None);

                Assert.HasCount(2, result);
                Assert.IsTrue(result.ContainsKey("VDS-1"));
                Assert.AreEqual("20240101000000", result["VDS-1"].CollectedDate);
                Assert.IsTrue(result.ContainsKey("VDS-2"));
                Assert.AreEqual(70, result["VDS-2"].Speed);

                Assert.IsNotNull(sentRequest);
                string? requestUri = sentRequest!.RequestUri?.ToString();
                Assert.IsNotNull(requestUri);
                Assert.IsTrue(requestUri!.Contains("apiKey=", StringComparison.Ordinal));
                Assert.IsTrue(requestUri.Contains("getType=json", StringComparison.Ordinal));
            }
            finally
            {
                Environment.SetEnvironmentVariable("SERVICE_KEY", original);
            }
        }

        [TestMethod]
        public async Task FetchAsync_DuplicateVdsId_UsesNewestParsedDate()
        {
            string? original = Environment.GetEnvironmentVariable("SERVICE_KEY");
            Environment.SetEnvironmentVariable("SERVICE_KEY", "test-key");

            try
            {
                string json = CreateResponseJson(
                    """
                    {"vdsId":"VDS-1","colctedDate":"20240101000000","speed":"60","volume":"10","occupancy":"5"},
                    {"vdsId":"VDS-1","colctedDate":"20240102000000","speed":"90","volume":"12","occupancy":"6"}
                    """);

                ItsVdsTrafficSnapshotSourceAdapter adapter = CreateAdapter(json);

                IReadOnlyDictionary<string, VdsTrafficObservation> result =
                    await adapter.FetchAsync(CancellationToken.None);

                Assert.HasCount(1, result);
                Assert.AreEqual("20240102000000", result["VDS-1"].CollectedDate);
                Assert.AreEqual(90, result["VDS-1"].Speed);
            }
            finally
            {
                Environment.SetEnvironmentVariable("SERVICE_KEY", original);
            }
        }

        [TestMethod]
        public async Task FetchAsync_DuplicateVdsId_UnparseableDate_KeepsFirst()
        {
            string? original = Environment.GetEnvironmentVariable("SERVICE_KEY");
            Environment.SetEnvironmentVariable("SERVICE_KEY", "test-key");

            try
            {
                string json = CreateResponseJson(
                    """
                    {"vdsId":"VDS-1","colctedDate":"not-a-date","speed":"60","volume":"10","occupancy":"5"},
                    {"vdsId":"VDS-1","colctedDate":"20240102000000","speed":"90","volume":"12","occupancy":"6"}
                    """);

                ItsVdsTrafficSnapshotSourceAdapter adapter = CreateAdapter(json);

                IReadOnlyDictionary<string, VdsTrafficObservation> result =
                    await adapter.FetchAsync(CancellationToken.None);

                Assert.HasCount(1, result);
                Assert.AreEqual("not-a-date", result["VDS-1"].CollectedDate);
                Assert.AreEqual(60, result["VDS-1"].Speed);
            }
            finally
            {
                Environment.SetEnvironmentVariable("SERVICE_KEY", original);
            }
        }

        private static ItsVdsTrafficSnapshotSourceAdapter CreateAdapter(
            string responseJson,
            Action<HttpRequestMessage>? onRequest = null)
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler((request, _) =>
            {
                onRequest?.Invoke(request);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                };

                return response;
            });

            HttpClient client = new HttpClient(handler);
            return new ItsVdsTrafficSnapshotSourceAdapter(client);
        }

        private static string CreateResponseJson(string itemsJson)
        {
            return "{\n"
                + "  \"body\": {\n"
                + "    \"items\": [\n"
                + "      " + itemsJson + "\n"
                + "    ]\n"
                + "  }\n"
                + "}";
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _handler;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_handler(request, cancellationToken));
            }
        }
    }
}
