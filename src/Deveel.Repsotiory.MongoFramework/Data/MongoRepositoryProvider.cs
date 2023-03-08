using System;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using MongoFramework;

namespace Deveel.Data {
	public class MongoRepositoryProvider<TEntity> : IRepositoryProvider<TEntity>, IDisposable where TEntity : class, IEntity {
		private readonly MongoPerTenantConnectionOptions options;
		private readonly IEnumerable<IMultiTenantStore<MongoTenantInfo>> stores;
		private readonly ILoggerFactory loggerFactory;
		private bool disposedValue;

		private IDictionary<string, MongoRepository<TEntity>>? repositories;

		public MongoRepositoryProvider(IOptions<MongoPerTenantConnectionOptions> options, 
			IEnumerable<IMultiTenantStore<MongoTenantInfo>> stores, ILoggerFactory? loggerFactory = null) {
			this.stores = stores;
			this.options = options.Value;
			this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		protected virtual MongoTenantInfo GetTenantInfo(string tenantId) {
			foreach(var store in stores) {
				// TODO: making the IRepositoryProvider to be async
				var tenantInfo = store.TryGetAsync(tenantId)
					.ConfigureAwait(false).GetAwaiter().GetResult();

				if (tenantInfo == null)
					tenantInfo = store.TryGetByIdentifierAsync(tenantId)
						.ConfigureAwait(false).GetAwaiter().GetResult();

				if (tenantInfo != null) {
					return tenantInfo;
				}
			}

			throw new RepositoryException($"Unable to get a context for tenant '{tenantId}'");
		}

		protected IMongoPerTenantConnection CreateConnection(MongoTenantInfo tenantInfo) {
			return new MongoPerTenantConnection(tenantInfo, Options.Create(options));
		}

		protected virtual ILogger CreateLogger() {
			return loggerFactory.CreateLogger(typeof(TEntity));
		}

		IRepository<TEntity> IRepositoryProvider<TEntity>.GetRepository(string tenantId) => GetRepository(tenantId);

		IRepository IRepositoryProvider.GetRepository(string tenantId) => GetRepository(tenantId);

		public MongoRepository<TEntity> GetRepository(string tenantId) {
			try {
				if (repositories == null)
					repositories = new Dictionary<string, MongoRepository<TEntity>>();

				if (!repositories.TryGetValue(tenantId, out var repository)) {
					var tenantInfo = GetTenantInfo(tenantId);
					var connection = CreateConnection(tenantInfo);

					var logger = CreateLogger();
					var context = new MongoDbTenantContext(connection, tenantInfo.Id);

					repository = CreateRepository(context, logger);

					repositories[tenantId] = repository;
				}

				return repository;
			} catch (Exception) {
				// TODO: specialize the exception
				throw;
			}
		}

		protected virtual MongoRepository<TEntity> CreateRepository(MongoDbTenantContext context, ILogger logger) {
			return new MongoRepository<TEntity>(context, logger);
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
