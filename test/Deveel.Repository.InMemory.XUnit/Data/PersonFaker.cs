using Bogus;

namespace Deveel.Data {
	public class PersonFaker : Faker<Person> {
		public PersonFaker() {
			RuleFor(x => x.FirstName, f => f.Name.FirstName());
			RuleFor(x => x.LastName, f => f.Name.LastName().OrNull(f));
			RuleFor(x => x.BirthDate, f => f.Date.Past(20));
		}
	}
}
