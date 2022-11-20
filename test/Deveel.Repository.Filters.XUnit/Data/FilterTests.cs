using System;

using Deveel.Filters;

namespace Deveel.Data {
	public class FilterTests {
		private readonly InMemoryRepository<Person> people;
		private readonly IList<Person> peopleList;

		public FilterTests() {
			var randomGen = new RandomNameGenerator();
			peopleList = Enumerable.Range(1, 210)
				.Select(_ => randomGen.NewName())
				.Select(x => new Person { Id = Guid.NewGuid().ToString("N"), FirstName = x.Item1, LastName = x.Item2 })
				.ToList();

			people = new InMemoryRepository<Person>(peopleList);
		}

		[Fact]
		public async Task FindFirst() {
			var person = peopleList[33];

			var result = await people.FindAsync(Filter.Equal("FirstName", person.FirstName));

			Assert.NotNull(result);
			Assert.Equal(person.FirstName, result.FirstName);
		}

		[Fact]
		public async Task FindAll() {
			var person = peopleList[54];
			var expected = peopleList.Where(x => x.FirstName == person.FirstName).ToList();

			var result = await people.FindAllAsync(Filter.Equal("FirstName", person.FirstName));

			Assert.NotEmpty(result);
			Assert.Equal(expected.Count, result.Count);
		}

		[Fact]
		public async Task Count() {
			var person = peopleList[54];
			var expected = peopleList.Where(x => x.FirstName == person.FirstName).LongCount();

			var result = await people.CountAsync(Filter.Equal("FirstName", person.FirstName));

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task Exists() {
			var person = peopleList[92];
			var expected = peopleList.Any(x => x.FirstName == person.FirstName);

			var result = await people.ExistsAsync(Filter.Equal("FirstName", person.FirstName));

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task FilteredPage() {
			var person = peopleList[14];
			var expected = peopleList.Where(x => x.FirstName == person.FirstName).ToList();
			var expectedCount = Math.Min(10, expected.Count);

			var page = new RepositoryPageRequest<Person>(1, 10)
				.Where(Filter.Equal("FirstName", person.FirstName));

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
