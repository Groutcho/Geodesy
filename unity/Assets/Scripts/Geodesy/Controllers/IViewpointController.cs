using OpenTerra.Views;

namespace OpenTerra.Controllers
{
	public interface IViewpointController
	{
		Viewpoint ActiveViewpoint { get; }

		event CameraMovedEventHandler ActiveViewpointMoved;

		void OnDrawGizmos();
	}
}