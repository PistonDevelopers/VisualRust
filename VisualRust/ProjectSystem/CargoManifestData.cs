using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.ProjectSystem
{
	internal class CargoManifestData : ICargoManifestData
	{
		public string PackageName { get; }

		public CargoManifestData(string packageName)
		{
			PackageName = packageName;
		}
	}
}
