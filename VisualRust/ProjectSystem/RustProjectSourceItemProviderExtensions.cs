// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
//#if VS14
using Microsoft.VisualStudio.ProjectSystem.Items;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
//#endif

namespace VisualRust.ProjectSystem
{
	[Export(typeof(IProjectSourceItemProviderExtension))]
	[Export(typeof(IProjectFolderItemProviderExtension))]
	[AppliesTo(VisualRustPackage.UniqueCapability)]
	internal sealed class RustProjectSourceItemProviderExtension : FileSystemMirroringProjectSourceItemProviderExtensionBase
	{
		[ImportingConstructor]
		public RustProjectSourceItemProviderExtension(UnconfiguredProject unconfiguredProject, ConfiguredProject configuredProject, IProjectLockService projectLockService, IFileSystemMirroringProjectTemporaryItems2 temporaryItems)
			: base(unconfiguredProject, configuredProject, projectLockService, temporaryItems)
		{
		}
	}
}