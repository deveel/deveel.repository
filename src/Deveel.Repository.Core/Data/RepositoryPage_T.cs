using System;

namespace Deveel.Data {
	/// <summary>
	/// The strongly typed page from a repository, obtained from a query
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <seealso cref="RepositoryPageRequest{TEntity}"/>
	/// <seealso cref="IPageableRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>
	public class RepositoryPage<TEntity> : RepositoryPage where TEntity : class {
		/// <inheritdoc/>
		public RepositoryPage(RepositoryPageRequest request, int totalItems, IEnumerable<TEntity>? items = null)
			: base(request, totalItems, items) {
		}

		/// <inheritdoc/>
		public new IEnumerable<TEntity>? Items {
			get => base.Items?.Cast<TEntity>();
			set => base.Items = value;
		}

		/// <summary>
		/// Creates an empty page response to the given request
		/// </summary>
		/// <param name="page">
		/// The request that originated the page
		/// </param>
		/// <returns>
		/// Returns a new instance of <see cref="RepositoryPage{TEntity}"/> that
		/// represents an empty page.
		/// </returns>
		public static new RepositoryPage<TEntity> Empty(RepositoryPageRequest page) => new RepositoryPage<TEntity>(page, 0);
	}
}