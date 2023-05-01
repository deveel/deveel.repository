using System;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public class MongoDbConnectionOptions {
		public string? ConnectionString { get; set; }

		public string? DatabaseName { get; set; }

		public MongoUrl BuildUrl(string connectionString) {
			var urlBuilder = new MongoUrlBuilder(connectionString);

			if (!String.IsNullOrWhiteSpace(DatabaseName) &&
				String.IsNullOrWhiteSpace(urlBuilder.DatabaseName))
				urlBuilder.DatabaseName = DatabaseName;

			return urlBuilder.ToMongoUrl();
		}

		public MongoUrl BuildUrl() {
			if (String.IsNullOrWhiteSpace(ConnectionString))
				throw new ArgumentNullException(nameof(ConnectionString));

			return BuildUrl(ConnectionString);
		}

		public MongoDbConnection BuildConnection(string connectionString)
			=> MongoDbConnection.FromUrl(BuildUrl(connectionString));

		public MongoDbConnection BuildConnection()
			=> BuildConnection(ConnectionString!);
	}
}
