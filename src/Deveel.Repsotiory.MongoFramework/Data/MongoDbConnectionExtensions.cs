using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure.Diagnostics;

namespace Deveel.Data {
    public static class MongoDbConnectionExtensions {
        public static IMongoDbConnection<TContext> ForContext<TContext>(this IMongoDbConnection connection)
            where TContext : class, IMongoDbContext {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (connection is IMongoDbConnection<TContext>)
                return (IMongoDbConnection<TContext>)connection;

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
