using System;

namespace Deveel.Repository {
	/// <summary>
	/// The strongly typed page from a repository, obtained from a query
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <seealso cref="PageRequest{TEntity}"/>
	/// <seealso cref="IRepository{TEntity}.GetPageAsync(PageRequest{TEntity}, CancellationToken)"/>
	public class PaginatedResult<TEntity> : PaginatedResult where TEntity : class, IEntity {
		/// <inheritdoc/>
		public PaginatedResult(PageRequest<TEntity> request, int totalItems, IEnumerable<TEntity>? items = null) 
			: base(request, totalItems, items) {
		}

		public new PageRequest<TEntity> Request {
			get => (PageRequest<TEntity>)base.Request;
		}

		/// <inheritdoc/>
		public new IEnumerable<TEntity>? Items { 
			get => base.Items?.Cast<TEntity>(); 
			set => base.Items = value;
		}

		public PaginatedResult<TOther> CastTo<TOther>() where TOther : class, IEntity {
			var request = Request.Cast<TOther>();
			var items = Items?.Cast<TOther>();

			return new PaginatedResult<TOther>(request, TotalItems, items);
		}

		/// <summary>
		/// Creates an empty page result 
		/// </summary>
		/// <param name="request">The original page request</param>
		/// <returns>
		/// Returns an instance of <see cref="PaginatedResult"/> that
		/// contains no results and no pages.
		/// </returns>
		public static PaginatedResult<TEntity> Empty(PageRequest<TEntity> request)
			=> new PaginatedResult<TEntity>(request, 0);
	}
}
