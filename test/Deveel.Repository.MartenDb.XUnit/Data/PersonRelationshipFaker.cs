using Bogus;

namespace Deveel.Data {
	public class PersonRelationshipFaker : Faker<PersonRelationship> {
		public PersonRelationshipFaker() {
			RuleFor(x => x.Type, f => f.PickRandom(new[] {"father", "mother", "sister", "brother"}));
			RuleFor(x => x.FullName, f => f.Name.FullName());
		}
	}
}
