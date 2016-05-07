using System;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    internal class BenchmarkAutoOutputTargetViewModel : AutoOutputTargetViewModel
    {
        public BenchmarkAutoOutputTargetViewModel(Manifest m) : base(m)
        {
        }

        public override OutputTargetType Type { get { return OutputTargetType.Benchmark; } }
        public override string Name { get { return AutoDetect; } }
        public override string Path { get { return @"benches\*.rs"; } }
    }
}