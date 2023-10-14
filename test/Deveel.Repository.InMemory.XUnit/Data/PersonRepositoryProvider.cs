namespace Deveel.Data {
	public class PersonRepositoryProvider : InMemoryRepositoryProvider<Person> {
		public PersonRepositoryProvider() 
			: base(null) {
		}

		public override InMemoryRepository<Person> CreateRepository(string tenantId, IList<Person>? entities = null)
			=> new PersonRepository(tenantId, entities);
	}
}
