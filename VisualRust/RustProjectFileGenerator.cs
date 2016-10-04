// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;

namespace VisualRust
{
	// TODO: build a correct project file generator, probably not based on FileSystemMirroringProjectFileGenerator
	[Guid(GuidList.ProjectFileGenerationGuidString)]
	internal sealed class RustProjectFileGenerator : FileSystemMirroringProjectFileGenerator
	{
		private static readonly string[] _imports = {
			 //ProjectConstants.RtvsRulesPropsRelativePath,
			 //ProjectConstants.RtvsTargetsRelativePath,
		};

		public RustProjectFileGenerator()
			: base(GuidList.CpsProjectFactoryGuid, null, ".rsproj", _imports)
		{
		}

		protected override XPropertyGroup CreateProjectSpecificPropertyGroup(string cpsProjFileName)
		{
			//var scripts = Directory.GetFiles(Path.GetDirectoryName(cpsProjFileName), "*.rs");
			//if (scripts.Length > 0)
			//{
			//	var startupFile = Path.GetFileName(scripts[0]);
			//	return new XPropertyGroup(new XProperty("StartupFile", startupFile));
			//}
			return null;
		}
	}
}
