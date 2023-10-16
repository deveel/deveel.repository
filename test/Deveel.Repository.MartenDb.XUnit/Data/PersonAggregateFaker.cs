using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bogus;

namespace Deveel.Data {
	public class PersonAggregateFaker : Faker<PersonAggregate> {
		public PersonAggregateFaker() {
			CustomInstantiator(f => {
				var p = new PersonAggregate();
				var firstName = f.Name.FirstName();
				var lastName = f.Name.LastName();
				var id = f.Random.Guid().ToString();
				var birthDate = f.Date.Past(100).OrNull(f);
				var email = f.Internet.Email(firstName, lastName).OrNull(f);
				var phone = f.Phone.PhoneNumber().OrNull(f);

				p.Apply(new PersonCreated(id, firstName, lastName, birthDate, email, phone));

				return p;
			});

			RuleFor(x => x.Relationships, (f, p) => {
				if (f.Random.Bool()) {
					var faker = new PersonRelationshipFaker();
					var relationships = faker.GenerateBetween(1, 5);
					foreach (var relationship in relationships) {
						p.Apply(new RelationshipAdded(relationship.Type, relationship.FullName));
					}
				}

				return p.Relationships;
			});
		}
	}
}
