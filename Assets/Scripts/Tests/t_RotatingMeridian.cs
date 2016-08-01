using UnityEngine;
using System.Collections;
using Geodesy.Models;
using Geodesy.Views;

public class t_RotatingMeridian : MonoBehaviour
{
	private float degrees;
	private Ellipse meridian;
	private float reductionFactor = 1;

	public float DegreesPerSecond;

	// Use this for initialization
	void Start ()
	{
	
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
	
	// Update is called once per frame
	void Update ()
	{
		degrees += DegreesPerSecond * Time.deltaTime;
		if (degrees >= 360) {
			degrees = 0;
		}
		GeoMatrix m = GeoMatrix.Identity;
		m.Rotate (0, 0, degrees);
		meridian = new Ellipse (100, 50, m);
	}

	void OnDrawGizmos()
	{
		if (enabled) {
			GeoMatrix m0 = GeoMatrix.Identity;
			m0.Rotate (0, 0, degrees);

			GeoMatrix m1 = GeoMatrix.Identity;
			m1.Rotate (0, 0, degrees + 25);

			var meridian0 = new Ellipse (100, 50, m0);
			var meridian1 = new Ellipse (100, 50, m1);

			DrawEllipse (meridian0, 0, 360, 1, Color.red);
			DrawEllipse (meridian1, 0, 360, 1, Color.green);
		}
	}
}
