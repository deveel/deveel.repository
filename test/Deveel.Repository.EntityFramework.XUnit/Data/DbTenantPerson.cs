using System.ComponentModel.DataAnnotations.Schema;

namespace Deveel.Data {
	[Table("tenant_people")]
	public class DbTenantPerson : DbPerson {
		public string TenantId { get; set; }

		public new List<DbTenantPersonRelationship> Relationships { get; set; }
	}

	[Table("tenant_person_relationships")]	
	public class DbTenantPersonRelationship : DbPersonRelationship {
		public new DbTenantPerson Person { get; set; }
	}
}
