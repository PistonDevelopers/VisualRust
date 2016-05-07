using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using VisualRust.Project.Controls;

namespace VisualRust.Project.Forms
{
    [ComVisible(true)]
    public abstract class WpfPropertyPage : IPropertyPage
    {
        IPropertyPageContext data;
        HwndSource source;
        FrameworkElement control;

        protected IPropertyPageSite Site { get; private set; }
        public abstract string Name { get; }
        internal RustProjectNodeProperties Config { get; private set; }

        public void Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            int width = pRect[0].right - pRect[0].left;
            int height = pRect[0].bottom - pRect[0].top;
            source = new HwndSource(0, 0x54000000, 0, pRect[0].left, pRect[0].top, width, height, "", hWndParent);
            control = CreateControl();
            control.DataContext = data;
            source.RootVisual = control;
        }

        abstract protected FrameworkElement CreateControl();
        abstract protected IPropertyPageContext CreateDataContext();

        public int Apply()
        {
            try
            {
                data.Apply();
                return VSConstants.S_OK;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public void Deactivate()
        {
            source.Dispose();
        }

        public void GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            Utilities.ArgumentNotNull("pPageInfo", pPageInfo);

            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = Name;
            if (source == null)
            {
                info.SIZE.cx = 0;
                info.SIZE.cy = 0;
            }
            else
            {
                RECT[] pRect;
                NativeMethods.GetWindowRect(source.Handle, out pRect);
                int width = pRect[0].right - pRect[0].left;
                int height = pRect[0].bottom - pRect[0].top;
                info.SIZE.cx = width;
                info.SIZE.cx = height;
            }
            pPageInfo[0] = info;
        }

        public void Help(string pszHelpDir)
        {
        }

        public void Move(RECT[] pRect)
        {
            int width = pRect[0].right - pRect[0].left;
            int height = pRect[0].bottom - pRect[0].top;
            NativeMethods.SetWindowPos(this.source.Handle, IntPtr.Zero, pRect[0].left, pRect[0].top, width, height, 0x44);
        }

        public void SetObjects(uint cObjects, object[] ppunk)
        {
            if (ppunk == null)
            {
                return;
            }

            if (cObjects > 0)
            {
                RustProjectNodeProperties ctx = ppunk[0] as RustProjectNodeProperties;
                if(ctx != null)
                {
                    this.Config = ctx;
                    data = CreateDataContext();
                    data.DirtyChanged += (_, __) => Site.OnStatusChange((uint)(data.IsDirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                }
            }
        }

        public void SetPageSite(IPropertyPageSite pPageSite)
        {
            Site = pPageSite;
        }

        public void Show(uint nCmdShow)
        {
            if (nCmdShow == NativeMethods.SW_SHOW || nCmdShow == NativeMethods.SW_SHOWNORMAL)
                control.Visibility = Visibility.Visible;
            if (nCmdShow == NativeMethods.SW_HIDE)
                control.Visibility = Visibility.Collapsed;
        }

        public int TranslateAccelerator(Microsoft.VisualStudio.OLE.Interop.MSG[] pMsg)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int IsPageDirty()
        {
            if(data == null)
                return VSConstants.S_FALSE;
            return data.IsDirty ? VSConstants.S_OK : VSConstants.S_FALSE;
        }
    }
}
