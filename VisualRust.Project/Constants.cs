using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public static class Constants
    {
        public const string BuildPropertyPage = "10C47C08-3645-42C8-BBCA-BCBAA0129DF5";
        public const string ApplicationPropertyPage = "7B1CA802-91F7-4EF4-A1C0-83E40BA72429";
        public const string DebugPropertyPage = "F85DDD2A-EA8D-4CAD-A796-43407E8D91A0";

        public const string BuiltinDebugger = "Built-in Debugger";
        public const string GdbDebugger = "GDB Debugger";

        public static readonly Guid NativeOnlyEngine = new Guid("3B476D35-A401-11D2-AAD4-00C04F990171");
        public static readonly Guid GdbEngine = new Guid("EA6637C6-17DF-45B5-A183-0951C54243BC");
    }
}
