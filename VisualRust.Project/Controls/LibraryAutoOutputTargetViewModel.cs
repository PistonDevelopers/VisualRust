using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;
using VisualRust.Project.Controls;

namespace VisualRust.Project.Controls
{
    class LibraryAutoOutputTargetViewModel : AutoOutputTargetViewModel
    {
        public LibraryAutoOutputTargetViewModel(Manifest m) : base(m)
        {
        }

        public override OutputTargetType Type { get { return OutputTargetType.Library; } }
        public override string Name { get { return Manifest.Name; } }
        public override string Path { get { return @"src\lib.rs"; } }
    }
}
