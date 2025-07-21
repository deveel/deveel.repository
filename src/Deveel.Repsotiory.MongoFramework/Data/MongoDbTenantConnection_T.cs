using Finbuckle.MultiTenant;

using Microsoft.Extensions.Options;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using MongoFramework;

namespace Deveel.Data
{
	public class MongoDbTenantConnection<TContext> : MongoDbConnection<TContext>
		where TContext : class, IMongoDbContext
	{
		public MongoDbTenantConnection(IOptions<MongoTenantConnectionOptions> options, IMultiTenantContextAccessor<MongoDbTenantInfo> multiTenantContextAccessor)
			: base(GetConnectionString(options, multiTenantContextAccessor))
		{
		}

		private static string GetConnectionString(IOptions<MongoTenantConnectionOptions> options, IMultiTenantContextAccessor<MongoDbTenantInfo> multiTenantContextAccessor)
		{
			var tenantInfo = multiTenantContextAccessor.MultiTenantContext?.TenantInfo as MongoDbTenantInfo;
			if (tenantInfo == null)
				throw new InvalidOperationException("Tenant info is not available or not of type MongoDbTenantInfo.");

			var connectionString = tenantInfo.ConnectionString;
			if (string.IsNullOrEmpty(connectionString))
			{
				connectionString = options.Value.DefaultConnectionString;
			}

			if (string.IsNullOrEmpty(connectionString))
				throw new InvalidOperationException("Connection string is not provided in the tenant info or options.");

			return connectionString;
		}
	}
}
