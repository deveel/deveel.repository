using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data
{
	/// <summary>
	/// An implementation of a repository that is bound to a specific user
	/// context, where the entities are owned by a specific user.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity managed by the repository.
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the unique identifier of the entity.
	/// </typeparam>
	/// <typeparam name="TUserKey">
	/// The type of the key that identifies the owner of the entity.
	/// </typeparam>
	public class EntityUserRepository<TEntity, TKey, TUserKey> : EntityRepository<TEntity, TKey>, IUserRepository<TEntity, TKey, TUserKey>
		where TEntity : class, IHaveOwner<TUserKey>
	{
		/// <summary>
		/// Constructs the repository using the given <see cref="DbContext"/>.
		/// </summary>
		/// <param name="context">
		/// The <see cref="DbContext"/> used to access the data of the entities.
		/// </param>
		/// <param name="userAccessor">
		/// A service used to get the current user context.
		/// </param>
		/// <param name="logger">
		/// A logger used to log the operations of the repository.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given <paramref name="userAccessor"/> is <c>null</c>.
		/// </exception>
		public EntityUserRepository(DbContext context, IUserAccessor<TUserKey> userAccessor, ILogger<EntityUserRepository<TEntity, TKey, TUserKey>>? logger = null) : base(context, logger)
		{
			ArgumentNullException.ThrowIfNull(userAccessor, nameof(userAccessor));
			UserAccessor = userAccessor;
		}

		/// <summary>
		/// Constructs the repository using the given <see cref="DbContext"/> for
		/// the given tenant.
		/// </summary>
		/// <param name="context">
		/// The <see cref="DbContext"/> used to access the data of the entities.
		/// </param>
		/// <param name="tenantInfo">
		/// The information about the tenant that the repository will use to access the data.
		/// </param>
		/// <param name="userAccessor">
		/// A service used to get the current user context.
		/// </param>
		/// <param name="logger">
		/// A logger used to log the operations of the repository.
		/// </param>
		public EntityUserRepository(DbContext context, ITenantInfo? tenantInfo, IUserAccessor<TUserKey> userAccessor, ILogger<EntityUserRepository<TEntity, TKey, TUserKey>>? logger = null) : base(context, tenantInfo, logger)
		{
			ArgumentNullException.ThrowIfNull(userAccessor, nameof(userAccessor));
			UserAccessor = userAccessor;
		}

		IUserAccessor<TUserKey> IUserRepository<TEntity, TKey, TUserKey>.UserAccessor => UserAccessor;

		/// <summary>
		/// Gets the accessor to the user context of the repository.
		/// </summary>
		protected IUserAccessor<TUserKey> UserAccessor { get; }

		/// <summary>
		/// Gets the identifier of the current user.
		/// </summary>
		protected TUserKey? UserId => UserAccessor.GetUserId();

		/// <inheritdoc/>
		public override async Task<TEntity?> FindAsync(TKey key, CancellationToken cancellationToken = default)
		{
			var result = await base.FindAsync(key, cancellationToken);

			if (result == null || !Equals(UserId, result.Owner))
				return null;
			return result;
		}

		/// <inheritdoc/>
		protected override TEntity OnAddingEntity(TEntity entity)
		{
			var userId = UserId;
			if (!Equals(default(TUserKey), UserId) 
				&& !Equals(entity.Owner, userId))
				entity.SetOwner(userId!);

			return entity;
		}
	}
}
