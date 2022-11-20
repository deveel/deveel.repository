using System;

namespace Deveel.Data {
	/// <summary>
	/// The strongly typed page from a repository, obtained from a query
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <seealso cref="RepositoryPageRequest{TEntity}"/>
	/// <seealso cref="IRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>
	public class RepositoryPage<TEntity> : RepositoryPage where TEntity : class, IEntity {
		/// <inheritdoc/>
		public RepositoryPage(RepositoryPageRequest<TEntity> request, int totalItems, IEnumerable<TEntity>? items = null)
			: base(request, totalItems, items) {
		}

		public new RepositoryPageRequest<TEntity> Request {
			get => (RepositoryPageRequest<TEntity>)base.Request;
		}

		/// <inheritdoc/>
		public new IEnumerable<TEntity>? Items {
			get => base.Items?.Cast<TEntity>();
			set => base.Items = value;
		}

		public RepositoryPage<TOther> As<TOther>() where TOther : class, IEntity {
			var request = Request.As<TOther>();
			var items = Items?.Cast<TOther>();

			return new RepositoryPage<TOther>(request, TotalItems, items);
		}

		/// <summary>
		/// Creates an empty page result 
		/// </summary>
		/// <param name="request">The original page request</param>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPage"/> that
		/// contains no results and no pages.
		/// </returns>
		public static RepositoryPage<TEntity> Empty(RepositoryPageRequest<TEntity> request)
			=> new RepositoryPage<TEntity>(request, 0);
	}
}