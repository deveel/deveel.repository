// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

            return new MongoDbConnection<TContext>(connection);
        }

        //private class MongoDbConnectionWrapper<TContext> : IMongoDbConnection<TContext> where TContext : class, IMongoDbContext {
        //    private readonly IMongoDbConnection _connection;

        //    public MongoDbConnectionWrapper(IMongoDbConnection connection) {
        //        _connection = connection;
        //    }

        //    public IMongoClient Client => _connection.Client;

        //    public IDiagnosticListener DiagnosticListener {
        //        get => _connection.DiagnosticListener;
        //        set => _connection.DiagnosticListener = value;
        //    }

        //    public void Dispose() {
        //        _connection?.Dispose();
        //    }

        //    public IMongoDatabase GetDatabase() {
        //        return _connection.GetDatabase();
        //    }
        //}
    }
}
