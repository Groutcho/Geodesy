using OpenTerra.Models;

namespace OpenTerra.Controllers.Workers
{
	public interface IMeshBuilder
	{
		event MeshGeneratedEventHandler PatchRequestReady;

		MeshObject RequestPatchMesh(Location location);
		void Update();
	}
}