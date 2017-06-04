using Microsoft.Common.Core;
using VisualRust.ProjectSystem.FileSystemMirroring.MsBuild;
using VisualRust.ProjectSystem.FileSystemMirroring.Shell;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace VisualRust.ProjectSystem
{
    [Guid(GuidList.ProjectFileGenerationGuidString)]
    internal sealed class RustProjectFileGenerator : IVsProjectGenerator
    {
        public void RunGenerator(string szSourceFileMoniker, out bool pfProjectIsGenerated, out string pbstrGeneratedFile, out Guid pGuidProjType)
        {
            pfProjectIsGenerated = true;
            pbstrGeneratedFile = GetCpsProjFileName(szSourceFileMoniker);
            pGuidProjType = GuidList.CpsProjectFactoryGuid;

            EnsureCpsProjFile(pbstrGeneratedFile);
        }

        private string GetCpsProjFileName(string fileName)
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(fileName));
            return directory.FullName
                + Path.DirectorySeparatorChar
                + directory.Name
                + ".rsproj";
        }

        private void EnsureCpsProjFile(string cpsProjFileName)
        {
            var fileInfo = new FileInfo(cpsProjFileName);
            if (fileInfo.Exists)
            {
                return;
            }

            var xProj = new XProject();

            xProj.Add(
                new XPropertyGroup("Globals", null,
                    new XProperty("ProjectGuid", Guid.NewGuid().ToString("D")),
                    new XProperty("ManifestPath", "Cargo.toml")
                ),
                new XProjElement("Import", new XAttribute("Project", @"$(MSBuildExtensionsPath)\VisualRust\VisualRust.Rust.targets")),
                new XProjElement("Import",
                    new XAttribute("Project", @"$(MSBuildThisFileName).InMemory.Targets"),
                    new XAttribute("Condition", "Exists('$(MSBuildThisFileName).InMemory.Targets')")
                )
            );

            var xProjDocument = new XProjDocument(xProj);

            using (var writer = fileInfo.CreateText())
            {
                xProjDocument.Save(writer);
            }
        }
    }
}
