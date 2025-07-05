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

namespace Deveel.Data {
	/// <summary>
	/// A filter that can be used to filter a <see cref="IQueryable{T}"/>
	/// using a dynamic LINQ expression.
	/// </summary>
	public sealed class DynamicLinqFilter : IExpressionQueryFilter {
		/// <summary>
		/// Constructs the filter with the given parameter name and expression
		/// </summary>
		/// <param name="paramName">
		/// The name of the parameter to be used in the expression.
		/// </param>
		/// <param name="expression">
		/// The dynamic LINQ expression string to be used
		/// as a filter.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if either <paramref name="paramName"/> or
		/// the <paramref name="expression"/> is <c>null</c>.
		/// </exception>
		public DynamicLinqFilter(string paramName, string expression) {
			ArgumentNullException.ThrowIfNull(paramName, nameof(paramName));
			ArgumentNullException.ThrowIfNull(expression, nameof(expression));

			ParameterName = paramName;
			Expression = expression;
		}

		/// <summary>
		/// Constructs the filter with the given expression
		/// </summary>
		/// <param name="expression">
		/// The dynamic LINQ expression string to be used
		/// as a filter.
		/// </param>
		/// <remarks>
		/// This constructor will use <see cref="DefaultParameterName">the default 
		/// parameter name</see> for the expression.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the <paramref name="expression"/> is <c>null</c>.
		/// </exception>
		public DynamicLinqFilter(string expression)
			: this(DefaultParameterName, expression) {
		}

		/// <summary>
		/// The default name of the parameter to be used
		/// in a filter expression.
		/// </summary>
		public const string DefaultParameterName = "x";

		/// <summary>
		/// Gets the filter expression string as
		/// a dynamic LINQ expression.
		/// </summary>
		public string Expression { get; }

		/// <summary>
		/// Gets the name of the parameter to be used
		/// in the expression.
		/// </summary>
		public string ParameterName { get; }

		/// <summary>
		/// Converts the filter into a <see cref="LambdaExpression"/>
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the object to be filtered.
		/// </typeparam>
		/// <returns>
		/// Returns a <see cref="LambdaExpression"/> that can be used
		/// to filter a <see cref="IQueryable{T}"/>.
		/// </returns>
		/// <seealso cref="FilterExpression.AsLambda{T}(string, string)"/>
		public Expression<Func<TEntity, bool>> AsLambda<TEntity>() where TEntity : class {
			return FilterExpression.AsLambda<TEntity>(ParameterName, Expression);
		}
	}
}
