using Bogus;

namespace Deveel.Data {
	public class DbPersonFaker : Faker<DbPerson> {
		public DbPersonFaker() {
			var relTypes = new string[] { "father", "mother", "brother", "sister", "partner" };

			var relationshipFaker = new Faker<DbPersonRelationship>()
				.RuleFor(x => x.FullName, f => f.Name.FullName())
				.RuleFor(x => x.Type, f => f.PickRandom(relTypes));

			RuleFor(x => x.FirstName, f => f.Name.FirstName());
			RuleFor(x => x.LastName, f => f.Name.LastName());
			RuleFor(x => x.DateOfBirth, f => f.Date.Past(20));
			RuleFor(x => x.Email, f => f.Internet.Email().OrNull(f));
			RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber().OrNull(f));

			RuleFor(x => x.Relationships, (f, p) => {
				return relationshipFaker.FinishWith((f2, x) => { x.Person = p; }).Generate(3);
			});
		}
	}
}
