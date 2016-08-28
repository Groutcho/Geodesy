using System;
using Geodesy.Views;
using Geodesy.Models.QuadTree;
using UnityEngine;
using Geodesy.Models;
using System.Collections;
using System.Collections.Generic;
using Geodesy.Views.Debugging;
using Console = Geodesy.Views.Debugging.Console;

namespace Geodesy.Controllers
{
	public class Globe : MonoBehaviour
	{
		private const int sampleResolution_deg = 1;

		QuadTree tree;
		PatchManager patchManager;
		Datum datum;
		float reductionFactor;
		Viewpoint viewpoint;
		SphereCollider approximateCollider;
		List<Vector3> debugPoints = new List<Vector3> (10);
		bool atmosphereVisible;
		GameObject atmosphere;

		public bool AtmosphereVisible
		{
			get
			{
				return atmosphereVisible;
			}

			set
			{
				if (value != AtmosphereVisible)
				{
					atmosphereVisible = value;
					atmosphere.SetActive (value);
				}
			}
		}

		/// <summary>
		/// Every nth seconds, the globe will trigger a cleanup function to delete obsolete cached data.
		/// </summary>
		public const float CleanupFrequency = 10;

		public QuadTree Tree { get { return tree; } }

		public PatchManager PatchManager { get { return patchManager; } }

		public void Initialize (Datum datum, Material material, float reductionFactor, Viewpoint viewpoint)
		{
			this.reductionFactor = reductionFactor;
			this.viewpoint = viewpoint;
			this.datum = datum;

			this.tree = new QuadTree (this);
			patchManager = new PatchManager (this, material);
			this.tree.DepthChanged += patchManager.UpdateDepth;

			atmosphere = GameObject.Find ("Globe/Atmosphere");
			atmosphereVisible = true;
			float atmosphereHeight = 100;
			atmosphere.transform.localScale = new Vector3 (
				(float)(datum.SemimajorAxis * reductionFactor + atmosphereHeight),
				(float)(datum.SemiminorAxis * reductionFactor + atmosphereHeight),
				(float)(datum.SemimajorAxis * reductionFactor + atmosphereHeight));

			// Create a spherical approximation of the spheroid
			// for purposes that don't need exact calculations.
			approximateCollider = gameObject.AddComponent<SphereCollider> ();
			approximateCollider.radius = (float)(datum.SemimajorAxis * reductionFactor) * 0.995f;

			patchManager.ChangeDepth (tree.CurrentDepth);
			StartCoroutine (PatchManagerGarbageCollector ());

			Console.Instance.Register ("point", ExecutePointCommand);
			Console.Instance.Register ("atmosphere", ExecuteAtmosphereCommand);
		}

		public Vector3 Project (LatLon point)
		{
			return Project ((float)point.Latitude, (float)point.Longitude);
		}

		/// <summary>
		/// Return the geographic coordinates of the point under the cursor.
		/// If the cursor is not over the globe, return 0, 0, 0;
		/// </summary>
		/// <value>The cursor coordinates.</value>
		public LatLon CursorCoordinates
		{
			get
			{
				// TODO: implement correct trigonometric computation using actual datum instead of sphere
				RaycastHit hit;
				if (Physics.Raycast (viewpoint.Camera.ScreenPointToRay (Input.mousePosition), out hit))
				{
					float lat = Mathf.Clamp (Vector3.Angle (new Vector3 (hit.point.x, hit.point.y, 0), Vector3.right) * Mathf.Sign (hit.point.y), -90, 90);
					float lon = Mathf.Clamp (Vector3.Angle (new Vector3 (hit.point.x, 0, hit.point.z), Vector3.right) * Mathf.Sign (hit.point.z), -180, 180);

					return new LatLon (lat, lon, 0);
				}
				return new LatLon ();
			}
		}

		/// <summary>
		/// Project the point at the surface of the datum and return
		/// its cartesian coordinates.
		/// </summary>
		/// <param name="lat">Latitude of the point to project.</param>
		/// <param name="lon">Longitude of the point to project.</param>
		public Vector3 Project (float lat, float lon)
		{
			lat = Mathf.Deg2Rad * lat;
			lon = Mathf.Deg2Rad * lon;

			float x = 0;
			float y = 0;
			float z = 0;

			float redY = (float)(datum.SemiminorAxis * reductionFactor);
			float redX = (float)(datum.SemimajorAxis * reductionFactor);
			float hRadius = (float)(Mathf.Cos (lat) * redY);
			float wRadius = (float)(Mathf.Cos (lat) * redX);

			x = Mathf.Cos (lon) * wRadius;
			y = Mathf.Sin (lat) * redY;
			z = Mathf.Sin (lon) * wRadius;
			return new Vector3 (x, y, z);
		}

		private IEnumerator PatchManagerGarbageCollector ()
		{
			while (true)
			{
				yield return new WaitForSeconds (CleanupFrequency);
				PatchManager.Cleanup ();
			}
		}

		void Update ()
		{
			AtmosphereVisible = ViewpointController.Instance.DistanceFromView (Vector3.zero) > 12000;
		}

		#region Visual debugging

		public Vector3 Position { get { return datum.Transform.Position.ToVector3 (); } }

		private void DrawEllipse (Ellipse ellipse, float from, float to, float resolution, Color color)
		{
			for (int longitude = (int)from; longitude < (int)to; longitude += (int)resolution)
			{
				Vector3 vfrom = ellipse.Sample (longitude).ToVector3 () * reductionFactor;
				Vector3 vto = ellipse.Sample (longitude + resolution).ToVector3 () * reductionFactor;

				Gizmos.color = color;
				Gizmos.DrawLine (vfrom, vto);
			}	
		}

		/// <summary>
		/// Draws the graticule.
		/// </summary>
		private void DrawDebugGraticule ()
		{
			float distance = viewpoint.DistanceFromView (datum.Transform.Position.ToVector3 ());
			int resolution = 1;
			float ratio = (viewpoint.MaxDistance - viewpoint.MinDistance) / Mathf.Clamp (distance, viewpoint.MinDistance, viewpoint.MaxDistance);
			int subdivisions = (int)Mathf.Pow (2, 1 / ratio);
			subdivisions = 30;

			Ellipse equator = datum.GetParallel (0);
			DrawEllipse (equator, 0, 360, resolution, Color.green);

			Ellipse northernTropic = datum.GetParallel (23.43713);
			DrawEllipse (northernTropic, 0, 360, resolution, Color.cyan);

			Ellipse southernTropic = datum.GetParallel (-23.43713);
			DrawEllipse (southernTropic, 0, 360, resolution, Color.cyan);

			for (int latitude = subdivisions; latitude < 89; latitude += subdivisions)
			{
				Ellipse parallel = datum.GetParallel (latitude);
				DrawEllipse (parallel, 0, 360, resolution, Colors.LightGrey);
			}

			for (int latitude = subdivisions; latitude < 89; latitude += subdivisions)
			{
				Ellipse parallel = datum.GetParallel (-latitude);
				DrawEllipse (parallel, 0, 360, resolution, Colors.LightGrey);
			}

			Ellipse referenceMeridian = datum.GetMeridian (90);
			DrawEllipse (referenceMeridian, 0, 360, resolution, Color.red);

			for (int longitude = subdivisions; longitude < 360; longitude += subdivisions)
			{
				Ellipse meridian = datum.GetMeridian (longitude);
				DrawEllipse (meridian, 0, 360, resolution, Colors.LightGrey);
			}
		}

		private void DrawAxes ()
		{
			GeoVector3 orig = datum.Transform.Position;
			GeoVector3 semimaj = (orig + GeoVector3.Right * datum.Transform) * reductionFactor * datum.SemimajorAxis;
			GeoVector3 semimin = (orig + GeoVector3.Up * datum.Transform) * reductionFactor * datum.SemiminorAxis;

			Gizmos.color = Color.red;
			Gizmos.DrawLine (orig.ToVector3 (), semimaj.ToVector3 ());
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (orig.ToVector3 (), semimin.ToVector3 ());
		}

		private void DrawPoints ()
		{
			Gizmos.color = Color.green;
			foreach (var item in debugPoints)
			{
				Gizmos.DrawWireSphere (item, 100);
			}
		}

		// Update is called once per frame
		void OnDrawGizmos ()
		{
			DrawAxes ();
			DrawDebugGraticule ();
			DrawPoints ();
		}

		#endregion


		#region IConsoleCommandHandler implementation

		public CommandResult ExecuteAtmosphereCommand (Command command)
		{
			if (command.TokenCount != 1 ||
			    command.Tokens [0].TokenType != CommandToken.BOOL)
				throw new FormatException (Console.ExpectedGot ("bool", command.Tokens [0].Value));

			AtmosphereVisible = command.Tokens [0].Bool;
			return new CommandResult (AtmosphereVisible);
		}

		public CommandResult ExecutePointCommand (Command command)
		{
			if (command.TokenCount != 2)
			{
				throw new FormatException ("Expected geographic coordinates");
			}

			try
			{
				var lat = command.Tokens [0].Float;
				var lon = command.Tokens [1].Float;
				LatLon latlon = new LatLon (lat, lon, 0);
				DebugCreatePoint (latlon);
				return new CommandResult (latlon);
			} catch (Exception)
			{
				throw new FormatException ("Expected geographic coordinates");
			}
		}

		private void DebugCreatePoint (LatLon latlon)
		{
			Vector3 pos = Project (latlon);
			debugPoints.Add (pos);
		}

		#endregion
	}
}

