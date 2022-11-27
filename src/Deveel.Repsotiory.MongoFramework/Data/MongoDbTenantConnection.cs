using System;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.Options;

using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Utilities;

namespace Deveel.Data {
	public class MongoDbTenantConnection : IMongoDbTenantConnection {
		private readonly string? database;
		private readonly MongoDbConnection wrapped;

		public MongoDbTenantConnection(ITenantInfo tenantInfo, IOptions<MongoDbTenantConnectionOptions> options = null) {
			Check.NotNull(tenantInfo, nameof(tenantInfo));

			TenantInfo = tenantInfo;

			if (!String.IsNullOrWhiteSpace(tenantInfo.ConnectionString) &&
				IsMongoDbConnectionString(tenantInfo.ConnectionString)) {
				wrapped = MongoDbConnection.FromConnectionString(tenantInfo.ConnectionString);
			} else if (!String.IsNullOrWhiteSpace(options?.Value?.DefaultConnectionString) &&
					IsMongoDbConnectionString(options.Value.DefaultConnectionString)) {
				wrapped = MongoDbConnection.FromConnectionString(options.Value.DefaultConnectionString);
			} else {
				throw new ArgumentException("Connection String required.");
			}

			if (tenantInfo is MongoTenantInfo mongoTenantInfo &&
				!String.IsNullOrWhiteSpace(mongoTenantInfo.DatabaseName)) {
				database = mongoTenantInfo.DatabaseName;
			} else if (!String.IsNullOrWhiteSpace(options?.Value?.DefaultDatabase)) {
				database = options.Value.DefaultDatabase;
			}
		}

		public IMongoClient Client => wrapped.Client;

		public ITenantInfo TenantInfo { get; }


		public IDiagnosticListener DiagnosticListener {
			get => wrapped.DiagnosticListener;
			set => wrapped.DiagnosticListener = value;
		}

		public IMongoDatabase GetDatabase()
			=> !String.IsNullOrWhiteSpace(database) ? Client.GetDatabase(database) : wrapped.GetDatabase();

		private static bool IsMongoDbConnectionString(string value) {
			if (string.IsNullOrEmpty(value)) {
				return false;
			}
			return value.ToLowerInvariant().StartsWith("mongodb://") || value.ToLowerInvariant().StartsWith("mongodb+srv://");
		}

		public void Dispose() {
			wrapped?.Dispose();
		}
	}
}
