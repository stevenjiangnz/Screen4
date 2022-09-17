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
    .WriteTo.File("scrren4.log")
    .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
    .CreateLogger();

//Log.Information("Start Process...");

SharedSettings settings = config.GetRequiredSection("Settings").Get<SharedSettings>();

Log.Debug($"args: {ObjectHelper.ToJsonString(args)}");

return Parser.Default.ParseArguments<TickerOptions, CommitOptions, CloneOptions>(args)
    .MapResult(
      (TickerOptions opts) => RunTickerAndReturnExitCode(opts),
      (CommitOptions opts) => RunCommitAndReturnExitCode(opts),
      (CloneOptions opts) => RunCloneAndReturnExitCode(opts),
      errs => 1);

int RunTickerAndReturnExitCode(TickerOptions opts){
    
    if (opts.All)
    {
        Log.Information("about to load all tickers (from day 1)");
    } else
    {
        Log.Information($"about to load tickers for the last {opts.Days} days");
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


