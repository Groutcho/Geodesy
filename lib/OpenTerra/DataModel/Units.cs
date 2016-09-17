namespace OpenTerra.DataModel
{
	/// <summary>
	/// Helper class to perform unit conversions, such as kilometers to meters.
	/// </summary>
	public static class Units
	{
		/// <summary>
		/// Convert from kilometers to meters.
		/// </summary>
		public static int km_to_m(int km)
		{
			return km * 1000;
		}

		/// <summary>
		/// Convert from megabytes to bytes.
		/// </summary>
		public static long mb_to_b(long megabytes)
		{
			return megabytes * 1024 * 1024;
		}
	}
}
