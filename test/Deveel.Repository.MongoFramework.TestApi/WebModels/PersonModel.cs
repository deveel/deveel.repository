using System.Text.Json.Serialization;

namespace Deveel.Data.WebModels {
    public class PersonModel {
		[JsonPropertyName("id")]
        public string? Id { get; set; }

		[JsonPropertyName ("fitst_name")]
        public string FirstName { get; set; }

		[JsonPropertyName ("last_name")]
        public string LastName { get; set; }

		[JsonPropertyName ("birth_date")]
        public DateTime? BirthDate { get; set; }

		[JsonPropertyName ("tenant")]
        public string? TenantId { get; set; }
    }
}
