using OpenTerra.Unity.Views;

namespace OpenTerra
{
	public interface IViewpointController
	{
		Viewpoint ActiveViewpoint { get; }

		event CameraMovedEventHandler ActiveViewpointMoved;

		void OnDrawGizmos();
	}
}