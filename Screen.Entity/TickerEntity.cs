using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Utils;

namespace Screen.Entity
{
    public class TickerEntity
    {
        public TickerEntity()
        {

        }

        /// <summary>
        /// This for a email download data
        /// </summary>
        /// <param name="tickerString"></param>
        public TickerEntity(string tickerString)
        {
            if (!string.IsNullOrEmpty(tickerString))
            {
                string[] tickerParts = tickerString.Split(',');

                if (tickerParts.Length > 6)
                {
                    this.T = tickerParts[0];
                    this.P = int.Parse(tickerParts[1]);
                    this.O = float.Parse(tickerParts[2]);
                    this.H = float.Parse(tickerParts[3]);
                    this.L = float.Parse(tickerParts[4]);
                    this.C = float.Parse(tickerParts[5]);
                    this.V = long.Parse(tickerParts[6]);
                }
            }
        }

        public TickerEntity(string symbol, string yahooTickString)
        {
            if (!string.IsNullOrEmpty(yahooTickString))
            {
                string[] tickerParts = yahooTickString.Split(',');

                if (tickerParts.Length > 6)
                {
                    this.T = symbol;
                    this.P = int.Parse(tickerParts[0].Replace("-", ""));
                    this.O = float.Parse(tickerParts[1]);
                    this.H = float.Parse(tickerParts[2]);
                    this.L = float.Parse(tickerParts[3]);
                    this.C = float.Parse(tickerParts[4]);
                    this.A = float.Parse(tickerParts[5]);
                    this.V = long.Parse(tickerParts[6]);
                }
            }
        }


        public string T { get; set; } // Ticker or Code
        public int P { get; set; } // Period
        public long P_Stamp
        {
            get
            {
                return DateHelper.ToTimeStamp(P);
            }
        }

        public float O { get; set; } // Open
        public float H { get; set; } // High
        public float L { get; set; } // Low
        public float C { get; set; } // Close
        public float A { get; set; } // Adjusted close
        public long V { get; set; } // Volume

        public override string ToString()
        {
            return $"{this.T},{this.P},{this.O},{this.H},{this.L},{this.C},{this.A},{this.V}";
        }
    }
}
