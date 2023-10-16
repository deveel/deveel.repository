using Bogus;

namespace Deveel.Data {
	public class PersonDocumentFaker : Faker<PersonDocument> {
		public PersonDocumentFaker() {
			RuleFor(x => x.Id, f => f.Random.Guid().ToString());
			RuleFor(x => x.FirstName, f => f.Name.FirstName());
			RuleFor(x => x.LastName, f => f.Name.LastName().OrNull(f));
			RuleFor(x => x.DateOfBirth, f => f.Date.Past(10).OrNull(f));
			RuleFor(x => x.Email, (f, p) => f.Internet.Email(p.FirstName, p.LastName).OrNull(f));
			RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber().OrNull(f));
			RuleFor(x => x.Relationships, f => {
				var faker = new PersonRelationshipFaker();
				return f.Random.Bool() ? faker.GenerateBetween(1, 5) : null;
			});
		}
	}
}
