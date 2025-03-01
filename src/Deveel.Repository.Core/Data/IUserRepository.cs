namespace Deveel.Data
{
	/// <summary>
	/// Defines a repository that is bound to a user context,
	/// where the entities are owned by a specific user.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity handled by the repository.
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the unique identifier of the entity.
	/// </typeparam>
	/// <typeparam name="TOwnerKey">
	/// The type of the key that identifies the owner of the entity.
	/// </typeparam>
	public interface IUserRepository<TEntity, TKey, TOwnerKey> : IRepository<TEntity, TKey>
		where TEntity : class, IHaveOwner<TOwnerKey>
	{
		/// <summary>
		/// Gets the accessor to the user context of the repository.
		/// </summary>
		IUserAccessor<TOwnerKey> UserAccessor { get; }
	}
}
