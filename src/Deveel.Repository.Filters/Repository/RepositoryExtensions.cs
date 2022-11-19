using System;
using System.Linq.Dynamic.Core;

using Deveel.Filters;

namespace Deveel.Repository {
	public static class RepositoryExtensions {
		public static Task<TEntity> FindAsync<TEntity>(this IRepository<TEntity> repository, string paramName, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAsync<TEntity>(filter.AsLambda<TEntity>(paramName), cancellationToken);

		public static Task<TEntity> FindAsync<TEntity>(this IRepository<TEntity> repository, IFilter filter, CancellationToken cancellationToken = default)
	where TEntity : class, IEntity
	=> repository.FindAsync<TEntity>("x", filter, cancellationToken);

	}
}
