namespace Deveel.Data {
	public static class EnumerableExtensions {
		public static IRepository<TEntity> AsRepository<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class {
			return new RepositoryWrapper<TEntity>(entities);
		}
	}
}
