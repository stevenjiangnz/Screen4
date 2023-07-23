using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Screen.ETSymbol.Loader
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {

             Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs\\Screen.ETSymbol.Loader.log", 
                    rollingInterval: RollingInterval.Day,
                    buffered: false,
                    flushToDiskInterval: TimeSpan.FromMilliseconds(500))
                .CreateLogger();

            var host = CreateHostBuilder(args).Build();

            var dataManager = host.Services.GetService<ETInstrumentManager>();

            await dataManager.GetInstruments();

            // Close and flush the log
            Log.CloseAndFlush();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .UseSerilog()
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services.Configure<AppSettings>(context.Configuration);
            services.AddSingleton<ETInstrumentManager>();
            // Other service configurations...
        });
    }
}