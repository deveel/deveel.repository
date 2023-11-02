﻿// Copyright 2023 Deveel AS
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

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of <see cref="IRepository{TEntity}"/> contract
	/// that uses the MongoDB system to store and retrieve data.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity that is stored in the repository.
	/// </typeparam>
	public class MongoRepository<TEntity> : MongoRepository<TEntity, object>,
		IRepository<TEntity>, 
		IQueryableRepository<TEntity>, 
		IPageableRepository<TEntity>, 
		IFilterableRepository<TEntity>,
		IMultiTenantRepository<TEntity>
		where TEntity : class {
		/// <summary>
		/// Constructs the repository with the given context and logger.
		/// </summary>
		/// <param name="context">
		/// The context that is used to handle the connection to the MongoDB server.
		/// </param>
		/// <param name="logger">
		/// A logger instance that is used to log messages from the repository.
		/// </param>
		protected internal MongoRepository(IMongoDbContext context, ILogger? logger = null) : base(context, logger) {
		}

		/// <summary>
		/// Constructs the repository with the given context and logger.
		/// </summary>
		/// <param name="context">
		/// The context that is used to handle the connection to the MongoDB server.
		/// </param>
		/// <param name="logger">
		/// A logger instance that is used to log messages from the repository.
		/// </param>
		public MongoRepository(IMongoDbContext context, ILogger<MongoRepository<TEntity>>? logger = null)
			: base(context, logger) {
		}

		string? IMultiTenantRepository<TEntity, object>.TenantId => TenantId;

		IQueryable<TEntity> IQueryableRepository<TEntity, object>.AsQueryable() => DbSet.AsQueryable();

		object? IRepository<TEntity, object>.GetEntityKey(TEntity entity) => GetEntityKey(entity);
	}
}
