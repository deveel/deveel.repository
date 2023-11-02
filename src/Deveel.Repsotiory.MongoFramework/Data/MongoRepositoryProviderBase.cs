using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

using MongoFramework;

namespace Deveel.Data {
	/// <summary>
	/// A generic base class for the implementation of a repository provider
	/// for the MongoFramework library.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to create the repository.
	/// </typeparam>
	/// <typeparam name="TEntity">
	/// The type of the entity that is managed by the repository.
	/// </typeparam>
	/// <typeparam name="TTenantInfo">
	/// The type of the tenant information that is used to create the context.
	/// </typeparam>
	public abstract class MongoRepositoryProviderBase<TContext, TEntity, TTenantInfo> : IDisposable
		where TContext : class, IMongoDbContext
		where TTenantInfo : class, ITenantInfo, new()
		where TEntity : class {
		private readonly IEnumerable<IMultiTenantStore<TTenantInfo>>? stores;
		private bool disposedValue;

		private IDictionary<string, object>? repositories;

		internal MongoRepositoryProviderBase(
			IEnumerable<IMultiTenantStore<TTenantInfo>>? stores = null,
			ILoggerFactory? loggerFactory = null) {
			this.stores = stores;
			LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		/// <summary>
		/// Gets the factory to create the logger to be used by the repository.
		/// </summary>
		protected ILoggerFactory LoggerFactory { get; }

		/// <summary>
		/// Attempts to resolve the tenant information for a given tenant ID.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant to be resolved (that can
		/// be the ID or the identifier of the tenant).
		/// </param>
		/// <param name="cancellationToken">
		/// A token to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TTenantInfo"/> that
		/// is identified by the given <paramref name="tenantId"/>, or
		/// <c>null</c> if no tenant information could be resolved.
		/// </returns>
		protected virtual async Task<TTenantInfo?> GetTenantInfoAsync(string tenantId, CancellationToken cancellationToken) {
			if (stores == null)
				return null;

			foreach (var store in stores) {
				// TODO: making the IRepositoryProvider to be async
				var tenantInfo = await store.TryGetAsync(tenantId);

				if (tenantInfo == null)
					tenantInfo = await store.TryGetByIdentifierAsync(tenantId);

				if (tenantInfo != null) {
					return tenantInfo;
				}
			}

			return null;
		}

		/// <summary>
		/// Creates a logger to be used by the repository.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="ILogger"/> that is used
		/// to log messages from the repository.
		/// </returns>
		protected virtual ILogger CreateLogger() {
			return LoggerFactory.CreateLogger(typeof(MongoRepository<TEntity>));
		}

		/// <summary>
		/// Creates an instance of <typeparamref name="TContext"/> that
		/// can be used to create the repository for a given tenant.
		/// </summary>
		/// <param name="connection">
		/// The connection to the MongoDB database.
		/// </param>
		/// <param name="tenantInfo">
		/// The information about the tenant to be used to create the context.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TContext"/> that
		/// can be used to create the repository for a given tenant.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when either the <paramref name="connection"/> or the
		/// <paramref name="tenantInfo"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// Thrown when the <typeparamref name="TContext"/> has no
		/// suitable constructor to be created.
		/// </exception>
		protected virtual TContext? CreateContext(IMongoDbConnection connection, TTenantInfo tenantInfo) {
			ArgumentNullException.ThrowIfNull(connection, nameof(connection));
			ArgumentNullException.ThrowIfNull(tenantInfo, nameof(tenantInfo));

			return (TContext)MongoDbContextUtil.CreateContext<TContext>(connection, tenantInfo);
		}

		internal abstract object CreateRepositoryInternal(TContext context);

		internal async Task<TRepository> GetRepositoryAsync<TRepository>(string tenantId, CancellationToken cancellationToken = default) {
			try {
				if (repositories == null)
					repositories = new Dictionary<string, object>();

				if (!repositories.TryGetValue(tenantId, out var repository)) {
					var tenantInfo = await GetTenantInfoAsync(tenantId, cancellationToken);

					if (tenantInfo == null)
						throw new RepositoryException($"Unable to find any tenant for the ID '{tenantId}' - cannot construct a context");

					var connection = MongoDbConnection.FromConnectionString(tenantInfo.ConnectionString);

					// var tenantContext = CreateTenantContext(tenantInfo);
					var context = CreateContext(connection, tenantInfo);

					if (context == null)
						throw new RepositoryException($"Unable to create the Mongo DB Context");

					repository = CreateRepositoryInternal(context);

					repositories[tenantId] = repository;
				}

				return (TRepository) repository;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException($"Unabe to create the repository for tenant '{tenantId}'", ex);
			}
		}

		/// <summary>
		/// Disposes the provider and all the repositories created by it.
		/// </summary>
		/// <param name="disposing">
		/// A flag indicating if the provider is disposing.
		/// </param>
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					DisposeRepositories();
				}

				repositories = null;
				disposedValue = true;
			}
		}

		private void DisposeRepositories() {
			if (repositories != null) {
				foreach (var repository in repositories.Values) {
					if (repository is IDisposable disposable)
						disposable.Dispose();
				}

				repositories.Clear();
			}
		}

		/// <inheritdoc/>
		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
