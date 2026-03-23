using Microsoft.Extensions.DependencyInjection;
using TrafficForm.Adapter;
using TrafficForm.Port;

namespace TestProject1
{
    [TestClass]
    public sealed class DiWiringTest
    {
        [TestMethod]
        public void ServiceCollection_ResolvesTrafficSnapshotDependencies()
        {
            ServiceProvider provider = BuildProvider();

            IPublicTrafficApiPort trafficApi = provider.GetRequiredService<IPublicTrafficApiPort>();
            IVdsTrafficSnapshotSourcePort source = provider.GetRequiredService<IVdsTrafficSnapshotSourcePort>();
            IVdsGeoRepositoryPort geoRepository = provider.GetRequiredService<IVdsGeoRepositoryPort>();
            VdsTrafficSnapshotStore store1 = provider.GetRequiredService<VdsTrafficSnapshotStore>();
            VdsTrafficSnapshotStore store2 = provider.GetRequiredService<VdsTrafficSnapshotStore>();
            VdsTrafficSnapshotRefresher refresher = provider.GetRequiredService<VdsTrafficSnapshotRefresher>();
            IVdsTrafficSnapshotRefresherPort refresherPort = provider.GetRequiredService<IVdsTrafficSnapshotRefresherPort>();

            Assert.IsInstanceOfType(trafficApi, typeof(CachedPublicTrafficApiAdapter));
            Assert.IsInstanceOfType(source, typeof(ItsVdsTrafficSnapshotSourceAdapter));
            Assert.IsInstanceOfType(geoRepository, typeof(VdsRepository));
            Assert.AreSame(store1, store2);
            Assert.AreSame(refresher, refresherPort);
        }

        private static ServiceProvider BuildProvider()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<VdsTrafficSnapshotStore>();
            services.AddSingleton<IVdsTrafficSnapshotSourcePort, ItsVdsTrafficSnapshotSourceAdapter>();
            services.AddSingleton<VdsTrafficSnapshotRefresher>();
            services.AddSingleton<IVdsTrafficSnapshotRefresherPort>(
                provider => provider.GetRequiredService<VdsTrafficSnapshotRefresher>());
            services.AddSingleton<VdsRepository>();
            services.AddSingleton<IVdsGeoRepositoryPort>(
                provider => provider.GetRequiredService<VdsRepository>());
            services.AddSingleton<IPublicTrafficApiPort, CachedPublicTrafficApiAdapter>();
            services.AddSingleton<HttpClient>();

            return services.BuildServiceProvider();
        }
    }
}
