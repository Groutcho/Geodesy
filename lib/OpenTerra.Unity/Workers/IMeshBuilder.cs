using OpenTerra.DataModel;

namespace OpenTerra.Unity.Workers
{
	public interface IMeshBuilder
	{
		event MeshGeneratedEventHandler PatchRequestReady;

		MeshObject RequestPatchMesh(Location location);
		void Update();
	}
}