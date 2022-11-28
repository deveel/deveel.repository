using System;

using Finbuckle.MultiTenant;

namespace Deveel.Data {
	public class MongoTenantInfo : TenantInfo {
		public string ConnectionString { get; set; }

		public string DatabaseName { get; set; }
	}
}
