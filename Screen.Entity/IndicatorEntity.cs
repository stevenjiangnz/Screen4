using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Entity
{
    public class IndicatorEntity
    {
        public IndicatorEntity()
        {

        }
        public IndicatorEntity(string indicatorString)
        {
            if (!string.IsNullOrEmpty(indicatorString))
            {
                string[] indParts = indicatorString.Split(',');

                if (indParts.Length > 6)
                {
                    for (int i = 0; i < indParts.Length; i++)
                    {
                        this.Code = indParts[0];
                        this.Period = int.Parse(indParts[1]);
                        if (!string.IsNullOrEmpty(indParts[2]))
                            this.SMA5 = float.Parse(indParts[2]);
                        if (!string.IsNullOrEmpty(indParts[3]))
                            this.BB_H = float.Parse(indParts[3]);
                        if (!string.IsNullOrEmpty(indParts[4]))
                            this.BB_M = float.Parse(indParts[4]);
                        if (!string.IsNullOrEmpty(indParts[5]))
                            this.BB_L = float.Parse(indParts[5]);
                        if (!string.IsNullOrEmpty(indParts[6]))
                            this.MACD = float.Parse(indParts[6]);
                        if (!string.IsNullOrEmpty(indParts[7]))
                            this.MACD_Signal = float.Parse(indParts[7]);
                        if (!string.IsNullOrEmpty(indParts[8]))
                            this.MACD_Hist = float.Parse(indParts[8]);
                        if (!string.IsNullOrEmpty(indParts[9]))
                            this.DIPlus = float.Parse(indParts[9]);
                        if (!string.IsNullOrEmpty(indParts[10]))
                            this.DIMinus = float.Parse(indParts[10]);
                        if (!string.IsNullOrEmpty(indParts[11]))
                            this.ADX = float.Parse(indParts[11]);
                        if (!string.IsNullOrEmpty(indParts[12]))
                            this.WilliamR = float.Parse(indParts[12]);
                        if (!string.IsNullOrEmpty(indParts[13]))
                            this.RSI = float.Parse(indParts[13]);
                        if (!string.IsNullOrEmpty(indParts[14]))
                            this.Stoch_K = float.Parse(indParts[14]);
                        if (!string.IsNullOrEmpty(indParts[15]))
                            this.Stoch_D = float.Parse(indParts[15]);

                    }
                }
            }
        }

        public string Code { get; set; }
        public int Period { get; set; }
        public double? SMA5 { get; set; }
        public double? BB_H { get; set; }
        public double? BB_M { get; set; }
        public double? BB_L { get; set; }
        public double? MACD { get; set; }
        public double? MACD_Signal { get; set; }
        public double? MACD_Hist { get; set; }
        public double? DIPlus { get; set; }
        public double? DIMinus { get; set; }
        public double? ADX { get; set; }
        public double? WilliamR { get; set; }
        public double? RSI { get; set; }
        public double? Stoch_K { get; set; }
        public double? Stoch_D { get; set; }


        public override string ToString()
        {
            return $"{this.Code},{this.Period}," +
                (this.SMA5.HasValue ? this.SMA5.Value.ToString() : "") + "," +
                (this.BB_H.HasValue ? this.BB_H.Value.ToString() : "") + "," +
                (this.BB_M.HasValue ? this.BB_M.Value.ToString() : "") + "," +
                (this.BB_L.HasValue ? this.BB_L.Value.ToString() : "") + "," +
                (this.MACD.HasValue ? this.MACD.Value.ToString() : "") + "," +
                (this.MACD_Signal.HasValue ? this.MACD_Signal.Value.ToString() : "") + "," +
                (this.MACD_Hist.HasValue ? this.MACD_Hist.Value.ToString() : "") + "," +
                (this.DIPlus.HasValue ? this.DIPlus.Value.ToString() : "") + "," +
                (this.DIMinus.HasValue ? this.DIMinus.Value.ToString() : "") + "," +
                (this.ADX.HasValue ? this.ADX.Value.ToString() : "") + "," +
                (this.WilliamR.HasValue ? this.WilliamR.Value.ToString() : "") + "," +
                (this.RSI.HasValue ? this.RSI.Value.ToString() : "");
                //(this.Stoch_K.HasValue ? this.Stoch_K.Value.ToString() : "") + "," +
                //(this.Stoch_D.HasValue ? this.Stoch_D.Value.ToString() : "") ;
        }
    }
}
