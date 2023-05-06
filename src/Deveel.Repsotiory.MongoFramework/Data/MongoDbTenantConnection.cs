using CommunityToolkit.Diagnostics;

using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	/// <summary>
	/// Defines a connection to a MongoDB database for a specific tenant
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of <see cref="IMongoDbContext"/> for which this connection is
	/// specifically defined.
	/// </typeparam>
	/// <typeparam name="TTenantInfo">
	/// The type of <see cref="ITenantInfo"/> that defines the information
	/// of the tenant's connection.
	/// </typeparam>
	public class MongoDbTenantConnection<TContext, TTenantInfo> : MongoDbConnection, IMongoDbTenantConnection<TContext>
		where TContext : class, IMongoDbContext 
		where TTenantInfo : class, ITenantInfo, new() {
		/// <summary>
		/// Constructs the connection to the MongoDB database for the tenant
		/// resolved by the given context.
		/// </summary>
		/// <param name="tenantContext">
		/// The context used to resolve the tenant information.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given <paramref name="tenantContext"/> is <c>null</c>.
		/// </exception>
		public MongoDbTenantConnection(IMultiTenantContext<TTenantInfo> tenantContext) 
			: this(tenantContext?.TenantInfo ?? throw new ArgumentNullException("Unable to resolve the tenant")) {
		}

		/// <summary>
		/// Constructs the connection to the MongoDB database for the given tenant 
		/// </summary>
		/// <param name="tenantInfo">
		/// The <see cref="ITenantInfo"/> that defines the information on the
		/// connection to the tenant's database.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the given <paramref name="tenantInfo"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if the given <paramref name="tenantInfo"/> does not define
		/// any connection string.
		/// </exception>
		protected MongoDbTenantConnection(TTenantInfo tenantInfo) {
			Guard.IsNotNull(tenantInfo);
            var connectionString = tenantInfo.ConnectionString;

            Guard.IsNotNullOrWhiteSpace(connectionString, nameof(tenantInfo.ConnectionString));

            TenantInfo = tenantInfo;
			Url = MongoUrl.Create(connectionString);
		}

		/// <summary>
		/// Gets the <see cref="ITenantInfo"/> that defines the information
		/// on the tenant's connection.
		/// </summary>
		public TTenantInfo TenantInfo { get; }

		ITenantInfo IMongoDbTenantConnection<TContext>.TenantInfo => TenantInfo;
	}
}
