using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data.Entities {
	public class UserEntity : IHaveTenantId {
		[Key, Column("_id")]
		public ObjectId Id { get; set; }

		[Column("username")]
		public string Username { get; set; }

		[Column("email")]
		public string Email { get; set; }

		[Column("tenant")]
		public string TenantId { get; set; }

		[Column("roles")]
		public string[] Roles { get; set; }
	}
}
