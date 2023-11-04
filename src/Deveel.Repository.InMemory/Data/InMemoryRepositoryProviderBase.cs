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

namespace Deveel.Data {
	/// <summary>
	/// A generic base class for the implementation of a repository provider
	/// for in-memory storages.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity that is managed by the repository.
	/// </typeparam>
	public abstract class InMemoryRepositoryProviderBase<TEntity> : IDisposable where TEntity : class {
		private readonly Dictionary<string, object> repositories;
		private bool disposedValue;

		internal InMemoryRepositoryProviderBase(IDictionary<string, IList<TEntity>>? list = null, IFieldMapper<TEntity>? fieldMapper = null) {
			var repos = list?.ToDictionary(x => x.Key, y => CreateRepositoryObject(y.Key, y.Value));
			if (repos == null) {
				repositories = new Dictionary<string, object>();
			} else {
				repositories = new Dictionary<string, object>(repos);
			}

			FieldMapper = fieldMapper;
		}

		/// <summary>
		/// Gets the field mapper used to map the fields of the entity
		/// </summary>
		protected virtual IFieldMapper<TEntity>? FieldMapper { get; }


		internal abstract object CreateRepositoryObject(string tenantId, IList<TEntity>? entities = null);

		internal TRepository TryGetRepository<TRepository>(string tenantId) {
			lock (repositories) {
				if (!repositories.TryGetValue(tenantId, out var repository)) {
					repositories[tenantId] = repository = CreateRepositoryObject(tenantId);
				}

				return (TRepository)repository;
			}
		}

		/// <summary>
		/// Disposes the provider and all the repositories created.
		/// </summary>
		/// <param name="disposing">
		/// A flag indicating if the provider is disposing.
		/// </param>
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

		/// <inheritdoc/>
		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
