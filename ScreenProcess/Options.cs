using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ScreenProcess
{
    [Verb("ticker", HelpText = "Load Ticker information from mailbox and process.")]
    class TickerOptions
    {
        [Option('a', "all", Default = false, Required = false, HelpText = "Load all tickers from day 1.")]
        public bool All { get; set; }

        [Option('d', "days", Default = 3, Required = false, HelpText = "Load all tickers from days.")]
        public int Days { get; set; }

    }

    [Verb("process", HelpText = "Process tickers.")]
    class ProcessOptions
    {
        //[Option('i', "indicator", Default = false, Required = false, HelpText = "If process indicators included.")]
        //public bool CalculateIndicators { get; set; }

        [Option('a', "all", Default = false, Required = false, HelpText = "Process all tickers into individual file.")]
        public bool All { get; set; }

        [Option('d', "days", Default = 3, Required = false, HelpText = "Process number Days tickers into individual file.")]
        public int Days { get; set; }
    }

    [Verb("indicator", HelpText = "Process indicators.")]
    class IndicatorOptions
    {
        [Option('a', "all", Default = false, Required = false, HelpText = "Process all indicators for all files from day 1.")]
        public bool All { get; set; }

        [Option('d', "days", Default = 3, Required = false, HelpText = "Process number Days indicators into individual file.")]
        public int Days { get; set; }
    }
}
