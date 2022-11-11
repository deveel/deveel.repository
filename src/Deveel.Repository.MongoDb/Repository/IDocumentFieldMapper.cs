using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	public interface IDocumentFieldMapper<TDocument> where TDocument : class {
		string MapField(string fieldName);
	}
}
