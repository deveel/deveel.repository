using System;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using MongoFramework;

namespace Deveel.Data {
	public class MongoRepositoryProvider<TEntity, TTenantInfo> : IRepositoryProvider<TEntity>, IDisposable 
		where TTenantInfo : class, ITenantInfo, new()
		where TEntity : class {
		private readonly IEnumerable<IMultiTenantStore<TTenantInfo>> stores;
		private readonly ILoggerFactory loggerFactory;
		private bool disposedValue;

		private IDictionary<string, MongoRepository<TEntity>>? repositories;

		public MongoRepositoryProvider(
			IEnumerable<IMultiTenantStore<TTenantInfo>> stores, ILoggerFactory? loggerFactory = null) {
			this.stores = stores;
			this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		protected virtual async Task<TTenantInfo> GetTenantInfoAsync(string tenantId) {
			foreach(var store in stores) {
				// TODO: making the IRepositoryProvider to be async
				var tenantInfo = await store.TryGetAsync(tenantId);

				if (tenantInfo == null)
					tenantInfo = store.TryGetByIdentifierAsync(tenantId)
						.ConfigureAwait(false).GetAwaiter().GetResult();

				if (tenantInfo != null) {
					return tenantInfo;
				}
			}

			throw new RepositoryException($"Unable to get a context for tenant '{tenantId}'");
		}

		protected MongoDbTenantConnection<MongoDbTenantContext, TTenantInfo> CreateConnection(TTenantInfo tenantInfo) {
			var context = new MultiTenantContext<TTenantInfo> { TenantInfo = tenantInfo };
			return new MongoDbTenantConnection<MongoDbTenantContext, TTenantInfo>(context);
		}

		protected virtual ILogger CreateLogger() {
			return loggerFactory.CreateLogger(typeof(TEntity));
		}

		async Task<IRepository<TEntity>> IRepositoryProvider<TEntity>.GetRepositoryAsync(string tenantId) => await GetRepositoryAsync(tenantId);

		async Task<IRepository> IRepositoryProvider.GetRepositoryAsync(string tenantId) => await GetRepositoryAsync(tenantId);

		public async Task<MongoRepository<TEntity>> GetRepositoryAsync(string tenantId) {
			try {
				if (repositories == null)
					repositories = new Dictionary<string, MongoRepository<TEntity>>();

				if (!repositories.TryGetValue(tenantId, out var repository)) {
					var tenantInfo = await GetTenantInfoAsync(tenantId);
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
