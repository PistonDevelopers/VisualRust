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
        public CargoTarget target { get; set; }
        public RustcMessageJson message { get; set; }
    }
}
