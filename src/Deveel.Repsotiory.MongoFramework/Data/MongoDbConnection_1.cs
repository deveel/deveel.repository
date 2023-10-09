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

		public static MongoDbConnection<TContext> FromUrl(MongoUrl url)
			=> new MongoDbConnection<TContext>(MongoDbConnection.FromUrl(url));

		public static MongoDbConnection<TContext> FromConnectionString(string connectionString)
			=> new MongoDbConnection<TContext>(MongoDbConnection.FromConnectionString(connectionString));
	}
}
