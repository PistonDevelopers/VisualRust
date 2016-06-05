using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;

namespace VisualRust.Project
{
    public class ManifestLoadResult
    {
        public bool Cancel { get; private set; }
        public string Path { get; private set; }
        public Manifest Manifest { get; private set; }

        private ManifestLoadResult() { }

        public static ManifestLoadResult CreateCancel(string path)
        {
            return new ManifestLoadResult
            {
                Path = path,
                Cancel = true
            };
        }

        public static ManifestLoadResult CreateSuccess(string path, Manifest m)
        {
            return new ManifestLoadResult
            {
                Path = path,
                Manifest = m
            };
        }
    }
}
