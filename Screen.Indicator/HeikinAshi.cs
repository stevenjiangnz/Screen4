using System;


namespace Screen.Indicator
{
    public class HeikinAshi
    {
        public static Result Calculate(double[] inOpen, double[] inClose, double[] inHigh, double[] inLow,
            double?[] outOpen, double?[] outClose, double?[] outHigh, double?[] outLow)
        {
            Result result = new Result();
            result.Status = ResultStatus.Success;

            int len = inOpen.Length;

            outOpen[0] = (inOpen[0] + inClose[0]) / 2;
            outClose[0] = (inOpen[0] + inClose[0] + inHigh[0] + inLow[0]) / 4;
            outHigh[0] = inHigh[0];
            outLow[0] = inLow[0];

            for (int i = 1; i < len; i++)
            {
                outClose[i] = Math.Round((inOpen[i] + inHigh[i] + inLow[i] + inClose[i]) / 4, 4);
                outOpen[i] = Math.Round(((outOpen[i - 1] + outClose[i - 1]) / 2).Value, 4);

                var h2 = (outClose[i] > outOpen[i]) ? outClose[i] : outOpen[i];
                outHigh[i] = Math.Round(((h2 > inHigh[i]) ? h2 : inHigh[i]).Value, 4);

                var l2 = (outClose[i] < outOpen[i]) ? outClose[i] : outOpen[i];
                outLow[i] = Math.Round(((l2 < inLow[i]) ? l2 : inLow[i]).Value, 4);
            }


            return result;
        }
    }
}