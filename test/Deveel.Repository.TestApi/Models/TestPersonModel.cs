using System.ComponentModel.DataAnnotations;

namespace Deveel.Data.Models {
	public class TestPersonModel : IEntity {
		public string Id { get; set; }

		[Required]
		public string FirstName { get; set; }

		public string? LastName { get; set; }

		public DateTime? BirthDate { get; set; }
	}
}
