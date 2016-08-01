namespace Geodesy
{
	/// <summary>
	/// Provides services to allow caching of an object.
	/// </summary>
	public interface ICacheable
	{
		ulong InstanceId { get; set; }
	}
}