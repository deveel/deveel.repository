using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// Provides a default implementation of the result sort
	/// </summary>
	/// <seealso cref="IResultSort"/>
	public class ResultSort : IResultSort {
		public ResultSort(IFieldRef field, bool ascending = false) {
			Field = field ?? throw new ArgumentNullException(nameof(field));
			Ascending = ascending;
		}

		/// <inheritdoc/>
		public IFieldRef Field { get; }

		/// <inheritdoc/>
		public bool Ascending { get; }

		public static ResultSort Create(string fieldName, bool ascending = false)
			=> new ResultSort(new StringFieldRef(fieldName), ascending);

		public static ResultSort Create<TEntity>(Expression<Func<TEntity, object>> fieldSelector, bool ascending = false)
			where TEntity : class, IEntity
			=> new ResultSort(new ExpressionFieldRef<TEntity>(fieldSelector), ascending);
	}
}
