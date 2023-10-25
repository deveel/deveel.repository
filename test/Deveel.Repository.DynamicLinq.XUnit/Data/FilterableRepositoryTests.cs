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

			var result = await repository.FindFirstAsync("p", $"p.FirstName == \"{person.FirstName}\"");

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

		[Fact]
		public async Task GetFilteredPage_WithParameterName() {
			var person = RandomPerson();
			var list = persons.Where(x => x.FirstName == person.FirstName)
				.ToList();
			var totalPages = (int)Math.Ceiling((double)list.Count / 10);
			var pageItemCount = list.Count % 10;

			var pageRequest = new PageQuery<Person>(1, 10)
				.Where("p", $"p.FirstName == \"{person.FirstName}\"");

			var result = await repository.GetPageAsync(pageRequest);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(list.Count, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);

		}

		[Fact]
		public async Task GetFilteredPage() {
			var person = RandomPerson();
			var list = persons.Where(x => x.FirstName == person.FirstName)
				.ToList();
			var totalPages = (int)Math.Ceiling((double)list.Count / 10);
			var pageItemCount = list.Count % 10;

			var pageRequest = new PageQuery<Person>(1, 10)
				.Where($"x.FirstName == \"{person.FirstName}\"");

			var result = await repository.GetPageAsync(pageRequest);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(list.Count, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
		}

	}
}
