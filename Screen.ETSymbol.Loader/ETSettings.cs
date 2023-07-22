using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ETSymbol.Loader
{
    public class AppSettings
    {
        public ETSettings ETSettings { get; set; }
    }

    public class ETSettings
    {
        public string BaseUrl { get; set; }
        public ETFSettings ETFSettings { get; set; }
    }

    public class ETFSettings
    {
        public string Suffix { get; set; }
    }

}
