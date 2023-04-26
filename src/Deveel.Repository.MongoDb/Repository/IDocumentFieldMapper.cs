using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	[Obsolete("This class is obsolete: please use the Deveel.Repository.MongoFramework instead")]
	public interface IDocumentFieldMapper<TDocument> where TDocument : class {
		string MapField(string fieldName);
	}
}
