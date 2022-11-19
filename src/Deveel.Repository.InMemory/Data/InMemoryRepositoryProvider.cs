using System;

namespace Deveel.Data {
	public class InMemoryRepositoryProvider<TEntity> : IRepositoryProvider<TEntity>
		where TEntity : class, IEntity {
		public InMemoryRepositoryProvider() {
			repositories = new Dictionary<string, InMemoryRepository<TEntity>>();
		}

		public InMemoryRepositoryProvider(IDictionary<string, IList<TEntity>> list) {
			var repos = list.ToDictionary(x => x.Key, y => new InMemoryRepository<TEntity>(y.Value));
			repositories = new Dictionary<string, InMemoryRepository<TEntity>>(repos);
		}

		private readonly Dictionary<string, InMemoryRepository<TEntity>> repositories;

		public InMemoryRepository<TEntity> GetRepository(string tenantId) {
			lock (repositories) {
				if (!repositories.TryGetValue(tenantId, out var repository)) {
					repositories[tenantId] = repository = new InMemoryRepository<TEntity>();
				} 
				
				return repository;
			}
		}

		IRepository<TEntity> IRepositoryProvider<TEntity>.GetRepository(string tenantId) => GetRepository(tenantId);

		IRepository IRepositoryProvider.GetRepository(string tenantId) => GetRepository(tenantId);
	}
}
