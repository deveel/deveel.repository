// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations.Schema;

using Finbuckle.MultiTenant;

using MongoFramework;

namespace Deveel.Data {
	[MultiTenant, Table("persons")]
	public class MongoTenantPerson : MongoPerson, IHaveTenantId, IHaveTimeStamp {
		[Column("tenant")]
		public string TenantId { get; set; }
	}
}
