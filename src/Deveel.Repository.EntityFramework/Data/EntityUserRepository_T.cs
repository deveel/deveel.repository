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
	/// <seealso cref="EntityUserRepository{TEntity, TKey, TUserKey}"/>
	public class EntityUserRepository<TEntity> : EntityUserRepository<TEntity, object>
		where TEntity : class, IHaveOwner<object>
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
		public EntityUserRepository(DbContext context, IUserAccessor<object> userAccessor, ILogger<EntityUserRepository<TEntity, object>>? logger = null) : base(context, userAccessor, logger)
		{
		}

		/// <summary>
		/// Constructs the repository using the given <see cref="DbContext"/> for
		/// a specific tenant.
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
		public EntityUserRepository(DbContext context, ITenantInfo? tenantInfo, IUserAccessor<object> userAccessor, ILogger<EntityUserRepository<TEntity, object>>? logger = null) : base(context, tenantInfo, userAccessor, logger)
		{
		}
	}
}
