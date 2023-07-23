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
        public string GoogleServiceAccountKey { get; set; }
        public string GoogleRootId { get; set; }
        public ETFSettings ETFSettings { get; set; }
        public ASXSettings ASXSettings { get; set; }

    }

    public class ETFSettings
    {
        public string Suffix { get; set; }
    }

    public class ASXSettings
    {
        public string Suffix { get; set; }
    }

}
