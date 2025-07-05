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

using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure.Diagnostics;

namespace Deveel.Data
{
	public class MongoDbTenantConnection : IMongoDbTenantConnection
	{
		private readonly MongoUrl _mongoUrl;
		private IMongoClient? _client;
		private bool disposed;

#if NET7_0_OR_GREATER
		public MongoDbTenantConnection(TenantInfo tenantInfo)
#else
		public MongoDbTenantConnection(ITenantInfo tenantInfo)
#endif
		{
			ArgumentNullException.ThrowIfNull(tenantInfo, nameof(tenantInfo));

			if (!(tenantInfo is MongoDbTenantInfo mongoTenantInfo))
				throw new ArgumentException("The tenant info must be of type MongoDbTenantInfo", nameof(tenantInfo));

			TenantInfo = mongoTenantInfo;

			_mongoUrl = new MongoUrl(mongoTenantInfo.ConnectionString);
		}

		/// <inheritdoc/>
		public MongoDbTenantInfo TenantInfo { get; }

		/// <inheritdoc/>
		public IMongoClient Client
		{
			get
			{
				ThrowIfDisposed();
				if (_client == null)
					_client = new MongoClient(_mongoUrl);

				return _client;
			}
		}

		/// <inheritdoc/>
		public IDiagnosticListener DiagnosticListener { get; set; }

		private void ThrowIfDisposed()
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(MongoDbTenantConnection));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_client = null;
			disposed = true;
		}

		/// <inheritdoc/>
		public IMongoDatabase GetDatabase()
		{
			ThrowIfDisposed();

			return Client.GetDatabase(_mongoUrl.DatabaseName);
		}
	}
}
