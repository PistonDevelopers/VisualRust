using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Core
{
    public interface IActionLog
    {
        void Trace(string msg);
        void Error(string msg);
    }
}
