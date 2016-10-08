using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Shared.Message
{
    // See /src/cargo/util/machine_message.rs in the cargo repository
    public class CargoMessage
    {
        public string reason { get; set; }
        public string package_id { get; set; }
        public CargoMessageTarget target { get; set; }
        public RustcMessageJson message { get; set; }
    }

    public class CargoMessageTarget
    {
        public string text { get; set; }
        public int highlight_start { get; set; }
        public int highlight_end { get; set; }
    }
}
