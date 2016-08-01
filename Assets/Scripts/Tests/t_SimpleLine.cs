using UnityEngine;
using System.Collections;
using Geodesy;

public class t_SimpleLine : MonoBehaviour
{
	public Transform A;
	public Transform B;
	public Transform Center;
	private Mesh quad;

	public float t;
	public float width;

	void Update ()
	{
		var vertices = quad.vertices;
		Vector3 AB = B.position - A.position;
		Vector3 a = A.position;
		Vector3 b = B.position;

		Vector3 orientation = Camera.main.transform.forward;
	
		Vector3 ex = (AB * t) + a;
		Vector3 v; 

		v = Vector3.Normalize (Vector3.Cross (AB, ex - Center.position));
		vertices [2] = ex + v * width;

		vertices [3] = ex - v * width;

		vertices [0] = a - v * width;
		vertices [1] = a + v * width;

		quad.vertices = vertices;
	}

	void Start ()
	{
		quad = MeshProvider.Quad;
		MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter> ();
		meshFilter.mesh = quad;
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine (A.position, B.position);
	}
}
