using UnityEngine;
using System.Collections;
using Geodesy.Models;
using UnityEditor;

namespace Geodesy.Views
{
	public class DatumView : MonoBehaviour
	{
		private Datum source;
		private float reductionFactor;
		private int sampleResolution_deg;
		private Viewpoint viewpoint;

		public Vector3 Position { get { return source.Transform.Position.ToVector3 (); } }

		/// <summary>
		/// Initialize the DatumView with the specified source and reduction factor.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="reductionFactor">Reduction factor.</param>
		public void Initialize (Datum source, float reductionFactor, int sampleResolution_deg, Viewpoint viewpoint, Material lineMaterial)
		{
			this.source = source;
			this.reductionFactor = reductionFactor;
			this.sampleResolution_deg = sampleResolution_deg;
			this.viewpoint = viewpoint;

			AddGraticule (lineMaterial);
		}

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

		private void AddEllipseRendrer (Ellipse ellipse, Material material, Color color)
		{
			GameObject go = new GameObject ("ellipse");
			go.transform.parent = transform;
			LineRenderer lr = go.AddComponent<LineRenderer> ();
			lr.SetWidth (15, 15);
			lr.material = material;
			lr.material.color = color;
			Vector3 from = Vector3.zero;

			lr.SetVertexCount (360);
			for (int i = 0; i < 360; i += sampleResolution_deg)
			{
				from = (ellipse.Sample (i) * reductionFactor).ToVector3 ();
				lr.SetPosition (i, from);
			}
		}

		private void AddGraticule (Material lineMaterial)
		{
			Ellipse equator = source.GetParallel (0);
			Ellipse northernTropic = source.GetParallel (23.43713);
			Ellipse southernTropic = source.GetParallel (-23.43713);

			AddEllipseRendrer (equator, lineMaterial, Color.red);
			AddEllipseRendrer (northernTropic, lineMaterial, Color.blue);
			AddEllipseRendrer (southernTropic, lineMaterial, Color.blue);

			int everyNth = 10;

			for (int latitude = everyNth; latitude < 89; latitude += everyNth)
			{
				Ellipse parallel = source.GetParallel (latitude);
				AddEllipseRendrer (parallel, lineMaterial, Colors.LightGrey);
			}

			for (int latitude = everyNth; latitude < 89; latitude += everyNth)
			{
				Ellipse parallel = source.GetParallel (-latitude);
				AddEllipseRendrer (parallel, lineMaterial, Colors.LightGrey);
			}

			for (int longitude = 0; longitude < 360; longitude += everyNth)
			{
				Ellipse meridian = source.GetMeridian (longitude);
				AddEllipseRendrer (meridian, lineMaterial, Colors.LightGrey);
			}
		}

		/// <summary>
		/// Draws the graticule.
		/// </summary>
		private void DrawDebugGraticule ()
		{
			float distance = viewpoint.DistanceFromView (source.Transform.Position.ToVector3 ());
			int resolution = 1;
			float ratio = (viewpoint.MaxDistance - viewpoint.MinDistance) / Mathf.Clamp (distance, viewpoint.MinDistance, viewpoint.MaxDistance);
			int subdivisions = (int)Mathf.Pow (2, 1 / ratio);
			subdivisions = 30;

			Ellipse equator = source.GetParallel (0);
			DrawEllipse (equator, 0, 360, resolution, Color.green);

			Ellipse northernTropic = source.GetParallel (23.43713);
			DrawEllipse (northernTropic, 0, 360, resolution, Color.cyan);

			Ellipse southernTropic = source.GetParallel (-23.43713);
			DrawEllipse (southernTropic, 0, 360, resolution, Color.cyan);

			for (int latitude = subdivisions; latitude < 89; latitude += subdivisions)
			{
				Ellipse parallel = source.GetParallel (latitude);
				DrawEllipse (parallel, 0, 360, resolution, Colors.LightGrey);
			}

			for (int latitude = subdivisions; latitude < 89; latitude += subdivisions)
			{
				Ellipse parallel = source.GetParallel (-latitude);
				DrawEllipse (parallel, 0, 360, resolution, Colors.LightGrey);
			}

			Ellipse referenceMeridian = source.GetMeridian (0);
			DrawEllipse (referenceMeridian, 0, 360, resolution, Color.red);

			for (int longitude = subdivisions; longitude < 360; longitude += subdivisions)
			{
				Ellipse meridian = source.GetMeridian (longitude);
				DrawEllipse (meridian, 0, 360, resolution, Colors.LightGrey);
			}
		}

		private void DrawAxes ()
		{
			GeoVector3 orig = source.Transform.Position;
			GeoVector3 semimaj = (orig + GeoVector3.Right * source.Transform) * reductionFactor * source.SemimajorAxis;
			GeoVector3 semimin = (orig + GeoVector3.Up * source.Transform) * reductionFactor * source.SemiminorAxis;

			Gizmos.color = Color.red;
			Gizmos.DrawLine (orig.ToVector3 (), semimaj.ToVector3 ());
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (orig.ToVector3 (), semimin.ToVector3 ());
		}

		// Update is called once per frame
		void OnDrawGizmos ()
		{
			DrawAxes ();
			DrawDebugGraticule ();
		}

		public Vector3 Project (LatLon point)
		{
			return Project ((float)point.Latitude, (float)point.Longitude);
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

			float red = (float)(source.SemiminorAxis * reductionFactor);
			float hRadius = (float)(Mathf.Cos (lat) * red);

			x = Mathf.Cos (lon) * hRadius;
			y = Mathf.Sin (lat) * red;
			z = Mathf.Sin (lon) * hRadius;
			return new Vector3 (x, y, z);
		}
	}
}