using System;

namespace OpenTerra.DataModel
{
	/// <summary>
	/// Describes a rectangle defined by geographic coordinates.
	/// </summary>
	public struct GeoRectangle : IEquatable<GeoRectangle>
	{
		public LatLon BottomLeft { get; private set; }
		public LatLon TopRight { get; private set; }

		public double WidthDegrees
		{
			get
			{
				// Case where the rectangle overlaps the edges of the map
				if (TopRight.Longitude < 0 && BottomLeft.Longitude > 0)
				{
					double tr = (-180) % TopRight.Longitude;
					double bl = 180 % BottomLeft.Longitude;
					return Math.Abs(tr - bl);
				}
				else
				{
					return TopRight.Longitude - BottomLeft.Longitude;
				}
			}
		}
		public double HeightDegrees { get { return TopRight.Latitude - BottomLeft.Latitude; } }

		public GeoRectangle(LatLon bottomLeft, LatLon topRight)
		{
			if (bottomLeft.Longitude > topRight.Longitude)
				throw new ArgumentException("The bottom left longitude cannot be greater than the top right longitude.");

			if (bottomLeft.Latitude > topRight.Latitude)
				throw new ArgumentException("The bottom left latitude cannot be greater than the top right latitude.");

			this.TopRight = topRight;
			this.BottomLeft = bottomLeft;
		}

		/// <summary>
		/// Creates a GeoRectangle from the specified Quadtree location.
		/// </summary>
		/// <param name="source"></param>
		public GeoRectangle(Location source)
		{
			double pow = Math.Pow(2, source.depth);
			double width = 360 / pow;
			double height = 180 / pow;

			double minLon = width * source.i - 180;
			double maxLat = (pow - source.j) * height - 90;
			double maxLon = minLon + width;
			double minLat = maxLat - height;

			this.BottomLeft = new LatLon(minLat, minLon);
			this.TopRight = new LatLon(maxLat, maxLon);
		}

		public bool Intersects(GeoRectangle other)
		{
			// Trivial case where the two rectangles are the same
			if (Equals(other))
				return true;

			// case where the rectangles wrap around the edge of the map
			if (TopRight.Longitude < 0 && BottomLeft.Longitude > 0 || other.TopRight.Longitude < 0 && other.BottomLeft.Longitude > 0)
			{
				LatLon offset = new LatLon(0, Math.Min(BottomLeft.Longitude, other.BottomLeft.Longitude));

				GeoRectangle offsetOther = other + offset;
				GeoRectangle offsetThis = this + offset;
				return offsetThis.Intersects(offsetOther);
			}

			// bottom left point
			if (Contains(other.BottomLeft.Longitude, other.BottomLeft.Latitude))
				return true;

			// bottom right point
			if (Contains(other.TopRight.Longitude, other.BottomLeft.Latitude))
				return true;

			// top right point
			if (Contains(other.TopRight.Longitude, other.TopRight.Latitude))
				return true;

			// top left point
			if (Contains(other.BottomLeft.Longitude, other.TopRight.Latitude))
				return true;

			return false;
		}

		/// <summary>
		/// Test if this GeoRectangle contains the specified point.
		/// </summary>
		/// <param name="point">The geographic coordinates to test.</param>
		/// <returns>True if this rectangle contains the point.</returns>
		public bool Contains(LatLon point)
		{
			return Contains(point.Longitude, point.Latitude);
		}

		/// <summary>
		/// Test if this GeoRectangle contains the specified point.
		/// </summary>
		/// <param name="x">The longitude to test.</param>
		/// <param name="y">The latitude to test.</param>
		/// <returns>True if this rectangle contains the point.</returns>
		private bool Contains(double x, double y)
		{
			double minLon = BottomLeft.Longitude;
			double minLat = BottomLeft.Latitude;
			double maxLon = TopRight.Longitude;
			double maxLat = TopRight.Latitude;

			if (x > minLon && x < maxLon && y > minLat && y < maxLat)
				return true;

			return false;
		}

		public bool Intersects(Location other)
		{
			GeoRectangle geoOther = new GeoRectangle(other);
			return Intersects(geoOther);
		}

		public bool Equals(GeoRectangle other)
		{
			if (other.BottomLeft.Equals(BottomLeft) && other.TopRight.Equals(TopRight))
				return true;

			return false;
		}

		public static GeoRectangle operator +(GeoRectangle rect, LatLon operand)
		{
			LatLon bottomLeft = rect.BottomLeft + operand;
			LatLon topRight = rect.TopRight + operand;
			return new GeoRectangle(bottomLeft, topRight);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + BottomLeft.GetHashCode();
				hash = hash * 23 + TopRight.GetHashCode();
				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is GeoRectangle)
				return Equals((GeoRectangle)obj);

			return false;
		}

		public static bool operator ==(GeoRectangle a, GeoRectangle b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(GeoRectangle a, GeoRectangle b)
		{
			return a.Equals(b);
		}

		public override string ToString()
		{
			return string.Format(
				"[GeoRectangle: bottomLeft= {0} topRight= {1} (width: {2}° height: {3}°)",
				BottomLeft.ToShortString(),
				TopRight.ToShortString(),
				WidthDegrees,
				HeightDegrees);
		}
	}
}
