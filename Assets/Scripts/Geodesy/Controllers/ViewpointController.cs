using System;
using UnityEngine;
using Geodesy.Views;

namespace Geodesy.Controllers
{
	public class ViewpointController : MonoBehaviour
	{
		Viewpoint viewpoint;

		public void Initialize (Viewpoint viewpoint)
		{
			this.viewpoint = viewpoint;
		}

		void Update ()
		{
			float speed = 5000;
			float vector = Input.GetAxis ("Mouse ScrollWheel");
			transform.Translate(Vector3.forward * vector * speed,  Space.Self); 
		}
	}
}

