using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project.Controls
{
    public interface IPropertyPageContext
    {
        event EventHandler DirtyChanged;
        bool IsDirty { get; }
        void Apply();
    }
}
