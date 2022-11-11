﻿using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// References a expr of an entity through a selection expression
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity defining the expr to be selected</typeparam>
	public sealed class ExpressionFieldRef<TEntity> : IFieldRef where TEntity : class, IEntity {
		/// <summary>
		/// Constucts the reference with the expression to select
		/// the expr from the entity
		/// </summary>
		/// <param name="expr">The expression that is used to select the expr</param>
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
