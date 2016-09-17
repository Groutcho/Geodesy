using System;
using UnityEngine;

namespace OpenTerra.Unity.DataModel
{
	public class Layer
	{
		public const float CameraDepth = 10.5f;
		public const float MaxDepth = 9.9f;
		public const float MinDepth = 0.1f;

		public event EventHandler Changed;

		public string Name { get; set; }

		private float depth = MinDepth;

		protected int compositingLayer;

		public float Depth
		{
			get { return depth; }
			set
			{
				if (depth != value)
				{
					depth = value;
					this.node.transform.localPosition = new Vector3 (0, depth, 0);
					RaiseChanged ();
				}
			}
		}

		public bool Visible
		{
			get { return node.activeSelf; }
			set
			{
				if (node.activeSelf != value)
				{
					node.SetActive (value);
					if (Changed != null)
					{
						Changed (this, null);
					}
				}
			}
		}

		protected GameObject node;

		public GameObject Node { get { return node; } }

		public Layer (string name, float depth)
		{
			compositingLayer = LayerMask.NameToLayer ("Compositing");
			this.node = new GameObject (name);
			var compositer = GameObject.Find ("Compositer/layers");
			node.layer = compositingLayer;
			node.transform.parent = compositer.transform;
			this.Name = name;
			this.Depth = depth;
		}

		public override string ToString ()
		{
			return string.Format ("[Layer: Name={0}, Depth={1}]", Name, Depth);
		}

		protected void RaiseChanged ()
		{
			if (Changed != null)
				Changed (this, null);
		}

		public virtual void Cleanup ()
		{
			
		}

		public virtual void RequestTileForLocation (Location location)
		{
			
		}

		public virtual void Update ()
		{
			
		}
	}
}

