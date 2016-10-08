using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualRust.Shared;

namespace VisualRust.Build
{
    /// <summary>
    /// Task that wraps a cargo command and runs it in the directory of a given manifest
    /// </summary>
    public abstract class CargoTask : Microsoft.Build.Utilities.Task
    {
        public string ManifestPath { get; set; }

        public FileInfo Manifest
        {
            get
            {
                return new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), ManifestPath));
            }
        }

        protected abstract bool ExecuteCargo(Cargo cargo);

        public override bool Execute()
        {
            var cargo = Cargo.FindSupportedInstallation();
            if (cargo == null)
            {
                LogCargoError("No supported installation of cargo.exe found. A version from after 2016-10-06 is required.");
                return false;
            }
            cargo.WorkingDirectory = Manifest.Directory.FullName;
            Log.LogMessage(MessageImportance.Low, "Running cargo in directory {0}", cargo.WorkingDirectory);
            return ExecuteCargo(cargo);
        }

        /// <summary>
        /// Helper method to log an error as coming from Cargo.toml
        /// </summary>
        protected void LogCargoError(string message, params object[] messageArgs)
        {
            Log.LogError(null, null, null, Manifest.FullName, 0, 0, 0, 0, message, messageArgs);
        }
    }
}
