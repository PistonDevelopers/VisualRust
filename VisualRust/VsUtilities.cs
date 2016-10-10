//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualRust
{
    // This is copied from Microsoft.VisualStudioTools.Project (the old project system aka MPFProj)
    internal static class VsUtilities
    {
        internal static void NavigateTo(IServiceProvider serviceProvider, string filename, Guid docViewGuidType, int line, int col)
        {
            IVsTextView viewAdapter;
            IVsWindowFrame pWindowFrame;
            if (docViewGuidType != Guid.Empty)
            {
                OpenDocument(serviceProvider, filename, docViewGuidType, out viewAdapter, out pWindowFrame);
            }
            else
            {
                OpenDocument(serviceProvider, filename, out viewAdapter, out pWindowFrame);
            }

            ErrorHandler.ThrowOnFailure(pWindowFrame.Show());

            // Set the cursor at the beginning of the declaration.
            ErrorHandler.ThrowOnFailure(viewAdapter.SetCaretPos(line, col));
            // Make sure that the text is visible.
            viewAdapter.CenterLines(line, 1);
        }

        internal static void NavigateTo(IServiceProvider serviceProvider, string filename, Guid docViewGuidType, int pos)
        {
            IVsTextView viewAdapter;
            IVsWindowFrame pWindowFrame;
            if (docViewGuidType != Guid.Empty)
            {
                OpenDocument(serviceProvider, filename, docViewGuidType, out viewAdapter, out pWindowFrame);
            }
            else
            {
                OpenDocument(serviceProvider, filename, out viewAdapter, out pWindowFrame);
            }

            ErrorHandler.ThrowOnFailure(pWindowFrame.Show());
            int line, col;
            ErrorHandler.ThrowOnFailure(viewAdapter.GetLineAndColumn(pos, out line, out col));
            ErrorHandler.ThrowOnFailure(viewAdapter.SetCaretPos(line, col));
            // Make sure that the text is visible.
            viewAdapter.CenterLines(line, 1);
        }

        internal static void OpenDocument(IServiceProvider serviceProvider, string filename, out IVsTextView viewAdapter, out IVsWindowFrame pWindowFrame)
        {
            IVsTextManager textMgr = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));

            IVsUIHierarchy hierarchy;
            uint itemid;
            VsShellUtilities.OpenDocument(
                serviceProvider,
                filename,
                Guid.Empty,
                out hierarchy,
                out itemid,
                out pWindowFrame,
                out viewAdapter);
        }

        internal static void OpenDocument(IServiceProvider serviceProvider, string filename, Guid docViewGuid, out IVsTextView viewAdapter, out IVsWindowFrame pWindowFrame)
        {
            IVsUIHierarchy hierarchy;
            uint itemid;
            VsShellUtilities.OpenDocumentWithSpecificEditor(
                serviceProvider,
                filename,
                docViewGuid,
                Guid.Empty,
                out hierarchy,
                out itemid,
                out pWindowFrame
            );
            viewAdapter = VsShellUtilities.GetTextView(pWindowFrame);
        }
    }
}
