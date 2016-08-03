using System;

namespace Geodesy.Models
{
	/// <summary>
	/// Represents a point given by geographic coordinates
	/// </summary>
	public struct LatLon
	{
		char northingChar;
		char eastingChar;

		private double longitude;

		public double Longitude { get { return longitude; } }

		private double altitude;

		public double Altitude { get { return altitude; } }

		private double latitude;

		public double Latitude { get { return latitude; } }

		private double latDegrees;

		public double LatDegrees { get { return latDegrees; } }

		private double lonDegrees;

		public double LonDegrees { get { return lonDegrees; } }

		private double latMinutes;

		public double LatMinutes { get { return latMinutes; } }

		private double lonMinutes;

		public double LonMinutes { get { return lonMinutes; } }

		private double latSeconds;

		public double LatSeconds { get { return latSeconds; } }

		private double lonSeconds;

		public double LonSeconds { get { return lonSeconds; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Geodesy.Models.LatLon"/> using decimal angles.
		/// </summary>
		/// <param name="decimalLat">Decimal latitude.</param>
		/// <param name="decimalLon">Decimal longitude.</param>
		/// <param name="altitude">Altitude.</param>
		public LatLon (double decimalLat, double decimalLon, double alt)
		{
			if (decimalLon > 180 || decimalLon < -180)
				throw new FormatException ("Invalid longitude: " + decimalLon.ToString ());

			if (decimalLat > 90 || decimalLat < -90)
				throw new FormatException ("Invalid latitude: " + decimalLat.ToString ());

			longitude = decimalLon;
			latitude = decimalLat;
			altitude = alt;

			northingChar = latitude >= 0 ? 'N' : 'S';
			eastingChar = longitude >= 0 ? 'E' : 'W';

			double abslat = Math.Abs (latitude);
			double abslon = Math.Abs (longitude);

			latDegrees = (int)Math.Floor (abslat);
			lonDegrees = (int)Math.Floor (abslon);

			double abslatMinusDeg = abslat - latDegrees;
			double abslonMinusDeg = abslon - lonDegrees;

			latMinutes = (int)Math.Floor (60 * abslatMinusDeg);
			lonMinutes = (int)Math.Floor (60 * abslonMinusDeg);

			latSeconds = 3600 * abslatMinusDeg - 60 * latMinutes;
			lonSeconds = 3600 * abslonMinusDeg - 60 * lonMinutes;
		}

		public override string ToString ()
		{
			return string.Format (
				"{0} {1:D2}° {2:D2}' {3:F4}\" " +
				"{4} {5:D2}° {6:D2}' {7:F4}\", " +
				"{8:0}m", 
				northingChar, latDegrees, latMinutes, latSeconds,
				eastingChar, lonDegrees, lonMinutes, lonSeconds,
				Altitude);
		}
	}
}

