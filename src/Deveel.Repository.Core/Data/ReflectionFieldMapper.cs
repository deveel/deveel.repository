// Copyright 2023-2025 Antonello Provenzano
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

using System.Linq.Expressions;
using System.Reflection;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of <see cref="IFieldMapper{TEntity}"/> that
	/// uses reflection to map a field name to a member of the entity.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public sealed class ReflectionFieldMapper<TEntity> : IFieldMapper<TEntity> {
		private Expression<Func<TEntity, object?>>? cachedExpr;

		/// <inheritdoc/>
		public Expression<Func<TEntity, object?>> MapField(string fieldName) {
			ArgumentNullException.ThrowIfNull(fieldName, nameof(fieldName));

			if (cachedExpr == null) {

				// TODO: support dot-delimited field names to access nested properties
				var member = typeof(TEntity).GetMembers(BindingFlags.Public | BindingFlags.Instance)
					.Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
					.FirstOrDefault(x => String.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));

				if (member == null)
					throw new InvalidOperationException($"The field '{fieldName}' is not a valid member of the entity '{typeof(TEntity).Name}'");

				// TODO: find a way to identify a variable name
				//       that doesn't conflict with other variables
				var param = Expression.Parameter(typeof(TEntity), "x");
				var expr = Expression.MakeMemberAccess(param, member);

				cachedExpr = Expression.Lambda<Func<TEntity, object?>>(expr, param);
			}

			return cachedExpr;
		}
	}
}
