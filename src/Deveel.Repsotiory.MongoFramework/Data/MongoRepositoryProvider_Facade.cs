using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MongoFramework;

namespace Deveel.Data {
	public class MongoRepositoryProvider<TContext, TEntity, TFacade> : MongoRepositoryProvider<TContext, TEntity>,
		IRepositoryProvider<TFacade>
		where TContext : class, IMongoDbContext
		where TEntity : class, TFacade
		where TFacade : class {
		public MongoRepositoryProvider(IMongoDbConnection<TContext> connection, ISystemTime? systemTime = null, ILoggerFactory? loggerFactory = null) 
			: base(connection, systemTime, loggerFactory) {
		}

		protected override MongoRepository<TContext, TEntity> CreateRepository(TContext context) {
			var logger = LoggerFactory.CreateLogger<MongoRepository<TContext, TEntity, TFacade>>();
			return new MongoRepository<TContext, TEntity, TFacade>(context, SystemTime, logger);
		}

		async Task<IRepository<TFacade>> IRepositoryProvider<TFacade>.GetRepositoryAsync(string tenantId) 
			=> (IRepository<TFacade>)(await GetRepositoryAsync(tenantId));
	}
}
