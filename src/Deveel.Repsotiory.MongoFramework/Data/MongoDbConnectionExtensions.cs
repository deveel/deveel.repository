using System.Reflection;

using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure.Diagnostics;

namespace Deveel.Data {
    public static class MongoDbConnectionExtensions {
		public static MongoUrl? GetUrl(this IMongoDbConnection connection) {
			var connectionType = connection.GetType();
			var urlProperty = connectionType.GetProperty("Url", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			return urlProperty == null ? null : (MongoUrl?)urlProperty.GetValue(connection);
		}

		public static IMongoDbConnection<TContext> ForContext<TContext>(this IMongoDbConnection connection)
            where TContext : class, IMongoDbContext {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (connection is IMongoDbConnection<TContext> mongoConnection)
                return mongoConnection;

            return new MongoDbConnectionWrapper<TContext>(connection);
        }

        private class MongoDbConnectionWrapper<TContext> : IMongoDbConnection<TContext> where TContext : class, IMongoDbContext {
            private readonly IMongoDbConnection _connection;

            public MongoDbConnectionWrapper(IMongoDbConnection connection) {
                _connection = connection;
            }

            public IMongoClient Client => _connection.Client;

            public IDiagnosticListener DiagnosticListener {
                get => _connection.DiagnosticListener;
                set => _connection.DiagnosticListener = value;
            }

            public void Dispose() {
                _connection?.Dispose();
            }

            public IMongoDatabase GetDatabase() {
                return _connection.GetDatabase();
            }
        }
    }
}
