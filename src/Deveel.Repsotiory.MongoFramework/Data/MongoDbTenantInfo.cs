using Finbuckle.MultiTenant;

using System;

namespace Deveel.Data
{
	public class MongoDbTenantInfo : TenantInfo
	{
#if NET7_0_OR_GREATER
		public string? ConnectionString { get; set;}
#endif
	}
}
