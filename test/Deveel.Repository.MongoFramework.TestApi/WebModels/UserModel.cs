
using System.Text.Json.Serialization;

namespace Deveel.Data.WebModels {
	public class UserModel {
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("username")]
		public string Username { get; set; }

		[JsonPropertyName("email")]
		public string Email { get; set; }

		[JsonPropertyName("tenant")]
		public string? TenantId { get; set; }

		[JsonPropertyName("roles")]
		public string[]? Roles { get; set; }
	}
}
