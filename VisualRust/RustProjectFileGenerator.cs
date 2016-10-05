using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace VisualRust
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

			//var inMemoryTargetsFile = FileSystemMirroringProjectUtilities.GetInMemoryTargetsFileName(cpsProjFileName);

			var xProjDocument = new XProjDocument(
				new XProject(Toolset.Version, "Build",
					new XPropertyGroup("Globals", null,
						new XProperty("ProjectGuid", Guid.NewGuid().ToString("D")),
						new XProperty("ManifestPath", "Cargo.toml")
					),
					new XProjElement("Target", new XAttribute("Name", "Build")),
					new XProjElement("Import", new XAttribute("Project", @"$(MSBuildExtensionsPath)\VisualRust\VisualRust.Rust.targets")),
					new XProjElement("Import",
						new XAttribute("Project", @"$(MSBuildThisFileName).InMemory.Targets"),
						new XAttribute("Condition", "Exists('$(MSBuildThisFileName).InMemory.Targets')")
					)
				)
			);

			using (var writer = fileInfo.CreateText())
			{
				xProjDocument.Save(writer);
			}
		}
	}
}
