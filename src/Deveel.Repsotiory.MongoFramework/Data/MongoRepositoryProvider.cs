using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MongoFramework;

namespace Deveel.Data {
    public class MongoRepositoryProvider<TContext, TEntity, TTenantInfo> : IRepositoryProvider<TEntity>, IDisposable 
		where TContext : class, IMongoDbContext
		where TTenantInfo : class, ITenantInfo, new()
		where TEntity : class {
		private readonly IEnumerable<IMultiTenantStore<TTenantInfo>>? stores;
		private bool disposedValue;

		private IDictionary<string, MongoRepository<TEntity>>? repositories;

		public MongoRepositoryProvider(
			IEnumerable<IMultiTenantStore<TTenantInfo>>? stores = null,
			ISystemTime? systemTime = null,
			ILoggerFactory? loggerFactory = null) {
			this.stores = stores;
			SystemTime = systemTime ?? Deveel.Data.SystemTime.Default;
			LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		protected ILoggerFactory LoggerFactory { get; }

		protected ISystemTime SystemTime { get; }

		protected virtual async Task<TTenantInfo?> GetTenantInfoAsync(string tenantId, CancellationToken cancellationToken) {
			if (stores == null)
				return null;

			foreach(var store in stores) {
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

		//protected MongoDbTenantConnection<TContext, TTenantInfo> CreateConnection(TTenantInfo tenantInfo) {
		//	var context = new MultiTenantContext<TTenantInfo> { TenantInfo = tenantInfo };
		//	return new MongoDbTenantConnection<TContext, TTenantInfo>(context);
		//}

		protected virtual ILogger CreateLogger() {
			return LoggerFactory.CreateLogger(typeof(MongoRepository<TEntity>));
		}

		protected virtual TContext? CreateContext(IMongoDbConnection connection, TTenantInfo tenantInfo) {
			if (tenantInfo == null)
				throw new RepositoryException("Unable to resolve the tenant");

			if (typeof(TContext) == typeof(MongoDbTenantContext))
				return (new MongoDbTenantContext(connection, tenantInfo.Id)) as TContext;
			if (typeof(TContext) == typeof(MongoDbContext))
				return new MongoDbContext(connection) as TContext;

			var ctor1 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbConnection<TContext>), typeof(IMultiTenantContext<TTenantInfo>) });
			if (ctor1 != null)
				return Activator.CreateInstance(typeof(TContext), new object[] { connection.ForContext<TContext>(), CreateTenantContext(tenantInfo) }) as TContext;

			var ctor2 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbConnection<TContext>), typeof(string) });
			if (ctor2 != null)
				return Activator.CreateInstance(typeof(TContext), new object[] { connection.ForContext<TContext>(), tenantInfo.Id }) as TContext;

			throw new NotSupportedException($"Cannot create '{typeof(TContext)}' MongoDB Context");
        }

		protected virtual IMultiTenantContext<TTenantInfo> CreateTenantContext(TTenantInfo tenantInfo) {
			return new MultiTenantContext<TTenantInfo> {
				TenantInfo = tenantInfo
			};
		}

		async Task<IRepository<TEntity>> IRepositoryProvider<TEntity>.GetRepositoryAsync(string tenantId, CancellationToken cancellationToken) 
			=> await GetRepositoryAsync(tenantId, cancellationToken);

		public async Task<MongoRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default) {
			try {
				if (repositories == null)
					repositories = new Dictionary<string, MongoRepository<TEntity>>();

				if (!repositories.TryGetValue(tenantId, out var repository)) {
					var tenantInfo = await GetTenantInfoAsync(tenantId, cancellationToken);

					if (tenantInfo == null)
						throw new RepositoryException($"Unable to find any tenant for the ID '{tenantId}' - cannot construct a context");

					var connection = MongoDbConnection.FromConnectionString(tenantInfo.ConnectionString);

					// var tenantContext = CreateTenantContext(tenantInfo);
					var context = CreateContext(connection, tenantInfo);

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

		protected virtual MongoRepository<TEntity> CreateRepository(TContext context, ILogger logger) {
			return new MongoRepository<TEntity>(context, SystemTime, logger);
		}

        protected virtual MongoRepository<TEntity> CreateRepository(TContext context) {
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
