using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;
using VisualRust.Project.Controls;

namespace VisualRust.Project
{
    public class ManifestFile
    {
        private IFileSystem fs;
        public string Path { get; private set; }
        public Manifest Manifest { get; private set; }

        public static ManifestFile Create(IFileSystem fs, string startingPath, Func<string, ManifestLoadResult> fileOpener)
        {
            var file = new ManifestFile();
            file.fs = fs;
            ManifestLoadResult loadResult = fileOpener(startingPath);
            file.Path = loadResult.Path;
            file.Manifest = loadResult.Manifest;
            return file;
        }

        public void Apply(OutputTargetChanges changes)
        {
            foreach(OutputTargetRemoval removed in changes.TargetsRemoved)
            {
                this.Manifest.Remove(removed.Handle, removed.Type);
            }
            foreach(KeyValuePair<OutputTargetViewModel, OutputTarget> target in changes.TargetsChanged)
            {
               UIntPtr handle = this.Manifest.Set(target.Value);
                if(handle != UIntPtr.Zero)
                    target.Key.Handle = handle;
            }
            foreach(KeyValuePair<OutputTargetViewModel, OutputTarget> target in changes.TargetsAdded)
            {
                UIntPtr handle = this.Manifest.Add(target.Value);
                if(handle != UIntPtr.Zero)
                    target.Key.Handle = handle;
            }
            fs.File.WriteAllText(Path, Manifest.ToString());
        }
    }
}
