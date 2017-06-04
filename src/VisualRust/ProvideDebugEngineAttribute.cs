using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust
{
    class ProvideDebugEngineAttribute : RegistrationAttribute
    {
        readonly string name;
        readonly Type provider;
        readonly Type engine;
        readonly string id;

        public ProvideDebugEngineAttribute(string name, Type programProvider, Type debugEngine, string id)
        {
            this.name = name;
            this.id = id;
            this.provider = programProvider;
            this.engine = debugEngine;
        }

        public override void Register(RegistrationContext context)
        {
            var engineKey = context.CreateKey("AD7Metrics\\Engine\\" + id);
            engineKey.SetValue("Name", name);
            engineKey.SetValue("CLSID", engine.GUID.ToString("B"));
            engineKey.SetValue("ProgramProvider", provider.GUID.ToString("B"));
            engineKey.SetValue("PortSupplier", "{708C1ECA-FF48-11D2-904F-00C04FA302A1}");

            engineKey.SetValue("Attach", 1);
            engineKey.SetValue("AlwaysLoadLocal", 1);
            engineKey.SetValue("AlwaysLoadProgramProviderLocal", 1);
            engineKey.SetValue("Disassembly", 1);
            engineKey.SetValue("EnginePriority", 0x51);
            engineKey.SetValue("SetNextStatement", 1);
            
            var debuggerRegKey = context.CreateKey("CLSID\\" + engine.GUID.ToString("B"));
            debuggerRegKey.SetValue("Assembly", engine.Assembly.GetName().Name);
            debuggerRegKey.SetValue("Class", engine.FullName);
            debuggerRegKey.SetValue("InprocServer32", context.InprocServerPath);
            debuggerRegKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, engine.Module.Name));

            var provRegKey = context.CreateKey("CLSID\\" + provider.GUID.ToString("B"));
            provRegKey.SetValue("Assembly", provider.Assembly.GetName().Name);
            provRegKey.SetValue("Class", provider.FullName);
            provRegKey.SetValue("InprocServer32", context.InprocServerPath);
            provRegKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, provider.Module.Name));
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
