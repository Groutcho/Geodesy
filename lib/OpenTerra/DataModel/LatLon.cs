﻿using System;

namespace OpenTerra.DataModel
{
	/// <summary>
	/// Represents a point given by geographic coordinates
	/// </summary>
	public struct LatLon : IEquatable<LatLon>, IComparable<LatLon>
	{
		#region fields

		private double longitude;
		private double latitude;
		private double altitude;

		#endregion

		#region properties

		public double Longitude { get { return longitude; } }

		public double Altitude { get { return altitude; } }

		public double Latitude { get { return latitude; } }

		public double LatDegrees
		{
			get
			{
				return Math.Floor (Math.Abs (latitude));
			}
		}

		public double LonDegrees
		{
			get
			{
				return Math.Floor (Math.Abs (longitude));
			}
		}

		public double LatMinutes
		{
			get
			{
				double abslatMinusDeg = Math.Abs (latitude) - LatDegrees;
				return Math.Floor (60 * abslatMinusDeg);
			}
		}

		public double LonMinutes
		{
			get
			{
				double abslonMinusDeg = Math.Abs (longitude) - LonDegrees;
				return Math.Floor (60 * abslonMinusDeg);
			}
		}

		public double LatSeconds
		{
			get
			{
				double abslatMinusDeg = Math.Abs (latitude) - LatDegrees;
				return 3600 * abslatMinusDeg - 60 * LatMinutes;
			}
		}

		public double LonSeconds
		{
			get
			{
				double abslonMinusDeg = Math.Abs (longitude) - LonDegrees;
				return 3600 * abslonMinusDeg - 60 * LonMinutes;
			}
		}

		#endregion

		#region constructors

		public LatLon (double latitude, double longitude)
			: this (latitude, longitude, 0)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenTerra.Models.LatLon"/> using decimal angles.
		/// </summary>
		/// <param name="decimalLat">Decimal latitude in degrees.</param>
		/// <param name="decimalLon">Decimal longitude in degrees.</param>
		/// <param name="altitude">Altitude in meters.</param>
		public LatLon (double latitude, double longitude, double altitude)
		{
			// wrap around cases
			if (longitude > 180)
				longitude = -180 + (longitude % 180);
			if (longitude < -180)
				longitude = 180 + (longitude % -180);

			if (latitude > 90 || latitude < -90)
				throw new FormatException ("Invalid latitude: " + latitude.ToString ());

			this.longitude = longitude;
			this.latitude = latitude;
			this.altitude = altitude;
		}

		#endregion

		#region operators

		public static LatLon operator +(LatLon a, LatLon b)
		{
			return new LatLon(a.latitude + b.latitude, a.longitude + b.longitude);
		}

		#endregion

		public override string ToString ()
		{
			char northing = latitude >= 0 ? 'N' : 'S';
			char easting = longitude >= 0 ? 'E' : 'W';

			return string.Format (
				"{0} {1:00}° {2:00}' {3:F4} " +
				"{4} {5:00}° {6:00}' {7:F4}, " +
				"{8:0}m",
				northing, LatDegrees, LatMinutes, LatSeconds,
				easting, LonDegrees, LonMinutes, LonSeconds,
				Altitude);
		}

		public string ToShortString()
		{
			char northing = latitude >= 0 ? 'N' : 'S';
			char easting = longitude >= 0 ? 'E' : 'W';

			return string.Format(
				"{0} {1:00}° {2:00}' " +
				"{3} {4:00}° {5:00}', " +
				"{6:0}m",
				northing, LatDegrees, LatMinutes,
				easting, LonDegrees, LonMinutes,
				Altitude);
		}

		public override bool Equals (object obj)
		{
			if (obj is LatLon)
				return Equals ((LatLon)obj);

			return false;
		}

		public static bool operator ==(LatLon a, LatLon b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(LatLon a, LatLon b)
		{
			return !a.Equals(b);
		}

		public override int GetHashCode ()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + latitude.GetHashCode ();
				hash = hash * 23 + longitude.GetHashCode ();
				hash = hash * 23 + altitude.GetHashCode ();
				return hash;
			}
		}

		#region IEquatable implementation

		public bool Equals (LatLon other)
		{
			return (other.Altitude.Equals (altitude) &&
			other.latitude.Equals (latitude) &&
			other.longitude.Equals (longitude));
		}

		#endregion

		#region IComparable implementation

		public int CompareTo (LatLon other)
		{
			if (this.Equals (other))
				return 0;

			if (this.latitude == other.latitude)
			{
				if (this.longitude == other.longitude)
				{
					if (this.altitude > other.altitude)
						return -1;
					else
					{
						return 1;
					}
				} else
				{
					if (this.longitude > other.longitude)
					{
						return -1;
					} else
					{
						return 1;
					}
				}
			} else if (this.latitude > other.latitude)
			{
				return -1;
			} else
			{
				return 1;
			}
		}

		#endregion
	}
}

