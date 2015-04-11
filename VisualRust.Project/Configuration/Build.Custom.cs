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
    partial class Build
    {
        private static OptimizationLevel OptimizationLevelFromString(string p)
        {
            return OptimizationLevelExtension.Parse(p);
        }

        private string OptimizationLevelToString(OptimizationLevel OptimizationLevel)
        {
            return optimizationLevel.ToBuildString();
        }

        private static string PlatformTargetFromString(string p)
        {
            if(String.IsNullOrEmpty(p))
                return Shared.Environment.DefaultTarget;
            return p;
        }

        private string PlatformTargetToString(string target)
        {
            return target;
        }

    }
}
