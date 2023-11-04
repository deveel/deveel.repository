using System.Diagnostics.CodeAnalysis;

namespace Deveel.Data {
	public class PersonComparer<TPerson, TKey> : IEqualityComparer<TPerson>
		where TPerson : class, IPerson<TKey>
		where TKey : notnull {

		public bool Equals(TPerson? person, TPerson? other) {
			if (person == null && other == null)
				return true;
			if (person == null || other == null)
				return false;

			if (!Equals(person.Id, other.Id))
				return false;

			if (person.FirstName != other.FirstName ||
				person.LastName != other.LastName ||
				person.Email != other.Email ||
				person.PhoneNumber != other.PhoneNumber ||
				person.DateOfBirth != other.DateOfBirth)
				return false;


			// Related entities are unreliable to compare
			// because they are not loaded in the same way

			//if (person.Relationships == null && 
			//	other.Relationships == null)
			//	return true;

			//if (person.Relationships == null ||
			//	other.Relationships == null)
			//	return false;

			//var personRelationships = person.Relationships.ToList();
			//var otherRelationships = other.Relationships.ToList();

			//if (personRelationships.Count != otherRelationships.Count)
			//	return false;

			//for (var i = 0; i < personRelationships.Count; i++) {
			//	var personRelationship = personRelationships[i];
			//	var otherRelationship = otherRelationships[i];

			//	if (personRelationship.Type != otherRelationship.Type ||
			//		personRelationship.FullName != otherRelationship.FullName)
			//		return false;
			//}

			return true;
		}

		public int GetHashCode([DisallowNull] TPerson obj) => throw new NotImplementedException();
	}
}
