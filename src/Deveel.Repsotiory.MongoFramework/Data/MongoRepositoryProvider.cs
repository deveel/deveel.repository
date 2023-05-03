using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MongoFramework;

namespace Deveel.Data {
	public class MongoRepositoryProvider<TContext, TEntity> : IRepositoryProvider<TEntity>, IDisposable
		where TContext : class, IMongoDbContext 
		where TEntity : class {
		private bool disposedValue;

		private IDictionary<string, MongoRepository<TContext, TEntity>>? repositories;

		public MongoRepositoryProvider(
			IMongoDbConnection<TContext> connection,
			ILoggerFactory? loggerFactory = null) {
			Connection = connection;
			LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		protected ILoggerFactory LoggerFactory { get; }

		public IMongoDbConnection<TContext> Connection { get; }

		protected virtual ILogger CreateLogger() {
			return LoggerFactory.CreateLogger(typeof(MongoRepository<TContext, TEntity>));
		}

		protected virtual TContext? CreateContext(string tenantId) {
			if (typeof(TContext) == typeof(MongoDbTenantContext))
				return (new MongoDbTenantContext(Connection, tenantId)) as TContext;
			if (typeof(TContext) == typeof(MongoDbContext))
				return new MongoDbContext(Connection) as TContext;

			var ctor1 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbConnection<TContext>), typeof(string) });
			if (ctor1 != null)
				return Activator.CreateInstance(typeof(TContext), new object[] { Connection.ForContext<TContext>(), tenantId }) as TContext;

			var ctor2 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbTenantConnection<TContext>) });
			if (ctor2 != null)
				return Activator.CreateInstance(typeof(TContext), new[] { CreateTenantConnection(tenantId) }) as TContext;

			throw new NotSupportedException($"Cannot create '{typeof(TContext)}' MongoDB Context");
		}

		private IMongoDbTenantConnection<TContext> CreateTenantConnection(string tenantId) {
			var connectionString = Connection.GetUrl()?.ToString();

			var tenantContext = new MultiTenantContext<TenantInfo> {
				TenantInfo = new TenantInfo { 
					Id = tenantId,
					ConnectionString = connectionString
				}
			};

			return new MongoDbTenantConnection<TContext>(tenantContext);
		}

		async Task<IRepository<TEntity>> IRepositoryProvider<TEntity>.GetRepositoryAsync(string tenantId) => await GetRepositoryAsync(tenantId);

		async Task<IRepository> IRepositoryProvider.GetRepositoryAsync(string tenantId) => await GetRepositoryAsync(tenantId);

		public async Task<MongoRepository<TContext, TEntity>> GetRepositoryAsync(string tenantId) {
			try {
				if (repositories == null)
					repositories = new Dictionary<string, MongoRepository<TContext, TEntity>>();

				if (!repositories.TryGetValue(tenantId, out var repository)) {
					var context = CreateContext(tenantId);

					if (context == null)
						throw new RepositoryException($"Unable to create the Mongo DB Context");

					repository = CreateRepository(context);

					repositories[tenantId] = repository;
				}

				return repository;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException($"Unabe to create the repository for tenant '{tenantId}'", ex);
			}
		}

		protected virtual MongoRepository<TContext, TEntity> CreateRepository(TContext context, ILogger logger) {
			return new MongoRepository<TContext, TEntity>(context, logger);
		}

		protected virtual MongoRepository<TContext, TEntity> CreateRepository(TContext context) {
			return CreateRepository(context, CreateLogger());
		}


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

		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
