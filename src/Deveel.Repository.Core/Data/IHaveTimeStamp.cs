namespace Deveel.Data {
	/// <summary>
	/// A contract for an object that has timestamps on the
	/// creation and update.
	/// </summary>
	public interface IHaveTimeStamp {
		/// <summary>
		/// Gets or sets the timestamp of the creation of the object.
		/// </summary>
		DateTimeOffset? CreatedAtUtc { get; set; }

		/// <summary>
		/// Gets or sets the timestamp of the last update of the object.
		/// </summary>
		DateTimeOffset? UpdatedAtUtc { get; set; }
	}
}
