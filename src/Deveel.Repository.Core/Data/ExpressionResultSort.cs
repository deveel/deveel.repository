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

using System.Linq.Expressions;

using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
	/// <summary>
	/// Describes a sorting rule that uses an expression to 
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity that is the target of the sorting and that
	/// defines the field to select.
	/// </typeparam>
	public sealed class ExpressionResultSort<TEntity> : IResultSort where TEntity : class {
		/// <summary>
		/// Constructs the sorting rule using the given expression
		/// to select the field to sort.
		/// </summary>
		/// <param name="fieldSelector">
		/// The expression that selects the field to sort.
		/// </param>
		/// <param name="ascending">
		/// Whether the sorting is ascending or descending.
		/// </param>
		public ExpressionResultSort(Expression<Func<TEntity, object>> fieldSelector, bool ascending = true) {
			Guard.IsNotNull(fieldSelector, nameof(FieldSelector));

			FieldSelector = fieldSelector;
			Ascending = ascending;
		}

		/// <summary>
		/// Gets the expression that selects the field to sort.
		/// </summary>
		public Expression<Func<TEntity, object>> FieldSelector { get; }

		IFieldRef IResultSort.Field => new ExpressionFieldRef<TEntity>(FieldSelector);

		/// <summary>
		/// Gets a flag indicating whether the result should
		/// be sorted ascending or descending.
		/// </summary>
		public bool Ascending { get; }
	}
}
