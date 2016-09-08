using System;

namespace Geodesy.Models
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
			double minLat = width * source.j - 90;
			double maxLon = minLon + width;
			double maxLat = minLat + height;

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
			return Intersects(new GeoRectangle(other));
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
	}
}
