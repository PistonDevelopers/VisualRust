// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project {
    public abstract class FileSystemMirroringProjectFileGenerator : IVsProjectGenerator {
        private readonly Guid _projectType;
        private readonly string _projectUiSubcaption;
        private readonly string _cpsProjExtension;
        private readonly IEnumerable<string> _msBuildImports;

        protected FileSystemMirroringProjectFileGenerator(Guid projectType, string projectUiSubcaption, string cpsProjExtension, IEnumerable<string> msBuildImports) {
            _projectType = projectType;
            _projectUiSubcaption = projectUiSubcaption;
            _cpsProjExtension = cpsProjExtension;
            _msBuildImports = msBuildImports;
        }

        public void RunGenerator(string szSourceFileMoniker, out bool pfProjectIsGenerated, out string pbstrGeneratedFile, out Guid pGuidProjType) {
            pfProjectIsGenerated = true;
            pbstrGeneratedFile = GetCpsProjFileName(szSourceFileMoniker);
            pGuidProjType = _projectType;

            EnsureCpsProjFile(pbstrGeneratedFile);
        }

        private string GetCpsProjFileName(string fileName) {
            return Path.GetDirectoryName(fileName)
                + Path.DirectorySeparatorChar
                + Path.GetFileNameWithoutExtension(fileName)
                + _cpsProjExtension;
        }

        private void EnsureCpsProjFile(string cpsProjFileName) {
            var fileInfo = new FileInfo(cpsProjFileName);
            if (fileInfo.Exists) {
                return;
            }

            var inMemoryTargetsFile = FileSystemMirroringProjectUtilities.GetInMemoryTargetsFileName(cpsProjFileName);

            var xProjDocument = new XProjDocument(
                new XProject(Toolset.Version, "Build",
                    new XPropertyGroup("Globals", null,
                        new XProperty("ProjectGuid", Guid.NewGuid().ToString("D"))
                    ),
                    new XPropertyGroup(
                        new XDefaultValueProperty("VisualStudioVersion", Toolset.Version),
                        new XDefaultValueProperty("Configuration", "Debug"),
                        new XDefaultValueProperty("Platform", "AnyCPU")
                    ),
                    CreateProjectSpecificPropertyGroup(cpsProjFileName),
                    CreateProjectUiSubcaption(),
                    new XProjElement("ProjectExtensions",
                        new XProjElement("VisualStudio",
                            new XProjElement("UserProperties")
                        )
                    ),
                    new XProjElement("Target", new XAttribute("Name", "Build")),
                    _msBuildImports.SelectMany(CreateMsBuildExtensionXImports),
                    new XImportExisting(inMemoryTargetsFile)
                )
            );

            using (var writer = fileInfo.CreateText()) {
                xProjDocument.Save(writer);
            }
        }

        protected virtual XPropertyGroup CreateProjectSpecificPropertyGroup(string cpsProjFileName) {
            return null;
        }

        private XPropertyGroup CreateProjectUiSubcaption() {
            return _projectUiSubcaption != null
                ? new XPropertyGroup(new XProperty("ProjectUISubcaption", _projectUiSubcaption))
                : null;
        }

        private IEnumerable<XImport> CreateMsBuildExtensionXImports(string import) {
            var msBuildImportExtensionPath = Invariant($@"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\{import}");
            var msBuildImportUserExtensionPath = Invariant($@"$(MSBuildUserExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\{import}");

            yield return new XImportExisting(msBuildImportUserExtensionPath);
            yield return new XImportExisting(msBuildImportExtensionPath, $"!Exists('{msBuildImportUserExtensionPath}')");
        }
    }
}