using Bogus;

namespace Deveel.Data {
	public class MongoPersonFaker : Faker<MongoPerson> {
		public MongoPersonFaker() {
			RuleFor(x => x.FirstName, f => f.Name.FirstName());
			RuleFor(x => x.LastName, f => f.Name.LastName().OrNull(f));
			RuleFor(x => x.DateOfBirth, f => f.Date.Past(20));
			RuleFor(x => x.Description, f => f.Lorem.Sentence().OrNull(f));
			RuleFor(x => x.Email, f => f.Internet.Email().OrNull(f));
			RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber().OrNull(f));
		}
	}
}
