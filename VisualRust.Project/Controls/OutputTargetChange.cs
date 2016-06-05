using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VisualRust.Cargo;
using TargetPair = System.Collections.Generic.KeyValuePair<VisualRust.Project.Controls.OutputTargetViewModel, VisualRust.Cargo.OutputTarget>;

namespace VisualRust.Project.Controls
{
    public struct OutputTargetRemoval
    {
        public UIntPtr Handle { get; private set; }
        public string Type { get; private set; }
        internal OutputTargetRemoval(OutputTarget vm) : this()
        {
            Handle = vm.Handle.Value;
            Type = vm.Type.ToTypeString();
        }
    }

    public class OutputTargetChanges
    {
        public IReadOnlyList<TargetPair> TargetsAdded { get; private set; }
        public IReadOnlyList<OutputTargetRemoval> TargetsRemoved { get; private set; }
        public IReadOnlyList<TargetPair> TargetsChanged { get; private set; }

        public OutputTargetChanges(Manifest manifest, IEnumerable<OutputTargetViewModel> models)
        {
            List<TargetPair> added = new List<TargetPair>();
            List<TargetPair> changed = new List<TargetPair>();
            Dictionary<UIntPtr, OutputTarget> existingTargets = manifest.OutputTargets.ToDictionary(t => t.Handle.Value);
            foreach (var vm in models)
            {
                if (!vm.Handle.HasValue)
                {
                    added.Add(new TargetPair(vm, vm.ToOutputTarget()));
                }
                else
                {
                    OutputTarget diff = Difference(existingTargets[vm.Handle.Value], vm);
                    if (diff != null)
                        changed.Add(new TargetPair(vm, diff));
                    existingTargets.Remove(vm.Handle.Value);
                }
            }
            TargetsAdded = added;
            TargetsRemoved = existingTargets.Select(kvp => new OutputTargetRemoval(kvp.Value)).ToList();
            TargetsChanged = changed;
        }

        OutputTarget Difference(OutputTarget before, OutputTargetViewModel after)
        {
            Debug.Assert(before.Type == after.Type);
            OutputTarget result = new OutputTarget(before.Type) { Handle = before.Handle };
            bool changed = false;
            result.Name = CompareString(before.Name, after.RawName, ref changed);
            result.Path = CompareString(before.Path, after.RawPath, ref changed);
            result.Test = Compare(before.Test, after.RawTest, ref changed);
            result.Doctest = Compare(before.Doctest, after.RawDoctest, ref changed);
            result.Bench = Compare(before.Bench, after.RawBench, ref changed);
            result.Doc = Compare(before.Doc, after.RawDoc, ref changed);
            result.Plugin = Compare(before.Plugin, after.RawPlugin, ref changed);
            result.Harness = Compare(before.Harness, after.RawHarness, ref changed);
            if (!changed)
                return null;
            return result;
        }

        static T? Compare<T>(T? before, T? after, ref bool changed) where T : struct, IEquatable<T>
        {
            if (before.Equals(after))
                return null;
            changed = true;
            return after;
        }

        static string CompareString(string before, string after, ref bool changed)
        {
            if (String.Equals(before, after, StringComparison.InvariantCulture))
                return null;
            changed = true;
            return after;
        }
    }
}