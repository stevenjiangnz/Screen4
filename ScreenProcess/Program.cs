// See https://aka.ms/new-console-template for more information

using Screen.Shared;
using Screen.Ticks;
using Microsoft.Extensions.Configuration;
using Screen.Entity;
using Screen.Utils;
using Serilog;
using CommandLine;
using ScreenProcess;
using Screen.Symbols;

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

Parser.Default.ParseArguments<TickerOptions, ProcessOptions, CloneOptions>(args)
    .MapResult(
      (TickerOptions opts) => RunTickerAndReturnExitCode(opts),
      (ProcessOptions opts) => RunProcessAndReturnExitCode(opts),
      (CloneOptions opts) => RunCloneAndReturnExitCode(opts),
      errs => 1);

// Prepare to get out
//Console.WriteLine("Press Enter to exit");
//Console.ReadLine();

int RunTickerAndReturnExitCode(TickerOptions opts)
{
    try
    {
        SymbolManager symbolManager = new SymbolManager();
        var result = symbolManager.LoadFullSymbolList(settings, null);

        TickerManager tickerManager = new TickerManager();

        if (opts.All)
        {
            tickerManager.LoadTickerFromEmail(settings);
        }
        else
        {
            tickerManager.LoadTickerFromEmail(settings, opts.Days);
        }

    } catch (Exception ex)
    {
        Log.Error(ex, "Error in loading tickers");
        return 1;
    }

    return 0;
}


int RunProcessAndReturnExitCode(ProcessOptions opts)
{
    try
    {
        SymbolManager symbolManager = new SymbolManager();

        var result = symbolManager.LoadFullSymbolList(settings, null);
        Log.Debug($"Loaded symbol list {result.Count}.");

        TickerManager tickerManager = new TickerManager();

        if (opts.All)
        {
            tickerManager.ProcessTickersFromDownload(settings, result);
        } else
        {
            tickerManager.ProcessTickersFromDownload(settings, result, opts.Days);
        }
    } catch (Exception ex)
    {
        Log.Error(ex, "Error in processing tickers");
        return 1;
    }

    return 0;
}

int RunCloneAndReturnExitCode(CloneOptions opts)
{
    Console.WriteLine("Clone");
    return 0;
}


