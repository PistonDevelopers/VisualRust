using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;

namespace VisualRust.Project
{
    class ManifestLoadResult
    {
        public bool Cancel { get; private set; }
        public Manifest Manifest { get; private set; }

        private ManifestLoadResult() { }

        public static ManifestLoadResult CreateCancel()
        {
            return new ManifestLoadResult
            {
                Cancel = true
            };
        }

        public static ManifestLoadResult CreateSuccess(Manifest m)
        {
            return new ManifestLoadResult
            {
                Manifest = m
            };
        }
    }
}
