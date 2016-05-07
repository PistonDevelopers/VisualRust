using System;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    internal class ExampleAutoOutputTargetViewModel : AutoOutputTargetViewModel
    {
        public ExampleAutoOutputTargetViewModel(Manifest m) : base(m)
        {
        }

        public override OutputTargetType Type { get { return OutputTargetType.Example; } }
        public override string Name { get { return AutoDetect; } }
        public override string Path { get { return @"examples\*.rs"; } }
    }
}