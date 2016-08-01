using UnityEngine;
using System.Collections;
using Geodesy.Models;
using UnityEditor;

namespace Geodesy.Views
{
	public class DatumView : MonoBehaviour
	{
		private Datum source;
		private Vector3 semiminorAxis;
		private Vector3 semimajorAxis;
		private float reductionFactor;
		private int sampleResolution_deg;
		private Viewpoint viewpoint;

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
			semiminorAxis = Vector3.up * (float)source.SemiminorAxis * reductionFactor;
			semimajorAxis = Vector3.right * (float)source.SemimajorAxis * reductionFactor;
		}

		private void DrawEllipse (Ellipse ellipse, float from, float to, float resolution, Color color)
		{
			for (int longitude = (int)from; longitude < (int)to; longitude += (int)resolution) {
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
			float distance = viewpoint.DistanceFromView (source.Origin.ToVector3 ());
			int resolution = 1;
			float ratio = (viewpoint.MaxDistance - viewpoint.MinDistance) / Mathf.Clamp (distance, viewpoint.MinDistance, viewpoint.MaxDistance);
			int subdivisions = (int)Mathf.Pow (2, 1 / ratio);

			Ellipse equator = source.GetParallel (0);
			DrawEllipse (equator, 0, 360, resolution, Color.green);
			return;

			for (int latitude = subdivisions; latitude < 89; latitude += subdivisions) {
				Ellipse parallel = source.GetParallel (latitude);
				DrawEllipse (parallel, 0, 360, resolution, Colors.LightGrey);		
			}

			for (int latitude = subdivisions; latitude < 89; latitude += subdivisions) {
				Ellipse parallel = source.GetParallel (-latitude);
				DrawEllipse (parallel, 0, 360, resolution, Colors.LightGrey);		
			}

//			Ellipse referenceMeridian = source.GetMeridian (0);
//			DrawEllipse (referenceMeridian, 0, 360, resolution, Color.red);	
//
//			for (int longitude = subdivisions; longitude < 360; longitude += subdivisions) {				
//				Ellipse meridian = source.GetMeridian (longitude);
//				DrawEllipse (meridian, 0, 360, resolution, Colors.LightGrey);		
//			}
		}

		private void DrawAxes ()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine (Vector3.zero, semiminorAxis);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (Vector3.zero, semimajorAxis);
		}

		// Update is called once per frame
		void OnDrawGizmos ()
		{
			DrawAxes ();
			DrawGraticule ();
		}
	}
}