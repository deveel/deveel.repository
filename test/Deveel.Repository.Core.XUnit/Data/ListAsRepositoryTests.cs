namespace Deveel.Data {
	public class ListAsRepositoryTests {
		private List<Person> persons;
		private IRepository<Person> repository;

		public ListAsRepositoryTests() {
			persons = new PersonFaker()
				.RuleFor(x => x.Id, f => Guid.NewGuid().ToString())
				.Generate(100)
				.ToList();

			repository = persons.AsRepository();
		}

		private Person RandomPerson() => persons[Random.Shared.Next(0, persons.Count - 1)];

		[Fact]
		public async Task ReadOnly_Add() {
			var list = persons.AsReadOnly();
			var readOnlyRepository = list.AsRepository();
			var person = new PersonFaker().Generate();

			await Assert.ThrowsAsync<NotSupportedException>(() => readOnlyRepository.AddAsync(person));
		}

		[Fact]
		public async Task Mutable_Add() {
			var listCount = persons.Count;

			var person = new PersonFaker().Generate();

			await repository.AddAsync(person);

			Assert.Equal(listCount + 1, repository.CountAll());
			Assert.NotNull(person.Id);
		}

		[Fact]
		public async Task Mutable_Add_WithId() {
			var listCount = persons.Count;

			var personId = Guid.NewGuid().ToString();
			var person = new PersonFaker().Generate();
			person.Id = personId;

			await repository.AddAsync(person);

			Assert.Equal(listCount + 1, repository.AsFilterable().CountAll());
			Assert.NotEqual(personId, person.Id);
		}

		[Fact]
		public async Task Mutable_Remove() {
			var listCount = persons.Count;

			var person = RandomPerson();

			var result = await repository.RemoveAsync(person);
			Assert.True(result);

			Assert.Equal(listCount - 1, repository.AsFilterable().CountAll());
		}

		[Fact]
		public async Task Mutable_RemoveById() {
			var listCount = persons.Count;

			var person = RandomPerson();

			var result = await repository.RemoveByIdAsync(person.Id!);
			Assert.True(result);

			Assert.Equal(listCount - 1, repository.AsFilterable().CountAll());
		}

		[Fact]
		public async Task Mutable_RemoveById_NotFound() {
			var listCount = persons.Count;

			var person = RandomPerson();

			var result = await repository.RemoveByIdAsync(Guid.NewGuid().ToString());
			Assert.False(result);

			Assert.Equal(listCount, repository.AsFilterable().CountAll());
		}

		[Fact]
		public async Task ReadOnly_Remove() {
			var list = persons.AsReadOnly();
			var readOnlyRepository = list.AsRepository();

			var person = RandomPerson();

			await Assert.ThrowsAsync<NotSupportedException>(() => readOnlyRepository.RemoveAsync(person));
		}


		[Fact]
		public async Task Filterable_CountAll() {
			Assert.IsAssignableFrom<IFilterableRepository<Person>> (repository);

			var count = await repository.AsFilterable().CountAllAsync();

			Assert.Equal(persons.Count, count);
		}

		[Fact]
		public async Task Filterable_Count() {
			var person = RandomPerson();

			Assert.IsAssignableFrom<IFilterableRepository<Person>>(repository);

			var count = await repository.AsFilterable().CountAsync(x => x.FirstName == person.FirstName);

			Assert.Equal(persons.Count(x => x.FirstName == person.FirstName), count);
		}

		[Fact]
		public async Task Filterable_Count_NotFound() {
			Assert.IsAssignableFrom<IFilterableRepository<Person>>(repository);

			var count = await repository.AsFilterable().CountAsync(x => x.FirstName == "Not Found");

			Assert.Equal(0, count);
		}

		[Fact]
		public async Task Filterable_FindAll() {
			Assert.IsAssignableFrom<IFilterableRepository<Person>>(repository);

			var persons = await repository.AsFilterable().FindAllAsync();

			Assert.Equal(this.persons.Count, persons.Count());
		}

		[Fact]
		public async Task Filterable_FindAll_WithFilter() {
			var person = RandomPerson();

			Assert.IsAssignableFrom<IFilterableRepository<Person>>(repository);

			var persons = await repository.AsFilterable().FindAllAsync(x => x.FirstName == person.FirstName);

			Assert.Equal(this.persons.Count(x => x.FirstName == person.FirstName), persons.Count());
		}

		[Fact]
		public async Task Filterable_FindAll_WithFilter_NotFound() {
			Assert.IsAssignableFrom<IFilterableRepository<Person>>(repository);

			var persons = await repository.AsFilterable().FindAllAsync(x => x.FirstName == "Not Found");

			Assert.Empty(persons);
		}
	}
}
