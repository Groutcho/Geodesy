using UnityEngine;
using System.Collections;
using OpenTerra.Models;

public class RayEllipsoidIntersectionTests : MonoBehaviour
{
	Datum datum;
	Vector3 intersection;
	double reductionFactor = 0.001;

	// Use this for initialization
	void Start()
	{
		datum = new WGS84();
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 p = transform.position;
		Vector3 d = transform.forward;

		Cartesian3 pos = new Cartesian3(p.x / reductionFactor, p.y / reductionFactor, p.z / reductionFactor);
		Cartesian3 dir = new Cartesian3(d.x, d.y, d.z);

		Cartesian3 point;
		datum.Intersects(pos, dir, out point);

		intersection = new Vector3((float)point.x, (float)point.y, (float)point.z) * (float)reductionFactor;
		Debug.Log(intersection);
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawRay(transform.position, transform.forward * 10000);
		Gizmos.DrawWireSphere(Vector3.zero, (float)datum.SemimajorAxis * (float)reductionFactor);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(intersection, (float) (0.1/reductionFactor));
	}
}
