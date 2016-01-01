using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Shared;

namespace VisualRust.Project.Launcher
{
    public interface IRustProjectLauncher
    {
        void Launch(string path, string args, string workingDir);
    }
}
