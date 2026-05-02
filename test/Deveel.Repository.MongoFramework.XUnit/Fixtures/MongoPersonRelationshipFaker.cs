using Bogus;

namespace Deveel.Data {
	public class MongoPersonRelationshipFaker : Faker<MongoPersonRelationship> {
		public MongoPersonRelationshipFaker() {
			RuleFor(x => x.FullName, f => f.Name.FullName());
			RuleFor(x => x.Type, f => f.PickRandom("friend", "family", "colleague"));
		}
	}
}
