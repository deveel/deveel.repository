using Bogus;

namespace Deveel.Data {
	public class DbTenantPersonFaker : Faker<DbTenantPerson> {
		public DbTenantPersonFaker(string tenantId) {
			RuleFor(x => x.Id, f => Guid.NewGuid());
			RuleFor(x => x.FirstName, f => f.Person.FirstName);
			RuleFor(x => x.LastName, f => f.Person.LastName);
			RuleFor(x => x.Email, f => f.Person.Email.OrNull(f));
			RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth);
			RuleFor(x => x.PhoneNumber, f => f.Person.Phone.OrNull(f));
			RuleFor(x => x.TenantId, tenantId);

			var relTypes = new string[] { "father", "mother", "brother", "sister", "partner" };

			var relationshipFaker = new Faker<DbTenantPersonRelationship>()
				.RuleFor(x => x.FullName, f => f.Name.FullName())
				.RuleFor(x => x.Type, f => f.PickRandom(relTypes));

			RuleFor(x => x.Relationships, (f, p) => {
				return relationshipFaker.FinishWith((f2, x) => { x.Person = p; }).Generate(3);
			});
		}
	}
}
