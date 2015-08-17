using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;
using Microsoft.MIDebugEngine;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using VisualRust.Project;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;
using VisualRust.Options;

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
    [InstalledProductRegistration("#110", "#112", "0.1.1", IconResourceID = 400)]
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
    [ProvideProjectFactory(
        typeof(RustProjectFactory),
        "Rust",
        "Rust Project Files (*.rsproj);*.rsproj",
        "rsproj",
        "rsproj",
        ".\\NullPath",
        LanguageVsTemplate="Rust")]
    [ProvideLanguageExtension(typeof(RustLanguage), ".rs")]
    [Guid(GuidList.guidVisualRustPkgString)]
    [ProvideObject(typeof(Project.Forms.ApplicationPropertyPage))]
    [ProvideObject(typeof(Project.Forms.BuildPropertyPage))]
    [ProvideObject(typeof(Project.Forms.DebugPropertyPage))]
    [ProvideOptionPage(typeof(RustOptionsPage), "Visual Rust", "General", 110, 113, true)]
    [ProvideOptionPage(typeof(DebuggingOptionsPage), "Visual Rust", "Debugging", 110, 114, true)]
    [ProvideProfile(typeof(RustOptionsPage), "Visual Rust", "General", 110, 113, true)]
    [ProvideDebugEngine("Rust GDB", typeof(AD7ProgramProvider), typeof(AD7Engine), EngineConstants.EngineId)]
    public class VisualRustPackage : CommonProjectPackage
    {
        private RunningDocTableEventsListener docEventsListener;
        internal static VisualRustPackage Instance { get; private set; }

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
            Microsoft.VisualStudioTools.UIThread.InitializeAndAlwaysInvokeToCurrentThread();
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
            Instance = this;

            docEventsListener = new RunningDocTableEventsListener((IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable)));

            Racer.RacerSingleton.Init();
        }

        protected override void Dispose(bool disposing)
        {
            docEventsListener.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        public override ProjectFactory CreateProjectFactory()
        {
            return new RustProjectFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactory()
        {
            return null;
        }

        public override uint GetIconIdForAboutBox()
        {
            throw new NotImplementedException();
        }

        public override uint GetIconIdForSplashScreen()
        {
            throw new NotImplementedException();
        }

        public override string GetProductName()
        {
            return "Visual Rust";
        }

        public override string GetProductDescription()
        {
            return "Visual Studio integration for the Rust programming language (http://www.rust-lang.org/)";
        }

        public override string GetProductVersion()
        {
            return "0.1.1";
        }
    }
}
