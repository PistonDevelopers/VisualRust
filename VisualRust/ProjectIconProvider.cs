// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Forms;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualRust
{
	internal static class ProjectIconProvider
	{
		private static IVsImageMonikerImageList _monikerImageList;
		private static ImageList _imageList;

		public static ImageMoniker ProjectNodeImage { get; private set; }
		public static ImageMoniker RustFileNodeImage { get; private set; }
		public static ImageMoniker CargoManifestNodeImage { get; private set; }

		/// <summary>
		/// Creates image list and image monikers for project icons.
		/// Must be called on UI thread.
		/// </summary>
		public static void LoadProjectImages()
		{
			if (_monikerImageList == null)
			{
				IVsImageService2 imageService = VisualRustPackage.GetGlobalService(typeof(SVsImageService)) as IVsImageService2;

				if (_imageList == null)
				{
					_imageList = new ImageList();
				}

				_imageList.Images.Add(Resources.RustProjectNode);
				_imageList.Images.Add(Resources.RustFileNode);
				_imageList.Images.Add(Resources.CargoManifestNode);

				_monikerImageList = imageService.CreateMonikerImageListFromHIMAGELIST(_imageList.Handle);
				imageService.AddCustomImageList(_monikerImageList);

				ImageMoniker[] monikers = new ImageMoniker[3];
				_monikerImageList.GetImageMonikers(0, 3, monikers);

				ProjectNodeImage = monikers[0];
				RustFileNodeImage = monikers[1];
				CargoManifestNodeImage = monikers[2];
			}
		}

		public static void Close()
		{
			if (_imageList != null)
			{
				_imageList.Dispose();
				_imageList = null;
				_monikerImageList = null;
			}
		}
	}
}
