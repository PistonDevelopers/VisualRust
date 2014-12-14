using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    // Strong typing for indexes of images in Resources/IconList.bmp
    enum IconIndex
    {
        NoIcon = -1,
        RustProject = 0,
        RustFile,
        UntrackedRustFile,
        UntrackedFolder,
        UntrackedFolderOpen,
    }
}
