using System;

using Microsoft.Extensions.Options;

using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure.Diagnostics;

namespace Deveel.Data {
	class MongoDbConfiguredConnection : IMongoDbConnection {
		private readonly IMongoDbConnection wrapped;
		private readonly string databaseName;

		public MongoDbConfiguredConnection(IOptions<MongoDbConnectionOptions> options) {
			var connectionString = options.Value.ConnectionString;
			databaseName = options.Value.DatabaseName;

			wrapped = MongoDbConnection.FromConnectionString(connectionString);
		}

		public IMongoClient Client => wrapped.Client;

		public IDiagnosticListener DiagnosticListener {
			get => wrapped.DiagnosticListener;
			set => wrapped.DiagnosticListener = value;
		}

		public void Dispose() {
			wrapped?.Dispose();
		}

		public IMongoDatabase GetDatabase() => !String.IsNullOrWhiteSpace(databaseName) 
			? Client.GetDatabase(databaseName)
			: wrapped.GetDatabase();
	}
}
