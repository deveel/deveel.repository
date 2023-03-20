using System;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoFramework;

namespace Deveel.Data {
	public class MongoRepositoryProvider<TEntity, TFacade> : MongoRepositoryProvider<TEntity>, IRepositoryProvider<TFacade>
		where TFacade : class, IDataEntity 
		where TEntity : class, TFacade {
		public MongoRepositoryProvider(IOptions<MongoPerTenantConnectionOptions> options, IEnumerable<IMultiTenantStore<MongoTenantInfo>> stores, ILoggerFactory? loggerFactory = null) 
			: base(options, stores, loggerFactory) {
		}

		protected override MongoRepository<TEntity> CreateRepository(MongoDbTenantContext context, ILogger logger) {
			return new MongoRepository<TEntity, TFacade>(context, logger);
		}

		IRepository<TFacade> IRepositoryProvider<TFacade>.GetRepository(string tenantId)
			=> (MongoRepository<TEntity, TFacade>)base.GetRepository(tenantId);
	}
}
