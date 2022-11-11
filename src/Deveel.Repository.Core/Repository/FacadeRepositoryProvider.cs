using System;

using Deveel.Data;

namespace Deveel.Repository {
	class FacadeRepositoryProvider<TEntity, TFacade> : IRepositoryProvider<TFacade>
		where TEntity : class, TFacade, IEntity
		where TFacade : class, IEntity {
		private readonly IRepositoryProvider<TEntity> provider;

		public FacadeRepositoryProvider(IRepositoryProvider<TEntity> provider) {
			this.provider = provider;
		}

		public IRepository<TFacade> GetRepository(string tenantId) => (IRepository<TFacade>) provider.GetRepository(tenantId);

		IRepository IRepositoryProvider.GetRepository(string tenantId) => provider.GetRepository(tenantId);
	}
}
