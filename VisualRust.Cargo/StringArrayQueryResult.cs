using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct StringArrayQueryResult : IDisposable
    {
        public Utf8StringArray Result;
        public QueryError Error;

        public void Dispose()
        {
            Utf8StringArray.Drop(this.Result);
        }
    }
}
