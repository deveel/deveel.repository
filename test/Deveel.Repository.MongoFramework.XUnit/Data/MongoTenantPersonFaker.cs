using Bogus;

namespace Deveel.Data {
	public class MongoTenantPersonFaker : Faker<MongoTenantPerson> {
		public MongoTenantPersonFaker(string tenantId) {
			RuleFor(x => x.FirstName, f => f.Name.FirstName());
			RuleFor(x => x.LastName, f => f.Name.LastName().OrNull(f));
			RuleFor(x => x.BirthDate, f => f.Date.Past(20));
			RuleFor(x => x.Description, f => f.Lorem.Sentence().OrNull(f));
			RuleFor(x => x.TenantId, tenantId);
		}
	}
}
