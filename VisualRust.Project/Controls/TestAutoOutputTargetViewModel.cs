using System;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    internal class TestAutoOutputTargetViewModel : AutoOutputTargetViewModel
    {
        public TestAutoOutputTargetViewModel(Manifest m) : base(m)
        {
        }

        public override OutputTargetType Type { get { return OutputTargetType.Test; } }
        public override string TabName { get { return "Autodetect tests"; } }
        public override string Name { get { return AutoDetect; } }
        public override string Path { get { return @"tests\*.rs"; } }
        public override bool Bench { get { return false; } }
    }
}