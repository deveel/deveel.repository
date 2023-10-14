namespace Deveel.Data {
	public class PersonRepository : InMemoryRepository<Person> {
		public PersonRepository() 
			: base((IList<Person>) null) {
		}

		internal PersonRepository(string tenantId, IList<Person>? entities = null) 
			: base(tenantId, entities) {
		}
	}
}
