using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkeIndustries.VisualRust
{
    class Utils
    {
        public static VisualRustOptions GetOptionsPage(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(typeof(VisualRustOptions)) as VisualRustOptions;
        }
    }
}
