using OpenTerra.Controllers;
using OpenTerra.Models;
using UnityEngine;

namespace OpenTerra.Views
{
	public class Viewpoint
	{
		public const float MaxDistance = 30000f;
		public const float MinDistance = 6500f;

		private Camera camera;
		private IGlobe globe;
		private MouseOrbitImproved handler;

		public Camera Camera { get { return camera; } }

		public event CameraMovedEventHandler Moved;

		public Viewpoint (Transform viewpointRoot, IGlobe globe, string name, Vector3 initialPosition)
		{
			GameObject go = new GameObject(name);
			go.transform.parent = viewpointRoot;
			go.transform.localPosition = initialPosition;
			go.transform.LookAt(viewpointRoot);

			GameObject sunObject = new GameObject("sun");
			Light sun = sunObject.AddComponent<Light>();
			sun.type = LightType.Directional;
			sun.intensity = 1.5f;
			sun.color = new Color32(255, 244, 214, 255);
			sunObject.transform.parent = go.transform;
			sunObject.transform.localEulerAngles = new Vector3(44, 330, 160);

			this.camera = go.AddComponent<Camera>();
			this.camera.fieldOfView = 30;
			this.globe = globe;

			handler = go.AddComponent<MouseOrbitImproved>();
			handler.target = viewpointRoot;
			handler.distance = MaxDistance;
			handler.distanceMax = MaxDistance;
			handler.distanceMin = MinDistance;
			handler.xSpeed = 2;
			handler.ySpeed = 2;
			handler.yMinLimit = -89;
			handler.yMaxLimit = 89;

			handler.Moved += OnMoved;
		}

		private void OnMoved(object sender, CameraMovedEventArgs args)
		{
			if (Moved != null)
			{
				Moved(this, args);
			}
		}

		public void MoveTo(Vector3 newPosition)
		{
			camera.transform.localPosition = newPosition;
			handler.distance = newPosition.magnitude;
		}

		public LatLon CurrentPosition { get { return globe.Project(camera.transform.position); } }

		public Vector2 WorldToGUIPoint(Vector3 world)
		{
			Vector2 screenPoint = camera.WorldToScreenPoint(world);
			screenPoint.y = (float)Screen.height - screenPoint.y;
			return screenPoint;
		}
	}
}
