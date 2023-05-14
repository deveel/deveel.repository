namespace Deveel.Data {
	/// <summary>
	/// A default implementation of <see cref="ISystemTime"/> that
	/// uses the <see cref="DateTimeOffset"/> of the current system.
	/// </summary>
	public sealed class SystemTime : ISystemTime {
		/// <inheritdoc/>
		public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

		/// <inheritdoc/>
		public DateTimeOffset Now => DateTimeOffset.Now;

		/// <summary>
		/// Gets the default instance of the system time.
		/// </summary>
		public static ISystemTime Default { get; } = new SystemTime();
	}
}
