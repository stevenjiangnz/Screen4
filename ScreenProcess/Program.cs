// See https://aka.ms/new-console-template for more information

using Screen.Shared;
using Screen.Ticks;
using Microsoft.Extensions.Configuration;
using Screen.Utils;
using Serilog;
using CommandLine;
using ScreenProcess;
using Screen.Symbols;
using Screen.Entity;
using Screen.Indicator;

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

Parser.Default.ParseArguments<TickerOptions, ProcessOptions, IndicatorOptions>(args)
    .MapResult(
      (TickerOptions opts) => RunTickerAndReturnExitCode(opts),
      (ProcessOptions opts) => RunProcessAndReturnExitCode(opts),
      (IndicatorOptions opts) => RunIndicatorAndReturnExitCode(opts),
      errs => 1);

// Prepare to get out
//Console.WriteLine("Press Enter to exit");
//Console.ReadLine();

int RunTickerAndReturnExitCode(TickerOptions opts)
{
    try
    {
        SymbolManager symbolManager = new SymbolManager(settings);
        var result = symbolManager.LoadFullSymbolList(null);

        TickerManager tickerManager = new TickerManager(settings);

        if (opts.All)
        {
            tickerManager.LoadTickerFromEmail();
        }
        else
        {
            tickerManager.LoadTickerFromEmail(opts.Days);
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
        SymbolManager symbolManager = new SymbolManager(settings);

        var result = symbolManager.LoadFullSymbolList(null);
        Log.Debug($"Loaded symbol list {result.Count}.");

        TickerManager tickerManager = new TickerManager(settings);

        if (opts.All)
        {
            tickerManager.ProcessTickersFromDownload(result);
        } else
        {
            tickerManager.ProcessTickersFromDownload(result, opts.Days);
        }
    } catch (Exception ex)
    {
        Log.Error(ex, "Error in processing tickers");
        return 1;
    }

    return 0;
}

int RunIndicatorAndReturnExitCode(IndicatorOptions opts)
{
    TickerManager tickerManager = new TickerManager(settings);
    SymbolManager symbolManager = new SymbolManager(settings);
    IndicatorManager indicatorManager = new IndicatorManager(settings);
    
    var symbolList = symbolManager.LoadFullSymbolList(null);

    foreach(SymbolEntity symbol in symbolList)
    {
        try
        {
            var symbolTickers = tickerManager.GetTickerListByCode(symbol.Code);
            indicatorManager.ProcessIndicatorsForCode(symbol.Code, symbolTickers);
            Log.Information($"Successfully processed for indicators {symbol.Code}.");
        }
        catch(Exception ex)
        {
            Log.Error(ex, $"Error in processing indicators for {symbol.Code}");
        }
    }

    return 0;
}


