// Copyright 2023-2025 Antonello Provenzano
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
	/// <summary>
	/// An implementation of <see cref="IMongoDbConnection{TContext}"/> that
	/// is used to create a connection to a MongoDB database.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to create the connection.
	/// </typeparam>
	/// <remarks>
	/// This object wraps a <see cref="IMongoDbConnection"/> and it's used
	/// to strongly type the connection to a specific context.
	/// </remarks>
	public sealed class MongoDbConnection<TContext> : IMongoDbConnection<TContext> 
		where TContext : class, IMongoDbContext {
		private readonly IMongoDbConnection connection;

		internal MongoDbConnection(IMongoDbConnection connection) {
			this.connection = connection;
		}

		/// <inheritdoc/>
		public IMongoClient Client => connection.Client;

		/// <summary>
		/// Gets the URL of the connection to the MongoDB database.
		/// </summary>
		public MongoUrl? Url => connection.GetUrl();

		/// <inheritdoc/>
		public IDiagnosticListener DiagnosticListener {
			get => connection.DiagnosticListener;
			set => connection.DiagnosticListener = value;
		}

		/// <inheritdoc/>
		public void Dispose() {
			connection?.Dispose();
		}

		/// <inheritdoc/>
		public IMongoDatabase GetDatabase() {
			return connection.GetDatabase();
		}

		/// <summary>
		/// Creates a new connection to the MongoDB database using the given
		/// URL to the database.
		/// </summary>
		/// <param name="url">
		/// The MongoDB URL-formatted configuration string.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="MongoDbConnection{TContext}"/> that
		/// is strongly typed to the given context.
		/// </returns>
		public static MongoDbConnection<TContext> FromUrl(MongoUrl url)
			=> new MongoDbConnection<TContext>(MongoDbConnection.FromUrl(url));

		/// <summary>
		/// Creates a new connection to the MongoDB database using the given
		/// connection string to the database.
		/// </summary>
		/// <param name="connectionString">
		/// The MongoDB connection string.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="MongoDbConnection{TContext}"/> that
		/// is strongly typed to the given context.
		/// </returns>
		public static MongoDbConnection<TContext> FromConnectionString(string connectionString)
			=> new MongoDbConnection<TContext>(MongoDbConnection.FromConnectionString(connectionString));
	}
}
