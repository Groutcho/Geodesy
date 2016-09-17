﻿using UnityEngine;
using System.Collections;
using OpenTerra.Controllers;
using OpenTerra.Unity.Views;

public class MouseOrbitImproved : MonoBehaviour
{
	public Transform target;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	public float distanceMin = .5f;
	public float distanceMax = 15f;

	float x = 0.0f;
	float y = 0.0f;

	// Use this for initialization
	void Start ()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
	}

	public event CameraMovedEventHandler Moved;

	void LateUpdate ()
	{
		if (target)
		{
			if (Input.GetKey (KeyCode.Mouse0))
			{
				x += Input.GetAxis ("Mouse X") * xSpeed * distance * 0.0001f;
				y -= Input.GetAxis ("Mouse Y") * ySpeed * distance * 0.0001f;

				y = ClampAngle (y, yMinLimit, yMaxLimit);
			}

			Quaternion rotation = Quaternion.Euler (y, x, 0);

			distance = Mathf.Clamp (distance - Input.GetAxis ("Mouse ScrollWheel") * (distance - distanceMin) / 2, distanceMin, distanceMax);

			RaycastHit hit;
			if (Physics.Linecast (target.position, transform.position, out hit))
			{
				distance -= hit.distance;
			}
			Vector3 negDistance = new Vector3 (0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.position;

			if (Moved != null)
			{
				if (Vector3.Distance(position, transform.position) > 0.001f)
				{
					Vector3 newForward = rotation * Vector3.forward;
					Moved(this, new CameraMovedEventArgs(this, position, rotation, newForward));
				}
			}

			transform.rotation = rotation;
			transform.position = position;
		}
	}

	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
}