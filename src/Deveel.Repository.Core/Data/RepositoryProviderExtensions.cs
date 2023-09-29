using System.Linq.Expressions;

namespace Deveel.Data {
	[Obsolete("The extensions provided by this class are deprecated and will be removed in future versions")]
    public static class RepositoryProviderExtensions {
        private static IFilterableRepository RequireFilterable(this IRepositoryProvider provider, string tenantId) {
            var filterable = provider.GetRepository(tenantId) as IFilterableRepository;
            if (filterable == null)
                throw new NotSupportedException("The repository is not filterable");

            return filterable;
        }

        private static IFilterableRepository<TEntity> RequireFilterable<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId) where TEntity : class {
            var filterable = provider.GetRepository(tenantId) as IFilterableRepository<TEntity>;
            if (filterable == null)
                throw new NotSupportedException("The repository is not filterable");

            return filterable;
        }

		private static ITransactionalRepository<TEntity> RequireTransactional<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId) where TEntity : class {
			var transactional = provider.GetRepository(tenantId) as ITransactionalRepository<TEntity>;
			if (transactional == null)
				throw new NotSupportedException("The repository is not transactional");

			return transactional;
		}

		private static ITransactionalRepository RequireTransactional(this IRepositoryProvider provider, string tenantId) {
			var transactional = provider.GetRepository(tenantId) as ITransactionalRepository;
			if (transactional == null)
				throw new NotSupportedException("The repository is not transactional");

			return transactional;
		}


		#region GetRepository

		public static IRepository GetRepository(this IRepositoryProvider provider, string tenantId)
            => provider.GetRepositoryAsync(tenantId).GetAwaiter().GetResult();

        public static IRepository<TEntity> GetRepository<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId)
            where TEntity : class
            =>  provider.GetRepositoryAsync(tenantId).GetAwaiter().GetResult();

        #endregion


        #region  Create

        public static Task<string> CreateAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.GetRepository(tenantId).AddAsync(entity, cancellationToken);

        public static Task<string> CreateAsync<TEntity>(this IRepositoryProvider<TEntity> provider, IDataTransaction transaction, string tenantId, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.RequireTransactional<TEntity>(tenantId).CreateAsync(transaction, entity, cancellationToken);

        public static string Create<TEntity>(this IRepositoryProvider<TEntity> provider, IDataTransaction transaction, string tenantId, TEntity entity)
            where TEntity : class
            => provider.RequireTransactional<TEntity>(tenantId).Create(transaction, entity);

        public static Task<string> CreateAsync(this IRepositoryProvider provider, string tenantId, object entity, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).AddAsync(entity, cancellationToken);

        public static string Create(this IRepositoryProvider provider, string tenantId, object entity)
            => provider.GetRepository(tenantId).Create(entity);

        public static Task<string> CreateAsync(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, object entity, CancellationToken cancellationToken = default)
            => provider.RequireTransactional(tenantId).AddAsync(transaction, entity, cancellationToken);

        public static string Create(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, object entity)
            => provider.RequireTransactional(tenantId).Create(transaction, entity);

        #endregion

        #region  Delete

        public static Task<bool> DeleteAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.GetRepository(tenantId).RemoveAsync(entity, cancellationToken);

        public static Task<bool> DeleteAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.RequireTransactional<TEntity>(tenantId).DeleteAsync(transaction, entity, cancellationToken);


        public static bool Delete<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, TEntity entity)
            where TEntity : class
            => provider.GetRepository(tenantId).Delete(entity);

        public static bool Delete<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IDataTransaction transaction, TEntity entity)
            where TEntity : class
            => provider.RequireTransactional<TEntity>(tenantId).Delete(transaction, entity);


        public static Task<bool> DeleteAsync(this IRepositoryProvider provider, string tenantId, object entity, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).RemoveAsync(entity, cancellationToken);

        public static Task<bool> DeleteAsync(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, object entity, CancellationToken cancellationToken = default)
            => provider.RequireTransactional(tenantId).RemoveAsync(transaction, entity, cancellationToken);

        public static bool Delete(this IRepositoryProvider provider, string tenantId, object entity)
            => provider.GetRepository(tenantId).Delete(entity);

        public static bool Delete(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, object entity)
            => provider.RequireTransactional(tenantId).Delete(transaction, entity);

        public static Task<bool> DeleteByIdAsync(this IRepositoryProvider provider, string tenantId, string id, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).DeleteByIdAsync(id, cancellationToken);

        public static Task<bool> DeleteByIdAsync(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, string id, CancellationToken cancellationToken = default)
            => provider.RequireTransactional(tenantId).DeleteByIdAsync(transaction, id, cancellationToken);


        public static bool DeleteById(this IRepositoryProvider provider, string tenantId, string id)
            => provider.GetRepository(tenantId).DeleteById(id);

        public static bool DeleteById(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, string id)
            => provider.RequireTransactional(tenantId).DeleteById(transaction, id);


        #endregion

        #region Find

        public static Task<TEntity?> FindAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.RequireFilterable(tenantId).FindAsync(filter, cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.GetRepository(tenantId).FindAsync(filter, cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.GetRepository(tenantId).FindAsync(cancellationToken);

        public static Task<object?> FindAsync(this IRepositoryProvider provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            => provider.RequireFilterable(tenantId).FindAsync(filter, cancellationToken);

        public static Task<object?> FindAsync(this IRepositoryProvider provider, string tenantId, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).FindAsync(cancellationToken);

		#endregion

		#region FindById

		public static Task<TEntity?> FindByIdAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, string id, CancellationToken cancellationToken = default)
			where TEntity : class
			=> provider.GetRepository(tenantId).FindByIdAsync(id, cancellationToken);

		public static Task<object?> FindByIdAsync(this IRepositoryProvider provider, string tenantId, CancellationToken cancellationToken = default)
			=> provider.GetRepository(tenantId).FindAsync(cancellationToken);

		#endregion

		#region FindAll

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.RequireFilterable(tenantId).FindAllAsync(filter, cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.GetRepository(tenantId).FindAllAsync(filter, cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, CancellationToken cancellationToken = default)
            where TEntity : class
            => provider.GetRepository(tenantId).FindAllAsync(cancellationToken);

        public static Task<IList<object>> FindAllAsync(this IRepositoryProvider provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            => provider.RequireFilterable(tenantId).FindAllAsync(filter, cancellationToken);

        public static Task<IList<object>> FindAllAsync(this IRepositoryProvider provider, string tenantId, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).FindAllAsync(cancellationToken);

		#endregion

		#region Count

		public static Task<long> CountAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class
			=> provider.RequireFilterable(tenantId).CountAsync(filter, cancellationToken);

		public static Task<long> CountAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
			where TEntity : class
			=> provider.RequireFilterable(tenantId).CountAsync(filter, cancellationToken);

		public static Task<long> CountAsync(this IRepositoryProvider provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
			=> provider.RequireFilterable(tenantId).CountAsync(filter, cancellationToken);


		#endregion

		#region CountAll

		public static Task<long> CountAllAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class
			=> provider.RequireFilterable(tenantId).CountAllAsync(cancellationToken);


		public static Task<long> CountAllAsync(this IRepositoryProvider provider, string tenantId, CancellationToken cancellationToken = default)
			=> provider.RequireFilterable(tenantId).CountAllAsync(cancellationToken);

		#endregion

		#region Exists

		public static Task<bool> ExistsAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class
			=> provider.RequireFilterable(tenantId).ExistsAsync(filter, cancellationToken);

		public static Task<bool> ExistsAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
			where TEntity : class
			=> provider.GetRepository(tenantId).ExistsAsync(filter, cancellationToken);

		public static Task<bool> ExistsAsync(this IRepositoryProvider provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
			=> provider.RequireFilterable(tenantId).ExistsAsync(filter, cancellationToken);


		#endregion

		//#region GetPage

		//public static Task<RepositoryPage<TEntity>> GetPageAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default)
		//	where TEntity : class, IEntity
		//	=> provider.GetRepository(tenantId).GetPageAsync(request, cancellationToken);

		//public static Task<RepositoryPage> GetPageAsync(this IRepositoryProvider provider, string tenantId, RepositoryPageRequest request, CancellationToken cancellationToken = default)
		//	=> provider.GetRepository(tenantId).GetPageAsync(request, cancellationToken);


		//#endregion
	}
}