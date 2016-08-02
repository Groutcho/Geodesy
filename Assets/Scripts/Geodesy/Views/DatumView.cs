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
		Mesh patchMesh;

		/// <summary>
		/// Initialize the DatumView with the specified source and reduction factor.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="reductionFactor">Reduction factor.</param>
		public void Initialize (Datum source, float reductionFactor, int sampleResolution_deg, Viewpoint viewpoint)
		{
			this.source = source;
			this.reductionFactor = reductionFactor;
			this.sampleResolution_deg = sampleResolution_deg;
			this.viewpoint = viewpoint;
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

		/// <summary>
		/// Draws the graticule.
		/// </summary>
		private void DrawGraticule ()
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
			DrawGraticule ();
		}
	}
}