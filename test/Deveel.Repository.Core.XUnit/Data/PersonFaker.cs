using Bogus;

namespace Deveel.Data {
	public class PersonFaker : Faker<Person> {
		public PersonFaker() {
			RuleFor(x => x.FirstName, f => f.Name.FirstName());
			RuleFor(x => x.LastName, f => f.Name.LastName());
			RuleFor(x => x.Email, f => f.Internet.Email());
			RuleFor(x => x.Phone, f => f.Phone.PhoneNumber());
			RuleFor(x => x.DateOfBirth, f => f.Date.Past(20).OrNull(f));
		}
	}
}
