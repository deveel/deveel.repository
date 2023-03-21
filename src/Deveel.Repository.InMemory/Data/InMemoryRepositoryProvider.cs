using System;

namespace Deveel.Data {
	public class InMemoryRepositoryProvider<TEntity> : IRepositoryProvider<TEntity>, IDisposable
		where TEntity : class {
		public InMemoryRepositoryProvider(IEntityFieldMapper<TEntity>? fieldMapper = null) {
			repositories = new Dictionary<string, InMemoryRepository<TEntity>>();
			FieldMapper = fieldMapper;
		}

		public InMemoryRepositoryProvider(IDictionary<string, IList<TEntity>> list, IEntityFieldMapper<TEntity>? fieldMapper = null) {
			var repos = list.ToDictionary(x => x.Key, y => CreateRepository(y.Key, y.Value));
			repositories = new Dictionary<string, InMemoryRepository<TEntity>>(repos);
			FieldMapper = fieldMapper;
		}

		private readonly Dictionary<string, InMemoryRepository<TEntity>> repositories;
		private bool disposedValue;

		protected virtual IEntityFieldMapper<TEntity>? FieldMapper { get; }

		public InMemoryRepository<TEntity> GetRepository(string tenantId) {
			lock (repositories) {
				if (!repositories.TryGetValue(tenantId, out var repository)) {
					repositories[tenantId] = repository = CreateRepository(tenantId);
				} 
				
				return repository;
			}
		}

		public  virtual InMemoryRepository<TEntity> CreateRepository(string tenantId, IList<TEntity>? entities = null) {
			return new InMemoryRepository<TEntity>(tenantId, entities, FieldMapper);
		}

		IRepository<TEntity> IRepositoryProvider<TEntity>.GetRepository(string tenantId) => GetRepository(tenantId);

		IRepository IRepositoryProvider.GetRepository(string tenantId) => GetRepository(tenantId);

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					DisposeRepositories();
				}

				disposedValue = true;
			}
		}

		private void DisposeRepositories() {
			foreach (var repository in repositories.Values) {
				if (repository is IDisposable disposable)
					disposable.Dispose();
			}

			repositories.Clear();
		}

		void IDisposable.Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
