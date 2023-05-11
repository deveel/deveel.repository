using System;
using System.Linq.Expressions;

namespace Deveel.Data {
    /// <summary>
    /// Provides factory methods to create instances of sorting rules.
    /// </summary>
    /// <seealso cref="IResultSort"/>
    public static class ResultSort {
		/// <summary>
		/// Creates a new sorting rule for the given field
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field to sort the results
		/// </param>
		/// <param name="ascending">
		/// The flag indicating whether the results should be
		/// </param>
		/// <returns>
		/// Returns a new instance of <see cref="IResultSort"/> that
		/// sorts by the <paramref name="fieldName"/> given.
		/// </returns>
        public static IResultSort Create(string fieldName, bool ascending = false)
            => new FieldResultSort(fieldName, ascending);

		/// <summary>
		/// Creates a new sorting rule for the given field
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity that defines the field
		/// to be used to sort the results.
		/// </typeparam>
		/// <param name="fieldSelector"></param>
		/// <param name="ascending"></param>
		/// <returns></returns>
        public static IResultSort Create<TEntity>(Expression<Func<TEntity, object>> fieldSelector, bool ascending = false)
            where TEntity : class
            => new ExpressionResultSort<TEntity>(fieldSelector, ascending);
    }
}