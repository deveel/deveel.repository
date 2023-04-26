﻿using System.Net;
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