using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Deveel.Repository;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace Deveel.Data {
	public class MongoRepository<TDocument> : MongoStore<TDocument>, IRepository<TDocument>, IQueryableRepository<TDocument> 
		where TDocument : class, IEntity {
		private bool disposed;

		public MongoRepository(IOptions<MongoDbStoreOptions<TDocument>> options, IDocumentFieldMapper<TDocument>? fieldMapper = null, ILogger<MongoStore<TDocument>> logger = null) : base(options, logger) {
			FieldMapper = fieldMapper;
		}

		protected IDocumentFieldMapper<TDocument>? FieldMapper { get; private set; }

		bool IRepository.SupportsPaging => true;

		bool IRepository.SupportsFilters => true;

		protected void ThrowIfDisposed() {
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);
		}

		public void Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {
			disposed = true;
		}

		public void SetMapper(IDocumentFieldMapper<TDocument> mapper) {
			FieldMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public void SetMapper(Func<string, string> mapper)
			=> SetMapper(new DelegatedDocumentFieldMapper<TDocument>(mapper));

		IQueryable<TDocument> IQueryableRepository<TDocument>.AsQueryable() => AsQueryable();

		internal IClientSessionHandle AssertMongoDbSession(IDataTransaction dataSession) {
			if (dataSession is MongoDbSession session)
				return session.SessionHandle;

			throw new ArgumentException("The session type is invalid in this context");
		}

		private TDocument AssertIsEntity(object obj) {
			if (!(obj is TDocument entity))
				throw new ArgumentException($"The object provided is not of type {typeof(TDocument)}");

			return entity;
		}


		/// <inheritdoc />
		Task<string> IRepository<TDocument>.CreateAsync(IDataTransaction session, TDocument entity, CancellationToken cancellationToken) {
			return CreateAsync(AssertMongoDbSession(session), entity, cancellationToken);
		}

		/// <inheritdoc />
		Task<string> IRepository.CreateAsync(IEntity entity, CancellationToken cancellationToken) {
			return CreateAsync(AssertIsEntity(entity), cancellationToken);
		}

		/// <inheritdoc />
		Task<string> IRepository.CreateAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken) {
			return CreateAsync(AssertMongoDbSession(session), AssertIsEntity(entity), cancellationToken);
		}


		/// <inheritdoc />
		async Task<IEntity> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken) {
			return await FindByIdAsync(id, cancellationToken);
		}


		/// <inheritdoc />
		Task<bool> IRepository<TDocument>.UpdateAsync(IDataTransaction session, TDocument entity, CancellationToken cancellationToken) {
			return UpdateAsync(AssertMongoDbSession(session), entity, cancellationToken);
		}

		/// <inheritdoc />
		Task<bool> IRepository<TDocument>.DeleteAsync(IDataTransaction session, TDocument entity, CancellationToken cancellationToken) {
			return DeleteAsync(AssertMongoDbSession(session), entity, cancellationToken);
		}


		/// <inheritdoc />
		Task<bool> IRepository.DeleteAsync(IEntity entity, CancellationToken cancellationToken) {
			return DeleteAsync(AssertIsEntity(entity), cancellationToken);
		}

		/// <inheritdoc />
		Task<bool> IRepository.DeleteAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken) {
			return DeleteAsync(AssertMongoDbSession(session), AssertIsEntity(entity), cancellationToken);
		}

		/// <inheritdoc />
		Task<bool> IRepository.UpdateAsync(IEntity entity, CancellationToken cancellationToken) {
			return UpdateAsync(AssertIsEntity(entity), cancellationToken);
		}

		/// <inheritdoc />
		Task<bool> IRepository.UpdateAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken) {
			return UpdateAsync(AssertMongoDbSession(session), AssertIsEntity(entity), cancellationToken);
		}

		/// <inheritdoc />
		async Task<PaginatedResult> IRepository.GetPageAsync(PageRequest page, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public virtual async Task<PaginatedResult<TDocument>> GetPageAsync(PageRequest<TDocument> page, CancellationToken cancellationToken) {
			var pageQuery = page.AsPageQuery<TDocument>(Field);
			var result = await GetPageAsync(pageQuery, cancellationToken);

			return new PaginatedResult<TDocument>(page, result.TotalItems, result.Items);
		}

		protected override FieldDefinition<TDocument, object> Field(string fieldName) {
			if (FieldMapper != null) {
				fieldName = FieldMapper.MapField(fieldName);
			}

			return new StringFieldDefinition<TDocument, object>(fieldName);
		}
	}
}