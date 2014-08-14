using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;


namespace ArkeIndustries.VisualRust
{
    class VisualRustOptions : DialogPage
    {    
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool SmartIndent { get; set; }
    }
}
