using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct DependenciesQueryResult : IDisposable
    {
        public RawDependencyArray Dependencies;
        public RawDependencyErrorArray Errors;

        public void Dispose()
        {
            SafeNativeMethods.free_dependencies_result(this);
        }
    }
}
