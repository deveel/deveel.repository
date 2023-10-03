using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure.Diagnostics;

namespace Deveel.Data {
	public sealed class MongoDbConnection<TContext> : IMongoDbConnection<TContext> 
		where TContext : class, IMongoDbContext {
		private readonly IMongoDbConnection connection;

		internal MongoDbConnection(IMongoDbConnection connection) {
			this.connection = connection;
		}

		public IMongoClient Client => connection.Client;

		public MongoUrl? Url => connection.GetUrl();

		public IDiagnosticListener DiagnosticListener {
			get => connection.DiagnosticListener;
			set => connection.DiagnosticListener = value;
		}

		public void Dispose() {
			connection?.Dispose();
		}

		public IMongoDatabase GetDatabase() {
			return connection.GetDatabase();
		}

		public static new MongoDbConnection<TContext> FromUrl(MongoUrl url)
			=> new MongoDbConnection<TContext>(MongoDbConnection.FromUrl(url));

		public static new MongoDbConnection<TContext> FromConnectionString(string connectionString)
			=> new MongoDbConnection<TContext>(MongoDbConnection.FromConnectionString(connectionString));
	}
}
