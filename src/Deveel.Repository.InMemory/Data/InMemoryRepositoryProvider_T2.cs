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
	/// An implementation of <see cref="IRepositoryProvider{TEntity}"/>
	/// that allows to create <see cref="InMemoryRepository{TEntity}"/>
	/// for a given tenant.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity to be managed by the repository.
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the key of the entity to be managed by the repository.
	/// </typeparam>
	public class InMemoryRepositoryProvider<TEntity, TKey> : InMemoryRepositoryProviderBase<TEntity>, IRepositoryProvider<TEntity, TKey>, IDisposable
		where TEntity : class 
		where TKey : notnull {
		/// <summary>
		/// Constructs the provider with the given initial list 
		/// of entities.
		/// </summary>
		/// <param name="list">
		/// The initial list of entities to use to create the repositories.
		/// </param>
		/// <param name="fieldMapper">
		/// A service to map the fields of the entity to expressions
		/// that select the fields from the entity.
		/// </param>
		public InMemoryRepositoryProvider(IDictionary<string, IList<TEntity>>? list = null, IFieldMapper<TEntity>? fieldMapper = null)
			: base(list, fieldMapper) {
		}

		/// <summary>
		/// Gets a repository for the given tenant.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant to get the repository for.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="InMemoryRepository{TEntity}"/>
		/// for the given tenant.
		/// </returns>
		public InMemoryRepository<TEntity, TKey> GetRepository(string tenantId) {
			return TryGetRepository<InMemoryRepository<TEntity, TKey>>(tenantId);
		}

		/// <summary>
		/// Creates a new repository for the given tenant.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant to create the repository for.
		/// </param>
		/// <param name="entities">
		/// A list of entities to initialize the repository with.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="InMemoryRepository{TEntity}"/>
		/// for the given tenant.
		/// </returns>
		protected virtual InMemoryRepository<TEntity, TKey> CreateRepository(string tenantId, IList<TEntity>? entities = null) {
			return InMemoryRepository<TEntity, TKey>.Create(tenantId, entities, FieldMapper);
		}

		internal override object CreateRepositoryObject(string tenantId, IList<TEntity>? entities = null)
			=> CreateRepository(tenantId, entities);

		Task<IRepository<TEntity, TKey>> IRepositoryProvider<TEntity, TKey>.GetRepositoryAsync(string tenantId, CancellationToken cancellationToken) {
			var repo = GetRepository(tenantId);
			return Task.FromResult<IRepository<TEntity, TKey>>(repo);
		}
	}
}
