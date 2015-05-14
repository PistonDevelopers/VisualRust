using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace VisualRust
{
    /// <summary>
    /// This is copied from SassyStudio (https://github.com/darrenkopp/SassyStudio)
    /// </summary>
    abstract class VSCommandTarget<T> : IOleCommandTarget
    {
        readonly IOleCommandTarget _NextCommandTarget;
        protected readonly IWpfTextView TextView;
        protected readonly Guid CommandGroupId;
        protected readonly HashSet<uint> CommandIdSet;

        protected VSCommandTarget(IVsTextView vsTextView, IWpfTextView textView)
        {
            TextView = textView;
            CommandGroupId = typeof(T).GUID;
            CommandIdSet = new HashSet<uint>(SupportedCommands.Select(x => ConvertFromCommand(x)));

            _NextCommandTarget = AttachTo(vsTextView, this);
        }

        protected virtual bool IsEnabled { get { return true; } }
        protected virtual bool SupportsAutomation { get { return false; } }
        protected abstract IEnumerable<T> SupportedCommands { get; }
        protected abstract T ConvertFromCommandId(uint id);
        protected abstract uint ConvertFromCommand(T command);

        protected abstract bool Execute(T command, uint options, IntPtr pvaIn, IntPtr pvaOut);

        protected bool ExecuteNext(T command, uint options, IntPtr pvaIn, IntPtr pvaOut)
        {
            Guid pguidCmdGroup = CommandGroupId;

            return _NextCommandTarget.Exec(ref pguidCmdGroup, ConvertFromCommand(command), options, pvaIn, pvaOut) == VSConstants.S_OK;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (SupportsAutomation || !VsShellUtilities.IsInAutomationFunction(VisualRustPackage.Instance))
            {
                if (pguidCmdGroup == CommandGroupId && CommandIdSet.Contains(nCmdID))
                {
                    bool result = Execute(ConvertFromCommandId(nCmdID), nCmdexecopt, pvaIn, pvaOut);

                    if (result)
                        return VSConstants.S_OK;
                }
            }

            return _NextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == CommandGroupId)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    if (CommandIdSet.Contains(prgCmds[i].cmdID))
                    {
                        if (IsEnabled)
                        {
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        }

                        prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                    }
                }
            }

            return _NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        static IOleCommandTarget AttachTo(IVsTextView view, IOleCommandTarget command)
        {
            IOleCommandTarget next;
            view.AddCommandFilter(command, out next);
            return next;
        }
    }
}