// See https://aka.ms/new-console-template for more information

using Screen.Shared;
using Screen.Ticks;
using Microsoft.Extensions.Configuration;
using Screen.Entity;
using Screen.Utils;


// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();


SharedSettings settings = config.GetRequiredSection("Settings").Get<SharedSettings>();

Console.WriteLine("email: " + ObjectHelper.ToJsonString(settings));

//TickerManager tickManager = new TickerManager();

//tickManager.LoadTickerFromEmail(settings);


Console.WriteLine("finsihed...");
