using System;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Deveel.Repository {
	[Obsolete("This class is obsolete: please use the Deveel.Repository.MongoFramework instead")]
	public static class MongoDatabaseExtensions {
		public static async Task<bool> CollectionExistsAsync(this IMongoDatabase database, string collectionName, CancellationToken cancellationToken = default) {
			var options = new ListCollectionNamesOptions {
				Filter = new BsonDocument(new Dictionary<string, object> { { "name", collectionName } })
			};

			var collectionNames = await database.ListCollectionNamesAsync(options, cancellationToken);
			var list = await collectionNames.ToListAsync(cancellationToken);

			return list.Any();
		}
	}
}
