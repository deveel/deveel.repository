using System;

namespace Deveel.Data {
	/// <summary>
	/// The strongly typed page from a repository, obtained from a query
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <seealso cref="RepositoryPageRequest{TEntity}"/>
	/// <seealso cref="IRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>
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

		public static new RepositoryPage<TEntity> Empty(RepositoryPageRequest page) => new RepositoryPage<TEntity>(page, 0);
	}
}