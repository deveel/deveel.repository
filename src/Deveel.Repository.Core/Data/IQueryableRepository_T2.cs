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

using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a repository that is capable of being queried
	/// </summary>
	/// <typeparam name="TEntity">
	/// The strongly typed entity that is stored in the repository
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the key used to uniquely identify the entity.
	/// </typeparam>
	public interface IQueryableRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class {
		/// <summary>
		/// Gets a queryable object that can be used to query the repository
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="IQueryable{T}"/> that can be used
		/// to query the repository.
		/// </returns>
		IQueryable<TEntity> AsQueryable();
	}
}