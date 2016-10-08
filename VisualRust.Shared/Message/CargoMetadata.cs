using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Shared.Message
{
    public class CargoMetadata
    {
        public List<CargoPackage> packages { get; set; }
        // ... more fields omitted, can be added as necessary
    }

    public class CargoPackage
    {
        public string name { get; set; }
        public string version { get; set; } // TODO: use better type
        public string id { get; set; }
        public string manifest_path { get; set; }
        // ... more fields omitted, can be added as necessary
    }
}
