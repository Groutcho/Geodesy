using System;
using OpenTerra.Views;
using OpenTerra.Models.QuadTree;
using UnityEngine;
using OpenTerra.Models;
using System.Collections.Generic;
using OpenTerra.Controllers.Commands;

namespace OpenTerra.Controllers
{
	public class Globe : IGlobe
	{
		private Datum datum;
		private readonly float reductionFactor;
		private List<Vector3> debugPoints = new List<Vector3> (10);
		private GameObject atmosphere;

		const double deg2rad = Math.PI / 180;

		private bool atmosphereEnabled = true;
		private double observerAltitude;

		public bool AtmosphereEnabled
		{
			get
			{
				return atmosphereEnabled;
			}
			set
			{
				atmosphereEnabled = value;
				if (!value)
				{
					atmosphere.SetActive (false);
				}
			}
		}

		public double ObserverAltitude
		{
			set
			{
				if (value != observerAltitude)
				{
					AtmosphereVisible = value > Units.km_to_m(6000);
					observerAltitude = value;
				}
			}
		}

		public bool AtmosphereVisible
		{
			get
			{
				return atmosphere.activeSelf && AtmosphereEnabled;
			}

			set
			{
				if (AtmosphereEnabled)
				{
					atmosphere.SetActive (value);
				}
			}
		}

		public Globe (Datum datum, float reductionFactor, IShell shell)
		{
			this.reductionFactor = reductionFactor;
			this.datum = datum;

			atmosphere = GameObject.Find ("Globe/Atmosphere");
			float atmosphereHeight = 100;
			atmosphere.transform.localScale = new Vector3 (
				(float)(datum.SemimajorAxis * reductionFactor + atmosphereHeight),
				(float)(datum.SemiminorAxis * reductionFactor + atmosphereHeight),
				(float)(datum.SemimajorAxis * reductionFactor + atmosphereHeight));

			shell.Register ("point", ExecutePointCommand);
			shell.Register ("atmosphere", ExecuteAtmosphereCommand);
		}

		public Vector3 Project (LatLon point)
		{
			return Project (point.Latitude, point.Longitude, point.Altitude);
		}

		public LatLon Project (Vector3 point)
		{
			float lat = Mathf.Clamp (Vector3.Angle (new Vector3 (point.x, point.y, 0), Vector3.right) * Mathf.Sign (point.y), -90, 90);
			float lon = Mathf.Clamp (Vector3.Angle (new Vector3 (point.x, 0, point.z), Vector3.right) * Mathf.Sign (point.z), -180, 180);

			Vector3 hit;
			Intersects(new Ray(point, -point), out hit);

			float alt = Vector3.Distance(hit, point);

			return new LatLon (lat, lon, alt / reductionFactor);
		}

		/// <summary>
		/// Project the point at the surface of the datum and return
		/// its cartesian coordinates.
		/// </summary>
		/// <param name="lat">Latitude of the point to project.</param>
		/// <param name="lon">Longitude of the point to project.</param>
		public Vector3 Project (double lat, double lon, double alt = 0)
		{
			lat = deg2rad * lat;
			lon = deg2rad * lon;

			double redY = ((datum.SemiminorAxis + alt) * reductionFactor);
			double redX = ((datum.SemimajorAxis + alt) * reductionFactor);
			double wRadius = (Math.Cos (lat) * redX);

			double x = Math.Cos (lon) * wRadius;
			double y = Math.Sin (lat) * redY;
			double z = Math.Sin (lon) * wRadius;
			return new Vector3 ((float)x, (float)y, (float)z);
		}

		public bool Intersects(Ray ray, out Vector3 point)
		{
			return Intersects(ray.origin, ray.direction, out point);
		}

		public bool Intersects(Vector3 origin, Vector3 direction, out Vector3 point)
		{
			Cartesian3 intersection;
			Cartesian3 o = new Cartesian3(origin.x / reductionFactor, origin.y / reductionFactor, origin.z / reductionFactor);
			Cartesian3 d = new Cartesian3(direction.x, direction.y, direction.z);
			bool intersects = datum.Intersects(o, d, out intersection);

			point = new Vector3((float)intersection.x, (float)intersection.y, (float)intersection.z) * reductionFactor;
			return intersects;
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
			float altitude = (float)(observerAltitude * reductionFactor);
			int resolution = 1;
			float ratio = (Viewpoint.MaxDistance - Viewpoint.MinDistance) / Mathf.Clamp (altitude, Viewpoint.MinDistance, Viewpoint.MaxDistance);
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
			Cartesian3 orig = datum.Transform.Position;
			Cartesian3 semimaj = (orig + Cartesian3.Right * datum.Transform) * reductionFactor * datum.SemimajorAxis;
			Cartesian3 semimin = (orig + Cartesian3.Up * datum.Transform) * reductionFactor * datum.SemiminorAxis;

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
		public void OnDrawGizmos ()
		{
			DrawAxes ();
			DrawDebugGraticule ();
			DrawPoints ();
		}

		#endregion


		#region IConsoleCommandHandler implementation

		private Response ExecuteAtmosphereCommand (Command command)
		{
			if (!Command.Matches (command, Token.BOOL))
				throw new CommandException ("atmosphere [bool]");

			AtmosphereEnabled = command.Tokens [0].Bool;
			return new Response(AtmosphereEnabled, ResponseType.Success);
		}

		private Response ExecutePointCommand (Command command)
		{
			if (!Command.Matches (command, Token.FLOAT, Token.FLOAT))
				throw new CommandException ("point latitude longitude");

			var lat = command.Tokens [0].Float;
			var lon = command.Tokens [1].Float;

			LatLon latlon = new LatLon (lat, lon, 0);
			DebugCreatePoint (latlon);
			return new Response(latlon, ResponseType.Success);
		}

		private void DebugCreatePoint (LatLon latlon)
		{
			Vector3 pos = Project (latlon);
			debugPoints.Add (pos);
		}

		public float Scale(float meters)
		{
			return meters * reductionFactor;
		}

		#endregion
	}
}

