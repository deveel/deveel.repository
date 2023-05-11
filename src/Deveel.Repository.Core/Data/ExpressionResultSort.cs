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
