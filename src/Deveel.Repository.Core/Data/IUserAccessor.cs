namespace Deveel.Data
{
	/// <summary>
	/// A service that provides information about the current user
	/// of the application.
	/// </summary>
	/// <remarks>
	/// This contact can be used to retrieve identifier about the
	/// user that is currently using the application, such as the
	/// username or the user ID.
	/// </remarks>
	/// <typeparam name="TKey">
	/// The type of the key that identifies the user.
	/// </typeparam>
	public interface IUserAccessor<TKey>
	{
		/// <summary>
		/// Gets the identifier of the current user.
		/// </summary>
		/// <returns>
		/// Returns a string that represents the identifier of the
		/// user that is currently using the application.
		/// </returns>
		TKey? GetUserId();
	}
}
