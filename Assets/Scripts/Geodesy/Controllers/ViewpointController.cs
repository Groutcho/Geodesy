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
	}
}

