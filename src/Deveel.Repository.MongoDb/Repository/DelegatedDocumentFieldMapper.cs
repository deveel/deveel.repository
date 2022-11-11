using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	public sealed class DelegatedDocumentFieldMapper<TDocument> : IDocumentFieldMapper<TDocument> where TDocument : class {
		private readonly Func<string, string> mapper;

		public DelegatedDocumentFieldMapper(Func<string, string> mapper) {
			this.mapper = mapper;
		}

		public string MapField(string fieldName) => mapper(fieldName);
	}
}
