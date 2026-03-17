using TrafficForm.App;
using Microsoft.Extensions.DependencyInjection;
using TrafficForm.Port;
using TrafficForm.Adapter;

namespace TrafficForm
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var services = new ServiceCollection();
            services.AddTransient<Form1>();
            services.AddSingleton<RequestTrafficByPosService>();
            services.AddSingleton<IOpenStreetQueryPort, OpenStreetQueryAdapter>();
            services.AddSingleton<IPublicTrafficApiPort, PublicTrafficApiAdapter>();
            services.AddSingleton<VdsRepository>();
            services.AddSingleton<OpenStreetDbRepository>();
            services.AddSingleton<HttpClient>();
            using var provider = services.BuildServiceProvider();

            Application.Run(provider.GetRequiredService<Form1>());
        }
    }
}