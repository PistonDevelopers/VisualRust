using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test
{
    class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory parent;
        public string DirPath { get; private set; }

        public TemporaryDirectory()
        {
            string tempPath = Path.GetTempPath();
            DirPath = Path.Combine(tempPath, Path.GetRandomFileName());
            Directory.CreateDirectory(DirPath);
        }

        private TemporaryDirectory(string name,  TemporaryDirectory par)
        {
            DirPath = name;
            parent = par;
            Directory.CreateDirectory(DirPath);
        }

        public void Dispose()
        {
            if (parent != null)
                parent.Dispose();
            else
                Directory.Delete(DirPath, true);
        }

        // Only single sub temporary dir is currently supported
        public TemporaryDirectory SubDir(string name)
        {
            return new TemporaryDirectory(Path.Combine(this.DirPath, name), this);
        }
    }
}
