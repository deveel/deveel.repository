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
	/// Extends the <see cref="ITransactionalRepository{TEntity}"/> interface
	/// to provide more methods to handle entities in a transactional way.
	/// </summary>
	public static class TransactionalRepositoryExtensions {
		/// <summary>
		/// Adds a new entity in the repository synchronously
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to create
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to create the entity
		/// </param>
		/// <param name="transaction">
		/// A transaction to use to create the entity
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to create
		/// </param>
		public static void Add<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
			where TEntity : class
			=> repository.AddAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Removes an entity from the repository synchronously,
		/// using a transaction isolation given
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entity is removed.
		/// </param>
		/// <param name="transaction">
		/// The transaction state that isolates the operation.
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to remove.
		/// </param>
		public static void Remove<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
			where TEntity : class
			=> repository.RemoveAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Removes an entity, identified by the given key,
		/// from the repository, using a transaction isolation
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entity is removed.
		/// </param>
		/// <param name="transaction">
		/// A transaction state that isolates the operation.
		/// </param>
		/// <param name="id">
		/// The string that uniquely identifies the entity to remove.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a task that can be used to await the operation.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the entity with the given key is not found in the repository.
		/// </exception>
		public static async Task RemoveByIdAsync<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, string id, CancellationToken cancellationToken = default)
			where TEntity : class {
			// TODO: find within a transaction ...
			var entity = await repository.FindByIdAsync(id, cancellationToken);
			if (entity == null)
				throw new InvalidOperationException($"The entity with id '{id}' was not found in the repository");

			await repository.RemoveAsync(transaction, entity, cancellationToken);
		}

		/// <summary>
		/// Synchronously removes an entity, identified by the given key,
		/// from the repository, using a transaction isolation
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entity is removed.
		/// </param>
		/// <param name="transaction">
		/// A transaction state that isolates the operation.
		/// </param>
		/// <param name="id">
		/// The string that uniquely identifies the entity to remove.
		/// </param>
		/// <seealso cref="ITransactionalRepository{TEntity}"/>
		/// <seealso cref="ITransactionalRepository{TEntity}.RemoveAsync(IDataTransaction, TEntity, CancellationToken)"/>
		public static void RemoveById<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, string id)
			where TEntity : class
			=> repository.RemoveByIdAsync(transaction, id).ConfigureAwait(false).GetAwaiter().GetResult();
	}
}
