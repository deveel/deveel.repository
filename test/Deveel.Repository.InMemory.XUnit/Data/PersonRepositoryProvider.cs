namespace Deveel.Data {
	public class PersonRepositoryProvider : InMemoryRepositoryProvider<Person> {
		public PersonRepositoryProvider(ISystemTime? systemTime = null) 
			: base(null, systemTime) {
		}

		public override InMemoryRepository<Person> CreateRepository(string tenantId, IList<Person>? entities = null)
			=> new PersonRepository(tenantId, entities, SystemTime);
	}
}
