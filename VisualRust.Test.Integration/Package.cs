using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio;
using System.Windows.Automation;

namespace VisualRust.Test.Integration
{
    using EnvDTE;
    using Microsoft.VisualStudioTools.Project;
    using Microsoft.VisualStudioTools.Project.Automation;
    using System.IO;
    using VisualRust.Project;
    using Microsoft.VisualStudio.OLE.Interop;
    using System.Windows.Automation;
    using System.Windows.Forms;

    [TestClass]
    public class Package
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [HostType("VS IDE")]
        [TestCategory("Explicit")]
#if VS12
        [TestProperty("VsHiveName", "12.0Exp")]
#elif VS14
        [TestProperty("VsHiveName", "14.0Exp")]
#endif
        public void PackageLoad()
        {
            UIThreadInvoker.Invoke((Action)delegate() 
            {
                IVsShell shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
                Assert.IsNotNull(shellService);
                IVsPackage pkg;
                // TODO: refactor guid in the VisualRust project
                Guid visualRust = new Guid("40c1d2b5-528b-4966-a7b1-1974e3568abe");
                Assert.AreEqual(VSConstants.S_OK, shellService.LoadPackage(ref visualRust, out pkg));
                Assert.IsNotNull(pkg);
            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        [TestCategory("Explicit")]
#if VS12
        [TestProperty("VsHiveName", "12.0Exp")]
#elif VS14
        [TestProperty("VsHiveName", "14.0Exp")]
#endif
        public void CreateRustLibraryProject()
        {
            UIThreadInvoker.Invoke((Action)delegate()
            {
                string projName = Utils.GetCallingFunction();
                Utils.CreateEmptySolution(TestContext.TestDir, projName);
                Utils.CreateProjectFromTemplate(projName, "Rust Library", "Rust", false);
                ProjectNode root = Utils.GetProject(projName);
                Assert.IsNotNull(root);
                new HierarchyCheck(root)
                    .Child<FolderNode>("src")
                        .Child<FileNode>("lib.rs")
                .Run();
            });
        }

        private static void PickFileFromOpenDialog(Task<IntPtr> par, string path)
        {
            if (par.Exception != null)
                return;
            var elm = AutomationElement.FromHandle(par.Result);
            AutomationElement fileTextBox = elm.FindFirst(
                TreeScope.Subtree,
                new AndCondition(
                    new PropertyCondition(AutomationElement.ClassNameProperty, "Edit"),
                    new PropertyCondition(AutomationElement.NameProperty, "File name:")));
            ((ValuePattern)fileTextBox.GetCurrentPattern(ValuePattern.Pattern)).SetValue(path);
            SendKeys.SendWait("{Enter}");
        }

        [TestMethod]
        [HostType("VS IDE")]
        [TestCategory("Explicit")]
#if VS12
        [TestProperty("VsHiveName", "12.0Exp")]
#elif VS14
        [TestProperty("VsHiveName", "14.0Exp")]
#endif
        public void ExcludedNodesAreNotTracked()
        {
            UIThreadInvoker.Invoke((Action)delegate()
            {
                // Create solution and project
                string projName = Utils.GetCallingFunction();
                Utils.CreateEmptySolution(TestContext.TestDir, projName);
                string projectPath = Utils.CreateProjectFromTemplate(projName, "Rust Library", "Rust", false);
                string newFile = Path.Combine(projectPath, "src", "baz.rs");
                // Add file
                File.Create(newFile).Close();
                // Call idle processing, this triggers creation of show all elements
                IVsShell shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
                Guid visualRust = new Guid("40c1d2b5-528b-4966-a7b1-1974e3568abe");
                IVsPackage pkg;
                Assert.AreEqual(VSConstants.S_OK, shellService.LoadPackage(ref visualRust, out pkg));
                // It's not exact science, additions are usually triggered after 2 calls on my machine,
                // so doing it 100 times will suffice, right?
                for (int i = 0; i < 100; i++)
                    ((IOleComponent)pkg).FDoIdle(0); // we dont check this flag anyway
                var root = Utils.GetProject(projName);
                new HierarchyCheck(root)
                    .Child<FolderNode>("src")
                        .Child<FileNode>("lib.rs")
                        .Sibling<TrackedFileNode>("baz.rs", n => Assert.IsInstanceOfType(n.ItemNode, typeof(AllFilesProjectElement)),
                                                            n => Assert.IsTrue(String.IsNullOrEmpty(n.ItemNode.ItemTypeName)),
                                                            n => Assert.IsFalse(n.GetModuleTracking()),
                                                            n => AssertEx.IsThrown<Exception>(() => n.SetModuleTracking(true)))
                .Run();
            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        [TestCategory("Explicit")]
#if VS12
        [TestProperty("VsHiveName", "12.0Exp")]
#elif VS14
        [TestProperty("VsHiveName", "14.0Exp")]
#endif
        public void AddExistingFileFromFolder()
        {
            UIThreadInvoker.Invoke((Action)delegate()
            {
                // Create solution and project
                string projName = Utils.GetCallingFunction();
                Utils.CreateEmptySolution(TestContext.TestDir, projName);
                string projectPath = Utils.CreateProjectFromTemplate(projName, "Rust Library", "Rust", false);
                string newFile = Path.Combine(projectPath, "src", "baz.rs");
                // Add file
                File.Create(newFile).Close();
                // Call idle processing, this triggers creation of show all elements
                IVsShell shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
                Guid visualRust = new Guid("40c1d2b5-528b-4966-a7b1-1974e3568abe");
                IVsPackage pkg;
                Assert.AreEqual(VSConstants.S_OK, shellService.LoadPackage(ref visualRust, out pkg));
                // It's not exact science, additions are usually triggered after 2 calls on my machine,
                // so doing it 100 times will suffice, right?
                for (int i = 0; i < 100; i++)
                    ((IOleComponent)pkg).FDoIdle(0); // we dont check this flag anyway
                // open the dialog window and add the item, in the mean time spinning a new task to manipulate newly opened dialog window
                var root = Utils.GetProject(projName);
                Utils.GetNewDialogOwnerHwnd().ContinueWith(t => PickFileFromOpenDialog(t, newFile));
                Assert.AreEqual(VSConstants.S_OK, root.ExecCommand(root.FirstChild.ID, ref VsMenus.guidStandardCommandSet97, (uint)VSConstants.VSStd97CmdID.AddExistingItem, 0, IntPtr.Zero, IntPtr.Zero));
                // check the hierarchy now
                new HierarchyCheck(root)
                    .Child<FolderNode>("src")
                        .Child<FileNode>("lib.rs")
                        .Sibling<TrackedFileNode>("baz.rs", n => Assert.IsNotInstanceOfType(n.ItemNode, typeof(AllFilesProjectElement)),
                                                            n => Assert.IsFalse(String.IsNullOrEmpty(n.ItemNode.ItemTypeName)),
                                                            n => Assert.IsTrue(n.GetModuleTracking()))
                .Run();
            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        [TestCategory("Explicit")]
#if VS12
        [TestProperty("VsHiveName", "12.0Exp")]
#elif VS14
        [TestProperty("VsHiveName", "14.0Exp")]
#endif
        public void CreateRustApplicationProject()
        {
            UIThreadInvoker.Invoke((Action)delegate()
            {
                string projName = Utils.GetCallingFunction();
                Utils.CreateEmptySolution(TestContext.TestDir, projName);
                Utils.CreateProjectFromTemplate(projName, "Rust Application", "Rust", false);
                ProjectNode root = Utils.GetProject(projName);
                Assert.IsNotNull(root);
                new HierarchyCheck(root)
                    .Child<FolderNode>("src")
                        .Child<FileNode>("main.rs")
                .Run();
            });
        }
    }
}
