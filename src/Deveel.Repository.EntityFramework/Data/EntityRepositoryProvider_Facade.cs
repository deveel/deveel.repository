﻿using System;

using Finbuckle.MultiTenant;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
    public class EntityRepositoryProvider<TEntity, TFacade, TContext> : EntityRepositoryProvider<TEntity, TContext>, IRepositoryProvider<TFacade>
        where TContext : DbContext
        where TEntity : class, TFacade
        where TFacade : class {
        public EntityRepositoryProvider(DbContextOptions<TContext> options, IEnumerable<IMultiTenantStore<TenantInfo>> tenantStores, ILoggerFactory? loggerFactory = null) 
            : base(options, tenantStores, loggerFactory) {
        }

        public new EntityRepository<TEntity, TFacade> GetRepository(string tenantId) {
            return (EntityRepository<TEntity, TFacade>)base.GetRepository(tenantId);
        }

		Task<IRepository<TFacade>> IRepositoryProvider<TFacade>.GetRepositoryAsync(string tenantId) 
			=> Task.FromResult< IRepository<TFacade>>(GetRepository(tenantId));
	}
}
