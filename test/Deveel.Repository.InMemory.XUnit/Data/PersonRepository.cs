namespace Deveel.Data {
	public class PersonRepository : InMemoryRepository<Person> {
		public PersonRepository() 
			: base(Enumerable.Empty<Person>()) {
		}

		internal PersonRepository(string tenantId, IList<Person>? entities = null) 
			: base(tenantId, entities) {
		}
	}
}
