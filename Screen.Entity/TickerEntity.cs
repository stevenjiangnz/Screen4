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

        public TickerEntity(string tickerString)
        {
            if (!string.IsNullOrEmpty(tickerString))
            {
                string[] tickerParts = tickerString.Split(',');

                if (tickerParts.Length > 6)
                {
                    for (int i = 0; i < tickerParts.Length; i++)
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
        public long V { get; set; } // Volume

        public override string ToString()
        {
            return $"{this.T},{this.P},{this.O},{this.H},{this.L},{this.C},{this.V}";
        }

    }

}
