using System;

namespace Geodesy.Models
{
	public enum SrtmFormat
	{
		SRTM3 = 1201,
		SRTM1 = 3601
	}

	public enum Filtering
	{
		Point,
		Bilinear
	}

	public class SrtmTile
	{
		private const int Void = short.MinValue;

		private byte[] data;

		private SrtmFormat format;

		public SrtmFormat Format
		{
			get { return format; }
			set
			{
				format = value;
				size = (int)value - 1;
			}
		}

		private int size;

		private float latitude;
		private float longitude;

		public int Easting
		{
			get { return (int)(longitude) + 180; }
		}

		public int Northing
		{
			get { return (int)(latitude) + 90; }
		}

		public SrtmTile (float lat, float lon, byte[] data)
		{
			this.latitude = lat;
			this.longitude = lon;
			this.data = data;

			if (data.Length == 1201 * 1201 * 2)
				Format = SrtmFormat.SRTM3;
			else if (data.Length == 3601 * 3601 * 2)
				Format = SrtmFormat.SRTM1;
			else
				throw new FormatException ("Invalid SRTM data");
		}

		public bool Contains (float pLat, float pLon)
		{
			return pLat >= latitude &&
			pLat < latitude + 1 &&
			pLon >= longitude &&
			pLon < longitude + 1;
		}

		/// <summary>
		/// Return the elevation in meters at the specified point.
		/// Perform bilinear interpolation.
		/// </summary>
		/// <returns>The elevation.</returns>
		/// <param name="point">Point.</param>
		public int Sample (float lat, float lon, Filtering filtering)
		{
			float yratio = size - (size * lat);
			float xratio = size * lon;

			int x = (int)xratio;
			int y = (int)yratio;

			short a = Sample (x, y);
			if (filtering == Filtering.Point)
			{
				return a;
			}

			float tx = xratio - x;
			float ty = yratio - y;

			short b = Sample (x + 1, y);
			short c = Sample (x, y + 1);
			short d = Sample (x + 1, y + 1);

			float ab = a + (b - a) * tx;
			float cd = c + (d - c) * tx;

			float r = ab + (cd - ab) * ty;
			return (short)r;
		}

		private short Sample (int i, int j)
		{
			i = i >= size ? size : i;
			j = j >= size ? size : j;

			int index = sizeof(short) * (j * (size + 1) + i);

			int msb = (int)data [index];
			int lsb = (int)data [index + 1];

			// SRTM data is big endian
			short result = (short)(msb << 8 | lsb);
			if (result == Void)
			{
				return 0;
			}
			return result;
		}
	}
}

