namespace Deveel.Data {
	public class PersonRepository : InMemoryRepository<Person> {
		public PersonRepository(ISystemTime? systemTime = null) 
			: base(null, systemTime) {
		}

		internal PersonRepository(string tenantId, IList<Person>? entities = null, ISystemTime? systemTime = null) 
			: base(tenantId, entities, systemTime) {
		}
	}
}
