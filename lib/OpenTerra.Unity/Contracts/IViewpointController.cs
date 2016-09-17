using OpenTerra.Unity.Views;

namespace OpenTerra.Unity
{
	public interface IViewpointController
	{
		Viewpoint ActiveViewpoint { get; }

		event CameraMovedEventHandler ActiveViewpointMoved;

		void OnDrawGizmos();
	}
}