using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Shared;

namespace VisualRust.Project.Configuration
{
    partial class Application
    {
        private static BuildOutputType OutputTypeFromString(string p)
        {
            return BuildOutputTypeExtension.Parse(p);
        }

        private static string OutputTypeToString(BuildOutputType OutputType)
        {
            return OutputType.ToBuildString();
        }
    }
}
