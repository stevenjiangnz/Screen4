using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public abstract  class BaseIndicator<TInput, TSetting> 
    {
        public abstract  Result Calculate(TInput input, TSetting settings); 
    }
}