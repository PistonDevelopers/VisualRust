using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;
//using Microsoft.MIDebugEngine;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
//using VisualRust.Project;
//using Microsoft.VisualStudioTools.Project;
//using Microsoft.VisualStudioTools.Project.Automation;
using VisualRust.Options;
//using MICore;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Package.Registration;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using System.Collections.Generic;
using VisualRust.ProjectSystem;

namespace VisualRust
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "0.1.2", IconResourceID = 400)]
    [ProvideLanguageService(typeof(RustLanguage), "Rust", 100, 
        CodeSense = true, 
        DefaultToInsertSpaces = true,
        EnableCommenting = true,
        MatchBraces = true,
        MatchBracesAtCaret = true,
        ShowCompletion = false,
        ShowMatchingBrace = true,
        QuickInfo = false, 
        AutoOutlining = true,
        ShowSmartIndent = true, 
        EnableLineNumbers = true, 
        EnableFormatSelection = true,
        SupportCopyPasteOfHTML = false
    )]
    /*[ProvideProjectFactory(
        typeof(RustProjectFactory),
        "Rust",
        "Rust Project Files (*.rsproj);*.rsproj",
        "rsproj",
        "rsproj",
        ".\\NullPath",
        LanguageVsTemplate="Rust")]*/
    [ProvideLanguageExtension(typeof(RustLanguage), ".rs")]
    [Guid(GuidList.guidVisualRustPkgString)]
    //[ProvideObject(typeof(Project.Forms.ApplicationPropertyPage))]
    //[ProvideObject(typeof(Project.Forms.BuildPropertyPage))]
    //[ProvideObject(typeof(Project.Forms.DebugPropertyPage))]
    //[ProvideObject(typeof(Project.Forms.TargetOutputsPropertyPage))]
    [ProvideOptionPage(typeof(RustOptionsPage), "Visual Rust", "General", 110, 113, true)]
    [ProvideOptionPage(typeof(DebuggingOptionsPage), "Visual Rust", "Debugging", 110, 114, true)]
    [ProvideProfile(typeof(RustOptionsPage), "Visual Rust", "General", 110, 113, true)]
    //[ProvideDebugEngine("Rust GDB", typeof(AD7ProgramProvider), typeof(AD7Engine), EngineConstants.EngineId)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideCpsProjectFactory(GuidList.CpsProjectFactoryGuidString, "Rust")]
    [ProvideProjectFileGenerator(typeof(RustProjectFileGenerator), GuidList.CpsProjectFactoryGuidString, FileNames = "Cargo.toml", DisplayGeneratorFilter = 300)]
    // TODO: not sure what DeveloperActivity actually does
    [DeveloperActivity("Rust", GuidList.guidVisualRustPkgString, sortPriority: 40)]
    public class VisualRustPackage : Package, IOleCommandTarget
    {
        //private RunningDocTableEventsListener docEventsListener;
        private IOleCommandTarget packageCommandTarget;
        private Dictionary<IVsProjectGenerator, uint> _projectFileGenerators;

        internal static VisualRustPackage Instance { get; private set; }

        public const string UniqueCapability = "VisualRust";

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VisualRustPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            //Microsoft.VisualStudioTools.UIThread.InitializeAndAlwaysInvokeToCurrentThread();
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        ///
        protected override void Initialize()
        {
            base.Initialize();

            RegisterProjectFileGenerator(new RustProjectFileGenerator());

            ProjectIconProvider.LoadProjectImages();

            packageCommandTarget = GetService(typeof(IOleCommandTarget)) as IOleCommandTarget;
            Instance = this;

            //docEventsListener = new RunningDoc2TableEventsListener((IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable)));

            Racer.RacerSingleton.Init();
        }

        // TODO: add UnregisterProjectFileGenerators
        private void RegisterProjectFileGenerator(IVsProjectGenerator projectFileGenerator)
        {
            var registerProjectGenerators = GetService(typeof(SVsRegisterProjectTypes)) as IVsRegisterProjectGenerators;
            if (registerProjectGenerators == null)
            {
                throw new InvalidOperationException(typeof(SVsRegisterProjectTypes).FullName);
            }

            uint cookie;
            Guid riid = projectFileGenerator.GetType().GUID;
            registerProjectGenerators.RegisterProjectGenerator(ref riid, projectFileGenerator, out cookie);

            if (_projectFileGenerators == null)
            {
                _projectFileGenerators = new Dictionary<IVsProjectGenerator, uint>();
            }

            _projectFileGenerators[projectFileGenerator] = cookie;
        }

        int IOleCommandTarget.Exec(ref Guid cmdGroup, uint nCmdID, uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == GuidList.VisualRustCommandSet)
            {
                switch (nCmdID)
                {
                    //case 1:
                    //    return VRDebugExec(nCmdExecOpt, pvaIn, pvaOut);

                    default:
                        return VSConstants.E_NOTIMPL;
                }
            }
            return packageCommandTarget.Exec(cmdGroup, nCmdID, nCmdExecOpt, pvaIn, pvaOut);
        }

        int IOleCommandTarget.QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (cmdGroup == GuidList.VisualRustCommandSet)
            {
                switch (prgCmds[0].cmdID)
                {
                    case 1:
                        prgCmds[0].cmdf |= (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_INVISIBLE);
                        return VSConstants.S_OK;

                    default:
                        Debug.Fail("Unknown command id");
                        return VSConstants.E_NOTIMPL;
                }
            }

            return packageCommandTarget.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }

        //private int VRDebugExec(uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        //{
        //    int hr;

        //    if (IsQueryParameterList(pvaIn, pvaOut, nCmdExecOpt))
        //    {
        //        Marshal.GetNativeVariantForObject("$", pvaOut);
        //        return VSConstants.S_OK;
        //    }

        //    string arguments;
        //    hr = EnsureString(pvaIn, out arguments);
        //    if (hr != VSConstants.S_OK)
        //        return hr;

        //    if (string.IsNullOrWhiteSpace(arguments))
        //        throw new ArgumentException("Expected an MI command to execute (ex: Debug.VRDebugExec info sharedlibrary)");

        //    VRDebugExecAsync(arguments);

        //    return VSConstants.S_OK;
        //}

        //private async void VRDebugExecAsync(string command)
        //{
        //    var commandWindow = (IVsCommandWindow)GetService(typeof(SVsCommandWindow));

        //    string results = null;

        //    try
        //    {
        //        results = await MIDebugCommandDispatcher.ExecuteCommand(command);
        //    }
        //    catch (Exception e)
        //    {
        //        if (e.InnerException != null)
        //            e = e.InnerException;

        //        UnexpectedMIResultException miException = e as UnexpectedMIResultException;
        //        string message;
        //        if (miException != null && miException.MIError != null)
        //            message = miException.MIError;
        //        else
        //            message = e.Message;

        //        commandWindow.Print(string.Format("Error: {0}\r\n", message));
        //        return;
        //    }

        //    if (results.Length > 0)
        //    {
        //        // Make sure that we are printing whole lines
        //        if (!results.EndsWith("\n") && !results.EndsWith("\r\n"))
        //        {
        //            results = results + "\n";
        //        }

        //        commandWindow.Print(results);
        //    }
        //}

        static private bool IsQueryParameterList(System.IntPtr pvaIn, System.IntPtr pvaOut, uint nCmdexecopt)
        {
            ushort lo = (ushort)(nCmdexecopt & (uint)0xffff);
            ushort hi = (ushort)(nCmdexecopt >> 16);
            if (lo == (ushort)OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP)
            {
                if (hi == Microsoft.VisualStudio.Shell.VsMenus.VSCmdOptQueryParameterList)
                {
                    if (pvaOut != IntPtr.Zero)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static private int EnsureString(IntPtr pvaIn, out string arguments)
        {
            arguments = null;
            if (pvaIn == IntPtr.Zero)
            {
                // No arguments.
                return VSConstants.E_INVALIDARG;
            }

            object vaInObject = Marshal.GetObjectForNativeVariant(pvaIn);
            if (vaInObject == null || vaInObject.GetType() != typeof(string))
            {
                return VSConstants.E_INVALIDARG;
            }

            arguments = vaInObject as string;
            return VSConstants.S_OK;
        }

        protected override void Dispose(bool disposing)
        {
            ProjectIconProvider.Close();
            //docEventsListener.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        //public override ProjectFactory CreateProjectFactory()
        //{
        //    return new RustProjectFactory(this);
        //}

        //public override CommonEditorFactory CreateEditorFactory()
        //{
        //    return null;
        //}

        //public override uint GetIconIdForAboutBox()
        //{
        //    throw new NotImplementedException();
        //}

        //public override uint GetIconIdForSplashScreen()
        //{
        //    throw new NotImplementedException();
        //}

        //public override string GetProductName()
        //{
        //    return "Visual Rust";
        //}

        //public override string GetProductDescription()
        //{
        //    return "Visual Studio integration for the Rust programming language (http://www.rust-lang.org/)";
        //}

        //public override string GetProductVersion()
        //{
        //    return "0.1.2";
        //}
    }
}
