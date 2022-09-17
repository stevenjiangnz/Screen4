// See https://aka.ms/new-console-template for more information

using Screen.Shared;
using Screen.Ticks;
using Microsoft.Extensions.Configuration;
using Screen.Entity;
using Screen.Utils;
using Serilog;


// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("scrren4.log")
    .WriteTo.Console()
    .CreateLogger();


Log.Information("this is information");

SharedSettings settings = config.GetRequiredSection("Settings").Get<SharedSettings>();


//TickerManager tickerManager = new TickerManager();

//tickerManager.LoadTickerFromEmail(settings);

Console.WriteLine("finsihed...");
