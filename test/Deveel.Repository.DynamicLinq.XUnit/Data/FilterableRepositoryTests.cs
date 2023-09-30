using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	public class FilterableRepositoryTests {
		private readonly IList<Person> persons;
		private readonly IRepository<Person> repository;

		public FilterableRepositoryTests() {
			persons = new PersonFaker()
				.RuleFor(x => x.Id, f => Guid.NewGuid().ToString())
				.Generate(100)
				.ToList();

			repository = persons.AsRepository();
		}

		private Person RandomPerson() => persons[Random.Shared.Next(0, persons.Count - 1)];

		[Fact]
		public async Task Count_WithParameter() {
			var person = RandomPerson();
			var listCount = persons.Count(x => x.FirstName == person.FirstName);

			var count = await repository.CountAsync("p", $"p.FirstName == \"{person.FirstName}\"");

			Assert.Equal(listCount, count);
		}

		[Fact]
		public async Task Count_WithoutParameter() {
			var person = RandomPerson();
			var listCount = persons.Count(x => x.FirstName == person.FirstName);

			var count = await repository.CountAsync($"x.FirstName == \"{person.FirstName}\"");

			Assert.Equal(listCount, count);
		}

		[Fact]
		public async Task Count_WithInvalidExpression() {
			await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CountAsync("x.FirstName"));
		}

		[Fact]
		public async Task Exists_WithParameter() {
			var person = RandomPerson();
			var exists = persons.Any(x => x.FirstName == person.FirstName);

			var result = await repository.ExistsAsync("p", $"p.FirstName == \"{person.FirstName}\"");

			Assert.Equal(exists, result);
		}

		[Fact]
		public async Task Exists_WithoutParameter() {
			var person = RandomPerson();
			var exists = persons.Any(x => x.FirstName == person.FirstName);

			var result = await repository.ExistsAsync($"x.FirstName == \"{person.FirstName}\"");

			Assert.Equal(exists, result);
		}

		[Fact]
		public async Task Exists_WithInvalidExpression() {
			await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ExistsAsync("x.FirstName"));
		}

		[Fact]
		public async Task Find_WithParameter() {
			var person = RandomPerson();
			var expected = persons.FirstOrDefault(x => x.FirstName == person.FirstName);

			var result = await repository.FindAsync("p", $"p.FirstName == \"{person.FirstName}\"");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task Find_WithoutParameter() {
			var person = RandomPerson();
			var expected = persons.FirstOrDefault(x => x.FirstName == person.FirstName);

			var result = await repository.FindAsync($"x.FirstName == \"{person.FirstName}\"");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task Find_WithInvalidExpression() {
			await Assert.ThrowsAsync<InvalidOperationException>(() => repository.FindAsync("x.FirstName"));
		}

		[Fact]
		public async Task FindAll_WithParameter() {
			var person = RandomPerson();
			var expected = persons.Where(x => x.FirstName == person.FirstName).ToList();

			var result = await repository.FindAllAsync("p", $"p.FirstName == \"{person.FirstName}\"");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task FindAll_WithoutParameter() {
			var person = RandomPerson();
			var expected = persons.Where(x => x.FirstName == person.FirstName).ToList();

			var result = await repository.FindAllAsync($"x.FirstName == \"{person.FirstName}\"");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task FindAll_WithInvalidExpression() {
			await Assert.ThrowsAsync<InvalidOperationException>(() => repository.FindAllAsync("x.FirstName"));
		}
	}
}
