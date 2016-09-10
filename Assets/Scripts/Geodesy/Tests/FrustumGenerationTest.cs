using UnityEngine;
using System.Collections;
using OpenTerra;

public class FrustumGenerationTest : MonoBehaviour
{
	public float fov;
	public float near;

	private Vector3 o;
	private Vector3 A;
	private Vector3 B;
	private Vector3 C;
	private Vector3 D;

	private Camera cam;

	// Use this for initialization
	void Start()
	{
		cam = GetComponent<Camera>();
	}

	// Update is called once per frame
	void Update()
	{
		o = transform.position;

		float alpha = (float)Utils.DegToRad(fov) * 0.5f;

		float z = o.z + Mathf.Cos(alpha);

		float Ax = o.x - Mathf.Sin(alpha);
		float Ay = o.y + Mathf.Sin(alpha);

		A = new Vector3(Ax, Ay, z);
		A = Quaternion.Inverse(transform.localRotation) * A;

		float Bx = o.x + Mathf.Sin(alpha);
		float By = o.y + Mathf.Sin(alpha);

		B = new Vector3(Bx, By, z);
		B = Quaternion.Inverse(transform.localRotation) * B;

		float Cx = o.x - Mathf.Sin(alpha);
		float Cy = o.y - Mathf.Sin(alpha);

		C = new Vector3(Cx, Cy, z);
		C = Quaternion.Inverse(transform.localRotation) * C;

		float Dx = o.x + Mathf.Sin(alpha);
		float Dy = o.y - Mathf.Sin(alpha);

		D = new Vector3(Dx, Dy, z);
		D = Quaternion.Inverse(transform.localRotation) * D;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		//Gizmos.DrawRay(o, near * (A - o));
		//Gizmos.DrawRay(o, near * (B - o));
		//Gizmos.DrawRay(o, near * (C - o));
		//Gizmos.DrawRay(o, near * (D - o));

		Gizmos.DrawRay(cam.ViewportPointToRay(new Vector3(0, 0, 0)));
		Gizmos.DrawRay(cam.ViewportPointToRay(new Vector3(1, 0, 0)));
		Gizmos.DrawRay(cam.ViewportPointToRay(new Vector3(1, 1, 0)));
		Gizmos.DrawRay(cam.ViewportPointToRay(new Vector3(0, 1, 0)));
	}
}
