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
		private const int NoData = short.MinValue;

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
		public int Sample (float pLat, float pLon)
		{
			float lon = pLon - longitude;
			float lat = pLat - latitude;

			int size = (int)Format;

			int y = (int)(size - (size * lat));
			int x = (int)(size * lon);

			int index = sizeof(short) * ((y - 1) * size + (x - 1));

			if (data.Length <= index || index < 0)
				return 0;

			short msb = (short)data [index];
			short lsb = (short)data [index + 1];

			short result = (short)(msb << 8 | lsb);

			if (result == NoData)
			{
				return 0;
			}

			// SRTM data is big endian
			return result;
		}
	}
}

