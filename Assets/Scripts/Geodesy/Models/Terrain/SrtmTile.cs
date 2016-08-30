using System;

namespace Geodesy.Models
{
	public enum SrtmFormat
	{
		SRTM3 = 1201,
		SRTM1 = 3601
	}

	public class SrtmTile
	{
		private const int Void = short.MinValue;

		private byte[] data;

		public SrtmFormat Format { get; set; }

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
		/// </summary>
		/// <returns>The elevation.</returns>
		/// <param name="point">Point.</param>
		public int Sample (float lat, float lon)
		{
			int size = (int)Format - 1;

			int y = (int)(size - (size * lat));
			int x = (int)(size * lon);

			int index = sizeof(short) * (y * (size + 1) + x);

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

