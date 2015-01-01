using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test.Integration
{
    static class Utils
    {
        public static string GetCallingFunction([System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            return caller;
        }

        private static string GetNewFileOrDirectoryNameWithoutCreatingAnything(string directory, string baseFileName, string extension)
        {
            // - get a file name that we can use
            string fileName;
            int i = 1;

            string fullFileName = null;
            while (true)
            {
                // construct next file name
                fileName = baseFileName + i;
                if (extension != null)
                    fileName += '.' + extension;

                // check if that file exists in the directory
                fullFileName = Path.Combine(directory, fileName);

                if (!File.Exists(fullFileName) && !Directory.Exists(fullFileName))
                    break;
                else
                    i++;
            }

            return fullFileName;
        }

        /// <summary>
        /// Returns the first available directory name on the form
        ///   [baseDirectoryName]i
        /// where [i] starts at 1 and increases until there is an available directory name
        /// in the given directory. Also creates the directory to mark it as occupied.
        /// </summary>
        /// <param name="directory">Directory that the file should live in.</param>
        /// <param name="baseDirectoryName"></param>
        /// <returns>Full directory name.</returns>
        public static string GetNewDirectoryName(string directory, string baseDirectoryName)
        {
            // Get the new file name
            string directoryName = GetNewFileOrDirectoryNameWithoutCreatingAnything(directory, baseDirectoryName, null);

            // Create an empty directory to make it as occupied
            Directory.CreateDirectory(directoryName);

            return directoryName;
        }

        /// <summary>
        /// Closes the currently open solution (if any), and creates a new solution with the given name.
        /// </summary>
        /// <param name="solutionName">Name of new solution.</param>
        public static void CreateEmptySolution(string directory, string solutionName)
        {
            CloseCurrentSolution(__VSSLNSAVEOPTIONS.SLNSAVEOPT_NoSave);

            string solutionDirectory = GetNewDirectoryName(directory, solutionName);

            // Create and force save solution
            IVsSolution solutionService = (IVsSolution)VsIdeTestHostContext.ServiceProvider.GetService(typeof(IVsSolution));
            solutionService.CreateSolution(solutionDirectory, solutionName, (uint)__VSCREATESOLUTIONFLAGS.CSF_SILENT);
            solutionService.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave, null, 0);
            DTE dte = VsIdeTestHostContext.Dte;
            Assert.AreEqual(solutionName + ".sln", Path.GetFileName(dte.Solution.FileName), "Newly created solution has wrong Filename");
        }

        public static void CloseCurrentSolution(__VSSLNSAVEOPTIONS saveoptions)
        {
            // Get solution service
            IVsSolution solutionService = (IVsSolution)VsIdeTestHostContext.ServiceProvider.GetService(typeof(IVsSolution));

            // Close already open solution
            solutionService.CloseSolutionElement((uint)saveoptions, null, 0);
        }

        /// <summary>
        /// Creates a project.
        /// </summary>
        /// <param name="projectName">Name of new project.</param>
        /// <param name="templateName">Name of project template to use</param>
        /// <param name="language">language</param>
        /// <returns>New project.</returns>
        public static void CreateProjectFromTemplate(string projectName, string templateName, string language, bool exclusive)
        {
            DTE dte = (DTE)VsIdeTestHostContext.ServiceProvider.GetService(typeof(DTE));

            Solution2 sol = dte.Solution as Solution2;
            string projectTemplate = sol.GetProjectTemplate(templateName, language);

            // - project name and directory
            string solutionDirectory = Directory.GetParent(dte.Solution.FullName).FullName;
            string projectDirectory = GetNewDirectoryName(solutionDirectory, projectName);

            dte.Solution.AddFromTemplate(projectTemplate, projectDirectory, projectName, false);
        }

        /// <summary>
        /// Get current number of open project in solution
        /// </summary>
        /// <returns></returns>
        public static int ProjectCount()
        {
            // Get solution service
            IVsSolution solutionService = (IVsSolution)VsIdeTestHostContext.ServiceProvider.GetService(typeof(IVsSolution));
            object projectCount;
            solutionService.GetProperty((int)__VSPROPID.VSPROPID_ProjectCount, out projectCount);
            return (int)projectCount;
        }
    }
}
