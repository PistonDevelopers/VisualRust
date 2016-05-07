using System;

namespace VisualRust.Cargo
{
    internal struct OutputTargetsQueryResult : IDisposable
    {
        public RawOutputTargetArray Targets;
        public RawDependencyErrorArray Errors;

        public void Dispose()
        {
            SafeNativeMethods.free_output_targets_result(this);
        }
    }
}