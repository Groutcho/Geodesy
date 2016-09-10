using OpenTerra.Models;
using UnityEngine;

namespace OpenTerra.Controllers
{
	public interface IGlobe
	{
		bool AtmosphereEnabled { get; set; }
		bool AtmosphereVisible { get; set; }
		Vector3 Position { get; }
		double ObserverAltitude { set; }

		/// <summary>
		/// Scale the specified distance using the globe's reduction factor.
		/// </summary>
		/// <param name="meters"></param>
		/// <returns></returns>
		float Scale(float meters);

		LatLon Project(Vector3 point);
		Vector3 Project(LatLon point);
		Vector3 Project(double lat, double lon, double alt = 0);
		bool Intersects(Vector3 origin, Vector3 direction, out Vector3 point);
		void OnDrawGizmos();
	}
}