namespace Deveel.Data {
	/// <summary>
	/// A structure that identifies an aggregate by its key and
	/// revision version.
	/// </summary>
	public readonly struct AggregateKey {
		/// <summary>
		/// Constructs the key of an aggregate.
		/// </summary>
		/// <param name="key">
		/// The unique key of the aggregate.
		/// </param>
		/// <param name="version">
		/// An optional version of the aggregate to identify.
		/// </param>
		public AggregateKey(object key, long? version = null) {
			ArgumentNullException.ThrowIfNull(key, nameof(key));

			Key = key;
			Version = version;
		}

		/// <summary>
		/// Gets the unique key of the aggregate.
		/// </summary>
		public object Key { get; }

		/// <summary>
		/// Gets the version of the aggregate to identify.
		/// </summary>
		public long? Version { get; }
	}
}
