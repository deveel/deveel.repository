﻿// Copyright 2023-2025 Antonello Provenzano
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
	/// A repository that uses the memory of the process to store
	/// the entities.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity managed by the repository.
	/// </typeparam>
	public class InMemoryRepository<TEntity> : 
		InMemoryRepository<TEntity, string>,
		IRepository<TEntity>, 
		IQueryableRepository<TEntity>, 
		IPageableRepository<TEntity>, 
		IFilterableRepository<TEntity>,
		IDisposable
		where TEntity : class {
		/// <summary>
		/// Constructs the repository with the given list of
		/// initial entities.
		/// </summary>
		/// <param name="list">
		/// The list of entities to initialize the repository with.
		/// </param>
		/// <param name="fieldMapper">
		/// A service that maps a field by name to an expression that
		/// can select the field from an entity.
		/// </param>
		public InMemoryRepository(
			IEnumerable<TEntity>? list = null,
			IFieldMapper<TEntity>? fieldMapper = null) : base(list, fieldMapper) {
		}

		/// <summary>
		/// Destroys the instance of the repository.
		/// </summary>
		~InMemoryRepository() {
			Dispose(disposing: false);
		}

		IQueryable<TEntity> IQueryableRepository<TEntity, object>.AsQueryable() => Entities.AsQueryable();

		object? IRepository<TEntity, object>.GetEntityKey(TEntity entity) {
			return GetEntityKey(entity);
		}

		Task<TEntity?> IRepository<TEntity, object>.FindAsync(object key, CancellationToken cancellationToken) {
			return FindAsync(NormalizeKey(key), cancellationToken);
		}

		private string NormalizeKey(object key) {
			ArgumentNullException.ThrowIfNull(key, nameof(key));

			if (key is string s)
				return s;
			if (key is Guid guid)
				return guid.ToString("N");

			throw new RepositoryException($"The key '{key}' is not supported");
		}
	}
}
