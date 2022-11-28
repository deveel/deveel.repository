using Bogus;

namespace Deveel.Data {
	public class DynamicFilterTests {
		private readonly InMemoryRepository<Person> people;
		private readonly IList<Person> peopleList;

		public DynamicFilterTests() {
			var faker = new Faker<Person>()
				.RuleFor(x => x.Id, f => Guid.NewGuid().ToString("N"))
				.RuleFor(x => x.FirstName, f => f.Name.FirstName())
				.RuleFor(x => x.LastName, f => f.Name.LastName());

			peopleList = faker.Generate(210);

			people = new InMemoryRepository<Person>(peopleList);
		}

		[Fact]
		public async Task FindFirst() {
			var person = peopleList[33];

			var result = await people.FindAsync("person", $"FirstName == \"{person.FirstName}\"");

			Assert.NotNull(result);
			Assert.Equal(person.FirstName, result.FirstName);
		}

		[Fact]
		public async Task FindAll() {
			var person = peopleList[54];
			var expected = peopleList.Where(x => x.FirstName == person.FirstName).ToList();

			var result = await people.FindAllAsync($"FirstName == \"{person.FirstName}\"");

			Assert.NotEmpty(result);
			Assert.Equal(expected.Count, result.Count);
		}

		[Fact]
		public async Task Count() {
			var person = peopleList[54];
			var expected = peopleList.Where(x => x.FirstName == person.FirstName).LongCount();

			var result = await people.CountAsync($"FirstName == \"{person.FirstName}\"");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task Exists() {
			var person = peopleList[123];
			var expected = peopleList.Any(x => x.FirstName == person.FirstName);

			var result = await people.ExistsAsync($"FirstName == \"{person.FirstName}\"");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task FilteredPage() {
			var person = peopleList[14];
			var expected = peopleList.Where(x => x.FirstName == person.FirstName).ToList();
			var expectedCount = Math.Min(10, expected.Count);

			var page = new RepositoryPageRequest<Person>(1, 10)
				.Where($"FirstName == \"{person.FirstName}\"");

			var result = await people.GetPageAsync(page);

			Assert.NotNull(result);
			Assert.Equal(expected.Count, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(expectedCount, result.Items.Count());
		}



		protected class Person : IEntity {
			public string Id { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public DateOnly BirthDate { get; set; }
		}
	}
}
