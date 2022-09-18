// See https://aka.ms/new-console-template for more information

using Screen.Shared;
using Screen.Ticks;
using Microsoft.Extensions.Configuration;
using Screen.Entity;
using Screen.Utils;
using Serilog;
using CommandLine;
using ScreenProcess;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("/data/logs/screen4.log")
    .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
    .CreateLogger();

//Log.Information("Start Process...");

SharedSettings settings = config.GetRequiredSection("Settings").Get<SharedSettings>();

Log.Debug($"args: {ObjectHelper.ToJsonString(args)}");

Parser.Default.ParseArguments<TickerOptions, CommitOptions, CloneOptions>(args)
    .MapResult(
      (TickerOptions opts) => RunTickerAndReturnExitCode(opts),
      (CommitOptions opts) => RunCommitAndReturnExitCode(opts),
      (CloneOptions opts) => RunCloneAndReturnExitCode(opts),
      errs => 1);

// Prepare to get out
Console.WriteLine("Press Enter to exit");
Console.ReadLine();

int RunTickerAndReturnExitCode(TickerOptions opts)
{

    TickerManager tickerManager = new TickerManager();

    if (opts.All)
    {
        tickerManager.LoadTickerFromEmail(settings);
    }
    else
    {
        tickerManager.LoadTickerFromEmail(settings, opts.Days);
    }

    return 0;
}


int RunCommitAndReturnExitCode(CommitOptions opts)
{
    Console.WriteLine("Commit");
    return 0;
}

int RunCloneAndReturnExitCode(CloneOptions opts)
{
    Console.WriteLine("Clone");
    return 0;
}


