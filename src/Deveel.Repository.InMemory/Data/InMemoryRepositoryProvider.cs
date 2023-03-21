using System;

namespace Deveel.Data {
	public class InMemoryRepositoryProvider<TEntity> : IRepositoryProvider<TEntity>, IDisposable
		where TEntity : class {
		public InMemoryRepositoryProvider() {
			repositories = new Dictionary<string, InMemoryRepository<TEntity>>();
		}

		public InMemoryRepositoryProvider(IDictionary<string, IList<TEntity>> list) {
			var repos = list.ToDictionary(x => x.Key, y => new InMemoryRepository<TEntity>(y.Value));
			repositories = new Dictionary<string, InMemoryRepository<TEntity>>(repos);
		}

		private readonly Dictionary<string, InMemoryRepository<TEntity>> repositories;
		private bool disposedValue;

		public InMemoryRepository<TEntity> GetRepository(string tenantId) {
			lock (repositories) {
				if (!repositories.TryGetValue(tenantId, out var repository)) {
					repositories[tenantId] = repository = new InMemoryRepository<TEntity>(tenantId);
				} 
				
				return repository;
			}
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
