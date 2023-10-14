// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Deveel.Data {
	public class InMemoryRepositoryProvider<TEntity> : IRepositoryProvider<TEntity>, IDisposable
		where TEntity : class {

		public InMemoryRepositoryProvider(IDictionary<string, IList<TEntity>>? list = null, IEntityFieldMapper<TEntity>? fieldMapper = null) {
			var repos = list?.ToDictionary(x => x.Key, y => CreateRepository(y.Key, y.Value));
			if (repos == null) {
				repositories = new Dictionary<string, InMemoryRepository<TEntity>>();
			} else {
				repositories = new Dictionary<string, InMemoryRepository<TEntity>>(repos);
			}

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

		public virtual InMemoryRepository<TEntity> CreateRepository(string tenantId, IList<TEntity>? entities = null) {
			return InMemoryRepository<TEntity>.Create(tenantId, entities, FieldMapper);
		}

		Task<IRepository<TEntity>> IRepositoryProvider<TEntity>.GetRepositoryAsync(string tenantId, CancellationToken cancellationToken) {
			var repo = GetRepository(tenantId);
			return Task.FromResult<IRepository<TEntity>>(repo);
		}

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
