using Finbuckle.MultiTenant;

using System;

namespace Deveel.Data
{
	/// <summary>
	/// An implementation of <see cref="TenantInfo"/> that
	/// provides the connection string for a MongoDB tenant.
	/// </summary>
	public class MongoDbTenantInfo : TenantInfo
	{
#if NET7_0_OR_GREATER
		/// <summary>
		/// Gets or sets the connection string for the MongoDB tenant.
		/// </summary>
		public string? ConnectionString { get; set;}
#endif
	}
}
