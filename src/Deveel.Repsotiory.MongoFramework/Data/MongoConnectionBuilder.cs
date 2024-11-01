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

using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	/// <summary>
	/// An object that is used to build a <see cref="IMongoDbConnection"/>
	/// using a fluent pattern.
	/// </summary>
	public class MongoConnectionBuilder {
		private MongoUrl? mongoUrl;
		private Action<MongoClientSettings>? settings;
		private bool? useTenant;
		private Type? tenantType;

		/// <summary>
		/// Constructs the builder.
		/// </summary>
		public MongoConnectionBuilder() {
		}

		internal virtual bool IsUsingTenant => useTenant == true;

		internal virtual Type? TenantType => tenantType;

		/// <summary>
		/// Gets the connection instance built by this builder.
		/// </summary>
		public virtual IMongoDbConnection Connection {
			get {
				if (useTenant == true)
					return null;

				if (mongoUrl == null)
					throw new InvalidOperationException("No connection string or URL was specified");

				return MongoDbConnection.FromUrl(mongoUrl, settings);
			}
		}

		/// <summary>
		/// Uses the specified connection string to build the connection.
		/// </summary>
		/// <param name="connectionString">
		/// The connection string to use to build the connection.
		/// </param>
		/// <returns>
		/// Returns this builder to allow chaining.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the given <paramref name="connectionString"/> is <c>null</c>.
		/// </exception>
		public MongoConnectionBuilder UseConnection(string connectionString) {
			ArgumentNullException.ThrowIfNull(connectionString);

			mongoUrl = MongoUrl.Create(connectionString);
			return this;
		}

		/// <summary>
		/// Uses the specified encoded MongoDB URL to build the connection.
		/// </summary>
		/// <param name="url">
		/// The instance of <see cref="MongoUrl"/> to use to build the connection.
		/// </param>
		/// <returns>
		/// Returns this builder to allow chaining.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the given <paramref name="url"/> is <c>null</c>.
		/// </exception>
		public MongoConnectionBuilder UseUrl(MongoUrl url) {
			ArgumentNullException.ThrowIfNull(url);

			useTenant = false;
			mongoUrl = url;
			settings = null;
			return this;
		}

		/// <summary>
		/// Configures the settings of the connection.
		/// </summary>
		/// <param name="settings">
		/// The action to use to configure the settings.
		/// </param>
		/// <returns>
		/// Returns this builder to allow chaining.
		/// </returns>
		public MongoConnectionBuilder UseSettings(Action<MongoClientSettings> settings) {
			ArgumentNullException.ThrowIfNull(settings);

			this.settings = settings;
			useTenant = false;
			mongoUrl = null;
			return this;
		}

		public MongoConnectionBuilder UseTenant(bool useTenant = true)
			=> UseTenant<MongoDbTenantInfo>(useTenant);

		public MongoConnectionBuilder UseTenant<TTenantInfo>(bool useTenant = true)
			where TTenantInfo : MongoDbTenantInfo
		{
			this.useTenant = useTenant;
			this.tenantType = typeof(TTenantInfo);

			if (useTenant == true)
			{
				mongoUrl = null;
				settings = null;
			}

			return this;
		}
	}
}
