using Bogus;

namespace Deveel.Data.Entities {
	public class DbPersonRelationshipFaker : Faker<DbRelationship> {
		public DbPersonRelationshipFaker() {
			var relTypes = new string[] { "father", "mother", "brother", "sister", "partner" };

			RuleFor(x => x.FullName, f => f.Name.FullName());
			RuleFor(x => x.Type, f => f.PickRandom(relTypes));
		}
	}
}
