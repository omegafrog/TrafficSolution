using TrafficForm.App;
using Microsoft.Extensions.DependencyInjection;
using TrafficForm.Port;
using TrafficForm.Adapter;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace TrafficForm
{
    internal static class Program
    {
        private static int _showingGlobalErrorDialog;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);


            RegisterGlobalExceptionHandlers();

            var services = new ServiceCollection();
            services.AddTransient<Form1>();
            services.AddSingleton<RequestTrafficByPosService>();
            services.AddSingleton<RequestCctvByPosService>();
            services.AddSingleton<FavoriteService>();
            services.AddSingleton<IOpenStreetQueryPort, OpenStreetQueryAdapter>();
            services.AddSingleton<VdsTrafficSnapshotStore>();
            services.AddSingleton<IVdsTrafficSnapshotSourcePort, ItsVdsTrafficSnapshotSourceAdapter>();
            services.AddSingleton<VdsTrafficSnapshotRefresher>();
            services.AddSingleton<IVdsTrafficSnapshotRefresherPort>(
                provider => provider.GetRequiredService<VdsTrafficSnapshotRefresher>());
            services.AddSingleton<IVdsGeoRepositoryPort>(
                provider => provider.GetRequiredService<VdsRepository>());
            services.AddSingleton<IPublicTrafficApiPort, CachedPublicTrafficApiAdapter>();
            services.AddSingleton<ICctvApiPort, CctvApiAdapter>();
            services.AddSingleton<IFavoriteStorePort, JsonFavoriteStoreAdapter>();
            services.AddSingleton<VdsRepository>();
            services.AddSingleton<OpenStreetDbRepository>();
            services.AddSingleton<HttpClient>();
            services.AddDbContext<TrafficDbContext>(options =>
                options.UseNpgsql("Host=localhost;Port=5432;Database=gis;Username=renderer;Password=renderer"));
            using var provider = services.BuildServiceProvider();
            VdsTrafficSnapshotRefresher refresher = provider.GetRequiredService<VdsTrafficSnapshotRefresher>();
            refresher.Start();

            using (var scope = provider.CreateScope())
            {
                try
                {
                    Debug.WriteLine("db context check start.");
                    var context = scope.ServiceProvider.GetRequiredService<TrafficDbContext>();
                    bool created = context.Database.EnsureCreated();
                }catch(Exception ex)
                {
                    Debug.WriteLine($"Exception:{ex.Message}");
                }
                
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            try
            {
                Application.Run(provider.GetRequiredService<Form1>());
            }
            finally
            {
                refresher.StopAsync().GetAwaiter().GetResult();
            }
        }

        private static void RegisterGlobalExceptionHandlers()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, eventArgs) => ShowUnhandledException(eventArgs.Exception, "UI Thread");
            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            {
                if (eventArgs.ExceptionObject is Exception exception)
                {
                    ShowUnhandledException(exception, "AppDomain");
                }
            };
            TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
            {
                ShowUnhandledException(eventArgs.Exception, "TaskScheduler");
                eventArgs.SetObserved();
            };
        }

        private static void ShowUnhandledException(Exception exception, string source)
        {
            Debug.WriteLine($"[{source}] {exception}");

            if (Interlocked.CompareExchange(ref _showingGlobalErrorDialog, 1, 0) != 0)
            {
                return;
            }

            try
            {
                MessageBox.Show(
                    "예상하지 못한 오류가 발생했습니다.\n"
                    + "앱을 계속 사용할 수 있는지 확인하고, 동일 현상이 반복되면 재시작해 주세요.\n\n"
                    + exception.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch
            {
            }
            finally
            {
                Interlocked.Exchange(ref _showingGlobalErrorDialog, 0);
            }
        }
    }
}
