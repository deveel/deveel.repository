using Marten.Events;
using Marten.Events.Aggregation;

namespace Deveel.Data {
	public class PersonProjection : SingleStreamProjection<PersonDocument> {

		public PersonProjection() {
			CreateEvent<PersonCreated>(Create);
		}

		public PersonDocument Create(PersonCreated created) {
			return new PersonDocument {
				Id = created.Id,
				FirstName = created.FirstName,
				LastName = created.LastName,
				Email = created.Email,
				DateOfBirth = created.DateOfBirth,
				PhoneNumber = created.PhoneNumber,
			};
		}

		public void Apply(PersonDocument document, PersonNameChanged changed) {
			document.FirstName = changed.FirstName ?? document.FirstName;
			document.LastName = changed.LastName ?? document.LastName;
		}

		public void Apply(PersonDocument document, PersonEmailChanged changed) {
			document.Email = changed.Email;
		}

		public void Apply(PersonDocument document, RelationshipAdded added) {
			if (document.Relationships == null)
				document.Relationships = new List<PersonRelationship>();

			document.Relationships.Add(new PersonRelationship { Type = added.Type, FullName = added.FullName });
		}

		public void Apply(PersonDocument document, RelationshipRemoved removed) {
			if (document.Relationships == null)
				return;

			var relationship = document.Relationships.FirstOrDefault(x => x.Type == removed.Type && x.FullName == removed.FullName);
			if (relationship != null)
				document.Relationships.Remove(relationship);
		}
	}
}
