using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Utils
{
    public class StringHelper
    {
        public static List<String> SplitToLines(string inputText)
        {
            List<String> lineList = new List<string>();
            string[] lines = inputText.Split(
                new[] { Environment.NewLine, "\n" },
                StringSplitOptions.None
            );

            for (int i = 0; i < lines.Length; i++)
            {
                if (!String.IsNullOrEmpty(lines[i]))
                {
                    lineList.Add(lines[i]);
                }
            }
            return lineList;
        }
    }
}
