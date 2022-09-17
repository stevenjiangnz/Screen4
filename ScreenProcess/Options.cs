using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ScreenProcess
{
    [Verb("ticker", HelpText = "Load Ticker information.")]
    class TickerOptions
    {
        [Option('a', "all", Default = false, Required = false, HelpText = "Load all tickers from day 1.")]
        public bool All { get; set; }

        [Option('d', "days", Default = 3, Required = false, HelpText = "Load all tickers from days.")]
        public int Days { get; set; }

    }

    [Verb("commit", HelpText = "Record changes to the repository.")]
    class CommitOptions
    {
        //commit options here
    }

    [Verb("clone", HelpText = "Clone a repository into a new directory.")]
    class CloneOptions
    {
        //clone options here
    }
}
