namespace Deveel.Data {
	public class PersonRepositoryProviderNoKey : InMemoryRepositoryProvider<Person> {
		public PersonRepositoryProviderNoKey() 
			: base(null) {
		}

		public override InMemoryRepository<Person> CreateRepository(string tenantId, IList<Person>? entities = null)
			=> new PersonRepository(tenantId, entities);
	}

	public class PersonRepositoryProvider : InMemoryRepositoryProvider<Person, string> {
		public PersonRepositoryProvider(IDictionary<string, IList<Person>>? entities = null) 
			: base(entities) {
		}

		protected override InMemoryRepository<Person, string> CreateRepository(string tenantId, IList<Person>? entities = null)
			=> new PersonRepository(tenantId, entities);
	}
}
