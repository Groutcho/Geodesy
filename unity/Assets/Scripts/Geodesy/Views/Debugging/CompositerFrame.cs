using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompositerFrame : MonoBehaviour
{
	Camera compositerCamera;
	Vector3 lastPosition;
	List<Frame> frames = new List<Frame> (256);

	private struct Frame
	{
		public readonly Vector3 TopLeft;
		public readonly Vector3 TopRight;
		public readonly Vector3 BottomLeft;
		public readonly Vector3 BottomRight;

		public Frame (Transform transform, Camera cam)
		{
			float x0 = transform.position.x - cam.orthographicSize * cam.aspect;
			float z0 = transform.position.z - cam.orthographicSize;
			float x1 = x0 + cam.orthographicSize * 2 * cam.aspect;
			float z1 = z0 + cam.orthographicSize * 2;

			float depth = 0.1f;

			TopLeft = new Vector3 (x0, depth, z1);
			TopRight = new Vector3 (x1, depth, z1);

			BottomLeft = new Vector3 (x0, depth, z0);
			BottomRight = new Vector3 (x1, depth, z0);
		}
	}

	// Use this for initialization
	void Start ()
	{
		compositerCamera = GetComponent<Camera> ();
	}

	void OnDrawGizmos ()
	{
		if (compositerCamera == null)
			return;

		Gizmos.DrawRay (transform.position, transform.forward * 10);

		if (transform.position != lastPosition)
		{
			if (frames.Count > 64)
			{
				frames.Clear ();
			}
			frames.Add (new Frame (transform, compositerCamera));
			lastPosition = transform.position;
		}

		foreach (var frame in frames)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (frame.TopLeft, frame.TopRight);
			Gizmos.DrawLine (frame.BottomLeft, frame.BottomRight);
			Gizmos.DrawLine (frame.BottomLeft, frame.TopLeft);
			Gizmos.DrawLine (frame.TopRight, frame.BottomRight);
			Gizmos.color = Color.white;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
