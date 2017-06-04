using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct StringQueryResult : IDisposable
    {
        public Utf8String Result;
        public QueryError Error;

        public void Dispose()
        {
            Utf8String.Drop(this.Result);
        }
    }
}
