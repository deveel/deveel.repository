namespace Deveel.Data {
	public class ListAsRepositoryTests {
		private readonly List<Person> persons;
		private readonly IRepository<Person> repository;

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

			Assert.Equal(listCount + 1, await repository.CountAllAsync());
			Assert.NotNull(person.Id);
		}

		[Fact]
		public async Task Mutable_Add_WithId() {
			var listCount = persons.Count;

			var personId = Guid.NewGuid().ToString();
			var person = new PersonFaker().Generate();
			person.Id = personId;

			await repository.AddAsync(person);

			Assert.Equal(listCount + 1, repository.CountAll());
			Assert.NotEqual(personId, person.Id);
		}

		[Fact]
		public async Task Mutable_AddRange() {
			var listCount = persons.Count;

			var newPersons = new PersonFaker()
				.Generate(10)
				.ToList();

			var result = await repository.AddRangeAsync(newPersons);

			Assert.Equal(listCount + 10, repository.CountAll());
			Assert.Equal(10, result.Count);

			foreach (var person in newPersons) {
				Assert.NotNull(person.Id);
			}
		}

		[Fact]
		public async Task ReadOnly_AddRange() {
			var list = persons.AsReadOnly();
			var readOnlyRepository = list.AsRepository();
			var newPersons = new PersonFaker()
				.Generate(10)
				.ToList();

			await Assert.ThrowsAsync<NotSupportedException>(() => readOnlyRepository.AddRangeAsync(newPersons));
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

			Assert.Equal(this.persons.Count, persons.Count);
		}

		[Fact]
		public async Task Filterable_FindAll_WithFilter() {
			var person = RandomPerson();

			Assert.IsAssignableFrom<IFilterableRepository<Person>>(repository);

			var persons = await repository.AsFilterable().FindAllAsync(x => x.FirstName == person.FirstName);

			Assert.Equal(this.persons.Count(x => x.FirstName == person.FirstName), persons.Count);
		}

		[Fact]
		public async Task Filterable_FindAll_WithFilter_NotFound() {
			Assert.IsAssignableFrom<IFilterableRepository<Person>>(repository);

			var persons = await repository.AsFilterable().FindAllAsync(x => x.FirstName == "Not Found");

			Assert.Empty(persons);
		}

		[Fact]
		public async Task Queryable_GetPage() {
			var totalPages = (int)Math.Ceiling(persons.Count / 10.0);

			var request = new PageQuery<Person>(1, 10);
			var page = await repository.GetPageAsync(request);

			Assert.Equal(10, page.Request.Size);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(persons.Count, page.TotalItems);
			Assert.Equal(totalPages, page.TotalPages);
			Assert.NotNull(page.Items);
			Assert.Equal(10, page.Items.Count);
		}

		[Fact]
		public async Task Queryable_GetPage_WithFilter() {
			var person = RandomPerson();

			var subset = persons.Where(x => x.FirstName == person.FirstName).ToList();
			var itemCount = Math.Min(10, subset.Count);
			var totalPages = (int)Math.Ceiling(subset.Count / 10.0);

			var request = new PageQuery<Person>(1, 10)
				.Where(x => x.FirstName == person.FirstName);

			var page = await repository.GetPageAsync(request);

			Assert.Equal(10, page.Request.Size);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(subset.Count, page.TotalItems);
			Assert.Equal(totalPages, page.TotalPages);
			Assert.NotNull(page.Items);
			Assert.Equal(itemCount, page.Items.Count);
		}

		[Fact]
		public async Task Queryable_GetPage_WithFilter_NotFound() {
			var request = new PageQuery<Person>(1, 10)
				.Where(x => x.FirstName == "Not Found");

			var page = await repository.GetPageAsync(request);

			Assert.Equal(10, page.Request.Size);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(0, page.TotalItems);
			Assert.Equal(0, page.TotalPages);
			Assert.NotNull(page.Items);
			Assert.Empty(page.Items);
		}

		[Fact]
		public async Task Queryable_GetPage_WithSort() {
			var request = new PageQuery<Person>(1, 10)
				.OrderBy(x => x.FirstName);

			var page = await repository.GetPageAsync(request);

			Assert.Equal(10, page.Request.Size);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(persons.Count, page.TotalItems);
			Assert.NotNull(page.Items);
			Assert.Equal(10, page.Items.Count);
			Assert.Equal(persons.OrderBy(x => x.FirstName).First().FirstName, page.Items.First().FirstName);
		}

		[Fact]
		public async Task Queryable_GetPage_WithSort_Descending() {
			var request = new PageQuery<Person>(1, 10)
				.OrderByDescending(x => x.FirstName);

			var page = await repository.GetPageAsync(request);

			Assert.Equal(10, page.Request.Size);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(persons.Count, page.TotalItems);
			Assert.NotNull(page.Items);
			Assert.Equal(10, page.Items.Count);
			Assert.Equal(persons.OrderByDescending(x => x.FirstName).First().FirstName, page.Items.First().FirstName);
		}

		[Fact]
		public async Task Queryable_GetPage_WithFieldNameSort() {
			var request = new PageQuery<Person>(1, 10)
				.OrderBy("FirstName");

			var page = await repository.GetPageAsync(request);

			Assert.Equal(10, page.Request.Size);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(persons.Count, page.TotalItems);
			Assert.NotNull(page.Items);
			Assert.Equal(10, page.Items.Count);
			Assert.Equal(persons.OrderBy(x => x.FirstName).First().FirstName, page.Items.First().FirstName);
		}

		[Fact]
		public async Task Update_Existing() {
			var person = RandomPerson();

			var personId = person.Id;
			var personFirstName = person.FirstName;

			person.FirstName = "Updated";

			var result = await repository.UpdateAsync(person);

			Assert.True(result);
			Assert.Equal(personId, person.Id);
			Assert.Equal("Updated", person.FirstName);
		}

		[Fact]
		public async Task Update_NotFound() {
			var person = new PersonFaker()
				.RuleFor(x => x.Id, f => Guid.NewGuid().ToString())
				.Generate();

			var result = await repository.UpdateAsync(person);

			Assert.False(result);
		}
	}
}
