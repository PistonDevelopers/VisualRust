using System;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Application
    {

        public bool IsEqual(Application obj)
        {
            return true
            ;
        }

        public Application Clone()
        {
            return new Application
            {
            };
        }

        public static Application LoadFrom(CommonProjectNode proj)
        {
            var x = new Application();
            return x;
        }

        public void SaveTo(CommonProjectNode proj)
        {
        }
    }
}

