using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.ProjectSystem
{
	/// <summary>
	/// Interface definition for an immutable Cargo metadata snapshot.
	/// </summary>
	public interface ICargoManifestData
	{
		string PackageName { get; }
	}
}
