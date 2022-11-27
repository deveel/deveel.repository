using System;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using MongoFramework;

namespace Deveel.Data {
	public class MongoRepositoryProvider<TEntity> : IRepositoryProvider<TEntity> where TEntity : class, IEntity {
		private readonly MongoDbTenantConnectionOptions options;
		private readonly IEnumerable<IMultiTenantStore<MongoTenantInfo>> stores;
		private readonly ILoggerFactory loggerFactory;

		public MongoRepositoryProvider(IOptions<MongoDbTenantConnectionOptions> options, IEnumerable<IMultiTenantStore<MongoTenantInfo>> stores, ILoggerFactory? loggerFactory = null) {
			this.stores = stores;
			this.options = options.Value;
			this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		protected virtual MongoTenantInfo GetTenantInfo(string tenantId) {
			foreach(var store in stores) {
				// TODO: making the IRepositoryProvider to be async
				var tenantInfo = store.TryGetAsync(tenantId)
					.ConfigureAwait(false).GetAwaiter().GetResult();

				if (tenantInfo != null) {
					return tenantInfo;
				}
			}

			throw new RepositoryException($"Unable to get a context for tenant '{tenantId}'");
		}

		protected IMongoDbTenantConnection CreateConnection(MongoTenantInfo tenantInfo) {
			return new MongoDbTenantConnection(tenantInfo, Options.Create(options));
		}

		protected virtual ILogger CreateLogger() {
			return loggerFactory.CreateLogger(typeof(TEntity));
		}

		IRepository<TEntity> IRepositoryProvider<TEntity>.GetRepository(string tenantId) => GetRepository(tenantId);

		IRepository IRepositoryProvider.GetRepository(string tenantId) => GetRepository(tenantId);

		public virtual MongoRepository<TEntity> GetRepository(string tenantId) {
			var tenantInfo = GetTenantInfo(tenantId);
			var connection = CreateConnection(tenantInfo);

			var logger = CreateLogger();
			var context = new MongoDbTenantContext(connection, tenantInfo.Id);

			return CreateRepository(context, logger);
		}

		protected virtual MongoRepository<TEntity> CreateRepository(MongoDbTenantContext context, ILogger logger) {
			return new MongoRepository<TEntity>(context, logger);
		}
	}
}
