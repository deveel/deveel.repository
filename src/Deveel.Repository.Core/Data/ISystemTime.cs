namespace Deveel.Data {
	/// <summary>
	/// A service that provides the current system time.
	/// </summary>
	public interface ISystemTime {
		/// <summary>
		/// Gets the current system time in UTC.
		/// </summary>
		DateTimeOffset UtcNow { get; }

		/// <summary>
		/// Gets the current local system time.
		/// </summary>
		DateTimeOffset Now { get; }
	}
}
