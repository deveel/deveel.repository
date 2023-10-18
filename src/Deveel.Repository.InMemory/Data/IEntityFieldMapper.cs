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
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// A service that maps a field by name to an expression that
	/// selects the field from an entity.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity to map the field for.
	/// </typeparam>
	public interface IEntityFieldMapper<TEntity> where TEntity : class {
		/// <summary>
		/// Maps the given property name to an expression that
		/// selects the property from the entity.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>
		/// Returns an expression that selects the property from the entity.
		/// </returns>
		Expression<Func<TEntity, object>> Map(string propertyName);
	}
}
