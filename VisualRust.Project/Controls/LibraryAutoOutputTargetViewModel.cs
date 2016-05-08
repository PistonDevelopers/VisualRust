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
        public override string TabName { get { return "Autodetect library"; } }
        public override string Name { get { return Manifest.Name; } }
        public override string Path { get { return @"src\lib.rs"; } }
        public override bool Doctest { get { return true; } }
        public override bool Doc { get { return true; } }
    }
}
