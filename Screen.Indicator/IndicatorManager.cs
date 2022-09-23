using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Screen.Shared;

namespace Screen.Indicator
{
    public class IndicatorManager
    {
        private SharedSettings _settings;

        public IndicatorManager(SharedSettings settings)
        {
            _settings = settings;
        }

        public void ProcessIndicatorsForCode(IList<TickerEntity> tickerList)
        {
            if (tickerList == null || tickerList.Count == 0)
            {
                return;
            }

            int length = tickerList.Count;
            double[] close = tickerList.Select(p => (double)p.C).ToArray();
            double[] open = tickerList.Select(p => (double)p.O).ToArray();
            double[] high = tickerList.Select(p => (double)p.H).ToArray();
            double[] low = tickerList.Select(p => (double)p.L).ToArray();


            // SMA5
            double?[] outSMA5 = new double?[length];
            SMA.Calculate(close, 5, outSMA5);

            // BB
            double?[] outBB_H = new double?[length];
            double?[] outBB_M = new double?[length];
            double?[] outBB_L = new double?[length];

            BollingerBand.Calculate(close, 20, 2, outBB_M, outBB_H, outBB_L);

            // MACD
            double?[] outMACD = new double?[length];
            double?[] outMACD_Signal = new double?[length];
            double?[] outMACD_Hist = new double?[length];

            MACD.Calculate(close, 26, 12, 9, outMACD, outMACD_Signal, outMACD_Hist);

            // ADX
            double?[] outDiPlus = new double?[length];
            double?[] outDiMinus = new double?[length];
            double?[] outADX = new double?[length];

            ADX.Calculate(high, low, close, outDiPlus, outDiMinus, outADX);

            // WilliamR
            double?[] outWilliamR = new double?[length];
            WilliamR.Calculate(close, high, low, 14, outWilliamR);

            // RSI
            double?[] outRSI = new double?[length];
            RSI.Calculate(close, 14, outRSI);

            //// Stochastic
            //double?[] outStoch_K = new double?[length];
            //double?[] outStoch_D = new double?[length];
            //Stochastic.CalculateSlow(close, high, low, 14, 3, outStoch_K, outStoch_D);

            IList<IndicatorEntity> indList = new List<IndicatorEntity>();

            for (int i = 0; i < length; i++)
            {
                TickerEntity t = tickerList[i];

                IndicatorEntity indicator = new IndicatorEntity()
                {
                    Code = t.T,
                    Period = t.P,
                    SMA5 = outSMA5[i],
                    BB_H = outBB_H[i],
                    BB_M = outBB_M[i],
                    BB_L = outBB_L[i],
                    MACD = outMACD[i],
                    MACD_Signal = outMACD_Signal[i],
                    MACD_Hist = outMACD_Hist[i],
                    DIPlus = outDiPlus[i],
                    DIMinus = outDiMinus[i],
                    ADX = outADX[i],
                    WilliamR = outWilliamR[i],
                    RSI = outRSI[i],
                    //Stoch_K = outStoch_K[i],
                    //Stoch_D = outStoch_D[i],
                };

                indList.Add(indicator);
            }

            SaveIndicatorsToFile(indList);
        }

        public void SaveIndicatorsToFile(IList<IndicatorEntity> indList)
        {
            if (indList == null || indList.Count == 0)
            {
                return;
            }

            string code = string.Empty;

            try
            {
                code = indList[0].Code;

                var indicatorFolder = Path.Combine(_settings.BasePath, _settings.IndicatorPath);

                string filePath = Path.Combine(indicatorFolder, code + "_indicator.txt");

                Directory.CreateDirectory(indicatorFolder);

                StringBuilder sb = new StringBuilder();

                foreach (IndicatorEntity indicator in indList)
                {
                    sb.Append(indicator.ToString() + Environment.NewLine);
                }

                File.WriteAllText(filePath, sb.ToString());

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in saving indicator for {code}");
            }
        }
    }
}
