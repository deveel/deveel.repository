namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="IRepositoryProvider{TEntity}"/> interface
	/// with further methods to resolve a repository.
	/// </summary>
	/// <seealso cref="IRepositoryProvider{TEntity}"/>
	public static class RepositoryProviderExtensions {
		/// <summary>
		/// Synchronously resolves the repository for the given tenant.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="provider">
		/// The instance of the provider that resolves the repository.
		/// </param>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repository is resolved.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IRepository{TEntity}"/> that
		/// is isolated for the given tenant.
		/// </returns>
		public static IRepository<TEntity> GetRepository<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId)
			where TEntity : class
			=> provider.GetRepositoryAsync(tenantId).GetAwaiter().GetResult();
	}
}
