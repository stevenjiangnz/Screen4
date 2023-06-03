using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Utils
{
    public class DateHelper
    {
        public static DateTime ToDate(int period)
        {
            int year = int.Parse(period.ToString().Substring(0, 4));
            int month = int.Parse(period.ToString().Substring(4, 2));
            int day = int.Parse(period.ToString().Substring(6, 2));
            DateTime dt = new DateTime(year, month, day);

            return dt;
        }

        public static long ToTimeStamp(int period)
        {
            DateTime dt = DateHelper.ToDate(period);

            var epoch = dt - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)epoch.TotalSeconds;
        }

        public static long ToTimeStamp(DateTime dt)
        {
            return ToTimeStamp(ToInt(dt));
        }

        public static int ToInt(DateTime dt)
        {
            int year = dt.Year;
            int month = dt.Month;
            int day = dt.Day;

            return year * 10000 + month * 100 + day;
        }

        public static int BeginOfWeek(int intDate, DayOfWeek beginOfWeek = DayOfWeek.Monday)
        {
            DateTime dt = ToDate(intDate);

            while (dt.DayOfWeek != beginOfWeek)
            {
                dt = dt.AddDays(-1);
            }

            return ToInt(dt);
        }

    }
}
