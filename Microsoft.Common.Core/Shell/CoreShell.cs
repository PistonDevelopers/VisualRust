// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Common.Core.Shell {
    public static class CoreShell {
        // Normally shell object is set by the package or other top-level
        // application object that implements services needed by various 
        // modules such as MEF composition container and so on. However, 
        // in tests the application is not and objects often are instantiated
        // in isolation. In this case code uses reflection to instatiate 
        // service provider with a specific name.
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", 
                         MessageId = "System.Reflection.Assembly.LoadFrom", 
                         Justification ="Needed for test shell creation")]
        public static void TryCreateTestInstance(string assemblyName, string className) {
            string thisAssembly = Assembly.GetExecutingAssembly().GetAssemblyPath();
            string assemblyLoc = Path.GetDirectoryName(thisAssembly);
            string packageTestAssemblyPath = Path.Combine(assemblyLoc, assemblyName);
            Assembly testAssembly = null;

            // Catch exception when loading assembly since it is missing in non-test
            // environment but do throw when it is present but test types cannot be created.
            try {
                testAssembly = Assembly.LoadFrom(packageTestAssemblyPath);
            }
            catch(Exception) { }

            if (testAssembly != null) {
                Type[] types = testAssembly.GetTypes();
                IEnumerable<Type> classes = types.Where(x => x.IsClass);

                Type testAppShell = classes.FirstOrDefault(c => c.Name.Contains(className));
                Debug.Assert(testAppShell != null);

                MethodInfo mi = testAppShell.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
                mi.Invoke(null, null);
            }
        }
    }
}
