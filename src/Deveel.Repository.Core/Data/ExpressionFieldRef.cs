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

using System.Net;
using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// References a field of an entity through a selection expression
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity defining the field to be selected</typeparam>
	public sealed record class ExpressionFieldRef<TEntity> : IFieldRef where TEntity : class {
		/// <summary>
		/// Constucts the reference with the expression to select
		/// the field from the entity
		/// </summary>
		/// <param name="expr">The expression that is used to select the field</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the expression is empty
		/// </exception>
		public ExpressionFieldRef(Expression<Func<TEntity, object>> expr) {
			Expression = expr ?? throw new ArgumentNullException(nameof(expr));
		}

		/// <summary>
		/// Gets the expression used to select a field from the
		/// underlying entity
		/// </summary>
		public Expression<Func<TEntity, object>> Expression { get; }
	}
}