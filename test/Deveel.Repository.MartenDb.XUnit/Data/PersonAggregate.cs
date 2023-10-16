using System.ComponentModel.DataAnnotations;

using Marten.Schema;

namespace Deveel.Data {
	public class PersonAggregate : Aggregate, IPerson {
		public PersonAggregate(IEnumerable<object>? committedEvents = null) : base(committedEvents) {
		}

		public PersonAggregate() {
			ApplyEvent(new PersonCreated(Id = Guid.NewGuid().ToString("N")));
		}

		[Identity]
		public string? Id { get; private set; }

		public string FirstName { get; private set; }

		public string LastName { get; private set; }

		public string? Email { get; private set; }

		public DateTime? DateOfBirth { get; private set; }

		public string? PhoneNumber { get; private set; }

		IEnumerable<IRelationship> IPerson.Relationships => Relationships ?? new List<PersonRelationship>();

		public List<PersonRelationship>? Relationships { get; set; }

		protected override void ApplyEvent(object @event) {
			if (@event is PersonCreated createdEvent) {
				Id = createdEvent.Id;
				FirstName = createdEvent.FirstName ?? "";
				LastName = createdEvent.LastName ?? "";
				Email = createdEvent.Email;
				DateOfBirth = createdEvent.DateOfBirth;
				PhoneNumber = createdEvent.PhoneNumber;
			} else if (@event is PersonIdChanged personIdChanged) {
				Id = personIdChanged.Id;
			} else if (@event is PersonNameChanged nameChangedEvent) {
				FirstName = nameChangedEvent.FirstName ?? FirstName;
				LastName = nameChangedEvent.LastName ?? LastName;
			} else if (@event is PersonEmailChanged emailChangedEvent) {
				Email = emailChangedEvent.Email;
			} else if (@event is RelationshipAdded relationshipAdded) {
				if (Relationships == null)
					Relationships = new List<PersonRelationship>();

				Relationships.Add(new PersonRelationship { Type = relationshipAdded.Type, FullName = relationshipAdded.FullName });
			} else if (@event is RelationshipRemoved relationshipRemoved) {
				if (Relationships == null)
					return;

				var relationship = Relationships.FirstOrDefault(x => x.Type == relationshipRemoved.Type && x.FullName == relationshipRemoved.FullName);
				if (relationship != null)
					Relationships.Remove(relationship);
			}
		}
	}
}
